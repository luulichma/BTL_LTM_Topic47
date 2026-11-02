# Quyết định thiết kế Q1-Q9 (đã chốt theo đề xuất, 02/11)

| Người làm | Deadline | Ghi vào repo code |
| --- | :-: | --- |
| #1 chép vào docs; mỗi người đọc kỹ câu thuộc phần mình | 02/11 (tối) | `docs/architecture.md` (Q1, Q5, Q6, Q8), `docs/protocol-design.md` (Q2, Q3, Q4, Q7, Q9) |

Q1-Q9 là 9 lựa chọn thiết kế mà code của cả nhóm phụ thuộc vào, và là câu **chắc chắn bị hỏi khi vấn đáp**.
Dưới đây là phương án đã chọn kèm lý do và đánh đổi. Nếu ai thấy không ổn thì báo trong tối 02/11; sau đó coi như chốt.
Người ở cột "Phải giải thích được" cần tự nói lại được phần **Vì sao** và **Đánh đổi** bằng lời của mình.

## Tóm tắt

| # | Câu hỏi | Chốt | Phải giải thích được |
| --- | --- | --- | --- |
| Q1 | Server xử lý đồng thời thế nào? | `async/await` cho TCP + 1 vòng nhận UDP + **1 vòng tick riêng cho mỗi phòng**, hộp thư `ConcurrentQueue` | #1 |
| Q2 | Tách các message trên luồng TCP thế nào? | **Length-prefix** 4 byte, tối đa 4096 byte/frame | #1 |
| Q3 | Mã hoá message dạng gì? | **Nhị phân tự viết** (`BinaryPrimitives`), little-endian | #1 |
| Q4 | Server biết gói UDP là của ai thế nào? | **Session token** 4 byte cấp lúc LOGIN, kèm trong mọi gói UDP; endpoint học từ gói thật | #1 |
| Q5 | Code dùng chung giữa server và Unity? | Thư viện **`shared/T47.Shared`, netstandard2.1**, copy DLL vào Unity | #1, #2 |
| Q6 | Bộ mô phỏng mạng đặt ở đâu? | **Chỉ ở client**, bọc cả chiều gửi và nhận UDP | #3 |
| Q7 | Lag compensation tua về thời điểm nào? | **`viewTick`** (tick client đang nhìn thấy), tua tối đa **30 tick** | #3, #1 |
| Q8 | Một người rớt mạng giữa trận? | **Xử thua ngay**, không reconnect | #1 |
| Q9 | Lưu mật khẩu thế nào? | **PBKDF2-SHA256**, salt 16 byte, 100.000 vòng | #1 |

---

## Q1 - Mô hình đồng thời của server

- **Câu hỏi:** server dùng bao nhiêu luồng, luồng nào làm gì, và dữ liệu chung được bảo vệ ra sao?
- **Các phương án:** (a) async TCP + 1 vòng nhận UDP + 1 vòng tick mỗi phòng; (b) thread-per-client + 1 game loop chung
  cho mọi phòng; (c) event loop đơn luồng.
- **Chốt: (a).**
  - Mỗi kết nối TCP là một `ClientSession` chạy `async` (không chiếm thread khi chờ dữ liệu).
  - Một `UdpGateway` duy nhất `ReceiveAsync` mọi gói UDP, kiểm tra token rồi đẩy vào `room.Inbox` (`ConcurrentQueue`).
  - Mỗi `Room` một `Task` chạy vòng tick 30 Hz; **vòng tick là luồng duy nhất được sửa state của phòng**.
  - Dữ liệu chung khác: `SessionRegistry` (`ConcurrentDictionary`), hàng đợi ghép trận (`lock`), SQLite (`lock` 1 kết nối).
- **Vì sao:** async I/O của .NET chịu được hàng trăm kết nối mà không cần hàng trăm thread; mỗi phòng tự chạy nên một
  phòng lỗi hoặc chậm không kéo các phòng khác; chỉ một luồng ghi state phòng nên **không cần khoá trên state game**,
  tránh deadlock và race condition.
- **Đánh đổi:** code async khó debug hơn thread-per-client; nhiều phòng = nhiều vòng tick cùng tranh CPU (với 25 phòng
  vẫn nhẹ). Phương án (b) đơn giản hơn nhưng một phòng chậm làm chậm tất cả.

## Q2 - Framing TCP

- **Câu hỏi:** TCP là luồng byte, không có ranh giới message. Làm sao biết một message bắt đầu và kết thúc ở đâu?
- **Các phương án:** length-prefix; ký tự phân cách `\n` + JSON.
- **Chốt:** `[u32 length][u8 type][payload]`, `length` = số byte của `type + payload`, **tối đa 4096**; vượt quá hoặc
  bằng 0 → đóng kết nối.
- **Vì sao:** payload nhị phân có thể chứa byte `\n`, nên không dùng phân cách được; length-prefix đọc đúng số byte cần,
  không phải quét từng byte; giới hạn 4096 chặn client gửi `length` khổng lồ làm server cấp phát bộ nhớ lớn.
- **Đánh đổi:** không đọc được bằng mắt như JSON; phải viết `FrameReader` xử lý trường hợp một lần đọc chỉ ra nửa frame.

## Q3 - Định dạng nhị phân

- **Câu hỏi:** mã hoá các trường của message thành byte bằng cách nào?
- **Các phương án:** tự viết bằng `BinaryPrimitives` / `BinaryWriter`; MessagePack; Protobuf.
- **Chốt:** tự viết, little-endian, chuỗi = `[u16 length][UTF-8]`.
- **Vì sao:** gói UDP nhỏ (INPUT khoảng 20-40 byte) và gửi 30 lần/giây, nhị phân gọn hơn JSON nhiều lần; tự viết thì
  **giải thích được từng byte** khi vấn đáp và không phải lo thư viện chạy được trên Unity hay không.
- **Đánh đổi:** dễ sai lệch giữa encoder và decoder → bắt buộc có unit test encode/decode cho mọi message; thêm trường
  mới phải sửa cả hai phía cùng lúc.

## Q4 - Gắn gói UDP với người chơi

- **Câu hỏi:** UDP không có kết nối. Khi một gói INPUT tới, server biết nó là của ai, và chống giả mạo thế nào?
- **Các phương án:** tin `IPEndPoint` của gói đầu tiên; mỗi gói kèm session token nhận qua TCP.
- **Chốt:** LOGIN_RESULT trả `sessionToken` ngẫu nhiên 4 byte (`RandomNumberGenerator`). Mọi gói UDP có header
  `[u16 magic 0x4737][u8 version][u8 type][u32 token]`. Gói UDP_HELLO đầu tiên giúp server **học endpoint**; nếu endpoint
  đổi mà token vẫn đúng thì cập nhật endpoint.
- **Vì sao:** qua Docker/NAT, cổng nguồn của client bị đổi, nên server không thể tin cổng client tự khai báo mà phải học từ
  gói thật; token khiến người khác không gửi input thay mình được nếu không biết token.
- **Đánh đổi:** tốn 4 byte mỗi gói; token 4 byte không phải mã hoá thật (bị nghe lén vẫn lộ) - đủ cho phạm vi đề tài.

## Q5 - Thư viện dùng chung

- **Câu hỏi:** server, bot và Unity cùng cần Protocol và Simulation. Chia sẻ code thế nào?
- **Các phương án:** một project chung; copy file sang từng nơi.
- **Chốt:** `shared/T47.Shared` là class library **netstandard2.1**; server và bot tham chiếu project; Unity dùng DLL build
  ra, copy vào `client/unity/Assets/Plugins/` bằng script. Docker build context là thư mục gốc để thấy `shared/`.
- **Vì sao:** prediction và reconciliation chỉ đúng khi client và server chạy **cùng một** hàm `Simulation.Step`; copy
  file sẽ lệch nhau sau vài lần sửa → nhân vật rung khi reconcile. Unity hỗ trợ netstandard2.1, không nạp được `net8.0`.
- **Đánh đổi:** sửa `shared/` phải build lại và copy DLL vào Unity; không dùng được API mới của .NET 8 trong `shared/`.

## Q6 - Vị trí bộ mô phỏng mạng (G7)

- **Câu hỏi:** độ trễ, jitter, mất gói giả lập đặt ở client hay server?
- **Các phương án:** chỉ client; cả client và server.
- **Chốt:** chỉ ở client, lớp `SimulatedUdp` bọc cả gửi và nhận (nằm trong `shared/` để bot dùng lại).
- **Vì sao:** cảnh 6 cần A = 50 ms và B = 250 ms cùng lúc; đặt ở client thì mỗi cửa sổ tự chỉnh bằng thanh trượt của
  mình, server giữ nguyên như chạy thật.
- **Đánh đổi:** chỉ mô phỏng UDP, không mô phỏng TCP (login chậm không quan trọng với demo); latency trên panel là
  **RTT** (mỗi chiều trễ một nửa) - phải nói rõ khi demo.

## Q7 - Lag compensation tua về tick nào

- **Câu hỏi:** khi phân xử ai nhặt được món hiếm, server tua lại thế giới về thời điểm nào?
- **Các phương án:** `clientTick` của INPUT; tick mà client **đang nhìn thấy** đối thủ/món đồ (`viewTick`).
- **Chốt:** `viewTick` = tick của snapshot đang được vẽ sau khi trừ interpolation delay; INPUT có thêm trường `viewTick`.
  Server lưu lịch sử 30 tick; `viewTick` cũ hơn 30 tick hoặc lớn hơn tick hiện tại thì **kẹp** về khoảng hợp lệ.
- **Vì sao:** người chơi bấm J dựa trên cái họ **thấy**, và cái họ thấy là quá khứ (latency + 100 ms interp). Tua về
  `viewTick` mới đúng tinh thần "phân xử theo cái người chơi thấy". Giới hạn 1 giây để gian lận gửi `viewTick` giả
  cũng chỉ lợi được tối đa 1 giây.
- **Đánh đổi:** người mạng tốt đôi khi "bị nhặt mất dù đã tới trước trên màn hình mình"; server tốn bộ nhớ lịch sử
  (nhỏ: 30 tick × 2 người).

## Q8 - Rớt mạng giữa trận

- **Câu hỏi:** một người mất kết nối giữa trận thì xử lý ra sao?
- **Các phương án:** xử thua ngay; giữ chỗ N giây chờ kết nối lại.
- **Chốt:** xử thua ngay. Coi là rớt khi TCP đóng **hoặc** 5 giây không nhận gói UDP nào. Người còn lại nhận MATCH_END
  lý do `OPPONENT_LEFT`, kết quả lưu SQLite, phòng đóng.
- **Vì sao:** đơn giản, chắc chắn đúng, đủ yêu cầu "chịu được ngắt đột ngột" của đề; reconnect cần lưu và khôi phục state,
  không kịp trong hạn nộp 09/11.
- **Đánh đổi:** mạng chập 5 giây là mất trận. Reconnect ghi vào mục "ngoài phạm vi".

## Q9 - Mật khẩu

- **Câu hỏi:** lưu mật khẩu người dùng trong SQLite thế nào?
- **Các phương án:** lưu thô; hash nhanh (SHA256); hash chậm có salt (PBKDF2).
- **Chốt:** `Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, SHA256, 32)`, salt 16 byte ngẫu nhiên mỗi user; so sánh
  bằng `CryptographicOperations.FixedTimeEquals`.
- **Vì sao:** lộ file DB cũng không lộ mật khẩu; salt khiến 2 người cùng mật khẩu có hash khác nhau và vô hiệu bảng tra
  sẵn; PBKDF2 chậm có chủ đích nên dò mật khẩu tốn kém; có sẵn trong .NET, không thêm thư viện.
- **Đánh đổi:** mỗi lần login tốn vài chục ms CPU (chấp nhận được). Mật khẩu vẫn đi trên TCP không mã hoá - ghi rõ là
  giới hạn của đề tài (không làm TLS).

---

## Xong khi

- [ ] Cả nhóm đã đọc, không ai phản đối trong tối 02/11.
- [ ] #1 đã chép phần "Chốt / Vì sao / Đánh đổi" vào mục Design Decisions của `architecture.md` và `protocol-design.md`.
