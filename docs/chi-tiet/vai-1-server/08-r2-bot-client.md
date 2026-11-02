# R2 - Bot client headless (test tải)

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến + #2 Nguyễn Đức Anh | `feature/r2-bot-client` | 07/11 (bản đầu login + ghép trận: 05/11) | S4, S5, C1 | R4 test 1, 2 |

## Mục tiêu

Chứng minh **nhiều phòng chạy song song** (tiêu chí B1): game chỉ 2 người/phòng nên phải dùng bot để có 20-50 client.

## Các bước

- [ ] `client/bot/T47.Bot`: console .NET dùng `T47.Shared` (Protocol + Simulation).
- [ ] Luồng mỗi bot: REGISTER/LOGIN (`bot_<n>`) → FIND_MATCH → UDP_HELLO → gửi INPUT ngẫu nhiên 30 Hz trong N giây → thoát.
- [ ] Tham số qua env/args: `BOT_COUNT`, `DURATION`, host/port (đọc `CLIENT_TARGET_HOST` như starter).
- [ ] Một tiến trình chạy N bot bằng N `Task` (không cần N tiến trình).
- [ ] Bot in thống kê: số snapshot nhận/giây, RTT trung bình (PING), lỗi.
- [ ] `client/Dockerfile` build bot; `docker compose run --rm client` chạy được.
- [ ] Phần #2 làm: client-side prediction tối giản trong bot để đo độ lệch (dùng cho T4) - không bắt buộc.

## Xong khi

- `BOT_COUNT=20` và `BOT_COUNT=50` chạy hết thời gian, server log 10/25 phòng, tick mỗi phòng ≈ 30.

## Chuẩn bị vấn đáp

- Vì sao test bằng bot là bằng chứng hợp lệ cho "nhiều client đồng thời"?
