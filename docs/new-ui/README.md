# Kế hoạch làm lại UI client Unity (T47)

> Ngày chốt: 08/11/2026 (qua phiên grilling 18 câu). Hạn nộp: 09/11, vấn đáp: 10/11.
> Repo làm việc: classroom `int1433-05-20261-grp-int1433-05-0005`, thư mục `client/unity`.
> Tài liệu này là kế hoạch nội bộ (repo Topic47). Sau khi code xong mới tóm tắt sang `docs/` + `docs/ai-log.md` của repo classroom.

## 1. Vấn đề hiện tại

| # | Vấn đề | Nguyên nhân trong code |
|---|--------|------------------------|
| 1 | UI không responsive, rất xấu | Toàn bộ là IMGUI (`OnGUI`) với kích thước pixel cố định (420×440, 380×540, 320×170…), không scale theo độ phân giải; dùng `GUI.skin` mặc định. |
| 2 | Đăng nhập/đăng ký lẫn lộn | `LoginController` gộp host/port/user/pass, nút Đăng nhập và Đăng ký cạnh nhau, chung một dòng status; không có ô nhập lại mật khẩu. |
| 3 | Network panel che vùng chơi | Màn Game có 4 lớp chồng nhau: debug `GameController` (trái trên), "G3 — Trạng thái từ server" của `NetworkGameplayView` (phải trên), `MatchHud` (giữa trên), `NetworkPanel` (trái, đè lên debug). Các panel debug luôn hiển thị. |

Hiện trạng kỹ thuật đã kiểm tra:
- Unity `6000.6.7f1`; `Packages/manifest.json` chỉ có `imgui`, `jsonserialize`, `2d.sprite` (không có uGUI/TextMeshPro).
- `NetworkPanel` chỉ gắn trong scene `Game`. `CustomersView` chỉ có trong scene `SupermarketOffline`.
- `CameraFollow` tính zoom theo `_camera.aspect` nên tự thích nghi khi đổi `Camera.rect`.
- `ProjectSettings`: 1280×720 mặc định, `resizableWindow: 1`.

## 2. Quyết định đã chốt

| ID | Quyết định | Tài liệu chi tiết |
|----|-----------|-------------------|
| Q1 | **Giữ IMGUI**, thêm lớp dùng chung `UiScale` + `UiTheme`. Không dùng UI Toolkit/uGUI (rủi ro với hạn nộp, phải giải thích thêm framework khi vấn đáp). | [01](01-layout-system.md) |
| Q2 | Tham chiếu 1280×720; hỗ trợ 800×450 → 2560×1440; scale chặn trong [0.65, 2.0]. | [01](01-layout-system.md) |
| Q8 | Theme tối, màu nhấn xanh lá, sinh bằng code, không thêm asset; font mặc định (hiện được tiếng Việt). | [01](01-layout-system.md) |
| Q3 | Register là **modal riêng**: user, mật khẩu, nhập lại mật khẩu; kiểm tra phía client; thành công → đóng modal, điền sẵn username, **không tự đăng nhập**. Protocol không đổi. | [02](02-login-register.md) |
| Q4 | Host/port vào mục thu gọn "⚙ Cài đặt máy chủ", mặc định ẩn, dùng chung cho Login/Register. | [02](02-login-register.md) |
| Q13 | Phím tắt: Enter gửi, Tab chuyển ô, Esc đóng modal; Result Enter = Về sảnh; Lobby Enter = Tìm trận. | [02](02-login-register.md), [04](04-hud-and-screens.md) |
| Q5 | Network Panel là **drawer bên phải**, mặc định ẩn, mở bằng F1 hoặc nút góc dưới phải; khi mở **co `Camera.rect`** để không che sân. | [03](03-network-panel.md) |
| Q11 | Drawer rộng 380 px tham chiếu; nếu vùng chơi còn < 480 px thật thì chuyển sang overlay bán trong suốt. | [03](03-network-panel.md) |
| Q6 | Gộp panel debug `GameController` + panel G3 của `NetworkGameplayView` vào drawer thành 3 tab: **Mạng / Trận / Server state**. | [03](03-network-panel.md) |
| Q12 | Tab Mạng chia 5 section: Trạng thái, Preset demo (lưới 2×3), Mạng giả lập, Kỹ thuật client, Nâng cao. | [03](03-network-panel.md) |
| Q7 | Giữ phím 1–6 khi panel đóng, thêm toast xác nhận preset. | [03](03-network-panel.md) |
| Q15 | HUD thanh ngang trên cùng, đơn hàng dạng chip màu, xuống 2 dòng khi < 900 px; Result là modal nền tối. | [04](04-hud-and-screens.md) |
| Q14 | Lobby máy trạng thái Rảnh/Đang tìm theo `QUEUE_STATUS`, đồng hồ chờ, nút Đăng xuất góc trên. | [04](04-hud-and-screens.md) |
| Q16 | Banner đỏ "Mất kết nối tới máy chủ" + nút Về đăng nhập (P1). | [04](04-hud-and-screens.md) |
| Q17 | Kiểm chứng bằng checklist thủ công, chạy thật; ảnh chụp chỉ từ lần chạy thật. | [§5](#5-checklist-kiểm-chứng) |
| Q18 | Nhánh `feature/ui-revamp`, 5 commit, mỗi commit chạy được. | [§4](#4-thứ-tự-thực-hiện) |
| Q10 | Tài liệu nằm ở thư mục này. | — |

**Ngoài phạm vi**: scene `SupermarketOffline` (`SupermarketOfflineController`, `CustomersView`) — chỉ áp `UiTheme` nếu còn thời gian. Không thay đổi protocol, server hay `T47.Shared`.

## 3. Ưu tiên (Q9)

> Cập nhật 08/11: cả P0 và P1 đã code xong và đã vào `main` repo classroom (commit ghi bên cạnh từng mục).
> Phần kiểm chứng chạy thật vẫn theo checklist §5.

**P0 — trước khi nộp 09/11**
- [x] 1. `UiScale` + `UiTheme` (nền tảng). (`3d10e6c`)
- [x] 2. Login + modal Register + mục Cài đặt máy chủ. (`c56ebea`)
- [x] 3. Network Panel dạng drawer + gộp 3 tab + co camera. (`71a39ac`)
- [x] 4. HUD + Result responsive. (`fd42d7f`, sửa chú thích `760f96c`)

**P1 — nếu kịp**
- [x] 5. Lobby máy trạng thái + đồng hồ chờ. (`3112925`)
- [x] 6. Toast preset, banner mất kết nối. (`79a42ec`)
- [x] 7. Nền màu cho Login/Lobby; áp theme cho scene offline. (`24ac1dc`)

## 4. Thứ tự thực hiện

Nhánh `feature/ui-revamp` (repo classroom). Mỗi commit build và chạy được để có thể merge giữa chừng nếu hết giờ.

| Commit | Nội dung | File chính |
|--------|----------|-----------|
| 1 | `UiScale`, `UiTheme`, `UiWidgets` | `Scripts/UI/Core/*.cs` (mới) |
| 2 | Login/Register | `Scripts/UI/LoginController.cs` |
| 3 | Drawer + tab + gộp debug | `Scripts/UI/NetworkPanel.cs`, `GameController.cs`, `Gameplay/NetworkGameplayView.cs` |
| 4 | HUD/Result | `Scripts/UI/MatchHud.cs` |
| 5 | Lobby + toast + banner (P1) | `Scripts/UI/LobbyController.cs`, `Scripts/UI/Core/UiToast.cs` |

Sau mỗi commit: nhắc thêm entry vào `docs/ai-log.md` (repo classroom).

## 5. Checklist kiểm chứng

Chạy thật server (`docker compose up --build`) + client build/Editor. Đánh dấu khi đã thử thật.

> 08/11: nhóm đã chạy thử thật toàn bộ checklist dưới đây và xác nhận đạt.

**Kích thước**
- [x] 800×450: mọi màn đọc được, không có nút bị cắt.
- [x] 1280×720: bố cục chuẩn.
- [x] 1920×1080 và 2560×1440: UI to theo, không bị nhỏ xíu ở giữa.
- [x] 2 client cạnh nhau trên một màn hình (mỗi cửa sổ ~960×540).
- [x] Kéo resize cửa sổ khi đang ở Login, Lobby, giữa trận.

**Login/Register**
- [x] Đăng ký thành công → modal đóng, username được điền sẵn.
- [x] Đăng ký trùng tên → lỗi hiện trong modal.
- [x] Mật khẩu nhập lại không khớp → chặn ở client, không gửi.
- [x] Bỏ trống trường → chặn ở client.
- [x] Sai mật khẩu khi đăng nhập → lỗi hiện ở form Login.
- [x] Enter/Tab/Esc hoạt động.
- [x] Đổi host/port trong "Cài đặt máy chủ" có hiệu lực.

**Network Panel**
- [x] Mặc định ẩn; F1 và nút góc dưới phải mở/đóng.
- [x] Mở ở 1280×720: sân chơi co sang trái, panel không che nhân vật/quầy.
- [x] Mở ở 800×450: chuyển overlay.
- [x] Preset 1–6 (nút và phím) vẫn hoạt động như trước; toast hiện.
- [x] 3 tab hiển thị đủ thông tin cũ (không mất chỉ số nào so với bản cũ).

**HUD/Result/Lobby**
- [x] HUD hiện đồng hồ, điểm, đơn hàng; < 900 px xuống 2 dòng.
- [x] Hết giờ → modal Result; Về sảnh / Ở lại xem đúng.
- [x] Lobby: Tìm trận → trạng thái Đang tìm + đồng hồ; Huỷ → về Rảnh.
- [x] Tắt server khi đang chơi → banner mất kết nối (P1).
