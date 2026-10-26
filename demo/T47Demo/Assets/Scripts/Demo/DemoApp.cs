using System.Collections.Generic;
using T47.Netcode;
using UnityEngine;

/// <summary>
/// Demo sơ bộ: 1 server + 2 client chạy trong cùng tiến trình, nối qua mạng giả lập.
/// Màn hình chia 3 khung (Client A · Server · Client B) và bảng điều khiển phía dưới.
/// Vẽ bằng IMGUI để scene không cần dựng sẵn đối tượng nào.
/// </summary>
public sealed class DemoApp : MonoBehaviour
{
    const byte IdA = 1, IdB = 2;
    const int TrailLength = 45;

    static readonly Vec2 StartA = new Vec2(5, 5), StartB = new Vec2(11, 5);
    static readonly Color ColorA = new Color(0.95f, 0.36f, 0.33f);
    static readonly Color ColorB = new Color(0.32f, 0.62f, 0.98f);
    static readonly Color Background = new Color(0.09f, 0.10f, 0.12f);
    static readonly Color ArenaColor = new Color(0.15f, 0.16f, 0.19f);
    static readonly Color GridColor = new Color(1f, 1f, 1f, 0.05f);
    static readonly Color ShadowColor = new Color(1f, 1f, 1f, 0.85f);

    struct DemoScene
    {
        public string Title, Action, Expect;
        public AuthorityMode Mode;
        public float Rtt, Jitter, Loss, SnapshotRate;
        public bool Prediction, Reconciliation, Interpolation;
    }

    // Kịch bản demo, theo bảng ở mục 7.3 của NoiDung_ChiTiet.md.
    static readonly DemoScene[] Scenes =
    {
        new DemoScene
        {
            Title = "Cảnh 0 · Client tự quyết", Mode = AuthorityMode.ClientAuthoritative,
            Rtt = 100, SnapshotRate = 20,
            Action = "Bấm T: gửi gói vị trí đã bị sửa",
            Expect = "Ô A nhảy tới chỗ bất kỳ; server và B đều chấp nhận (VĐ1)",
        },
        new DemoScene
        {
            Title = "Cảnh 1 · Server quyết định, chưa bật kỹ thuật nào", Mode = AuthorityMode.ServerAuthoritative,
            Rtt = 200, SnapshotRate = 20,
            Action = "A chạy qua lại; bấm lại T",
            Expect = "T hết tác dụng, nhưng bấm phím ~200 ms sau mới thấy chạy (VĐ1 → VĐ2)",
        },
        new DemoScene
        {
            Title = "Cảnh 2 · Bật dự đoán phía client", Mode = AuthorityMode.ServerAuthoritative,
            Rtt = 200, SnapshotRate = 20, Prediction = true,
            Action = "Nhấp nhanh D; rồi chạy và thả",
            Expect = "Nhấp: bị kéo về chỗ cũ (giật ngược); chạy: rung; thả: trôi thêm (VĐ2)",
        },
        new DemoScene
        {
            Title = "Cảnh 3 · Bật thêm đối chiếu với server", Mode = AuthorityMode.ServerAuthoritative,
            Rtt = 200, SnapshotRate = 20, Prediction = true, Reconciliation = true,
            Action = "Lặp lại thao tác",
            Expect = "Mượt, khớp với bóng server khi dừng; lệnh chờ ≈ RTT / nhịp (VĐ2)",
        },
        new DemoScene
        {
            Title = "Cảnh 4 · Đối thủ hiển thị giật", Mode = AuthorityMode.ServerAuthoritative,
            Rtt = 200, Jitter = 50, SnapshotRate = 10, Prediction = true, Reconciliation = true,
            Action = "Nhìn khung Client B; A chạy vòng tròn",
            Expect = "Ô A trong mắt B nhảy từng nấc, nhịp nhảy không đều (VĐ3)",
        },
        new DemoScene
        {
            Title = "Cảnh 5 · Bật nội suy thực thể", Mode = AuthorityMode.ServerAuthoritative,
            Rtt = 200, Jitter = 50, SnapshotRate = 10, Prediction = true, Reconciliation = true, Interpolation = true,
            Action = "Lặp lại; sau đó kéo tần số ảnh lên 20",
            Expect = "Mượt nhưng trễ hơn khung server; độ trễ nội suy 250 ms → 150 ms (VĐ3)",
        },
    };

    readonly NetworkConditions _net = new NetworkConditions();
    readonly List<string> _log = new List<string>();
    readonly Queue<Vec2> _trailSeenByA = new Queue<Vec2>();
    readonly Queue<Vec2> _trailSeenByB = new Queue<Vec2>();

    GameServer _server;
    GameClient _a, _b;
    SimulatedTransport _aClientEnd, _aServerEnd, _bClientEnd, _bServerEnd;

    AuthorityMode _mode = AuthorityMode.ServerAuthoritative;
    float _snapshotRate = 20;
    bool _prediction, _reconciliation, _interpolation;
    int _scene = -1;
    bool _autoplay;

    GUIStyle _title, _label, _small, _center;
    float _scale = 1f;

    static double Now => Time.realtimeSinceStartupAsDouble;

    /// <summary>Tự dựng demo trong bất kỳ scene nào, kể cả scene trống.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (FindAnyObjectByType<DemoApp>() != null) return;
        new GameObject("T47 Demo").AddComponent<DemoApp>();
    }

    void Awake()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        if (Camera.main == null)
        {
            var cam = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Background;
        }

        SimulatedTransport.CreatePair(_net, () => Now, 100, out _aClientEnd, out _aServerEnd);
        SimulatedTransport.CreatePair(_net, () => Now, 200, out _bClientEnd, out _bServerEnd);

        _server = new GameServer();
        _server.AddPlayer(IdA, _aServerEnd, StartA);
        _server.AddPlayer(IdB, _bServerEnd, StartB);
        _server.Log += AddLog;

        _a = new GameClient(IdA, _aClientEnd, StartA);
        _b = new GameClient(IdB, _bClientEnd, StartB);

        ApplyScene(0);
        ReadCommandLine();
    }

    /// <summary>
    /// Tham số dòng lệnh: -scene N (mở sẵn cảnh N), -autoplay (A tự chạy vòng tròn, tiện khi tập một mình),
    /// -screenshot path.png (chụp màn hình sau 4 giây rồi thoát, dùng để lấy hình cho slide).
    /// </summary>
    void ReadCommandLine()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-scene" && i + 1 < args.Length && int.TryParse(args[i + 1], out int n) && n >= 0 && n < Scenes.Length)
                ApplyScene(n);
            else if (args[i] == "-autoplay")
                _autoplay = true;
            else if (args[i] == "-screenshot" && i + 1 < args.Length)
                StartCoroutine(ScreenshotAndQuit(args[i + 1]));
        }
    }

    System.Collections.IEnumerator ScreenshotAndQuit(string path)
    {
        yield return new WaitForSecondsRealtime(4f);
        ScreenCapture.CaptureScreenshot(path);
        yield return new WaitForSecondsRealtime(1f);
        Application.Quit();
    }

    /// <summary>A bám theo một điểm chạy vòng tròn quanh giữa sân (bán kính 3, mỗi vòng 4 giây).</summary>
    Keys AutoplayKeys()
    {
        double angle = Now * Mathf.PI / 2.0;
        var target = new Vec2(8f + 3f * Mathf.Cos((float)angle), 5f + 3f * Mathf.Sin((float)angle));
        var d = target - _a.Self.Pos;
        const float deadZone = 0.15f;
        var k = Keys.None;
        if (d.X > deadZone) k |= Keys.Right;
        if (d.X < -deadZone) k |= Keys.Left;
        if (d.Y > deadZone) k |= Keys.Up;
        if (d.Y < -deadZone) k |= Keys.Down;
        return k;
    }

    void ApplyScene(int index)
    {
        var s = Scenes[index];
        _scene = index;
        _net.RttMs = s.Rtt;
        _net.JitterMs = s.Jitter;
        _net.LossPercent = s.Loss;
        _snapshotRate = s.SnapshotRate;
        _prediction = s.Prediction;
        _reconciliation = s.Reconciliation;
        _interpolation = s.Interpolation;
        SetMode(s.Mode);
        AddLog($"→ {s.Title}");
    }

    void SetMode(AuthorityMode mode)
    {
        if (mode == _mode) return;
        _mode = mode;
        ResetWorld();
    }

    void ResetWorld()
    {
        // Bỏ các gói đang trên đường của chế độ cũ.
        _aClientEnd.Outgoing.Clear();
        _aClientEnd.Incoming.Clear();
        _bClientEnd.Outgoing.Clear();
        _bClientEnd.Incoming.Clear();

        _server.ResetPlayer(IdA, StartA);
        _server.ResetPlayer(IdB, StartB);
        _a.Reset(StartA);
        _b.Reset(StartB);
        _trailSeenByA.Clear();
        _trailSeenByB.Clear();
    }

    void AddLog(string line)
    {
        _log.Add(line);
        if (_log.Count > 4) _log.RemoveAt(0);
    }

    void Update()
    {
        HandleHotkeys();

        _server.Mode = _a.Mode = _b.Mode = _mode;
        _server.SnapshotRate = _snapshotRate;
        double interpDelay = 2.0 / _snapshotRate + _net.JitterMs / 1000.0;
        foreach (var c in new[] { _a, _b })
        {
            c.Prediction = _prediction;
            c.Reconciliation = _reconciliation;
            c.Interpolation = _interpolation;
            c.InterpDelay = interpDelay;
        }

        double now = Now;
        var keysA = ReadKeys(KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D);
        if (_autoplay && keysA == Keys.None) keysA = AutoplayKeys();
        _a.Update(now, keysA);
        _b.Update(now, ReadKeys(KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow));
        _server.Update(now);

        if (_a.HasRemote(IdB)) Push(_trailSeenByA, _a.GetRemoteDisplay(IdB));
        if (_b.HasRemote(IdA)) Push(_trailSeenByB, _b.GetRemoteDisplay(IdA));
    }

    void HandleHotkeys()
    {
        for (int i = 0; i < Scenes.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                ApplyScene(i);
        }

        if (Input.GetKeyDown(KeyCode.F1))
            SetMode(_mode == AuthorityMode.ServerAuthoritative ? AuthorityMode.ClientAuthoritative : AuthorityMode.ServerAuthoritative);
        if (Input.GetKeyDown(KeyCode.F2)) _prediction = !_prediction;
        if (Input.GetKeyDown(KeyCode.F3)) _reconciliation = !_reconciliation;
        if (Input.GetKeyDown(KeyCode.F4)) _interpolation = !_interpolation;
        if (Input.GetKeyDown(KeyCode.F5)) ResetWorld();
        if (Input.GetKeyDown(KeyCode.T)) Teleport();
    }

    void Teleport()
    {
        var pos = new Vec2(Random.Range(1f, Simulation.ArenaWidth - 1f), Random.Range(1f, Simulation.ArenaHeight - 1f));
        _a.SendForgedPosition(pos);
        AddLog($"Client A gửi gói vị trí giả mạo {pos}");
    }

    static Keys ReadKeys(KeyCode up, KeyCode down, KeyCode left, KeyCode right)
    {
        var k = Keys.None;
        if (Input.GetKey(up)) k |= Keys.Up;
        if (Input.GetKey(down)) k |= Keys.Down;
        if (Input.GetKey(left)) k |= Keys.Left;
        if (Input.GetKey(right)) k |= Keys.Right;
        return k;
    }

    static void Push(Queue<Vec2> trail, Vec2 p)
    {
        trail.Enqueue(p);
        while (trail.Count > TrailLength) trail.Dequeue();
    }

    // ───────────────────────────── Vẽ ─────────────────────────────

    void OnGUI()
    {
        InitStyles();

        // Thiết kế theo khung ảo cao 900; co giãn theo cửa sổ.
        _scale = Screen.height / 900f;
        GUI.matrix = Matrix4x4.Scale(new Vector3(_scale, _scale, 1f));
        float w = Screen.width / _scale;
        const float pad = 16f;

        Fill(new Rect(0, 0, w, 900), Background);

        var scene = _scene >= 0 ? Scenes[_scene] : default;
        GUI.Label(new Rect(pad, 8, w - 2 * pad, 30), $"T47 · Demo sơ bộ    <color=#ffb454>{scene.Title}</color>", _title);
        GUI.Label(new Rect(pad, 40, w - 2 * pad, 22), $"Thao tác: <b>{scene.Action}</b>      Khán giả thấy: <b>{scene.Expect}</b>", _label);

        float colW = (w - 4 * pad) / 3f;
        // Màn hình quá rộng thì giới hạn bề ngang khung để bảng điều khiển vẫn vừa.
        float panelW = Mathf.Min(colW, 600f);
        float inset = (colW - panelW) / 2f;
        float top = 72f;
        float panelBottom = DrawClientPanel(new Rect(pad + inset, top, panelW, 0), _a, "Client A — người chơi WASD", ColorA, ColorB, IdB, _trailSeenByA);
        DrawServerPanel(new Rect(2 * pad + colW + inset, top, panelW, 0));
        DrawClientPanel(new Rect(3 * pad + 2 * colW + inset, top, panelW, 0), _b, "Client B — người chơi phím mũi tên", ColorB, ColorA, IdA, _trailSeenByB);

        float controlsTop = panelBottom + 12f;
        var controls = new Rect(pad, controlsTop, w - 2 * pad, 900 - controlsTop - pad);
        DrawControls(controls, colW, pad);
    }

    Rect ArenaRect(Rect panel) => new Rect(panel.x, panel.y + 26, panel.width, panel.width * Simulation.ArenaHeight / Simulation.ArenaWidth);

    float DrawClientPanel(Rect panel, GameClient c, string title, Color selfColor, Color otherColor, byte otherId, Queue<Vec2> trail)
    {
        GUI.Label(new Rect(panel.x, panel.y, panel.width, 24), $"<b>{title}</b>", _label);
        var arena = ArenaRect(panel);
        DrawArena(arena);

        var dotColor = otherColor;
        dotColor.a = 0.35f;
        foreach (var p in trail) Fill(Square(arena, p, 0.18f), dotColor);

        if (c.HasRemote(otherId))
        {
            var other = Square(arena, c.GetRemoteDisplay(otherId), Simulation.PlayerSize);
            Fill(other, otherColor);
            GUI.Label(other, otherId == IdA ? "A" : "B", _center);
        }

        Fill(Square(arena, c.Self.Pos, Simulation.PlayerSize), selfColor);
        GUI.Label(Square(arena, c.Self.Pos, Simulation.PlayerSize), c.Id == IdA ? "A" : "B", _center);
        if (c.HasServerSelf) Outline(Square(arena, c.ServerSelf, Simulation.PlayerSize), ShadowColor, 2f);

        string interp = _interpolation ? $"{c.InterpDelay * 1000:0} ms" : "tắt";
        if (c.IsExtrapolating(otherId)) interp += "  <color=#ffb454>(đang ngoại suy)</color>";

        float y = arena.yMax + 6;
        GUI.Label(new Rect(panel.x, y, panel.width, 22), $"RTT đo được: <b>{c.RttMs:0} ms</b>    Nhịp server mới nhất: <b>{c.LastServerTick}</b>", _label);
        GUI.Label(new Rect(panel.x, y + 22, panel.width, 22), $"Lệnh chờ xác nhận: <b>{c.PendingCount}</b>    Kéo lệch lần gần nhất: <b>{c.LastCorrection:0.00}</b>", _label);
        GUI.Label(new Rect(panel.x, y + 44, panel.width, 22), $"Độ trễ nội suy: <b>{interp}</b>", _label);
        GUI.Label(new Rect(panel.x, y + 66, panel.width, 22), "Ô đặc = hình người chơi thấy · viền trắng = bóng vị trí theo server · chấm = vết đối thủ", _small);
        return y + 90;
    }

    void DrawServerPanel(Rect panel)
    {
        GUI.Label(new Rect(panel.x, panel.y, panel.width, 24), "<b>Server — sự thật</b>", _label);
        var arena = ArenaRect(panel);
        DrawArena(arena);

        var a = _server.GetPosition(IdA);
        var b = _server.GetPosition(IdB);
        Fill(Square(arena, a, Simulation.PlayerSize), ColorA);
        GUI.Label(Square(arena, a, Simulation.PlayerSize), "A", _center);
        Fill(Square(arena, b, Simulation.PlayerSize), ColorB);
        GUI.Label(Square(arena, b, Simulation.PlayerSize), "B", _center);

        string mode = _mode == AuthorityMode.ServerAuthoritative ? "server quyết định" : "client tự quyết (chuyển tiếp)";
        float y = arena.yMax + 6;
        GUI.Label(new Rect(panel.x, y, panel.width, 22), $"Nhịp: <b>{_server.Tick}</b> ({Simulation.TickRate}/giây)    Chế độ: <b>{mode}</b>", _label);
        GUI.Label(new Rect(panel.x, y + 22, panel.width, 22), $"Ảnh trạng thái: <b>{_snapshotRate:0}/giây</b>    Lệnh đã xử lý: <b>{_server.ProcessedInputs}</b>", _label);
        GUI.Label(new Rect(panel.x, y + 44, panel.width, 22), $"Gói vị trí giả mạo bị bỏ: <b>{_server.RejectedStatePackets}</b>", _label);
        GUI.Label(new Rect(panel.x, y + 66, panel.width, 22),
            $"Gói mất  A↑ {_aClientEnd.Outgoing.Dropped}  A↓ {_aClientEnd.Incoming.Dropped}  B↑ {_bClientEnd.Outgoing.Dropped}  B↓ {_bClientEnd.Incoming.Dropped}", _small);
    }

    void DrawControls(Rect r, float colW, float pad)
    {
        Fill(r, new Color(1f, 1f, 1f, 0.03f));
        float x1 = r.x + 12, x2 = r.x + colW + pad + 12, x3 = r.x + 2 * (colW + pad) + 12;
        float cw = colW - 24;
        float y = r.y + 10;

        // Cột 1: mạng giả lập
        GUI.Label(new Rect(x1, y, cw, 24), "<b>Mạng giả lập</b>", _label);
        _net.RttMs = Slider(x1, y + 30, cw, $"RTT: {_net.RttMs:0} ms", _net.RttMs, 0, 500, 10);
        _net.JitterMs = Slider(x1, y + 80, cw, $"Dao động độ trễ: ±{_net.JitterMs:0} ms", _net.JitterMs, 0, 100, 5);
        _net.LossPercent = Slider(x1, y + 130, cw, $"Mất gói: {_net.LossPercent:0} %", _net.LossPercent, 0, 30, 1);
        _snapshotRate = Slider(x1, y + 180, cw, $"Tần số gửi ảnh trạng thái: {_snapshotRate:0} lần/giây", _snapshotRate, 5, 30, 1);
        GUI.Label(new Rect(x1, y + 230, cw, 40),
            $"Độ trễ nội suy tự tính = 2 × {1000f / _snapshotRate:0} ms + {_net.JitterMs:0} ms = <b>{2000f / _snapshotRate + _net.JitterMs:0} ms</b>", _label);

        // Cột 2: kỹ thuật
        GUI.Label(new Rect(x2, y, cw, 24), "<b>Kỹ thuật</b>", _label);
        float half = (cw - 8) / 2;
        bool clientAuth = _mode == AuthorityMode.ClientAuthoritative;
        if (GUI.Toggle(new Rect(x2, y + 30, half, 30), clientAuth, "Client tự quyết", GUI.skin.button) && !clientAuth)
            SetMode(AuthorityMode.ClientAuthoritative);
        if (GUI.Toggle(new Rect(x2 + half + 8, y + 30, half, 30), !clientAuth, "Server quyết định", GUI.skin.button) && clientAuth)
            SetMode(AuthorityMode.ServerAuthoritative);

        GUI.enabled = !clientAuth;
        _prediction = GUI.Toggle(new Rect(x2, y + 72, cw, 24), _prediction, "  Dự đoán phía client  [F2]");
        GUI.enabled = !clientAuth && _prediction;
        _reconciliation = GUI.Toggle(new Rect(x2, y + 100, cw, 24), _reconciliation, "  Đối chiếu với server  [F3]");
        GUI.enabled = true;
        _interpolation = GUI.Toggle(new Rect(x2, y + 128, cw, 24), _interpolation, "  Nội suy thực thể (đối thủ)  [F4]");

        if (GUI.Button(new Rect(x2, y + 166, cw, 30), "Dịch chuyển A bằng gói giả mạo  [T]")) Teleport();
        if (GUI.Button(new Rect(x2, y + 204, half, 28), "Đặt lại vị trí  [F5]")) ResetWorld();
        GUI.Label(new Rect(x2 + half + 8, y + 206, half, 24), "Đổi chế độ: [F1]", _small);

        // Cột 3: kịch bản
        GUI.Label(new Rect(x3, y, cw, 24), "<b>Kịch bản demo</b>  (phím 0–5)", _label);
        float bw = (cw - 5 * 6) / 6;
        for (int i = 0; i < Scenes.Length; i++)
        {
            bool on = GUI.Toggle(new Rect(x3 + i * (bw + 6), y + 30, bw, 30), _scene == i, i.ToString(), GUI.skin.button);
            if (on && _scene != i) ApplyScene(i);
        }

        float ly = y + 72;
        GUI.Label(new Rect(x3, ly, cw, 22), "<b>Nhật ký</b>", _label);
        for (int i = 0; i < _log.Count; i++)
            GUI.Label(new Rect(x3, ly + 24 + i * 22, cw, 22), _log[i], _small);
        GUI.Label(new Rect(x3, ly + 24 + 4 * 22 + 8, cw, 44),
            "A: WASD · B: phím mũi tên · F1 đổi chế độ · F2/F3/F4 bật tắt kỹ thuật", _small);
    }

    float Slider(float x, float y, float w, string label, float value, float min, float max, float step)
    {
        GUI.Label(new Rect(x, y, w, 22), label, _label);
        float v = GUI.HorizontalSlider(new Rect(x, y + 26, w, 18), value, min, max);
        return Mathf.Round(v / step) * step;
    }

    static void DrawArena(Rect arena)
    {
        Fill(arena, ArenaColor);
        for (int gx = 1; gx < Simulation.ArenaWidth; gx++)
            Fill(new Rect(arena.x + arena.width * gx / Simulation.ArenaWidth, arena.y, 1, arena.height), GridColor);
        for (int gy = 1; gy < Simulation.ArenaHeight; gy++)
            Fill(new Rect(arena.x, arena.y + arena.height * gy / Simulation.ArenaHeight, arena.width, 1), GridColor);
    }

    /// <summary>Đổi toạ độ thế giới (trục y hướng lên) sang hình vuông trên màn hình.</summary>
    static Rect Square(Rect arena, Vec2 p, float size)
    {
        float unit = arena.width / Simulation.ArenaWidth;
        float cx = arena.x + p.X * unit;
        float cy = arena.y + (Simulation.ArenaHeight - p.Y) * unit;
        float s = size * unit;
        return new Rect(cx - s / 2, cy - s / 2, s, s);
    }

    static void Fill(Rect r, Color c)
    {
        var old = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Texture2D.whiteTexture);
        GUI.color = old;
    }

    static void Outline(Rect r, Color c, float t)
    {
        Fill(new Rect(r.x, r.y, r.width, t), c);
        Fill(new Rect(r.x, r.yMax - t, r.width, t), c);
        Fill(new Rect(r.x, r.y, t, r.height), c);
        Fill(new Rect(r.xMax - t, r.y, t, r.height), c);
    }

    void InitStyles()
    {
        if (_label != null) return;
        var text = new Color(0.88f, 0.90f, 0.93f);
        _title = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, richText = true };
        _title.normal.textColor = text;
        _label = new GUIStyle(GUI.skin.label) { fontSize = 15, richText = true, wordWrap = true };
        _label.normal.textColor = text;
        _small = new GUIStyle(_label) { fontSize = 13 };
        _small.normal.textColor = new Color(0.65f, 0.68f, 0.73f);
        _center = new GUIStyle(_label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        _center.normal.textColor = Color.white;
        GUI.skin.button.fontSize = 14;
        GUI.skin.toggle.fontSize = 15;
        GUI.skin.toggle.normal.textColor = text;
        GUI.skin.toggle.onNormal.textColor = text;
    }
}
