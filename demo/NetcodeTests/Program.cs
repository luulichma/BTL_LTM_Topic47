using System;
using System.Collections.Generic;
using T47.Netcode;

// Kiểm tra lõi netcode bằng đồng hồ giả, chạy 60 khung hình/giây. Mỗi kịch bản ứng với một cảnh demo.
static class Program
{
    static int _failed;

    static void Main()
    {
        ClientAuthoritativeAcceptsTeleport();
        ServerAuthoritativeRejectsTeleport();
        NoPredictionHasOneRttDelay();
        PredictionOnlyRubberBands();
        ReconciliationRemovesCorrection();
        ReconciliationSurvivesJitterAndLoss();
        SpeedHackIsCapped();
        InterpolationSmoothsOpponent();
        OutOfOrderSnapshotsAreDropped();

        Console.WriteLine(_failed == 0 ? "\nTẤT CẢ ĐỀU ĐẠT" : $"\n{_failed} KIỂM TRA KHÔNG ĐẠT");
        Environment.Exit(_failed == 0 ? 0 : 1);
    }

    sealed class World
    {
        public double Now;
        public readonly NetworkConditions Net = new NetworkConditions();
        public readonly GameServer Server = new GameServer();
        public readonly GameClient A, B;
        public readonly SimulatedTransport AEnd, BEnd;

        public static readonly Vec2 StartA = new Vec2(4, 5), StartB = new Vec2(12, 5);

        public World(float rttMs, float jitterMs = 0, float lossPct = 0, float snapshotRate = 20)
        {
            Net.RttMs = rttMs;
            Net.JitterMs = jitterMs;
            Net.LossPercent = lossPct;
            Server.SnapshotRate = snapshotRate;

            SimulatedTransport.CreatePair(Net, () => Now, 1, out AEnd, out var aServer);
            SimulatedTransport.CreatePair(Net, () => Now, 10, out BEnd, out var bServer);
            Server.AddPlayer(1, aServer, StartA);
            Server.AddPlayer(2, bServer, StartB);
            A = new GameClient(1, AEnd, StartA);
            B = new GameClient(2, BEnd, StartB);
            double delay = 2.0 / snapshotRate + jitterMs / 1000.0;
            A.InterpDelay = B.InterpDelay = delay;
        }

        public void SetMode(AuthorityMode m) { Server.Mode = A.Mode = B.Mode = m; }

        public void SetFlags(bool pred, bool rec, bool interp)
        {
            foreach (var c in new[] { A, B }) { c.Prediction = pred; c.Reconciliation = rec; c.Interpolation = interp; }
        }

        public void Frame(Keys a, Keys b = Keys.None)
        {
            Now += 1.0 / 60.0;
            A.Update(Now, a);
            B.Update(Now, b);
            Server.Update(Now);
        }

        public void Run(double seconds, Keys a, Keys b = Keys.None, Action each = null)
        {
            int frames = (int)Math.Round(seconds * 60);
            for (int i = 0; i < frames; i++) { Frame(a, b); each?.Invoke(); }
        }
    }

    static void Check(string name, bool ok, string detail)
    {
        Console.WriteLine($"[{(ok ? "ĐẠT" : "LỖI")}] {name}: {detail}");
        if (!ok) _failed++;
    }

    static void ClientAuthoritativeAcceptsTeleport()
    {
        var w = new World(100);
        w.SetMode(AuthorityMode.ClientAuthoritative);
        w.Run(0.5, Keys.None);
        w.A.SendForgedPosition(new Vec2(15, 9));
        w.Run(0.5, Keys.None);
        var server = w.Server.GetPosition(1);
        var seenByB = w.B.GetRemoteDisplay(1);
        Check("Cảnh 0 – client tự quyết", server.X > 14 && seenByB.X > 14,
            $"server thấy A ở {server}, B thấy A ở {seenByB}");
    }

    static void ServerAuthoritativeRejectsTeleport()
    {
        var w = new World(200);
        w.SetMode(AuthorityMode.ServerAuthoritative);
        w.SetFlags(true, true, false);
        w.Run(0.5, Keys.None);
        w.A.SendForgedPosition(new Vec2(15, 9));
        w.Run(0.5, Keys.None);
        var server = w.Server.GetPosition(1);
        Check("Cảnh 1 – server bỏ gói vị trí giả mạo",
            server.X == World.StartA.X && w.Server.RejectedStatePackets == 1 && w.A.Self.Pos.X == World.StartA.X,
            $"server giữ A ở {server}, A bị kéo về {w.A.Self.Pos}, gói bị bỏ = {w.Server.RejectedStatePackets}");
    }

    static void NoPredictionHasOneRttDelay()
    {
        var w = new World(200);
        w.SetFlags(false, false, false);
        w.Run(0.5, Keys.None);
        double pressed = w.Now;
        double moved = -1;
        w.Run(1.0, Keys.Right, each: () => { if (moved < 0 && w.A.Self.Pos.X > World.StartA.X) moved = w.Now; });
        double ms = (moved - pressed) * 1000;
        Check("Cảnh 1 – chờ server", ms >= 200 && ms <= 300, $"bấm phím {ms:0} ms sau mới thấy chạy (RTT 200 ms)");
    }

    static float MaxCorrectionWhileRunning(World w)
    {
        w.Run(0.5, Keys.None);
        float max = 0;
        w.Run(1.5, Keys.Right, each: () => max = Math.Max(max, w.A.LastCorrection));
        return max;
    }

    static void PredictionOnlyRubberBands()
    {
        var w = new World(200);
        w.SetFlags(true, false, false);
        float max = MaxCorrectionWhileRunning(w);
        Check("Cảnh 2 – chỉ dự đoán bị giật ngược", max > 0.3f, $"mỗi ảnh trạng thái kéo lùi nhân vật tới {max:0.00} đơn vị");
    }

    static void ReconciliationRemovesCorrection()
    {
        var w = new World(200);
        w.SetFlags(true, true, false);
        float max = MaxCorrectionWhileRunning(w);
        int pending = w.A.PendingCount;
        w.Run(1.0, Keys.None);
        float diff = (w.A.Self.Pos - w.Server.GetPosition(1)).Length;
        Check("Cảnh 3 – đối chiếu", max < 0.01f && pending >= 5 && pending <= 10 && diff < 0.001f,
            $"sai lệch tối đa {max:0.000}, lệnh chờ {pending} (≈ (RTT + chờ nhịp + chờ ảnh) / nhịp), lệch cuối {diff:0.000}");
    }

    static void ReconciliationSurvivesJitterAndLoss()
    {
        var w = new World(200, jitterMs: 50, lossPct: 10);
        w.SetFlags(true, true, false);
        w.Run(0.5, Keys.None);
        w.Run(2.0, Keys.Right);
        w.Run(1.0, Keys.Up);
        w.Run(1.5, Keys.None);
        float diff = (w.A.Self.Pos - w.Server.GetPosition(1)).Length;
        Check("Đối chiếu khi có dao động trễ 50 ms + mất 10% gói", diff < 0.001f,
            $"client {w.A.Self.Pos}, server {w.Server.GetPosition(1)}, gói lệnh mất {w.AEnd.Outgoing.Dropped}/{w.AEnd.Outgoing.Sent}");
    }

    static void SpeedHackIsCapped()
    {
        var w = new World(50);
        w.Run(0.5, Keys.None);
        uint seq = 0;
        double start = w.Server.GetPosition(1).X;
        // Client bị sửa gửi 60 lệnh/giây thay vì 30.
        for (int f = 0; f < 60; f++)
        {
            w.AEnd.Send(Protocol.Encode(new InputPacket
            {
                PlayerId = 1,
                ClientTime = w.Now,
                Inputs = new[] { new InputCmd { Seq = ++seq, Keys = Keys.Right } },
            }));
            w.Frame(Keys.None);
        }
        w.Run(0.5, Keys.None);
        double moved = w.Server.GetPosition(1).X - start;
        Check("Chống hack tốc độ", moved <= Simulation.Speed * 1.0 + 0.6,
            $"gửi 60 lệnh trong 1 s, server chỉ cho đi {moved:0.00} đơn vị (tối đa hợp lệ ≈ 6)");
    }

    static void InterpolationSmoothsOpponent()
    {
        float MaxJump(bool interp, out double lagMs)
        {
            var w = new World(200, jitterMs: 50, snapshotRate: 10);
            w.SetFlags(true, true, interp);
            w.Run(1.0, Keys.None);
            float max = 0;
            var prev = w.B.GetRemoteDisplay(1);
            var samples = new List<double>();
            Keys[] circle = { Keys.Right, Keys.Right | Keys.Up, Keys.Up, Keys.Up | Keys.Left, Keys.Left, Keys.Left | Keys.Down, Keys.Down, Keys.Down | Keys.Right };
            for (int lap = 0; lap < 3; lap++)
                foreach (var k in circle)
                    w.Run(0.25, k, each: () =>
                    {
                        var cur = w.B.GetRemoteDisplay(1);
                        max = Math.Max(max, (cur - prev).Length);
                        prev = cur;
                    });
            // Đo độ trễ: chạy thẳng, so vị trí B thấy với lịch sử vị trí trên server.
            var history = new List<(double, float)>();
            w.Run(1.0, Keys.None);
            w.Run(1.2, Keys.Right, each: () =>
            {
                history.Add((w.Now, w.Server.GetPosition(1).X));
                float seen = w.B.GetRemoteDisplay(1).X;
                foreach (var (t, x) in history)
                    if (x >= seen) { samples.Add(w.Now - t); break; }
            });
            samples.Sort();
            lagMs = samples[samples.Count / 2] * 1000;
            return max;
        }

        float raw = MaxJump(false, out double rawLag);
        float smooth = MaxJump(true, out double smoothLag);
        Check("Cảnh 4/5 – nội suy (10 ảnh/giây, dao động 50 ms)", smooth < 0.2f && raw > 0.5f,
            $"bước nhảy lớn nhất mỗi khung: không nội suy {raw:0.00}, có nội suy {smooth:0.00}; " +
            $"độ trễ hình ảnh đối thủ {rawLag:0} ms → {smoothLag:0} ms (lý thuyết ≈ 100 + 250 = 350 ms)");
    }

    static void OutOfOrderSnapshotsAreDropped()
    {
        var w = new World(100, jitterMs: 80, snapshotRate: 30);
        w.SetFlags(true, true, false);
        uint last = 0;
        bool monotonic = true;
        w.Run(3.0, Keys.Right, each: () =>
        {
            if (w.A.LastServerTick < last) monotonic = false;
            last = w.A.LastServerTick;
        });
        Check("Ảnh trạng thái đến sai thứ tự bị bỏ", monotonic, $"nhịp server mới nhất ở client luôn tăng, cuối = {last}");
    }
}
