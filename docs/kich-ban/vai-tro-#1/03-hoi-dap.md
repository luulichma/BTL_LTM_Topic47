# 03 - Hỏi đáp (ôn C2 Reasoning)

> Chưa nắm khái niệm nào thì mở [00-ly-thuyet.md](00-ly-thuyet.md). Dòng **Lý thuyết** dưới mỗi mục chỉ tới đúng phần cần đọc trước.

Mỗi câu: **trả lời ngắn** (nói trong 20-30 giây), rồi **nếu bị hỏi thêm**. Khuôn chung khi trả lời câu "vì sao":
*em chọn X → vì Y → đã cân nhắc Z nhưng bỏ vì ... → cái giá phải trả là ...*

Câu có dấu ★ là câu mẫu có trong đề (INSTRUCTION.md §9), gần như chắc chắn bị hỏi.

---

## A. Giao thức

### ★ A1. Vì sao dùng cả TCP và UDP? Đổi hết sang TCP thì sao?

**Lý thuyết:** [L4 TCP](00-ly-thuyet.md#lt-tcp) · [L10 UDP](00-ly-thuyet.md#lt-udp) · [L6 Nghẽn đầu hàng](00-ly-thuyet.md#lt-hol) · [L7 Nagle](00-ly-thuyet.md#lt-nagle) · [L13 Số thứ tự, bù mất gói](00-ly-thuyet.md#lt-seq)

> TCP cho đăng ký, đăng nhập, ghép trận, kết thúc trận: các tin này phải tới và đúng thứ tự, mất `MATCH_FOUND` là
> kẹt luôn, mà lại gửi ít nên trễ một chút không sao. UDP cho INPUT và SNAPSHOT, gửi 20-30 lần một giây, gói mới
> quan trọng hơn gói cũ.
>
> Nếu đổi hết sang TCP: khi một gói mất, TCP giữ lại mọi gói sau để chờ gửi lại gói đó, gọi là nghẽn đầu hàng.
> Nhân vật đứng hình rồi nhảy. Code thì đơn giản hơn: bỏ được token, `UDP_HELLO`, học endpoint.
>
> Nếu đổi hết sang UDP: em phải tự làm gửi lại và sắp thứ tự cho đăng nhập, tức là tự viết lại một phần TCP.

**Hỏi thêm: "UDP mất gói thì sao?"** Bù ở tầng ứng dụng: mỗi gói INPUT mang thêm 3 input cũ chưa được xác nhận;
SNAPSHOT là trạng thái đầy đủ nên mất một cái thì cái sau thay thế.

**Hỏi thêm: "tắt Nagle là đủ chứ gì?"** Nagle chỉ làm trễ lúc gửi. Nghẽn đầu hàng là do cơ chế đảm bảo thứ tự của
TCP, tắt Nagle không hết được. Em có tắt Nagle cho kênh TCP (`NoDelay = true`) vì frame của em nhỏ.

### ★ A2. Bên nhận biết tin nhắn kết thúc ở đâu? Tin nhắn bị chia làm 2 lần `recv()` thì sao?

**Lý thuyết:** [L5 Tách frame TCP](00-ly-thuyet.md#lt-framing)

> Mỗi frame TCP có 4 byte độ dài ở đầu. Bên nhận đọc đủ 4 byte, rồi đọc đủ đúng số byte đó. Hàm `ReadExactAsync`
> lặp `ReadAsync` tới khi đủ. Nếu frame đến thành hai mẩu, lần `ReadAsync` đầu trả về phần đầu, vòng lặp đọc tiếp
> phần sau. Nếu hai frame dính nhau, em chỉ đọc đúng độ dài của frame đầu, phần còn lại để lần sau.

Mở `FrameReader.cs:45-73` chỉ vào vòng `while (read < buffer.Length)`. Test:
`SessionTests.Frame_split_across_many_writes_is_reassembled`.

### A3. Vì sao length-prefix, không dùng `\n` + JSON?

**Lý thuyết:** [L5 Tách frame TCP](00-ly-thuyet.md#lt-framing) · [L17 Nhị phân, codec](00-ly-thuyet.md#lt-codec)

> Payload của em là nhị phân, có thể chứa byte `0x0A`, chính là `\n`. Dùng ký tự phân cách thì phải escape. Với
> length-prefix, em biết trước phải đọc bao nhiêu byte, và chặn được độ dài quá lớn trước khi cấp phát.
> Cái giá: không đọc bằng mắt được như JSON, và phải tự viết `FrameReader`.

### A4. Vì sao tự viết nhị phân, không dùng JSON hay Protobuf?

**Lý thuyết:** [L17 Nhị phân, codec](00-ly-thuyet.md#lt-codec) · [L15 Byte, hex](00-ly-thuyet.md#lt-byte) · [L38 Các loại test](00-ly-thuyet.md#lt-test)

> Gói INPUT tối đa 61 byte, gửi 30 lần một giây. JSON to gấp nhiều lần. Protobuf thì thêm thư viện, phải chắc chạy
> được trên Unity, và khó giải thích từng byte khi vấn đáp. Tự viết thì em chỉ được từng byte nằm ở đâu.
> Cái giá: encoder và decoder dễ lệch nhau, nên mọi message đều có unit test mã hoá rồi giải mã lại.

### A5. Little-endian là gì, sao chọn?

**Lý thuyết:** [L16 Little-endian](00-ly-thuyet.md#lt-endian)

> Là byte thấp đứng trước: số 13 kiểu u32 ghi thành `0D 00 00 00`. Điều quan trọng không phải chọn bên nào, mà là
> ghi rõ một thứ tự cố định và cả hai phía đều dùng `BinaryPrimitives.*LittleEndian`, nên không phụ thuộc máy.

### A6. Server biết gói UDP là của ai thế nào?

**Lý thuyết:** [L11 NAT, token](00-ly-thuyet.md#lt-nat)

> Lúc đăng nhập qua TCP, server cấp token ngẫu nhiên 4 byte. Mọi gói UDP mang token ở header. Server tra token ra
> phiên, rồi lấy IP:port thật từ chính gói tới. Em không tin cổng client tự khai, vì qua NAT của Docker hay router
> cổng bị đổi. Em cũng không tin "gói đầu tiên từ IP này", vì ai gửi trước là chiếm được.
> Giới hạn: token không mã hoá, ai nghe lén cùng mạng thì lấy được. Đề tài không làm TLS/DTLS.

### A7. Stateful hay stateless? Vì sao?

**Lý thuyết:** [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state)

> Stateful. Server nhớ mỗi kết nối đang ở trạng thái nào: `Connected → LoggedIn → InQueue → InMatch`. Mỗi lệnh được
> kiểm theo trạng thái, ví dụ tìm trận khi đang trong trận thì trả `INVALID_STATE`. Game có trận kéo dài, server phải
> biết ai ở phòng nào để gửi SNAPSHOT, nên stateless kiểu HTTP không hợp.
> Cái giá: phải dọn trạng thái cẩn thận khi kết nối chết; khởi động lại server thì mất các trận đang chơi.

### A8. Gặp message hỏng thì server làm gì?

**Lý thuyết:** [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate) · [L20 Mã lỗi](00-ly-thuyet.md#lt-status) · [L14 Bỏ gói rác im lặng](00-ly-thuyet.md#lt-udp-im-lang)

| Tình huống | Server làm |
| --- | --- |
| Độ dài 0 hoặc > 4096 | `ERROR(FRAME_TOO_LARGE)`, đóng kết nối |
| Type lạ, payload thiếu/thừa byte, gửi sai chiều | `ERROR(BAD_FRAME)`, giữ kết nối; lần 3 thì đóng |
| Lệnh đúng nhưng sai trạng thái | Trả mã `NOT_LOGGED_IN` / `INVALID_STATE`, giữ kết nối |
| Gói UDP hỏng, token lạ | Bỏ im lặng, tăng bộ đếm |

> Kết nối gửi rác chỉ làm hỏng kết nối đó. Test `Oversized_frame_closes_only_that_connection` kiểm client khác vẫn
> được phục vụ.

---

## B. Đồng thời và an toàn luồng

### ★ B1. Dữ liệu nào dùng chung giữa các luồng, bảo vệ thế nào? Deadlock có thể xảy ra ở đâu?

**Lý thuyết:** [L23 Race condition, lock](00-ly-thuyet.md#lt-race) · [L24 Deadlock](00-ly-thuyet.md#lt-deadlock) · [L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu)

| Dữ liệu chung | Bảo vệ bằng |
| --- | --- |
| Danh sách phiên, bảng token, bảng playerId | `ConcurrentDictionary`, chỉ thao tác một bước (`TryAdd`, `TryRemove`) |
| Hàng đợi ghép trận | `lock (_queueLock)` |
| Hộp thư của phòng | `ConcurrentQueue` (nhiều nơi ghi, một nơi đọc) |
| State trong phòng | Không cần lock: chỉ vòng tick đọc/ghi |
| Hàng gửi của mỗi client, hàng gửi UDP | `Channel`, một vòng gửi duy nhất |
| Kết nối SQLite | `lock (_dbLock)` |
| Trạng thái phiên (`_state`, `_room`) | `lock (_lock)` trong `ClientSession` |

> Deadlock cần ít nhất hai lock bị giữ lồng nhau. Ở server của em không chỗ nào giữ hai lock cùng lúc, và không làm
> I/O mạng khi đang giữ lock. Ví dụ Matchmaker lấy 2 người ra trong lock, nhả lock rồi mới tạo phòng và gửi
> `MATCH_FOUND`. Phiên và phòng nói chuyện qua hộp thư, gửi xong không chờ trả lời, nên không có vòng chờ nhau.

**Hỏi thêm: "chỗ nào dễ race nhất?"** Matchmaker. Nếu "kiểm đủ 2 người" và "lấy 2 người ra" nằm ở hai lần lock
khác nhau, hai luồng cùng thấy đủ người và lấy trùng một người.

### ★ B2. 1.000 client thì sao? Nút thắt ở đâu?

**Lý thuyết:** [L21 Thread, async/await](00-ly-thuyet.md#lt-async) · [L28 Tick](00-ly-thuyet.md#lt-tick) · [L33 Băm mật khẩu](00-ly-thuyet.md#lt-pbkdf2) · [L37 SQLite](00-ly-thuyet.md#lt-sqlite)

> 1.000 kết nối TCP thì async chịu được, chủ yếu tốn bộ nhớ đệm. 500 phòng × 30 tick/giây là 15.000 lần chạy tick
> mỗi giây trên thread pool, nên **nút thắt đầu tiên là CPU cho vòng tick**. Thứ hai là đợt đăng nhập dồn dập: mỗi
> lần PBKDF2 tốn vài chục ms CPU, cộng lock SQLite (em đã tính hash ngoài lock). Thứ ba là một `Channel` gửi UDP
> duy nhất có thể thành hàng dài; khi đó tách thành nhiều socket gửi.
>
> Số em đo thật là 50 bot, 25 phòng, vẫn 30 tick/giây, 0 lỗi. 1.000 em chưa đo, nên em không khẳng định con số.

### B3. Vì sao không thread-per-client?

**Lý thuyết:** [L21 Thread, async/await](00-ly-thuyet.md#lt-async) · [L22 Kiểu tổ chức server](00-ly-thuyet.md#lt-mo-hinh-server)

> Dễ viết, nhưng mỗi client giữ một thread, tốn bộ nhớ; và state phòng bị nhiều thread client cùng sửa nên phải lock
> khắp nơi. Em cũng không dùng một game loop chung cho mọi phòng, vì một phòng chậm sẽ kéo tụt tick của cả server.

### B4. Một phòng bị lỗi thì sao?

**Lý thuyết:** [L22 Kiểu tổ chức server](00-ly-thuyet.md#lt-mo-hinh-server)

> Vòng tick có `try/catch` bao ngoài (`Room.cs:153-158`): lỗi thì ghi log, gửi `MATCH_END`, đóng riêng phòng đó.
> Phòng khác chạy ở Task khác nên không bị ảnh hưởng.

### B5. Client đọc chậm, không nhận kịp thì sao?

**Lý thuyết:** [L26 Hàng đợi có giới hạn](00-ly-thuyet.md#lt-backpressure)

> Mỗi client có hàng gửi tối đa 256 frame. `Send` dùng `TryWrite`, không bao giờ chờ. Đầy thì đóng phiên đó. Nếu chờ,
> vòng tick của phòng sẽ bị một client chậm giữ lại.

---

## C. Ngắt kết nối và lỗi

### ★ C1. `kill -9` một client, đọc log

**Lý thuyết:** [L8 FIN, RST, đóng kết nối](00-ly-thuyet.md#lt-fin-rst) · [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state)

Xem [01-demo-va-mo-dau.md](01-demo-va-mo-dau.md) mục E. Ý chính:

> Tiến trình chết thì hệ điều hành vẫn đóng socket và gửi FIN, nên server thấy `ReadAsync` trả 0, log ghi
> `peer closed`. Khối dọn dẹp chạy: gỡ khỏi hàng chờ, báo phòng người này rời, phòng xử người còn lại thắng
> `OpponentLeft`, gỡ phiên khỏi registry, `sessions` giảm.

### C2. Rút dây mạng (không có FIN)?

**Lý thuyết:** [L8 FIN, RST, đóng kết nối](00-ly-thuyet.md#lt-fin-rst) · [L9 Timeout, PING](00-ly-thuyet.md#lt-timeout)

> Không có FIN thì `ReadAsync` cứ chờ. Có hai lưới: trong trận, phòng thấy 5 giây không có UDP là xử rớt mạng;
> ngoài trận, 15 giây không có frame TCP nào là đóng kết nối. Client rảnh gửi PING mỗi 5 giây để không bị đóng nhầm.

### C3. Vì sao rớt mạng là thua luôn, không cho vào lại?

**Lý thuyết:** [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state)

> Cho vào lại thì phải giữ chỗ, lưu và khôi phục state, xác thực lại, đồng bộ lại tick. Không kịp trong thời hạn và
> dễ sinh lỗi. Xử thua ngay thì chắc chắn đúng và luôn giải phóng tài nguyên. Cái giá: mạng chập chờn 5 giây là mất
> trận.

### C4. Có lỗi nào chỉ thấy khi chạy thật không?

**Lý thuyết:** [L8 FIN, RST, đóng kết nối](00-ly-thuyet.md#lt-fin-rst)

> Có hai lỗi. Một: server gửi `ERROR` rồi đóng ngay, Windows gửi RST vì bộ đệm còn byte chưa đọc, client mất luôn
> frame `ERROR`. Em sửa bằng đóng êm: gửi FIN, đọc bỏ tối đa 500 ms rồi mới đóng. Hai: bot gửi thừa INPUT, phát hiện
> khi chạy bot trong Docker.

---

## D. Bảo mật, chống gian lận

### D1. Client sửa gói để dịch chuyển?

**Lý thuyết:** [L27 Authoritative, relay](00-ly-thuyet.md#lt-authoritative)

> Ở chế độ authoritative, client không gửi vị trí, chỉ gửi phím bấm; server tự chạy `Simulation.Step`. Gói
> `POS_REPORT` bị bỏ qua. Chế độ relay là bản đối chứng cố ý tin client, để demo cảnh 1 cho thấy gian lận thành công.

### D2. Gửi dồn input để chạy nhanh?

**Lý thuyết:** [L13 Số thứ tự, bù mất gói](00-ly-thuyet.md#lt-seq) · [L26 Hàng đợi có giới hạn](00-ly-thuyet.md#lt-backpressure)

> Mỗi tick server áp tối đa 4 input một người, hàng chờ tối đa 32. Gửi bao nhiêu thì tốc độ trung bình vẫn không quá
> một bước mỗi tick.

### D3. Gửi `viewTick` giả rất cũ để luôn thắng món hiếm?

**Lý thuyết:** [L31 Lag compensation](00-ly-thuyet.md#lt-lag-comp) · [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate)

> Server kẹp `viewTick` về khoảng `[tick - 30, tick]`, tức lùi tối đa 1 giây. Khai gian chỉ được lợi trong giới hạn
> đó.

### D4. Mật khẩu?

**Lý thuyết:** [L33 Băm mật khẩu](00-ly-thuyet.md#lt-pbkdf2) · [L34 Tấn công đo thời gian](00-ly-thuyet.md#lt-timing)

> PBKDF2-SHA256, 100.000 vòng, salt 16 byte mỗi user, so bằng `FixedTimeEquals`. Salt làm hai người trùng mật khẩu
> có hash khác nhau, bảng tra sẵn vô dụng. 100.000 vòng làm mỗi lần đoán tốn vài chục ms. So sánh thời gian cố định
> để không lộ "đúng bao nhiêu byte đầu" qua thời gian trả lời.
> Giới hạn em biết: mật khẩu vẫn đi trên TCP không mã hoá.

---

## E. Tick, lag compensation (phần server)

### E1. Vì sao 30 tick, 20 snapshot/giây?

**Lý thuyết:** [L28 Tick](00-ly-thuyet.md#lt-tick) · [L29 Snapshot](00-ly-thuyet.md#lt-snapshot) · [L30 Prediction, reconcile](00-ly-thuyet.md#lt-prediction)

> 30 tick đủ mượt cho game di chuyển chậm như siêu thị, tốn ít CPU. SNAPSHOT gửi 2 trên 3 tick, khoảng 20 lần/giây,
> vì client có nội suy nên không cần nhiều hơn, đỡ băng thông.

**Hỏi thêm: "đổi `TICK_RATE=60` thì sao?"** Phía server: chạy 60 tick, `dt` = 1/60 (`Room.cs:50`), số tick của trận
tính lại theo tick rate nên vẫn 2:30; snapshot vẫn 2 trên 3 tick nên thành 40 lần/giây; giới hạn `viewTick` 30 tick
chỉ còn 0,5 giây.

Nhưng **client sẽ lệch**: client Unity gửi INPUT và dự đoán theo hằng `SimConstants.TickRate = 30`, `Dt = 1/30`
(`shared/T47.Shared/Simulation/SimConstants.cs:11-14`, `NetClient.cs:503`). Client đi mỗi bước 1/30 giây, server
áp mỗi input 1/60 giây, nên server thấy nhân vật đi chậm bằng nửa, reconcile kéo ngược liên tục. Muốn đổi tick rate
thật thì client phải lấy `dt` từ `MATCH_FOUND.tickRate` (trường này đã có trong giao thức) thay vì hằng số.

> Đây là chỗ em biết là giới hạn: tick rate cấu hình được ở server, nhưng client hiện chỉ chạy đúng với 30.

### E2. Lag compensation ở server làm gì?

**Lý thuyết:** [L31 Lag compensation](00-ly-thuyet.md#lt-lag-comp)

> Mỗi INPUT mang `viewTick` là tick của thế giới mà người chơi đang nhìn thấy. Hai người cùng giành món hiếm thì bật
> lag comp: ai có `viewTick` nhỏ hơn thắng, tức ai bấm trước theo màn hình của mình. Server chờ tối đa 12 tick
> (400 ms) để gom đủ lượt giành của cả hai.
>
> Em chỉ "tua thời điểm", không lưu và tua lịch sử vị trí như game bắn súng, vì kệ hiếm đứng yên.
> Cái giá: người mạng tốt có lúc tới trước trên màn hình mình mà vẫn thua; và khi bật lag comp, giành một mình cũng
> phải chờ tối đa 400 ms mới biết kết quả.

---

## F. Triển khai

### F1. `docker compose up` làm gì?

**Lý thuyết:** [L35 Docker](00-ly-thuyet.md#lt-docker)

> Build image 2 tầng: tầng `sdk:8.0` để build, tầng `runtime:8.0` để chạy, cho image nhẹ. Build context là gốc repo
> để thấy thư mục `shared/`. Map cổng `5000/tcp` và `5001/udp`. SQLite nằm trên volume `./data`, xoá container không
> mất tài khoản. Mọi cấu hình đọc từ `.env`.

### F2. Vì sao Radmin VPN?

**Lý thuyết:** [L36 VPN, CGNAT](00-ly-thuyet.md#lt-vpn) · [L11 NAT, token](00-ly-thuyet.md#lt-nat)

> Hai máy ở xa không gọi thẳng vào nhau được vì NAT. Radmin tạo mạng LAN ảo, mỗi máy có thêm IP `26.x.x.x`, giữ nguyên
> cổng TCP và UDP, nên không phải sửa giao thức. Mở port router thì nhiều nhà mạng dùng CGNAT, không mở được. Tunnel
> như ngrok đổi cổng, mà client lấy cổng UDP từ `MATCH_FOUND`, nên UDP sẽ đi sai chỗ.

### F3. `shared/` dùng chung có vi phạm "client và server độc lập" không?

**Lý thuyết:** [L1 Client, server, giao thức](00-ly-thuyet.md#lt-client-server)

> Không. `shared/` chỉ là đặc tả giao thức viết thành code và luật di chuyển. Client và server vẫn là hai chương trình
> riêng, chỉ nói chuyện qua socket. Một client viết bằng ngôn ngữ khác chỉ cần đọc `protocol-design.md` là kết nối
> được.

---

## G. Dùng AI

### G1. Code này AI viết phải không? Em kiểm chứng thế nào?

**Lý thuyết:** [L38 Các loại test](00-ly-thuyet.md#lt-test)

Trả lời thật, giống README:

> Dạ, phần server em dùng Claude sinh phần lớn code, em ghi mức "Generated" trong README. Các quyết định thiết kế
> Q1-Q10 nhóm chốt trong buổi họp 02/11, sau khi so các phương án. Em tự quyết một số chỗ AI đề xuất, ví dụ lag comp
> chỉ tua thời điểm không tua vị trí, lưu kết quả trận ngoài vòng tick. Em kiểm bằng chạy thật: 196 test .NET, 20-50
> bot trong Docker, `docker kill` giữa trận, script gửi dữ liệu rác. Hai lỗi chỉ lộ ra khi chạy thật là RST làm mất
> frame `ERROR` và bot gửi thừa INPUT.

Đừng chối, đừng nói quá. Thầy đã nói khai AI không bị trừ điểm, giấu mới bị.

---

## Mẹo khi trả lời

- Không chắc thì nói "em chưa đo / em chưa thử, theo em dự đoán là ..." và giải thích lý do. Không bịa số.
- Bị hỏi phần của #2 / #3: "Phần đó bạn ... làm, em hiểu ở mức ..., chi tiết bạn ấy trả lời rõ hơn". Nhưng vẫn
  phải nói được phía server nhận gì, trả gì cho phần đó.
- Luôn mở code khi giải thích. Chỉ vào dòng code thuyết phục hơn nói suông.
