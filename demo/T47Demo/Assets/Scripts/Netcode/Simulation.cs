using System;

namespace T47.Netcode
{
    /// <summary>Vectơ 2 chiều tự viết để code mô phỏng không phụ thuộc UnityEngine.</summary>
    public struct Vec2
    {
        public float X, Y;

        public Vec2(float x, float y) { X = x; Y = y; }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, float k) => new Vec2(a.X * k, a.Y * k);

        public float Length => (float)Math.Sqrt(X * X + Y * Y);

        public static Vec2 Lerp(Vec2 a, Vec2 b, float t) => new Vec2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

        public override string ToString() => $"({X:0.00}, {Y:0.00})";
    }

    /// <summary>Mỗi bit ứng với một phím (trường keys:u8 của gói INPUT).</summary>
    [Flags]
    public enum Keys : byte
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 4,
        Right = 8,
    }

    /// <summary>Trạng thái một người chơi. Kiểu struct để sao chép, lưu lịch sử và chạy lại được.</summary>
    public struct PlayerState
    {
        public Vec2 Pos;
    }

    /// <summary>
    /// Mô phỏng dùng chung cho client và server (VĐ2): cùng trạng thái + cùng lệnh => cùng kết quả.
    /// Không dùng API của Unity (Transform, Physics, Time.deltaTime).
    /// </summary>
    public static class Simulation
    {
        public const int TickRate = 30;
        public const float Dt = 1f / TickRate;

        /// <summary>6 đơn vị/giây = 0,2 đơn vị mỗi nhịp (ví dụ bằng số ở VĐ2).</summary>
        public const float Speed = 6f;

        public const float ArenaWidth = 16f;
        public const float ArenaHeight = 10f;
        public const float PlayerSize = 1f;

        public static PlayerState Step(PlayerState s, Keys keys, float dt)
        {
            float dx = 0, dy = 0;
            if ((keys & Keys.Up) != 0) dy += 1;
            if ((keys & Keys.Down) != 0) dy -= 1;
            if ((keys & Keys.Left) != 0) dx -= 1;
            if ((keys & Keys.Right) != 0) dx += 1;

            if (dx != 0 && dy != 0)
            {
                // Đi chéo không được nhanh hơn đi thẳng.
                const float invSqrt2 = 0.70710678f;
                dx *= invSqrt2;
                dy *= invSqrt2;
            }

            const float half = PlayerSize / 2f;
            s.Pos.X = Clamp(s.Pos.X + dx * Speed * dt, half, ArenaWidth - half);
            s.Pos.Y = Clamp(s.Pos.Y + dy * Speed * dt, half, ArenaHeight - half);
            return s;
        }

        static float Clamp(float v, float min, float max) => v < min ? min : (v > max ? max : v);
    }
}
