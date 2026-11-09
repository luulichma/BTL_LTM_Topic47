# 03 — Network Panel dạng drawer, gộp panel debug

Quyết định liên quan: Q5, Q6, Q7, Q11, Q12.
File sửa: `Assets/Scripts/UI/NetworkPanel.cs`, `Assets/Scripts/UI/GameController.cs`, `Assets/Scripts/Gameplay/NetworkGameplayView.cs`.

## 1. Hiện trạng ở màn Game

| Panel | Vị trí | Luôn hiện? |
|-------|--------|-----------|
| `GameController` debug: phòng, seed, UDP, toggle arena C4/C5 | Trái trên, 380×470 | Có (ẩn khi Network Panel mở) |
| `NetworkGameplayView` "G3 — Trạng thái từ server", G6 rollback, danh sách món | Phải trên, 350×440 | Có |
| `MatchHud` | Giữa trên | Có |
| `NetworkPanel` | Trái trên, 380×540 | Bật bằng F1 |
| Dòng "F1: mở Network Panel" | Trái dưới | Khi panel đóng |

Hai panel trên cùng đã chiếm gần hết chiều ngang ở 1280×720 và đè lên sân chơi.

## 2. Thiết kế mới

```
Panel đóng (mặc định):                       Panel mở (đủ rộng):
┌──────────────────────────────────┐         ┌─────────────────────┬────────────┐
│         [  HUD trận đấu  ]       │         │   [ HUD trận đấu ]  │ Mạng│Trận│SV│
│                                  │         │                     │────────────│
│            SÂN CHƠI              │         │   SÂN CHƠI (co lại) │ section... │
│                                  │         │   Camera.rect.width │ (cuộn)     │
│                            [📶] │         │   = 1 - drawer/W    │      [✕]   │
└──────────────────────────────────┘         └─────────────────────┴────────────┘
```

- **Mặc định ẩn.** Mở/đóng bằng `F1` hoặc nút nhỏ `📶 Mạng` ở góc dưới phải (đơn vị ảo 96×32). Bỏ dòng chữ "F1: mở Network Panel".
- **Drawer bên phải**, rộng `DrawerRef = 380` đơn vị ảo, cao toàn màn hình.
- `GameController` và `NetworkGameplayView` **không tự vẽ panel nữa**. Chúng cung cấp hàm vẽ nội dung để drawer gọi trong tab tương ứng:
  - `GameController.DrawMatchTab()`: thông tin phòng, đối thủ, seed, tick rate, cổng UDP, trạng thái UDP, toggle arena C4/C5. Arena debug vẫn vẽ ở giữa màn khi bật.
  - `NetworkGameplayView.DrawServerStateTab()`: G6 toggle, số lần rollback/hết hạn, đồng hồ, điểm và món đang cầm, đơn hàng, chi tiết bản thể món.
  - Logic và trạng thái (`_showPredictionArena`, `_predictRarePickup`…) giữ nguyên trong component cũ. Chỉ chuyển phần vẽ.

## 3. Co camera để panel không che sân (Q5, Q11)

Trong `NetworkPanel` (pixel thật):

```csharp
float drawerPx = DrawerRef * UiScale.Factor;
float playPx   = Screen.width - drawerPx;
bool  overlay  = playPx < MinPlayPx;          // MinPlayPx = 480
Camera cam = Camera.main;
cam.rect = (_visible && !overlay)
    ? new Rect(0, 0, playPx / Screen.width, 1)  // sân chơi chiếm phần bên trái
    : new Rect(0, 0, 1, 1);
```

- Cập nhật mỗi frame trong `LateUpdate`, vì cửa sổ có thể resize bất kỳ lúc nào. Khi `OnDisable` hoặc khi rời scene thì trả `cam.rect` về `(0,0,1,1)`.
- `CameraFollow` dùng `_camera.aspect`, nên tự tính lại zoom và giới hạn bản đồ theo vùng hẹp hơn. Không cần sửa.
- **Overlay**: khi vùng chơi còn dưới 480 px thật (ví dụ 2 client cạnh nhau ở 800 px), camera giữ nguyên toàn màn, drawer vẽ đè với nền `Panel` alpha 0.85.
- HUD và các toạ độ GUI khác phải căn theo **vùng chơi** chứ không theo toàn màn: thêm `UiScale.PlayWidth` (= Width trừ drawer khi đang co) để `MatchHud` căn giữa đúng.
- Vùng chuột: khi drawer mở thì click vào drawer không được lọt xuống gameplay. Hiện gameplay chủ yếu dùng bàn phím, nhưng vẫn cần kiểm tra lại.

## 4. Tab và section (Q6, Q12)

Thanh tab trên cùng drawer: **Mạng | Trận | Server state** và nút `✕`. Tab đang chọn được tô màu `Accent`. Nội dung mỗi tab nằm trong `BeginScrollView` riêng, mỗi tab nhớ vị trí cuộn riêng.

**Tab Mạng** (từ `NetworkPanel` hiện tại, sắp lại):

1. **Trạng thái** (luôn ở trên): Ping TCP và tuổi mẫu, Tick server, Input chờ ACK, Sai số reconcile (m), Số lần reconcile, chế độ luồng Relay/Authoritative. Hiển thị dạng bảng 2 cột nhãn/giá trị.
2. **Preset demo**: lưới nút 2×3 (1 Relay, 2 Tắt P+R, 3 P only, 4 P+R, 5 +Interp jitter, 6 Món hiếm lag comp). Preset đang yêu cầu gần nhất được tô `Accent`. Dưới lưới là `_presetStatus` và `client.DemoStatus`. Dòng chú thích: "Server cần DEMO_MODE=true · hai cửa sổ chọn cùng preset".
3. **Mạng giả lập (UDP)**: 3 slider RTT (0–500), Jitter (0–100), Loss (0–30%), giá trị hiện bên phải, nút "Đưa về 0".
4. **Kỹ thuật client**: toggle Prediction, Reconciliation, Interpolation (bị khoá khi Relay, kèm chú thích), dòng ViewTick và bộ đệm C6.
5. **Nâng cao** (mặc định thu gọn): R4 test 7 teleport và `DemoCheatStatus`; RTT A 50 / B 250; Lag comp ON/OFF và trạng thái yêu cầu gần nhất.

Các đoạn giải thích dài hiện nay ("Không tự nhận thay đổi do đối thủ gửi", "Ping TCP đo mỗi 5 giây…") được rút thành một dòng chữ `Caption` màu `TextDim` dưới section tương ứng.

**Không được bỏ bớt chỉ số hay nút nào** so với bản cũ, vì kịch bản demo và phần vấn đáp đang dựa vào chúng. Điều kiện `GUI.enabled` của từng nút giữ nguyên.

**Tab Trận**: nội dung `GameController.DrawMatchTab()`.
**Tab Server state**: nội dung `NetworkGameplayView.DrawServerStateTab()`.

## 5. Phím 1–6 và toast (Q7)

- Giữ nguyên xử lý `Alpha1..Alpha6` trong `Update` (hoạt động cả khi drawer đóng).
- Khi `RequestDemoPreset` trả về true thì gọi `UiToast.Show("Preset 4 — Prediction + Reconciliation")`. Toast hiện ở góc trên phải vùng chơi trong 2,5 s, mờ dần.
- `UiToast` (P1) là component static nhỏ trong `Scripts/UI/Core/`. Nếu chưa kịp làm ở P0 thì tạm ghi vào `_presetStatus` như cũ.

## 6. Tiêu chí hoàn thành

- Các mục Network Panel trong checklist ở [README §5](README.md#5-checklist-kiểm-chứng).
- So sánh với bản cũ: mọi dòng thông tin của 3 panel cũ đều xuất hiện ở một trong 3 tab.
- Chạy kịch bản demo preset 1→6 với 2 client, hành vi giống bản trước.
