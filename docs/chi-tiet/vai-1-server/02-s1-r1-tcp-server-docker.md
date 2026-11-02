# S1 + R1 - TCP server nhiều kết nối và Docker

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến | `feature/s1-tcp-server` | 04/11 (M2) | S2 | S3, C2, R2 |

## Mục tiêu

Server .NET nhận nhiều client TCP cùng lúc, không client nào chặn client khác, và chạy được bằng
`docker compose up --build` từ clone sạch.

## Các bước

### S1 - TCP server

- [ ] `TcpListener` bind `SERVER_HOST:SERVER_PORT` (mặc định `0.0.0.0:5000`), vòng `AcceptTcpClientAsync`.
- [ ] Mỗi kết nối → một `ClientSession` chạy `async` riêng: đọc frame bằng `FrameReader`, dispatch theo `MessageType`.
- [ ] `SessionRegistry` dùng `ConcurrentDictionary<int, ClientSession>`; thêm khi accept, xoá trong `finally` khi ngắt.
- [ ] Ghi bằng một hàng đợi gửi / `SemaphoreSlim` mỗi session để 2 luồng không ghi đè frame lên nhau.
- [ ] PING → PONG để test end-to-end trước khi có LOGIN.
- [ ] Log: kết nối mới, ngắt kết nối (kèm lý do), số session hiện tại.

### R1 - Docker

- [ ] `server/Dockerfile` 2 tầng: `mcr.microsoft.com/dotnet/sdk:8.0` build → `mcr.microsoft.com/dotnet/runtime:8.0` chạy.
- [ ] `docker-compose.yml`: build context `.` (thấy `shared/`), map `${SERVER_PORT}:${SERVER_PORT}` và
      `${UDP_PORT}:${UDP_PORT}/udp`, volume cho SQLite (`./data:/app/data`).
- [ ] `.env.example` thêm `UDP_PORT=5001`, `DB_PATH=/app/data/t47.db`, `DEMO_MODE=false`, `TICK_RATE=30`.
- [ ] `client/Dockerfile` build bot (R2) - tạm thời có thể chỉ là bot gửi PING.
- [ ] Thử: `docker compose up --build`, rồi chạy bot/`nc` từ máy host tới `localhost:5000`.

## Xong khi

- 10 kết nối mở cùng lúc, mỗi cái gửi PING đều nhận PONG; tắt ngang 1 client không làm hỏng các client khác.
- Clone sạch → `cp .env.example .env` → `docker compose up --build` chạy được.

## Lưu ý

- Server trong container phải bind `0.0.0.0`, không phải `127.0.0.1`.
- Windows + Docker Desktop: UDP map cổng có thể chập chờn, test sớm (rủi ro trong plan).
- Đừng `Task.Run` vô tội vạ; `async` I/O là đủ, không cần 1 thread thật mỗi client.

## Chuẩn bị vấn đáp

- Mô hình đồng thời: bao nhiêu thread khi có 50 client? Vì sao không chặn nhau?
- Dữ liệu nào được chia sẻ giữa các session và được bảo vệ thế nào?
- Vì sao Dockerfile 2 tầng?
