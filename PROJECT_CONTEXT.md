# T47 — Real-time Multiplayer Game Networking: Client Prediction

> Tài liệu sườn (context) cho bài tập lớn môn Lập trình mạng (LTM).
> Mọi quyết định thiết kế, slide và demo nên bám theo file này.
> Các mục đánh dấu **TBD** là chưa chốt; mục đánh dấu **(đề xuất)** là gợi ý, nhóm có thể đổi.

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

---

## 3. Nhóm thực hiện (3 người)

| # | Họ và tên | MSSV | Vai trò chính |
|---|---|---|---|
| 1 | TBD | TBD | Server & giao thức mạng |
| 2 | TBD | TBD | Client netcode (prediction / reconciliation / interpolation) |
| 3 | TBD | TBD | Gameplay tương tác, lag compensation, công cụ demo |

Chi tiết phân công ở **mục 7**.

---

## 4. Dự án tham khảo: "Game xếp đơn hàng"

Nguồn: báo cáo BTL Lập trình mạng của một nhóm khác (Nhóm BTL 06, 4 thành viên, 2025).
Dự án T47 của nhóm dựa trên ý tưởng này nhưng **tập trung vào netcode**.

### 4.1 Tóm tắt ý tưởng gốc

- Game online đối kháng **2 người**. Mỗi người là một nhân viên siêu thị, nhặt đồ đúng yêu cầu của khách tại quầy của mình.
- Mỗi trận server gửi 6 id ngẫu nhiên khác nhau (0–9), tương ứng 6 món đồ có trong siêu thị ở trận đó.
- Điều khiển: **WASD** để di chuyển, **J** để nhặt đồ / đưa đồ cho khách, **K** để thả đồ.
- Bong bóng yêu cầu chỉ hiện khi đứng gần khách. Mỗi khách yêu cầu 3 món, đưa đúng thứ tự được **+5 điểm**.
- Trận kéo dài **2 phút 30 giây**, ai nhiều điểm hơn thì thắng. Thoát giữa trận thì đối thủ thắng.
- Chức năng ngoài gameplay: đăng nhập, ghép trận ngẫu nhiên, bạn bè / kết bạn, thách đấu, lịch sử đấu.

### 4.2 Công nghệ dự án gốc

| Thành phần | Công nghệ |
|---|---|
| Client | Unity 6000.2.7f2, C#, URP 17.2.0, Unity UI + TextMeshPro, DOTween |
| Server | .NET 9.0, C#, `System.Net.Sockets` (TCP/UDP) |
| CSDL | MySQL (MySql.Data) |
| TCP | Đăng nhập, matchmaking, bạn bè, thách đấu, cập nhật điểm |
| UDP | Đồng bộ vị trí người chơi |

### 4.3 Điểm yếu netcode của dự án gốc (động lực cho T47)

- **Client-authoritative:** client tự gửi *vị trí* qua UDP, server chỉ chuyển tiếp cho đối thủ. Như vậy client có thể gian lận (teleport, speed hack).
- **Client tự kiểm tra** đồ đưa cho khách đúng hay sai rồi báo kết quả lên server. Server không xác thực lại.
- Không có tick, không có sequence number. Đối thủ được vẽ theo gói mới nhất nên bị giật khi có jitter hoặc mất gói.
- Hai người cùng nhặt một món đồ thì không có cơ chế phân xử. Dự án gốc không có tranh chấp tài nguyên.

Bản gốc này dùng làm **baseline** trong demo: chế độ "naive relay" để so sánh với phiên bản có netcode.

---

## 5. Ý tưởng triển khai của nhóm (đề xuất)

Giữ nguyên tinh thần "xếp đơn hàng siêu thị" (2 người, đối kháng, WASD + J/K). Thay đổi để thể hiện đúng tech focus của T47:

| Hạng mục | Dự án gốc | Phiên bản T47 |
|---|---|---|
| Thẩm quyền | Client gửi vị trí | **Server-authoritative:** client chỉ gửi *input* (kèm `seq`, `tick`) |
| Mô phỏng | Không có tick | **Fixed tick** (vd. 30 Hz) trên server và client, dùng chung code mô phỏng |
| Nhân vật của mình | Vẽ theo vị trí local | **Client-side prediction + server reconciliation** (replay input chưa được ack) |
| Nhân vật đối thủ | Vẽ gói mới nhất | **Entity interpolation** (buffer ~100 ms) |
| Nhặt đồ / giao đồ | Client tự kiểm tra | **Server xác thực**, client dự đoán trước rồi **rollback** nếu server từ chối |
| Tranh chấp | Không có | **Món đồ dùng chung:** hai người tranh cùng một món, server phân xử bằng **lag compensation** (tua lại vị trí theo tick của input) |
| Mạng xấu | Không mô phỏng | **Bộ mô phỏng** latency / jitter / packet loss, chỉnh được trong lúc demo |
| Debug | Không có | Overlay: ping, tick, số input đang chờ, "bóng" vị trí server |

**Cơ chế tạo tranh chấp (đề xuất):** một số món đồ hiếm chỉ có 1 cái trên kệ chung ở giữa bản đồ. Khi hai người cùng bấm J gần nhau về thời gian, server dùng lag compensation để xác định ai chạm tới trước theo góc nhìn của từng client. Người thua thấy món đồ "bật ra khỏi tay", đó là rollback của hành động đã dự đoán. Tình huống này dễ thấy trên sân khấu và giải thích tốt cả hai kỹ thuật.

### Thu gọn phạm vi (vì nhóm 3 người)

- **Giữ:** đăng nhập đơn giản, ghép trận ngẫu nhiên, gameplay, màn kết quả.
- **Tuỳ chọn / bỏ:** bạn bè, kết bạn, thách đấu, lịch sử đấu. Các chức năng này không phục vụ tiêu chí chấm của T47.
- **CSDL:** có thể thay MySQL bằng SQLite hoặc file JSON để giảm công cài đặt (TBD).

### Stack (đề xuất)

Theo dự án gốc để tận dụng tài liệu tham khảo:

- Client Unity (C#)
- Server .NET (C#)
- TCP cho lobby / matchmaking
- UDP cho input và snapshot

Lợi ích lớn nhất là client và server viết chung C#, nên có thể **dùng chung code mô phỏng** (shared simulation), một điều kiện quan trọng để prediction chính xác.

**Ngôn ngữ / stack đã chốt:** TBD

---

## 6. Phạm vi kỹ thuật (sườn nội dung)

Các khái niệm cốt lõi cần nắm và trình bày được:

1. **Mô hình authoritative server:** server là nguồn sự thật duy nhất, client chỉ gửi input.
2. **Tick / fixed timestep:** server và client mô phỏng theo bước thời gian cố định. Mỗi input được đánh số thứ tự (sequence number).
3. **Client-side prediction:** client áp dụng input ngay lên trạng thái local, không chờ server phản hồi.
4. **Server reconciliation:** khi nhận trạng thái authoritative kèm số input cuối server đã xử lý, client bỏ các input đã được xác nhận, đặt lại trạng thái theo server rồi replay các input còn chờ.
5. **Entity interpolation:** người chơi khác được hiển thị trễ một khoảng (buffer) và nội suy giữa các snapshot.
6. **Lag compensation:** server "tua lại" vị trí các entity về thời điểm client thực hiện hành động (vd. nhặt món đồ tranh chấp) để phân xử công bằng.
7. **Rollback:** khi phát hiện dự đoán sai, quay về trạng thái đã xác nhận rồi mô phỏng lại. Liên hệ với rollback netcode (GGPO) trong game đối kháng.
8. **Điều kiện mạng:** latency, jitter, packet loss. Lựa chọn transport (UDP hay TCP/WebSocket) và ảnh hưởng của nó tới thiết kế.

---

## 7. Phân công 3 thành viên (đề xuất)

| Thành viên | Netcode | Gameplay / khác | Slide & thuyết trình |
|---|---|---|---|
| **#1: Server & giao thức** | Vòng lặp tick của server, xử lý input queue, gửi snapshot, định dạng gói tin (TCP + UDP), sequence/ack | Đăng nhập, matchmaking, quản lý phòng, CSDL | Kiến trúc, authoritative server, giao thức, UDP hay TCP |
| **#2: Client netcode** | Client-side prediction, server reconciliation, entity interpolation, shared simulation | Di chuyển nhân vật, camera, UI trận đấu | Prediction, reconciliation, interpolation |
| **#3: Tương tác & demo** | Lag compensation (lịch sử vị trí theo tick), rollback hành động nhặt/giao đồ, bộ mô phỏng mạng | Đồ vật, yêu cầu khách, điểm, kết quả trận, overlay debug, công tắc bật/tắt | Lag compensation, rollback, **chạy live demo** |

Việc chung: thống nhất giao thức (#1 làm chủ), viết demo script (#3 làm chủ), tập thuyết trình. Mỗi người phải trả lời được Q&A về phần mình.

---

## 8. Kế hoạch demo

Mục tiêu: cho người xem **thấy được sự khác biệt** khi bật/tắt từng kỹ thuật.

- Game: "xếp đơn hàng siêu thị", phiên bản netcode (mục 5)
- Thành phần: 1 server + 2 client (hai cửa sổ trên cùng màn hình chiếu)
- Mô phỏng mạng xấu: latency / jitter / packet loss chỉnh được
- Công tắc bật/tắt: prediction, reconciliation, interpolation, lag compensation
- Hiển thị debug: ping, tick, số input đang chờ, "bóng" vị trí server so với vị trí dự đoán

Kịch bản demo (5–7 phút):

1. **Baseline "naive relay"** (giống dự án gốc) + latency 200 ms: đối thủ bị giật, có thể teleport bằng cách sửa gói tin.
2. Chuyển sang authoritative, chưa bật prediction: điều khiển bị trễ ("khựng").
3. Bật client-side prediction: mượt nhưng có lúc lệch so với server (xem "bóng").
4. Bật server reconciliation: tự sửa lệch.
5. Bật entity interpolation: đối thủ chuyển động mượt dù có jitter.
6. Hai người tranh món đồ hiếm. Lần đầu tắt lag compensation, lần sau bật, để thấy kết quả phân xử khác nhau và cảnh rollback.

---

## 9. Dàn ý slide (theo thời lượng)

| # | Nội dung | Người trình bày | Thời gian |
|---|---|---|---|
| 1 | Giới thiệu bài toán + game; vì sao latency là vấn đề | #1 | ~1 phút |
| 2 | Kiến trúc client–server, authoritative server, tick, giao thức | #1 | ~2 phút |
| 3 | Client-side prediction + server reconciliation | #2 | ~3 phút |
| 4 | Entity interpolation | #2 | ~1.5 phút |
| 5 | Lag compensation & rollback | #3 | ~2.5 phút |
| 6 | So sánh với baseline (dự án gốc) + kết quả đo | #3 | ~1.5 phút |
| — | Live demo | #3 (+#2 điều khiển client thứ hai) | 5–7 phút |
| — | Q&A | Cả nhóm | 3–5 phút |

---

## 10. Chuẩn bị Q&A

- Vì sao không tin trạng thái do client gửi lên? (chống gian lận; so sánh với dự án gốc)
- UDP hay TCP/WebSocket: đánh đổi gì? Vì sao dự án dùng cả hai?
- Chọn tick rate và độ trễ interpolation như thế nào?
- Xử lý mất gói và gói đến sai thứ tự ra sao?
- Rollback khác reconciliation ở điểm nào?
- Lag compensation gây ra trường hợp "bị cướp đồ dù đã chạy đi" vì sao, và đánh đổi gì?
- Tính deterministic ảnh hưởng thế nào tới rollback và prediction? (float, thứ tự xử lý)

---

## 11. Việc cần làm

- [ ] Điền tên + MSSV 3 thành viên, chốt phân công
- [ ] Chốt ngôn ngữ / stack và phạm vi chức năng
- [ ] Thiết kế giao thức (định dạng message input / snapshot / ack)
- [ ] Cài đặt server: tick loop, authoritative movement
- [ ] Cài đặt client: prediction, reconciliation, interpolation
- [ ] Đồ vật, khách hàng, điểm; món đồ tranh chấp + lag compensation + rollback
- [ ] Chế độ baseline "naive relay" để so sánh
- [ ] Mô phỏng latency / jitter / packet loss
- [ ] Công tắc bật/tắt từng kỹ thuật + overlay debug
- [ ] Viết demo script
- [ ] Làm slide
- [ ] Tập thuyết trình đúng thời lượng 15–20 phút
- [ ] Nộp lên db.ptit.edu.vn
