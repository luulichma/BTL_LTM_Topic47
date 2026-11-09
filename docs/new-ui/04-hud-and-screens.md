# 04 — HUD, Result, Lobby, banner mất kết nối

Quyết định liên quan: Q13, Q14, Q15, Q16.

## 1. HUD trận đấu (P0) — `Assets/Scripts/UI/MatchHud.cs`

Nguồn dữ liệu giữ nguyên: đồng hồ lấy từ `snapshot.TicksLeft / TickRate`, điểm lấy từ `SnapshotPlayer.Score`, đơn hàng lấy từ `SnapshotOrder` của slot mình. HUD không tự đếm giờ hay tự cộng điểm.

```
Rộng (≥ 900 px thật):
┌──────────────────────────────────────────────────────┐
│  chien   12   │   1:23   │   8   opponent            │
└──────────────────────────────────────────────────────┘
      Đơn 3:  [✓ Sữa]  [▶ Bánh mì]  [ Táo ]

Hẹp (< 900 px):
┌──────────────────────┐
│        1:23          │
│ chien 12 — 8 opp     │
└──────────────────────┘
 [✓ Sữa] [▶ Bánh] [Táo]
```

- Thanh rộng tối đa 560 đơn vị ảo, căn giữa theo `UiScale.PlayWidth` để không bị lệch khi drawer đang mở.
- Chip món có 3 trạng thái màu: đã giao (`Accent` mờ + ✓ hoặc chữ "Đã giao"), món tiếp theo (viền `Accent` sáng), còn lại (`PanelBorder`). Nếu font không có ký tự ✓ thì dùng màu kèm chữ ngắn thay thế (hiện code đang dùng ASCII vì lý do này).
- Đồng hồ ≤ 10 s chuyển sang màu `Danger` (giữ nguyên hành vi hiện tại).
- Ngưỡng 2 dòng: `Screen.width < 900` (pixel thật).

## 2. Result (P0) — trong `MatchHud`

- Modal giữa màn (`UiWidgets.Modal`), rộng 420 đơn vị ảo, nền `Overlay`.
- Nội dung: tiêu đề **BẠN THẮNG! / BẠN THUA / HOÀ** (màu `Accent` / `Danger` / `Text`), tỉ số lớn, dòng "Bạn vs X · Hết giờ / Đối thủ đã rời trận", dòng chú thích "Kết quả do server gửi (MATCH_END), đã lưu SQLite".
- Nút: **Về sảnh** (primary, Enter) và **Ở lại xem** (secondary, Esc). Nút "Xem lại kết quả" trên HUD giữ nguyên.

## 3. Lobby (P1) — `Assets/Scripts/UI/LobbyController.cs`

Máy trạng thái chỉ phía client, **chuyển trạng thái theo message từ server**:

```
           FindMatch gửi đi           QUEUE_STATUS inQueue=true
[Rảnh] ─────────────────> [Đang gửi] ──────────────────────────> [Đang tìm]
   ^                          │ QUEUE_STATUS lỗi / ERROR              │
   └──────────────────────────┘                                       │
   ^        QUEUE_STATUS inQueue=false (sau CancelFind)               │
   └──────────────────────────────────────────────────────────────────┘
                                   MATCH_FOUND → LoadScene("Game")
```

- *Rảnh*: nút primary lớn **Tìm trận** (Enter).
- *Đang tìm*: dòng "Đang tìm đối thủ… 00:12" (đếm bằng `Time.unscaledTime` từ lúc nhận `inQueue=true`; đây chỉ là thời gian hiển thị, không phải dữ liệu game), nút secondary **Huỷ tìm**.
- *Đang gửi*: nút bị khoá trong lúc chờ phản hồi để tránh gửi trùng.
- Thanh trên cùng: "Người chơi: chien · ID 3" bên trái, nút **Đăng xuất** bên phải (hành vi giữ như nút "Quay lại đăng nhập" hiện tại).
- Panel giữa màn rộng 460 đơn vị ảo, nền màn `Bg`.

## 4. Banner mất kết nối (P1)

- Component nhỏ `UiConnectionBanner`, gắn cùng `NetClient` (`DontDestroyOnLoad`) để có mặt ở cả Lobby và Game.
- Nghe `NetClient.ConnectionChanged(false, message)` khi không ở scene Login: hiện banner `Danger` full-width ở trên cùng "Mất kết nối tới máy chủ — {message}" kèm nút **Về đăng nhập** (gọi `LoadScene("Login")`).
- Không tự kết nối lại (ngoài phạm vi; protocol không có resume session).

## 5. Nền màn hình (P1)

Login và Lobby: vẽ `UiWidgets.Fill(fullscreen, Bg)` trước panel, có thể thêm một dải màu `Accent` mảnh ở trên cùng. Scene Game giữ nền camera.

## 6. Ngoài phạm vi

- Scene `SupermarketOffline`: `SupermarketOfflineController.OnGUI` (hướng dẫn góc dưới) và `CustomersView` (popup đơn theo quầy). Chỉ bọc `UiScale.Begin/End` + `UiTheme` nếu còn thời gian. Riêng `CustomersView` phải chia toạ độ `WorldToScreenPoint` cho `UiScale.Factor`.
- Không thay đổi protocol, server, `T47.Shared`.

## 7. Tiêu chí hoàn thành

Các mục HUD/Result/Lobby trong checklist ở [README §5](README.md#5-checklist-kiểm-chứng).
