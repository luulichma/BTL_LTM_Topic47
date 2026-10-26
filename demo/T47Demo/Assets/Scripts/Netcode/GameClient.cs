using System;
using System.Collections.Generic;

namespace T47.Netcode
{
    /// <summary>
    /// Client: gửi lệnh điều khiển, dự đoán phía client, đối chiếu với server (VĐ2)
    /// và nội suy thực thể cho đối thủ (VĐ3). Mỗi kỹ thuật bật/tắt được để so sánh.
    /// </summary>
    public sealed class GameClient
    {
        sealed class RemoteEntity
        {
            public readonly List<(double Time, Vec2 Pos)> Buffer = new List<(double, Vec2)>();
            public Vec2 Display;
            public bool Extrapolating;
        }

        /// <summary>Số lệnh trước gửi kèm mỗi gói (gửi dư).</summary>
        const int Redundancy = 3;

        /// <summary>Ngoại suy tối đa khi thiếu ảnh trạng thái (VĐ3), rồi dừng lại.</summary>
        const double MaxExtrapolation = 0.1;

        readonly ITransport _transport;
        readonly List<InputCmd> _pending = new List<InputCmd>();
        readonly Dictionary<byte, RemoteEntity> _remotes = new Dictionary<byte, RemoteEntity>();
        double _accumulator;
        double _lastUpdate = -1;
        double _now;
        uint _seq;
        uint _clientTick;
        bool _hasSnapshot;
        bool _hasClockOffset;
        double _clockOffset;

        public readonly byte Id;
        public AuthorityMode Mode = AuthorityMode.ServerAuthoritative;
        public bool Prediction;
        public bool Reconciliation;
        public bool Interpolation;

        /// <summary>Độ trễ nội suy (giây); lớp demo đặt theo 2 × chu kỳ gửi ảnh + dao động độ trễ.</summary>
        public double InterpDelay = 0.1;

        /// <summary>Vị trí nhân vật của mình đang được vẽ.</summary>
        public PlayerState Self;

        /// <summary>Vị trí của mình theo ảnh trạng thái mới nhất ("bóng" server).</summary>
        public Vec2 ServerSelf;
        public bool HasServerSelf;

        public double RttMs { get; private set; }
        public uint LastServerTick { get; private set; }
        public int PendingCount => _pending.Count;

        /// <summary>Khoảng cách nhân vật bị kéo đi ở lần nhận ảnh trạng thái gần nhất.</summary>
        public float LastCorrection { get; private set; }

        public GameClient(byte id, ITransport transport, Vec2 startPos)
        {
            Id = id;
            _transport = transport;
            Self.Pos = startPos;
        }

        public IEnumerable<byte> RemoteIds => _remotes.Keys;

        public bool HasRemote(byte id) => _remotes.ContainsKey(id);

        public Vec2 GetRemoteDisplay(byte id) => _remotes.TryGetValue(id, out var r) ? r.Display : default;

        public bool IsExtrapolating(byte id) => _remotes.TryGetValue(id, out var r) && r.Extrapolating;

        public void Reset(Vec2 pos)
        {
            Self.Pos = pos;
            ServerSelf = pos;
            _pending.Clear();
            _accumulator = 0;
            foreach (var r in _remotes.Values) r.Buffer.Clear();
        }

        /// <summary>Gọi mỗi khung hình với các phím đang giữ.</summary>
        public void Update(double now, Keys keys)
        {
            _now = now;
            if (_lastUpdate < 0) _lastUpdate = now;
            _accumulator += now - _lastUpdate;
            _lastUpdate = now;

            while (_transport.TryReceive(out var data))
            {
                if (Protocol.Decode(data) is SnapshotPacket snap)
                    OnSnapshot(snap);
            }

            while (_accumulator >= Simulation.Dt)
            {
                _accumulator -= Simulation.Dt;
                SimulateTick(keys);
            }

            UpdateRemotes();
        }

        void SimulateTick(Keys keys)
        {
            _clientTick++;

            if (Mode == AuthorityMode.ClientAuthoritative)
            {
                // Client tự quyết: tự di chuyển rồi báo vị trí cho server.
                Self = Simulation.Step(Self, keys, Simulation.Dt);
                _transport.Send(Protocol.Encode(new StatePacket { PlayerId = Id, ClientTime = _now, Pos = Self.Pos }));
                return;
            }

            var cmd = new InputCmd { Seq = ++_seq, ClientTick = _clientTick, Keys = keys };
            _pending.Add(cmd);

            int count = Math.Min(_pending.Count, Redundancy + 1);
            var batch = _pending.GetRange(_pending.Count - count, count).ToArray();
            _transport.Send(Protocol.Encode(new InputPacket { PlayerId = Id, ClientTime = _now, Inputs = batch }));

            if (Prediction)
                Self = Simulation.Step(Self, keys, Simulation.Dt);
        }

        /// <summary>Giả lập client bị sửa: tự đặt vị trí rồi gửi gói vị trí lên server.</summary>
        public void SendForgedPosition(Vec2 pos)
        {
            Self.Pos = pos;
            _transport.Send(Protocol.Encode(new StatePacket { PlayerId = Id, ClientTime = _now, Pos = pos }));
        }

        void OnSnapshot(SnapshotPacket s)
        {
            // Ảnh cũ hoặc đến sai thứ tự: bỏ.
            if (_hasSnapshot && s.ServerTick <= LastServerTick) return;
            _hasSnapshot = true;
            LastServerTick = s.ServerTick;

            if (s.EchoClientTime >= 0)
            {
                double sample = (_now - s.EchoClientTime - s.HoldTime) * 1000.0;
                RttMs = RttMs <= 0 ? sample : RttMs + (sample - RttMs) * 0.2;
            }

            // Ước lượng đồng hồ server, làm mượt để không giật theo dao động độ trễ.
            double offset = s.ServerTime - _now;
            if (!_hasClockOffset || Math.Abs(offset - _clockOffset) > 1.0)
            {
                _clockOffset = offset;
                _hasClockOffset = true;
            }
            else
            {
                _clockOffset += (offset - _clockOffset) * 0.05;
            }

            foreach (var e in s.Entities)
            {
                if (e.Id == Id)
                {
                    ServerSelf = e.Pos;
                    HasServerSelf = true;
                    continue;
                }

                if (!_remotes.TryGetValue(e.Id, out var remote))
                {
                    remote = new RemoteEntity { Display = e.Pos };
                    _remotes[e.Id] = remote;
                }
                remote.Buffer.Add((s.ServerTime, e.Pos));
                while (remote.Buffer.Count > 2 && remote.Buffer[0].Time < s.ServerTime - 1.0)
                    remote.Buffer.RemoveAt(0);
            }

            if (Mode == AuthorityMode.ClientAuthoritative)
            {
                _pending.Clear();
                return;
            }

            var before = Self.Pos;

            // (1) tin theo server, (2) bỏ lệnh đã được xác nhận.
            Self.Pos = ServerSelf;
            _pending.RemoveAll(i => i.Seq <= s.LastProcessedSeq);

            // (3) chạy lại các lệnh còn chờ. Không có bước này thì nhân vật bị giật ngược.
            if (Prediction && Reconciliation)
            {
                foreach (var i in _pending)
                    Self = Simulation.Step(Self, i.Keys, Simulation.Dt);
            }

            LastCorrection = (Self.Pos - before).Length;
        }

        void UpdateRemotes()
        {
            double renderTime = _now + _clockOffset - InterpDelay;

            foreach (var remote in _remotes.Values)
            {
                var buf = remote.Buffer;
                remote.Extrapolating = false;
                if (buf.Count == 0) continue;

                if (!Interpolation)
                {
                    remote.Display = buf[buf.Count - 1].Pos;
                    continue;
                }

                if (renderTime <= buf[0].Time)
                {
                    remote.Display = buf[0].Pos;
                    continue;
                }

                int i = 0;
                while (i < buf.Count - 1 && buf[i + 1].Time <= renderTime) i++;

                if (i < buf.Count - 1)
                {
                    var s0 = buf[i];
                    var s1 = buf[i + 1];
                    float t = (float)((renderTime - s0.Time) / (s1.Time - s0.Time));
                    remote.Display = Vec2.Lerp(s0.Pos, s1.Pos, t);
                }
                else if (buf.Count >= 2)
                {
                    // Hết dữ liệu phía sau thời điểm vẽ: ngoại suy ngắn theo vận tốc cuối.
                    var a = buf[buf.Count - 2];
                    var b = buf[buf.Count - 1];
                    double ahead = Math.Min(renderTime - b.Time, MaxExtrapolation);
                    var velocity = (b.Pos - a.Pos) * (float)(1.0 / (b.Time - a.Time));
                    remote.Display = b.Pos + velocity * (float)ahead;
                    remote.Extrapolating = true;
                }
                else
                {
                    remote.Display = buf[0].Pos;
                }
            }
        }
    }
}
