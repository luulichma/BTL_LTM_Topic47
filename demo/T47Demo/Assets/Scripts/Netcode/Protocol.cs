using System;
using System.IO;

namespace T47.Netcode
{
    public enum PacketType : byte
    {
        /// <summary>Client → server: lệnh điều khiển, kèm vài lệnh trước (gửi dư).</summary>
        Input = 1,

        /// <summary>Client → server: vị trí do client tự quyết (chế độ client tự quyết, hoặc gói giả mạo).</summary>
        State = 2,

        /// <summary>Server → client: ảnh trạng thái.</summary>
        Snapshot = 3,
    }

    public struct InputCmd
    {
        public uint Seq;
        public uint ClientTick;
        public Keys Keys;
    }

    public sealed class InputPacket
    {
        public byte PlayerId;

        /// <summary>Đồng hồ của client lúc gửi; server gửi trả lại để client đo RTT.</summary>
        public double ClientTime;

        /// <summary>Lệnh mới nhất nằm cuối; các lệnh trước là phần gửi dư.</summary>
        public InputCmd[] Inputs;
    }

    public sealed class StatePacket
    {
        public byte PlayerId;
        public double ClientTime;
        public Vec2 Pos;
    }

    public struct EntityState
    {
        public byte Id;
        public Vec2 Pos;
    }

    public sealed class SnapshotPacket
    {
        public uint ServerTick;

        /// <summary>Thời gian mô phỏng của server = ServerTick × Dt.</summary>
        public double ServerTime;

        /// <summary>Xác nhận: server đã xử lý tới lệnh số này của client nhận gói.</summary>
        public uint LastProcessedSeq;

        /// <summary>ClientTime mới nhất server nhận được, và thời gian server giữ nó trước khi gửi ảnh này.</summary>
        public double EchoClientTime;
        public double HoldTime;

        public EntityState[] Entities;
    }

    /// <summary>Mã hoá nhị phân các gói tin (little-endian, như BinaryWriter).</summary>
    public static class Protocol
    {
        public static byte[] Encode(InputPacket p)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write((byte)PacketType.Input);
                w.Write(p.PlayerId);
                w.Write(p.ClientTime);
                w.Write((byte)p.Inputs.Length);
                foreach (var i in p.Inputs)
                {
                    w.Write(i.Seq);
                    w.Write(i.ClientTick);
                    w.Write((byte)i.Keys);
                }
                return ms.ToArray();
            }
        }

        public static byte[] Encode(StatePacket p)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write((byte)PacketType.State);
                w.Write(p.PlayerId);
                w.Write(p.ClientTime);
                w.Write(p.Pos.X);
                w.Write(p.Pos.Y);
                return ms.ToArray();
            }
        }

        public static byte[] Encode(SnapshotPacket p)
        {
            using (var ms = new MemoryStream())
            using (var w = new BinaryWriter(ms))
            {
                w.Write((byte)PacketType.Snapshot);
                w.Write(p.ServerTick);
                w.Write(p.ServerTime);
                w.Write(p.LastProcessedSeq);
                w.Write(p.EchoClientTime);
                w.Write(p.HoldTime);
                w.Write((byte)p.Entities.Length);
                foreach (var e in p.Entities)
                {
                    w.Write(e.Id);
                    w.Write(e.Pos.X);
                    w.Write(e.Pos.Y);
                }
                return ms.ToArray();
            }
        }

        /// <summary>Trả về InputPacket, StatePacket hoặc SnapshotPacket; null nếu gói hỏng.</summary>
        public static object Decode(byte[] data)
        {
            try
            {
                using (var r = new BinaryReader(new MemoryStream(data)))
                {
                    switch ((PacketType)r.ReadByte())
                    {
                        case PacketType.Input:
                        {
                            var p = new InputPacket { PlayerId = r.ReadByte(), ClientTime = r.ReadDouble() };
                            p.Inputs = new InputCmd[r.ReadByte()];
                            for (int i = 0; i < p.Inputs.Length; i++)
                                p.Inputs[i] = new InputCmd { Seq = r.ReadUInt32(), ClientTick = r.ReadUInt32(), Keys = (Keys)r.ReadByte() };
                            return p;
                        }
                        case PacketType.State:
                            return new StatePacket
                            {
                                PlayerId = r.ReadByte(),
                                ClientTime = r.ReadDouble(),
                                Pos = new Vec2(r.ReadSingle(), r.ReadSingle()),
                            };
                        case PacketType.Snapshot:
                        {
                            var p = new SnapshotPacket
                            {
                                ServerTick = r.ReadUInt32(),
                                ServerTime = r.ReadDouble(),
                                LastProcessedSeq = r.ReadUInt32(),
                                EchoClientTime = r.ReadDouble(),
                                HoldTime = r.ReadDouble(),
                            };
                            p.Entities = new EntityState[r.ReadByte()];
                            for (int i = 0; i < p.Entities.Length; i++)
                                p.Entities[i] = new EntityState { Id = r.ReadByte(), Pos = new Vec2(r.ReadSingle(), r.ReadSingle()) };
                            return p;
                        }
                        default:
                            return null;
                    }
                }
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }
    }
}
