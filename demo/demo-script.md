# Demo script T47 — buổi vấn đáp 10/11/2026

> Tài liệu nội bộ, **không nộp**. Phím bấm và preset lấy theo code trên `main` (`98ff410`, 08/11/2026):
> `NetworkPanel.cs`, `NetClient.cs` (`ApplyMessage`, nhánh `DemoConfigResult`), `NetworkGameplayView.cs`.
> Bảng preset đầy đủ: `docs/architecture.md` §8.6 của repo classroom.
> Đề yêu cầu (INSTRUCTION.md): demo ngắn **chạy từ đầu sạch**, chuẩn bị trước, rồi mới hỏi đáp.

Mục tiêu: 5-7 phút, 6 cảnh. Mỗi cảnh: thao tác → khán giả nhìn gì → 1-2 câu thuyết minh.

## 0. Vai và bố trí

| Người | Việc khi demo |
| --- | --- |
| #3 Quang Anh | Dẫn, điều khiển cửa sổ A (bên trái, slot 0) |
| #2 Đức Anh | Điều khiển cửa sổ B (bên phải, slot 1) |
| #1 Chiến | Theo dõi terminal server log, xử lý sự cố, chuyển preset cùng lúc nếu cần |

Hai client Unity cạnh nhau trên cùng màn hình chiếu; terminal server log ở góc dưới.

## 1. Checklist trước demo (làm trước giờ vấn đáp ≥ 15 phút)

- [ ] Clone sạch repo classroom vào thư mục mới (đề yêu cầu "fresh start").
- [ ] `cp .env.example .env`, sửa **`DEMO_MODE=true`** (mặc định `false` → preset bị từ chối).
- [ ] `docker compose down -v && docker compose up --build`; thấy log lắng nghe TCP 5000 / UDP 5001.
- [ ] (Tuỳ chọn, 20 giây) chạy bot cho thấy nhiều client đồng thời:
      `dotnet run --project client/bot/T47.Bot -- --host 127.0.0.1 --port 5000 --bots 20 --duration 20`.
- [ ] Mở 2 client Unity (build hoặc 2 Editor), đăng nhập 2 tài khoản khác nhau, `Tìm trận` → vào cùng phòng.
- [ ] Cửa sổ nào là slot 0 thì đặt bên trái (preset 6 gán RTT theo slot).
- [ ] Tắt thông báo Windows, Zalo, Messenger; sạc pin; tắt Wi-Fi nếu chạy localhost.
- [ ] Nhớ: trận dài 2:30. Nếu sắp hết giờ giữa demo, để `MATCH_END` hiện màn kết quả (là một điểm demo), bấm
      "Về sảnh" rồi tìm trận lại.

## 2. Phím cần nhớ

| Phím | Tác dụng |
| --- | --- |
| W A S D | Di chuyển |
| J | Nhặt / thả / giao đồ (ở kệ hiếm: giành món hiếm) |
| F1 | Mở / đóng Network Panel |
| 1-6 | Chọn preset (khi panel **đóng**); hai cửa sổ phải chọn **cùng** preset |
| C | Camera bám người chơi ↔ nhìn toàn bản đồ (nên để toàn bản đồ khi demo) |

Trên Network Panel: nút "R4 test 7: thử dịch chuyển bằng vị trí giả" (cheat), nút "Lag comp ON / OFF",
thanh trượt RTT / jitter / loss. Bảng bên phải: công tắc "G6: dự đoán nhặt món hiếm".

## 3. Kịch bản 6 cảnh

| # | Preset | Thao tác | Khán giả nhìn thấy | Thuyết minh (1-2 câu) | Thời gian |
| :-: | :-: | --- | --- | --- | :-: |
| 1 | 1 | A chạy vòng; A bấm nút cheat | B thấy A giật (relay, RTT 200 ms); A nhảy sang vị trí giả và B **thấy luôn** | "Relay: server tin vị trí client gửi, nên client sửa gói là gian lận được." | ~1 phút |
| 2 | 2 | A chạy lại, bấm nút cheat | Cheat **không có tác dụng**; nhưng bấm phím thì nhân vật phản hồi chậm ~200 ms | "Authoritative: client chỉ gửi phím bấm, server tự tính vị trí. Hết gian lận, nhưng bị trễ." | ~1 phút |
| 3 | 3 | A chạy, va vào B | A phản hồi **ngay**; khi va chạm, A lệch khỏi bóng vị trí server | "Prediction: client chạy cùng hàm `Simulation.Step` với server để đoán trước. Va chạm với người khác không đoán được nên lệch." | ~1 phút |
| 4 | 4 | Lặp lại va chạm | A được kéo về trùng bóng server; kéo mượt chứ không giật (C7) | "Reconciliation: snapshot báo `lastProcessedSeq`, client đặt lại state server rồi chạy lại các input chưa ACK. C7 trượt dần sai số nhỏ." | ~1 phút |
| 5 | 5 | Nhìn cửa sổ B, A chạy qua lại | Đối thủ di chuyển mượt dù jitter 100 ms | "Interpolation: vẽ đối thủ trễ 100 ms, nội suy giữa 2 snapshot, nên jitter không làm giật." | ~45 giây |
| 6 | 6 | A, B đứng 2 bên kệ hiếm giữa bản đồ, cùng bấm J. Lần 1: **Lag comp OFF**; lần 2: **ON**. Bật G6 để thấy rollback | OFF: A (50 ms) luôn thắng. ON: người bấm trước *trên màn hình của mình* thắng, người thua nhận `LOST_TO_OPPONENT`, món "bay về" kệ (G6). #1 chỉ log phân xử trên server | "Lag compensation: server phân xử theo `viewTick`, tức người chơi đang nhìn thế giới ở tick nào, kẹp tối đa 30 tick để không khai gian. G6 vẽ trước món lên tay rồi hoàn tác nếu thua." | ~1,5 phút |

Tổng ~6 phút. Nếu thiếu thời gian: gộp cảnh 3 + 4 (va chạm ở preset 3 rồi chuyển 4 ngay).

## 4. Sự cố thường gặp và cách xử lý (#1)

| Triệu chứng | Nguyên nhân hay gặp | Xử lý |
| --- | --- | --- |
| Panel báo "Không áp dụng preset: ..." | `DEMO_MODE=false` hoặc chưa vào trận | Sửa `.env`, `docker compose up -d --force-recreate`; vào trận lại |
| Hai cửa sổ hành xử khác nhau | Chỉ một cửa sổ đổi preset | Bấm cùng phím preset ở cửa sổ kia |
| Cảnh 6 không phân định rõ | Bấm J quá xa nhau về thời gian | Đếm "3-2-1" rồi bấm cùng lúc; lặp lại |
| Một client mất kết nối | Đóng nhầm cửa sổ / rớt UDP 5 s | Đối thủ thắng `OpponentLeft` (cũng là demo chịu lỗi); đăng nhập lại, tìm trận |
| Server không lên | Cổng 5000/5001 đang bị chiếm | `docker compose down`, đóng server native đang chạy, chạy lại |

## 5. Tập dượt

| Lần | Ngày giờ | Thời lượng | Vấn đề gặp |
| :-: | --- | :-: | --- |
| 1 | | | |
| 2 | | | |
| 3 | | | |

Sau demo, mỗi người sẵn sàng **sửa tại chỗ** phần mình (ví dụ đổi max frame size, thêm 1 lệnh giao thức, đổi
interp delay, đổi tick rate) và chạy lại test liên quan.
