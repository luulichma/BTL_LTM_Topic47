using System;
using System.Collections.Generic;

namespace T47.Netcode
{
    public enum AuthorityMode
    {
        /// <summary>Client gửi vị trí, server chỉ chuyển tiếp (VĐ1, hướng A).</summary>
        ClientAuthoritative,

        /// <summary>Client chỉ gửi lệnh điều khiển, server mô phỏng (VĐ1, hướng C).</summary>
        ServerAuthoritative,
    }

    /// <summary>Server quyết định: vòng lặp nhịp cố định 30 nhịp/giây, gửi ảnh trạng thái theo tần số chỉnh được.</summary>
    public sealed class GameServer
    {
        sealed class Peer
        {
            public byte Id;
            public ITransport Transport;
            public PlayerState State;
            public readonly List<InputCmd> Queue = new List<InputCmd>();
            public uint LastProcessedSeq;
            public uint LastQueuedSeq;
            public float Credits;
            public double LastClientTime = -1;
            public double LastRecvAt;
        }

        /// <summary>Quỹ lệnh tối đa: cho phép bắt kịp khi lệnh đến dồn, nhưng trung bình không quá 1 lệnh/nhịp.</summary>
        const float MaxCredits = 3f;
        const int MaxQueue = 32;

        readonly List<Peer> _peers = new List<Peer>();
        double _accumulator;
        double _lastUpdate = -1;
        double _snapshotAccumulator;

        public AuthorityMode Mode = AuthorityMode.ServerAuthoritative;
        public float SnapshotRate = 20f;

        public uint Tick { get; private set; }
        public int ProcessedInputs { get; private set; }
        public int RejectedStatePackets { get; private set; }
        public int SnapshotsSent { get; private set; }

        /// <summary>Báo sự kiện cho lớp hiển thị (vd. gói vị trí giả mạo bị bỏ).</summary>
        public event Action<string> Log;

        public void AddPlayer(byte id, ITransport transport, Vec2 startPos)
        {
            _peers.Add(new Peer { Id = id, Transport = transport, State = new PlayerState { Pos = startPos } });
        }

        public Vec2 GetPosition(byte id)
        {
            foreach (var p in _peers)
                if (p.Id == id) return p.State.Pos;
            return default;
        }

        public void ResetPlayer(byte id, Vec2 pos)
        {
            foreach (var p in _peers)
            {
                if (p.Id != id) continue;
                p.State.Pos = pos;
                p.Queue.Clear();
                p.Credits = 0;
            }
        }

        /// <summary>Gọi mỗi khung hình; tự chạy đủ số nhịp cố định theo thời gian thực đã trôi qua.</summary>
        public void Update(double now)
        {
            if (_lastUpdate < 0) _lastUpdate = now;
            _accumulator += now - _lastUpdate;
            _lastUpdate = now;

            while (_accumulator >= Simulation.Dt)
            {
                _accumulator -= Simulation.Dt;
                SimulateTick(now);
            }
        }

        void SimulateTick(double now)
        {
            foreach (var peer in _peers)
                ReceiveAll(peer, now);

            Tick++;

            if (Mode == AuthorityMode.ServerAuthoritative)
            {
                foreach (var peer in _peers)
                    ProcessInputs(peer);
            }

            _snapshotAccumulator += Simulation.Dt;
            float period = 1f / Math.Max(1f, SnapshotRate);
            if (_snapshotAccumulator >= period)
            {
                _snapshotAccumulator %= period;
                foreach (var peer in _peers)
                    SendSnapshot(peer, now);
            }
        }

        void ReceiveAll(Peer peer, double now)
        {
            while (peer.Transport.TryReceive(out var data))
            {
                switch (Protocol.Decode(data))
                {
                    case InputPacket input:
                        TouchClientTime(peer, input.ClientTime, now);
                        if (Mode != AuthorityMode.ServerAuthoritative) break;
                        foreach (var cmd in input.Inputs)
                        {
                            // Bỏ lệnh trùng (gửi dư) hoặc đến muộn.
                            if (cmd.Seq <= peer.LastQueuedSeq) continue;
                            if (peer.Queue.Count >= MaxQueue) break;
                            peer.Queue.Add(cmd);
                            peer.LastQueuedSeq = cmd.Seq;
                        }
                        break;

                    case StatePacket state:
                        TouchClientTime(peer, state.ClientTime, now);
                        if (Mode == AuthorityMode.ClientAuthoritative)
                        {
                            // Client tự quyết: server tin luôn vị trí client gửi lên.
                            peer.State.Pos = state.Pos;
                        }
                        else
                        {
                            RejectedStatePackets++;
                            Log?.Invoke($"Server bỏ gói vị trí {state.Pos} của client {peer.Id}: chỉ nhận lệnh điều khiển");
                        }
                        break;
                }
            }
        }

        static void TouchClientTime(Peer peer, double clientTime, double now)
        {
            if (clientTime <= peer.LastClientTime) return;
            peer.LastClientTime = clientTime;
            peer.LastRecvAt = now;
        }

        void ProcessInputs(Peer peer)
        {
            // Mỗi nhịp được thêm 1 "lượt"; lượt để dành giúp bắt kịp khi lệnh đến dồn do dao động độ trễ.
            // Client gửi 60 lệnh/giây cũng chỉ được xử lý trung bình 30 lệnh/giây: chống hack tốc độ.
            peer.Credits = Math.Min(MaxCredits, peer.Credits + 1f);

            while (peer.Queue.Count > 0 && peer.Credits >= 1f)
            {
                var cmd = peer.Queue[0];
                peer.Queue.RemoveAt(0);
                peer.State = Simulation.Step(peer.State, cmd.Keys, Simulation.Dt);
                peer.LastProcessedSeq = cmd.Seq;
                peer.Credits -= 1f;
                ProcessedInputs++;
            }
        }

        void SendSnapshot(Peer target, double now)
        {
            var entities = new EntityState[_peers.Count];
            for (int i = 0; i < _peers.Count; i++)
                entities[i] = new EntityState { Id = _peers[i].Id, Pos = _peers[i].State.Pos };

            var snap = new SnapshotPacket
            {
                ServerTick = Tick,
                ServerTime = Tick * (double)Simulation.Dt,
                LastProcessedSeq = target.LastProcessedSeq,
                EchoClientTime = target.LastClientTime,
                HoldTime = target.LastClientTime < 0 ? 0 : now - target.LastRecvAt,
                Entities = entities,
            };
            target.Transport.Send(Protocol.Encode(snap));
            SnapshotsSent++;
        }
    }
}
