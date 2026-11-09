# 01 - Demo và lời mở đầu của #1

> Chưa nắm khái niệm nào thì mở [00-ly-thuyet.md](00-ly-thuyet.md). Dòng **Lý thuyết** dưới mỗi mục chỉ tới đúng phần cần đọc trước.

Đề yêu cầu: demo ngắn, **chạy từ đầu sạch**, chuẩn bị trước. Kịch bản 6 cảnh do #3 dẫn
(`demo/demo-script.md`). Việc của #1: dựng server từ đầu, nói 1-2 câu về server, đọc log khi cần,
và xử lý sự cố.

## A. Trước giờ vấn đáp (≥ 15 phút)

**Lý thuyết:** [L35 Docker](00-ly-thuyet.md#lt-docker) · [L2 IP, cổng, 0.0.0.0](00-ly-thuyet.md#lt-ip-port)

```powershell
# thư mục mới, clone sạch
git clone <repo classroom> t47-fresh
cd t47-fresh
copy .env.example .env
# mở .env, sửa DEMO_MODE=true (mặc định false thì preset bị từ chối)
docker compose down -v
docker compose up --build
```

Chờ thấy dòng log này (từ `ServerApp.cs:62`):

```text
T47 server listening tcp=0.0.0.0:5000 udp=0.0.0.0:5001 tick=30 Hz, db=..., demo=True
```

Nếu `demo=False` là quên sửa `.env`.

## B. Lời nói khi mở màn (~40 giây, #1 nói)

**Lý thuyết:** [L1 Client, server, giao thức](00-ly-thuyet.md#lt-client-server) · [L2 IP, cổng, 0.0.0.0](00-ly-thuyet.md#lt-ip-port) · [L4 TCP](00-ly-thuyet.md#lt-tcp) · [L10 UDP](00-ly-thuyet.md#lt-udp) · [L27 Authoritative, relay](00-ly-thuyet.md#lt-authoritative)

> Em là Chiến, phụ trách server và giao thức. Em vừa chạy `docker compose up --build` trên bản clone sạch.
> Dòng log này cho thấy server lắng nghe TCP cổng 5000 và UDP cổng 5001 trên `0.0.0.0`, tức là mọi card mạng,
> nên máy khác qua Radmin VPN cũng vào được. Cổng, tick rate, chế độ demo đều đọc từ file `.env`, không ghi cứng
> trong code.
>
> Client và server chỉ nói chuyện với nhau qua giao thức nhóm em tự thiết kế, tên T47GP/1: TCP cho đăng nhập,
> ghép trận, kết thúc trận; UDP cho phím bấm và ảnh trạng thái. Server là bên quyết định mọi thứ: client chỉ gửi
> "em bấm phím gì", vị trí và điểm do server tính.

Rồi nhường cho #3 dẫn 6 cảnh.

## C. (Tuỳ chọn, 20 giây) Nhiều client cùng lúc

**Lý thuyết:** [L21 Thread, async/await](00-ly-thuyet.md#lt-async) · [L22 Kiểu tổ chức server](00-ly-thuyet.md#lt-mo-hinh-server) · [L38 Các loại test](00-ly-thuyet.md#lt-test)

Chỉ chạy nếu thầy hỏi "có chịu được nhiều client không" hoặc còn thời gian:

```powershell
dotnet run --project client/bot/T47.Bot -- --host 127.0.0.1 --port 5000 --bots 20 --duration 20
```

> Đây là 20 bot không giao diện, tự đăng ký, đăng nhập, ghép thành 10 phòng và gửi phím 30 lần/giây.
> Log server cho thấy 10 phòng chạy song song, mỗi phòng một vòng tick riêng. Lần đo trong `testing-guide.md`
> em chạy 50 bot, 25 phòng, vẫn giữ 30 tick/giây, không lỗi.

## D. Trong cảnh 6 (lag compensation), #1 đọc log

**Lý thuyết:** [L31 Lag compensation](00-ly-thuyet.md#lt-lag-comp) · [L28 Tick](00-ly-thuyet.md#lt-tick)

Khi hai người cùng bấm J ở kệ hiếm, server in một dòng kiểu:

```text
room 3: tick 812: món hiếm #6 - slot 1 chạm lúc tick 805 (tới server tick 811),
        slot 0 chạm lúc tick 807 (tới server tick 808) → slot 1 thắng (lagComp=on)
```

Cách đọc (nói chậm, chỉ tay vào từng số):

> Slot 0 có gói tới server trước, ở tick 808. Nhưng slot 1 bấm lúc đang nhìn thế giới ở tick 805, sớm hơn tick
> 807 của slot 0. Bật lag compensation thì server xét theo "lúc người chơi nhìn thấy", nên slot 1 thắng.
> Tắt đi thì ai tới server trước thắng, tức là người mạng nhanh luôn thắng.

## E. Nếu thầy yêu cầu "kill một client, giải thích log"

**Lý thuyết:** [L8 FIN, RST, đóng kết nối](00-ly-thuyet.md#lt-fin-rst) · [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state) · [L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu) · [L9 Timeout, PING](00-ly-thuyet.md#lt-timeout) · [L37 SQLite](00-ly-thuyet.md#lt-sqlite)

Đóng hẳn một cửa sổ Unity (hoặc `docker kill` container bot). Log mẫu đã chạy thật (`testing-guide.md` dòng 219-231):

```text
INFO  session 2 (kill_001) closed: reason=peer closed, sessions=0
INFO  room 1: kill_001 rời trận (disconnected)
INFO  room 1: kết thúc OpponentLeft, winner=0, score=0-0, tick=372
```

Giải thích từng dòng:

| Dòng | Nghĩa | Code |
| --- | --- | --- |
| `closed: reason=peer closed` | Tiến trình chết thì hệ điều hành tự đóng socket, gửi FIN. Bên server, `ReadAsync` trả 0 byte nên `FrameReader` báo `Closed` | `FrameReader.cs:69`, `ClientSession.cs:154` |
| `sessions=0` | Khối dọn dẹp đã gỡ phiên khỏi `SessionRegistry` | `ClientSession.cs:371` |
| `rời trận (disconnected)` | Dọn dẹp bỏ sự kiện `PlayerLeft` vào hộp thư phòng; vòng tick đọc ra ở đầu tick sau | `ClientSession.cs:370`, `Room.cs:215` |
| `kết thúc OpponentLeft, winner=0` | Người rời bị xử thua, người còn lại nhận `MATCH_END`, kết quả được ghi SQLite ở Task nền | `Room.cs:218`, `Room.cs:415-443` |

> Nếu rút dây mạng thay vì tắt tiến trình thì không có FIN. Khi đó có hai lưới an toàn: trong trận, 5 giây không có
> gói UDP là xử rớt mạng; ngoài trận, 15 giây không nhận frame TCP nào là đóng kết nối (`idle timeout`).

## F. Sự cố tại chỗ (#1 xử lý)

**Lý thuyết:** [L35 Docker](00-ly-thuyet.md#lt-docker) · [L36 VPN, CGNAT](00-ly-thuyet.md#lt-vpn)

| Triệu chứng | Làm gì |
| --- | --- |
| Panel báo "Không áp dụng preset" | `.env` chưa `DEMO_MODE=true` → sửa, `docker compose up -d --force-recreate`, vào trận lại |
| Server không lên, báo cổng bận | `docker compose down`, tắt server chạy tay nếu có, chạy lại |
| Máy B không vào được | `Test-NetConnection <IP Radmin máy A> -Port 5000`; kiểm firewall rule `profile=any` |
| Radmin hỏng | Chạy 2 client trên máy A bằng `localhost`, nói rõ lý do với thầy |
