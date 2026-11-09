# 01 — Hệ thống bố cục: UiScale, UiTheme, UiWidgets

Quyết định liên quan: Q1, Q2, Q8.

## 1. Vì sao giữ IMGUI

| Phương án | Ưu | Nhược | Kết luận |
|-----------|----|-------|----------|
| IMGUI + helper | Không đổi scene/prefab; code thuần C#, dễ giải thích khi vấn đáp; sửa trong vài giờ | Phải tự làm theme; chữ có thể hơi mờ khi phóng to nhiều | **Chọn** |
| UI Toolkit | Responsive thật (flex), đẹp | Viết lại 6 màn + UXML/USS; framework mới | Bỏ |
| uGUI + Canvas Scaler | Chuẩn công nghiệp | Thêm package, dựng prefab; `C2SceneSetup` đang tạo scene rỗng bằng code | Bỏ |

## 2. `UiScale` — scale theo độ phân giải

File mới: `Assets/Scripts/UI/Core/UiScale.cs` (static class).

```csharp
public static class UiScale
{
    public const float RefWidth = 1280f, RefHeight = 720f;
    public const float MinScale = 0.65f, MaxScale = 2.0f;

    // Hệ số scale: chọn cạnh "chật" hơn để không có gì tràn ra ngoài.
    public static float Factor =>
        Mathf.Clamp(Mathf.Min(Screen.width / RefWidth, Screen.height / RefHeight), MinScale, MaxScale);

    // Kích thước màn hình theo đơn vị ảo (sau khi chia scale).
    public static float Width  => Screen.width  / Factor;
    public static float Height => Screen.height / Factor;

    // Gọi đầu mỗi OnGUI. Trả về matrix cũ để khôi phục.
    public static Matrix4x4 Begin()
    {
        Matrix4x4 old = GUI.matrix;
        float s = Factor;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1));
        return old;
    }
    public static void End(Matrix4x4 old) => GUI.matrix = old;

    // Neo một hình chữ nhật theo 9 vị trí (TopLeft, TopCenter, Center, BottomRight...).
    public static Rect Anchor(Anchor a, float w, float h, float margin = 12f) { ... }
}
```

Quy tắc khi sửa các màn:
- Mọi `OnGUI` bắt đầu bằng `var m = UiScale.Begin();` và kết thúc bằng `UiScale.End(m);`.
- **Thay mọi `Screen.width/height` trong code vẽ GUI bằng `UiScale.Width/Height`.** Đây là lỗi dễ mắc nhất.
- Kích thước panel viết theo đơn vị ảo của màn 1280×720 và chặn bằng `Mathf.Min(w, UiScale.Width - 2*margin)`.
- Chuột/bàn phím: IMGUI tự biến đổi toạ độ sự kiện theo `GUI.matrix`, không cần xử lý thêm.
- Ngoại lệ: `Camera.rect` và `WorldToScreenPoint` dùng **pixel thật**. Khi đổi sang toạ độ GUI thì chia cho `UiScale.Factor`.

Rủi ro đã biết: phóng to bằng matrix (scale > 1) làm chữ hơi mờ. Nếu thấy rõ ở 1920×1080, cách khắc phục là nhân `fontSize` của style theo `Factor` và để matrix chỉ scale ≤ 1. Ghi nhận, chưa làm ở P0.

## 3. `UiTheme` — giao diện sinh bằng code

File mới: `Assets/Scripts/UI/Core/UiTheme.cs`. Tạo một `GUISkin` lazy (khởi tạo lần đầu trong `OnGUI`, vì `GUI.skin` chỉ dùng được trong `OnGUI`), cache dùng chung cho mọi màn.

Bảng màu (token):

| Token | Giá trị | Dùng cho |
|-------|---------|----------|
| `Bg` | `#14181A` | Nền màn Login/Lobby |
| `Panel` | `#1E2427` (alpha 0.96) | Panel, modal, drawer |
| `PanelBorder` | `#2E3A3F` | Viền panel |
| `Accent` | `#3DBE6B` | Nút primary, tab đang chọn, preset đang dùng |
| `AccentHover` | `#52D17F` | Hover |
| `Text` | `#E8ECEE` | Chữ chính |
| `TextDim` | `#8A979D` | Chú thích, mô tả |
| `Danger` | `#E5534B` | Lỗi, banner mất kết nối, đồng hồ ≤ 10 s |
| `Warn` | `#E3B341` | Cảnh báo |
| `Overlay` | `#000000` alpha 0.55 | Nền mờ sau modal |

Texture: sinh `Texture2D` 8×8 bo góc bằng code rồi dùng `GUIStyle.border` (9-slice) cho panel, nút và ô nhập. Không thêm file ảnh.

Thang chữ: 13 (chú thích) / 15 (nội dung) / 18 (tiêu đề section) / 24 (tiêu đề màn) / 32 (đồng hồ, kết quả). Dùng font mặc định vì hiện được tiếng Việt.

Style cung cấp: `Panel`, `Title`, `Heading`, `Body`, `Caption`, `ButtonPrimary`, `ButtonSecondary`, `ButtonLink`, `TextField`, `Tab`, `TabActive`, `Chip(state)`, `Toggle`, `Slider`.

## 4. `UiWidgets` — các mẩu dùng lại

File mới: `Assets/Scripts/UI/Core/UiWidgets.cs`.

- `Modal(Rect r, Action body)`: vẽ nền `Overlay` phủ toàn màn, rồi panel giữa màn; chặn click lọt xuống dưới (dùng `GUI.ModalWindow` hoặc vẽ overlay bằng một `GUI.Button` trong suốt).
- `LabeledField(label, ref value, maxLen, isPassword, controlName)`: label nằm trên ô nhập, đặt `GUI.SetNextControlName` để hỗ trợ Tab/Enter.
- `StatusLine(text, kind)`: một dòng status có màu theo loại Info/Ok/Error.
- `SectionHeader(text)`, `Tabs(ref index, string[])`, `Chip(text, state)`.
- `Fill(Rect, Color)`: dùng chung thay cho các hàm `DrawRect` tự viết rải rác.

## 5. Tiêu chí hoàn thành

- Ba file trong `Scripts/UI/Core/` biên dịch được và chưa có màn nào bị đổi hành vi (commit 1).
- Thử nhanh bằng một màn (Login) ở 800×450 và 1920×1080: panel to nhỏ đúng tỉ lệ, nằm giữa màn.
