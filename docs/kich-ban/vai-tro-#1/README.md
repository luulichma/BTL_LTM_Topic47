# Kịch bản vấn đáp - vai #1 (Chiến: server và giao thức)

Buổi vấn đáp 10/11/2026. Mỗi người khoảng 5 phút, tự trả lời, bạn không được trả lời thay.
Điểm cá nhân (4,0) chia làm 3 phần, và bộ kịch bản này chia theo đúng 3 phần đó:

| Tiêu chí | Điểm | Thầy kiểm tra gì | File để ôn |
| --- | :-: | --- | --- |
| C1 Ownership | 1,5 | Giải thích được code mình nhận, từng dòng nếu bị hỏi | [02-di-theo-mot-goi-tin.md](02-di-theo-mot-goi-tin.md), [05-tung-commit.md](05-tung-commit.md) |
| C2 Reasoning | 1,5 | Vì sao chọn vậy, đánh đổi gì, "nếu ... thì sao" | [03-hoi-dap.md](03-hoi-dap.md) |
| C3 Live change | 1,0 | Sửa nhỏ hoặc tìm lỗi ngay tại chỗ | [04-sua-tai-cho.md](04-sua-tai-cho.md) |
| (mở màn) Demo | - | Hệ thống chạy từ đầu sạch | [01-demo-va-mo-dau.md](01-demo-va-mo-dau.md) |
| (nền) Lý thuyết | - | Hiểu khái niệm để không học vẹt | [00-ly-thuyet.md](00-ly-thuyet.md) |

## Phần #1 nhận trong README (Contribution)

Thầy chỉ hỏi phần mình nhận, nên phải nắm chắc đúng danh sách này:

- **Giao thức T47GP/1** (`shared/T47.Shared/Protocol/`): frame TCP, header UDP, codec nhị phân, `FrameReader`.
- **Server** (`server/src/T47.Server/`): accept TCP, `ClientSession`, `SessionRegistry`, `Matchmaker`, `Room` (tick 30 Hz, SNAPSHOT), `UdpGateway`, `UserStore` (SQLite, PBKDF2).
- **Phần server của gameplay**: luật nhặt/thả/giao (`MatchItems.cs`), món hiếm, lag compensation theo `viewTick`.
- **Relay** + `DEMO_CONFIG`, lưu kết quả trận.
- **Test**: test tích hợp socket thật, bot headless, script gửi rác `scripts/r3-abuse.ps1`.
- **Docker**: `Dockerfile`, `docker-compose.yml`, `.env.example`.
- **UI Unity**: Login/Register, Lobby, HUD/Result, khung Network Panel.
- **Tài liệu**: `protocol-design.md`, `architecture.md` §1-§6, `testing-guide.md`.

Không phải phần của mình (bị hỏi thì nói rõ là của bạn nào, chỉ trả lời ở mức hiểu):
prediction/reconciliation/interpolation (#2), bộ mô phỏng mạng `SimulatedUdp` và rollback G6 (#3).

## Cách dùng

0. Chưa biết gì về lập trình mạng thì đọc [00-ly-thuyet.md](00-ly-thuyet.md) phần 1-3 và phần 6 trước. Phần còn
   lại tra khi cần: mỗi mục trong file 01-04 có dòng **Lý thuyết** trỏ đúng mục `L1`-`L38` cần đọc.
1. Đọc file 02: nó kể đường đi của một người chơi từ lúc kết nối tới lúc thoát. Hiểu câu chuyện này thì
   trả lời được phần lớn câu hỏi.
2. File 03: đọc to từng câu trả lời 1 lần, sau đó che câu trả lời và tự nói lại bằng lời của mình.
3. File 04: **làm thật** ít nhất 1 lần bài 1 và bài 2 trên máy trước ngày 10/11, rồi `git checkout .` để bỏ.
4. Tối 09/11: chỉ đọc lại phần "3 câu phải thuộc" dưới đây.

## 3 câu phải thuộc

1. **"Server của em dùng async I/O. Mỗi kết nối TCP có một vòng đọc và một vòng gửi, cả server có một vòng nhận UDP,
   và mỗi phòng có một vòng tick riêng 30 lần/giây. Chỉ vòng tick được sửa trạng thái phòng; luồng khác muốn tác động
   thì bỏ tin nhắn vào hộp thư của phòng."**
   Lý thuyết: [L21 async/await](00-ly-thuyet.md#lt-async) · [L25 hộp thư](00-ly-thuyet.md#lt-hop-thu) ·
   [L28 tick](00-ly-thuyet.md#lt-tick)
2. **"TCP là luồng byte, không có ranh giới tin nhắn, nên em gắn 4 byte độ dài ở đầu mỗi frame. Bên nhận đọc đủ 4 byte,
   kiểm tra độ dài không quá 4096, rồi đọc đủ chừng đó byte."**
   Lý thuyết: [L5 tách frame](00-ly-thuyet.md#lt-framing) · [L18 kiểm đầu vào](00-ly-thuyet.md#lt-validate)
3. **"TCP cho việc phải tới nơi: đăng nhập, ghép trận, kết thúc trận. UDP cho phím bấm và ảnh trạng thái gửi 20-30
   lần/giây, vì gói mới quan trọng hơn gói cũ; gói UDP được gắn với người chơi bằng token 4 byte cấp lúc đăng nhập."**
   Lý thuyết: [L4 TCP](00-ly-thuyet.md#lt-tcp) · [L6 nghẽn đầu hàng](00-ly-thuyet.md#lt-hol) ·
   [L10 UDP](00-ly-thuyet.md#lt-udp) · [L11 token](00-ly-thuyet.md#lt-nat)

## Bản đồ "bị hỏi X thì mở file Y"

| Bị hỏi về | Mở file (repo classroom) | Dòng |
| --- | --- | --- |
| Tách frame TCP, nửa frame | `shared/T47.Shared/Protocol/FrameReader.cs` | 45-73 |
| Giới hạn 4096, 1200, 3 lỗi | `shared/T47.Shared/Protocol/ProtocolConstants.cs` | 8-20 |
| Mã hoá / giải mã message | `shared/T47.Shared/Protocol/MessageCodec.cs` | 12, 29, 78 |
| Vòng accept, mỗi client một Task | `server/src/T47.Server/ServerApp.cs` | 66-95 |
| Đọc lệnh, timeout, đóng kết nối | `server/src/T47.Server/ClientSession.cs` | 57-88, 132-169 |
| Dọn dẹp khi client chết | `server/src/T47.Server/ClientSession.cs` | 356-372 |
| Ghép trận, lock | `server/src/T47.Server/Matchmaker.cs` | 19-34 |
| Token UDP, đăng nhập 2 nơi | `server/src/T47.Server/SessionRegistry.cs` | 20-30 |
| Nhận UDP, bỏ gói rác | `server/src/T47.Server/UdpGateway.cs` | 67-90 |
| Vòng tick, hộp thư, SNAPSHOT | `server/src/T47.Server/Room.cs` | 68, 111-200 |
| Áp dụng INPUT theo seq | `server/src/T47.Server/Room.cs` | 288-318 |
| Rớt UDP 5 s, kết thúc trận | `server/src/T47.Server/Room.cs` | 372-384, 415-444 |
| Mật khẩu | `server/src/T47.Server/UserStore.cs` | 150-179 |
| Log khi `docker kill` | `docs/testing-guide.md` | 219-231 |
