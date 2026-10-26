using System;
using System.Collections.Generic;

namespace T47.Netcode
{
    /// <summary>
    /// Lớp truyền tin. Bản demo dùng SimulatedTransport; bản cuối thay bằng UdpTransport
    /// mà không phải sửa code dự đoán / đối chiếu / nội suy.
    /// </summary>
    public interface ITransport
    {
        void Send(byte[] packet);
        bool TryReceive(out byte[] packet);
    }

    /// <summary>Thông số mạng giả lập, chỉnh bằng thanh trượt trong lúc demo.</summary>
    public sealed class NetworkConditions
    {
        public float RttMs;
        public float JitterMs;
        public float LossPercent;
    }

    /// <summary>
    /// Một chiều truyền tin giả lập. Khi gửi: bỏ gói theo tỉ lệ mất gói, nếu không thì hẹn giờ giao
    /// deliverAt = now + RTT/2 ± dao động. Khi nhận: chỉ trả gói đã tới hạn.
    /// Mỗi gói có độ trễ riêng nên có thể đến sai thứ tự, giống UDP.
    /// </summary>
    public sealed class SimulatedLink
    {
        struct InFlight
        {
            public double DeliverAt;
            public long Order;
            public byte[] Data;
        }

        readonly NetworkConditions _conditions;
        readonly Func<double> _clock;
        readonly Random _rng;
        readonly List<InFlight> _queue = new List<InFlight>();
        long _order;

        public int Sent { get; private set; }
        public int Dropped { get; private set; }
        public int InFlightCount => _queue.Count;

        public SimulatedLink(NetworkConditions conditions, Func<double> clock, int seed)
        {
            _conditions = conditions;
            _clock = clock;
            _rng = new Random(seed);
        }

        public void Send(byte[] packet)
        {
            Sent++;
            if (_rng.NextDouble() * 100.0 < _conditions.LossPercent)
            {
                Dropped++;
                return;
            }

            double delayMs = _conditions.RttMs / 2.0 + (_rng.NextDouble() * 2.0 - 1.0) * _conditions.JitterMs;
            if (delayMs < 0) delayMs = 0;

            var item = new InFlight { DeliverAt = _clock() + delayMs / 1000.0, Order = _order++, Data = packet };

            // Chèn giữ hàng đợi sắp theo DeliverAt (hàng đợi ưu tiên đơn giản).
            int idx = _queue.Count;
            while (idx > 0 && _queue[idx - 1].DeliverAt > item.DeliverAt) idx--;
            _queue.Insert(idx, item);
        }

        public bool TryReceive(out byte[] packet)
        {
            if (_queue.Count > 0 && _queue[0].DeliverAt <= _clock())
            {
                packet = _queue[0].Data;
                _queue.RemoveAt(0);
                return true;
            }
            packet = null;
            return false;
        }

        public void Clear() => _queue.Clear();
    }

    /// <summary>Một đầu kết nối: gửi theo một SimulatedLink, nhận từ SimulatedLink chiều ngược lại.</summary>
    public sealed class SimulatedTransport : ITransport
    {
        public readonly SimulatedLink Outgoing;
        public readonly SimulatedLink Incoming;

        SimulatedTransport(SimulatedLink outgoing, SimulatedLink incoming)
        {
            Outgoing = outgoing;
            Incoming = incoming;
        }

        public void Send(byte[] packet) => Outgoing.Send(packet);
        public bool TryReceive(out byte[] packet) => Incoming.TryReceive(out packet);

        public static void CreatePair(NetworkConditions conditions, Func<double> clock, int seed,
            out SimulatedTransport clientEnd, out SimulatedTransport serverEnd)
        {
            var up = new SimulatedLink(conditions, clock, seed);
            var down = new SimulatedLink(conditions, clock, seed + 1);
            clientEnd = new SimulatedTransport(up, down);
            serverEnd = new SimulatedTransport(down, up);
        }
    }
}
