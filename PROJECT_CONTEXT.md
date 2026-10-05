# T47 — Real-time Multiplayer Game Networking: Client Prediction

> Tài liệu sườn (context) cho bài tập lớn môn Lập trình mạng (LTM).
> Mọi quyết định thiết kế, slide và demo nên bám theo file này.
> Các mục đánh dấu **TBD** là chưa chốt.

---

## 1. Thông tin môn học

| Mục | Nội dung |
|---|---|
| Giảng viên | Hung Dang Ngoc |
| Email | [hungdn@ptit.edu.vn](mailto:hungdn@ptit.edu.vn) |
| Nộp bài | Slides + demo script, nộp qua **db.ptit.edu.vn** |

### Yêu cầu thuyết trình

Tổng thời lượng: **15–20 phút**

| Phần | Thời lượng |
|---|---|
| Giải thích kỹ thuật | 10–12 phút |
| Live demo | 5–7 phút |
| Q&A | 3–5 phút |

### Sản phẩm cần nộp

- [ ] Slide thuyết trình kỹ thuật
- [ ] Demo script / tài liệu demo

### Tiêu chí chấm điểm

| Tiêu chí | Trọng số | Ý nghĩa |
|---|---|---|
| Technical Depth | **30%** | Hiểu bên trong giao thức / công nghệ |
| Implementation Quality | **25%** | Demo chạy được, chất lượng code, tính sáng tạo |
| Presentation Skills | **25%** | Giải thích rõ ràng, demo hiệu quả |
| Q&A Handling | **20%** | Trả lời được câu hỏi kỹ thuật |

---

## 2. Đề tài T47

| Mục | Nội dung |
|---|---|
| Tên đề tài | Real-time Multiplayer Game Networking — Client Prediction |
| Tech focus | Client-side prediction, lag compensation, rollback |
| Demo | Game multiplayer đơn giản có movement prediction |
| Innovation | Xử lý độ trễ mạng (network latency) trong game real-time |
| Ngôn ngữ cho phép | C# (Unity), JavaScript, C++, Go |
| Độ khó | Challenging — hệ thống real-time phức tạp |

**Ngôn ngữ / stack đã chọn:** TBD

---

## 3. Phạm vi kỹ thuật (sườn nội dung)

Các khái niệm cốt lõi cần nắm và trình bày được:

1. **Mô hình authoritative server** — server là nguồn sự thật duy nhất; client chỉ gửi input.
2. **Tick / fixed timestep** — server và client mô phỏng theo bước thời gian cố định; input được đánh số thứ tự (sequence number).
3. **Client-side prediction** — client áp dụng input ngay lập tức lên trạng thái local, không chờ server phản hồi.
4. **Server reconciliation** — khi nhận trạng thái authoritative kèm số input cuối đã xử lý, client bỏ các input đã được xác nhận, đặt lại trạng thái và replay các input còn chờ.
5. **Entity interpolation** — các người chơi khác được hiển thị trễ một khoảng (buffer) và nội suy giữa các snapshot.
6. **Lag compensation** — server "tua lại" vị trí các entity về thời điểm client thực hiện hành động (vd. bắn) để xét va chạm công bằng.
7. **Rollback** — phát hiện sai lệch dự đoán, quay về trạng thái đã xác nhận rồi mô phỏng lại; liên hệ với rollback netcode (GGPO) trong game đối kháng.
8. **Điều kiện mạng** — latency, jitter, packet loss; lựa chọn transport (UDP vs TCP/WebSocket) và ảnh hưởng tới thiết kế.

---

## 4. Kế hoạch demo

Mục tiêu: cho người xem **thấy được sự khác biệt** khi bật/tắt từng kỹ thuật.

- Game: TBD (vd. nhiều người chơi di chuyển trên bản đồ 2D)
- Thành phần: 1 server + ≥ 2 client
- Mô phỏng mạng xấu: độ trễ / jitter / mất gói có thể điều chỉnh
- Công tắc bật/tắt: prediction, reconciliation, interpolation (và lag compensation nếu có)
- Hiển thị debug: ping, tick, số input đang chờ, "bóng" vị trí server so với vị trí dự đoán

Kịch bản demo gợi ý (5–7 phút):

1. Không có kỹ thuật nào + độ trễ cao → điều khiển bị "khựng".
2. Bật client-side prediction → mượt nhưng có thể lệch so với server.
3. Bật server reconciliation → tự sửa lệch.
4. Bật entity interpolation → người chơi khác chuyển động mượt.
5. (Tuỳ chọn) Lag compensation / rollback.

---

## 5. Dàn ý slide (theo thời lượng)

| # | Nội dung | Thời gian |
|---|---|---|
| 1 | Giới thiệu bài toán: vì sao latency là vấn đề trong game real-time | ~1 phút |
| 2 | Kiến trúc client–server, authoritative server, tick | ~2 phút |
| 3 | Client-side prediction + server reconciliation | ~3 phút |
| 4 | Entity interpolation | ~1.5 phút |
| 5 | Lag compensation & rollback | ~2.5 phút |
| 6 | Kiến trúc & implementation của nhóm | ~1.5 phút |
| — | Live demo | 5–7 phút |
| — | Q&A | 3–5 phút |

---

## 6. Chuẩn bị Q&A

- Vì sao không tin trạng thái do client gửi lên? (chống gian lận)
- UDP hay TCP/WebSocket — đánh đổi gì?
- Chọn tick rate / độ trễ interpolation như thế nào?
- Xử lý mất gói và gói đến sai thứ tự ra sao?
- Rollback khác reconciliation ở điểm nào?
- Lag compensation gây ra trường hợp "bị bắn sau khi đã nấp" — vì sao, và đánh đổi gì?
- Tính deterministic ảnh hưởng thế nào tới rollback?

---

## 7. Việc cần làm

- [ ] Chốt ngôn ngữ / stack
- [ ] Thiết kế giao thức (định dạng message input/state)
- [ ] Cài đặt server + client
- [ ] Mô phỏng latency / jitter / packet loss
- [ ] Công tắc bật/tắt từng kỹ thuật + overlay debug
- [ ] Viết demo script
- [ ] Làm slide
- [ ] Tập thuyết trình đúng thời lượng 15–20 phút
- [ ] Nộp lên db.ptit.edu.vn
