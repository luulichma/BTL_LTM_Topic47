# 02 - Đi theo một người chơi qua server (ôn C1 Ownership)

> Chưa nắm khái niệm nào thì mở [00-ly-thuyet.md](00-ly-thuyet.md). Dòng **Lý thuyết** dưới mỗi mục chỉ tới đúng phần cần đọc trước.

Cách ôn dễ nhất: kể lại hành trình của một người chơi tên **An**, từ lúc mở game tới lúc thoát. Mỗi bước có:
code ở đâu, **nói thế nào** (1-3 câu), và chỗ hay bị hỏi sâu.

Số dòng theo repo classroom, nhánh `main`.

```text
An mở game
 │ 1. kết nối TCP ............ ServerApp.AcceptLoopAsync
 │ 2. gửi frame .............. FrameReader + MessageCodec
 │ 3. LOGIN .................. ClientSession.HandleLogin + UserStore
 │ 4. FIND_MATCH ............. Matchmaker.Enqueue
 │ 5. vào phòng, mở UDP ...... Room.Start + UdpGateway
 │ 6. chơi: INPUT → tick → SNAPSHOT ... Room.Tick
 │ 7. hết giờ ................ Room.EndMatch
 └ 8. thoát / chết giữa chừng  ClientSession.Cleanup
```

---

## Bước 1. An kết nối TCP

**Lý thuyết:** [L2 IP, cổng, 0.0.0.0](00-ly-thuyet.md#lt-ip-port) · [L4 TCP](00-ly-thuyet.md#lt-tcp) · [L21 Thread, async/await](00-ly-thuyet.md#lt-async) · [L7 Nagle](00-ly-thuyet.md#lt-nagle)

`server/src/T47.Server/ServerApp.cs:66-95`

```csharp
client = await _listener!.AcceptTcpClientAsync(ct);   // chờ kết nối mới, không giữ thread
client.NoDelay = true;                                // tắt Nagle: frame nhỏ gửi ngay
var session = new ClientSession(id, client, _ctx!);
_ctx!.Registry.Add(session);
_sessionTasks[id] = Task.Run(async () => { await session.RunAsync(); ... });
```

**Nói:**

> Vòng accept chỉ làm một việc là nhận kết nối, tạo một `ClientSession` rồi cho nó chạy riêng bằng `Task.Run`.
> Vòng accept không chờ phiên nào xong, nên client này chậm không chặn client khác.

**Hay bị hỏi:**

- *`await` có tốn một thread không?* Không. Trong lúc chờ dữ liệu, thread được trả về thread pool. 50 client đang
  chờ không phải 50 thread bị khoá.
- *`NoDelay` là gì?* Tắt thuật toán Nagle. Nagle gom các gói nhỏ lại rồi mới gửi, làm frame PONG hay MATCH_FOUND
  bị trễ vô ích.
- *Bind `0.0.0.0` để làm gì?* Lắng nghe trên mọi card mạng. Trong Docker, bind `127.0.0.1` thì chỉ chính container
  vào được, máy host và máy khác không vào được.

---

## Bước 2. An gửi một frame (tách tin nhắn trên TCP)

**Lý thuyết:** [L5 Tách frame TCP](00-ly-thuyet.md#lt-framing) · [L15 Byte, hex](00-ly-thuyet.md#lt-byte) · [L16 Little-endian](00-ly-thuyet.md#lt-endian) · [L17 Nhị phân, codec](00-ly-thuyet.md#lt-codec) · [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate) · [L38 Các loại test](00-ly-thuyet.md#lt-test)

`shared/T47.Shared/Protocol/FrameReader.cs:45-73`

```text
[ length: u32 LE ][ type: u8 ][ payload: length-1 byte ]
   4 byte            1 byte
```

```csharp
if (!await ReadExactAsync(_header, ct)) return Closed;        // đọc ĐỦ 4 byte
uint length = BinaryPrimitives.ReadUInt32LittleEndian(_header);
if (length == 0 || length > MaxFrameLength) return TooLarge;  // kiểm TRƯỚC khi cấp phát
var body = new byte[length];
if (!await ReadExactAsync(body, ct)) return Closed;           // đọc ĐỦ length byte
```

`ReadExactAsync` (dòng 63-73) lặp `ReadAsync` cho tới khi đủ byte; `ReadAsync` trả 0 nghĩa là bên kia đã đóng.

**Nói:**

> TCP là luồng byte. Client gửi một frame 13 byte, server có thể nhận 5 byte rồi 8 byte, hoặc nhận một lần cả hai
> frame dính nhau. Vì vậy em không tin một lần đọc là một tin nhắn: em đọc đủ 4 byte độ dài, rồi đọc đủ đúng chừng
> đó byte. Dư thì để lần đọc sau.

**Ví dụ phải nói được bằng miệng** (LOGIN, user `an`, pass `abc123`):

```text
0D 00 00 00 | 03 | 02 00 61 6E | 06 00 61 62 63 31 32 33
length = 13   LOGIN  "an"          "abc123"
```

13 = 1 byte type + (2 + 2) byte "an" + (2 + 6) byte "abc123". Little-endian nên số 13 nằm ở byte đầu.

**Hay bị hỏi:**

- *Vì sao kiểm `length` trước khi `new byte[length]`?* Kẻ xấu gửi `FF FF FF FF` là đòi 4 GB. Kiểm trước thì không
  bao giờ cấp phát quá 4096 byte.
- *`TooLarge` thì sao không bỏ qua frame đó, đọc tiếp?* Độ dài đã sai thì không còn biết frame sau bắt đầu ở đâu.
  Chỉ còn cách báo `ERROR(FRAME_TOO_LARGE)` rồi đóng (`ClientSession.cs:156-159`).
- *Có test không?* `FrameReaderTests.SplitAndMergedFrames_AreSeparatedCorrectly` cắt luồng thành từng mẩu 1, 2, 3...
  byte; `SessionTests.Frame_split_across_many_writes_is_reassembled` gửi qua socket thật.

Sau khi có frame, `MessageCodec.TryDecodeTcp` (`MessageCodec.cs:29`) đọc payload theo `type`. Sai một trong các thứ
sau là hỏng: type lạ, type của UDP, thiếu byte, **thừa byte**, chuỗi quá 64 byte. Hỏng thì `ERROR(BAD_FRAME)`, vẫn
giữ kết nối; tới lần thứ 3 thì đóng (`ClientSession.cs:200-206`).

> Frame hỏng nội dung nhưng độ dài đúng thì em vẫn biết frame sau ở đâu, nên cho client sai thêm 2 lần. Còn độ dài
> sai thì đóng ngay.

---

## Bước 3. An đăng nhập

**Lý thuyết:** [L33 Băm mật khẩu](00-ly-thuyet.md#lt-pbkdf2) · [L34 Tấn công đo thời gian](00-ly-thuyet.md#lt-timing) · [L23 Race condition, lock](00-ly-thuyet.md#lt-race) · [L11 NAT, token](00-ly-thuyet.md#lt-nat) · [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state) · [L20 Mã lỗi](00-ly-thuyet.md#lt-status)

`server/src/T47.Server/ClientSession.cs:217-251`, `UserStore.cs:150-179`

Thứ tự kiểm tra:

1. Phiên đang ở trạng thái `Connected`? Không thì `INVALID_STATE`.
2. Định dạng username / mật khẩu đúng? Không thì `INVALID_CREDENTIAL_FORMAT`.
3. `VerifyLogin`: đọc `hash`, `salt` trong lock DB → **ra khỏi lock** mới tính PBKDF2 → so bằng `FixedTimeEquals`.
4. `TryClaimPlayer`: tài khoản này đã đăng nhập ở kết nối khác? Có thì `ALREADY_LOGGED_IN`.
5. Cấp token ngẫu nhiên 4 byte, đổi trạng thái sang `LoggedIn`, gửi `LOGIN_RESULT(OK, playerId, token)`.

**Nói:**

> Mật khẩu không lưu thô mà lưu PBKDF2-SHA256, 100.000 vòng, mỗi user một salt 16 byte. Hash tính ngoài lock để
> một người đăng nhập không chặn người khác. Sai username hay sai mật khẩu đều trả cùng một mã, để không lộ
> username nào có thật.

**Hay bị hỏi:**

- *Username không tồn tại sao vẫn tính hash?* (`UserStore.cs:172`, dùng `DummySalt`) Để thời gian trả lời giống
  nhau; nếu bỏ qua thì username sai trả lời nhanh hơn hẳn, đo thời gian là biết username nào có thật.
- *Hai máy đăng nhập cùng tài khoản cùng lúc?* `_players.TryAdd` (`SessionRegistry.cs:20`) là thao tác nguyên tử:
  đúng một bên thắng.
- *Token để làm gì?* Gói UDP không có kết nối, server cần biết gói là của ai. Mọi gói UDP mang token này ở header.

---

## Bước 4. An tìm trận

**Lý thuyết:** [L23 Race condition, lock](00-ly-thuyet.md#lt-race) · [L24 Deadlock](00-ly-thuyet.md#lt-deadlock) · [L38 Các loại test](00-ly-thuyet.md#lt-test)

`server/src/T47.Server/Matchmaker.cs:19-34`

```csharp
lock (_queueLock)
{
    if (!_queue.Contains(session)) _queue.Add(session);
    if (_queue.Count >= 2) { a = _queue[0]; b = _queue[1]; _queue.RemoveRange(0, 2); }
}
if (a != null && b != null) _rooms.StartMatch(a, b);   // NGOÀI lock
```

**Nói:**

> Kiểm "đủ 2 người" và lấy 2 người ra phải nằm trong cùng một lock. Nếu tách ra, hai luồng có thể cùng thấy hàng có
> 2 người và cùng lấy một người, thành ra một người ở hai phòng. Còn tạo phòng và gửi `MATCH_FOUND` thì làm sau khi
> nhả lock, vì không được làm việc chậm khi đang giữ lock.

Test: `MatchTests.Concurrent_find_match_pairs_everyone_exactly_once` cho nhiều client cùng tìm trận một lúc.

`QUEUE_STATUS` được đưa vào hàng gửi **trước** khi gọi `Enqueue` (`ClientSession.cs:274-276`), nên client luôn nhận
`QUEUE_STATUS` trước `MATCH_FOUND`.

---

## Bước 5. Vào phòng, mở đường UDP

**Lý thuyết:** [L10 UDP](00-ly-thuyet.md#lt-udp) · [L11 NAT, token](00-ly-thuyet.md#lt-nat) · [L14 Bỏ gói rác im lặng](00-ly-thuyet.md#lt-udp-im-lang) · [L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu) · [L26 Hàng đợi có giới hạn](00-ly-thuyet.md#lt-backpressure)

`Room.cs:80-107`, `Room.cs:233-258`, `UdpGateway.cs:67-82`

1. `Room.Start` chuyển từng phiên sang `InMatch` (`TryEnterMatch`) rồi gửi `MATCH_FOUND` (có `roomId`, slot,
   cổng UDP, seed, tick rate).
2. Client gửi `UDP_HELLO` mỗi 200 ms tới khi nhận `UDP_HELLO_ACK`.
3. `UdpGateway.Handle`: giải mã header → tìm phiên theo token → phiên có ở phòng nào không → `room.Post(...)`.
   Sai bất kỳ bước nào: **bỏ im lặng**, chỉ tăng bộ đếm.
4. Trong vòng tick, `HandleUdp` **học endpoint** (IP:port) từ chính gói vừa tới.

**Nói:**

> Header UDP 8 byte: 2 byte magic `0x4737` để loại nhanh gói lạc, 1 byte version, 1 byte type, 4 byte token.
> Server không tin cổng client tự khai, vì qua NAT của Docker hay router cổng nguồn bị đổi. Server lấy địa chỉ
> thật từ gói tới, miễn là token đúng.

**Hay bị hỏi:**

- *Gói UDP rác sao không trả lỗi?* Trả lời gói giả mạo địa chỉ là giúp kẻ xấu dội lưu lượng vào máy khác. Bỏ im
  lặng là an toàn nhất.
- *Log có ghi từng gói rác không?* Không, chỉ gói đầu và mỗi 1000 gói (`UdpGateway.cs:85-90`), để flood rác không
  làm ngập log.
- *Vì sao chỉ một chỗ gửi UDP?* Mọi phòng bỏ datagram vào một `Channel`, một vòng gửi duy nhất gọi `SendAsync`
  (`UdpGateway.cs:93`). Channel đầy thì bỏ gói (`DropWrite`), vì UDP vốn chịu được mất gói, còn chặn lại thì phòng
  bị trễ tick.

---

## Bước 6. Trong trận: INPUT → tick → SNAPSHOT

**Lý thuyết:** [L28 Tick](00-ly-thuyet.md#lt-tick) · [L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu)

Đây là phần quan trọng nhất về đồng thời.

`Room.cs:111-200` (vòng tick), `Room.cs:68-78` (hộp thư), `Room.cs:288-318` (áp INPUT)

### Hộp thư

**Lý thuyết:** [L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu) · [L26 Hàng đợi có giới hạn](00-ly-thuyet.md#lt-backpressure)

```csharp
public bool Post(RoomEvent ev, bool force = false) { ... _inbox.Enqueue(ev); ... }
```

**Nói:**

> State của phòng (vị trí, món, điểm) chỉ có **một** luồng đọc và ghi là vòng tick. `UdpGateway` nhận INPUT, hay
> `ClientSession` báo người chơi rời đi, đều không sửa phòng trực tiếp mà bỏ tin nhắn vào `ConcurrentQueue`. Đầu mỗi
> tick, vòng tick rút hết hộp thư ra xử lý. Vì một luồng duy nhất nên state game không cần lock, không thể có race
> condition.

Hộp thư giới hạn 1024 sự kiện chống flood; riêng `PlayerLeft` dùng `force: true` vì không được phép mất.

### Một tick (`Room.Tick`, dòng 166)

**Lý thuyết:** [L28 Tick](00-ly-thuyet.md#lt-tick) · [L29 Snapshot](00-ly-thuyet.md#lt-snapshot) · [L31 Lag compensation](00-ly-thuyet.md#lt-lag-comp)

```text
DrainInbox          rút hộp thư: INPUT vào hàng chờ, người rời, DEMO_CONFIG
CheckUdpTimeouts    ai im lặng UDP > 5 s → xử thua
ApplyInputs         mỗi người: tối đa 4 input, theo thứ tự seq, mỗi input một lần Simulation.Step
ResolvePlayers      tách hai người nếu chồng lên nhau
ResolveRareClaims   phân xử món hiếm (lag comp)
tick++, ticksLeft--
tick % 3 != 0  →    gửi SNAPSHOT (2 trên 3 tick = 20 lần/giây)
ticksLeft <= 0 →    EndMatch
```

### Hẹn giờ không trôi (dòng 123-137)

**Lý thuyết:** [L28 Tick](00-ly-thuyet.md#lt-tick)

```csharp
next += interval;                   // mốc tick sau tính từ Stopwatch
Tick(clock.Elapsed);
var wait = next - clock.Elapsed;
if (wait > TimeSpan.Zero) await Task.Delay(wait, ...);
else if (-wait > interval * MaxLagTicks) next = clock.Elapsed;  // chậm quá 5 tick thì bỏ phần trễ
```

> Em không `Sleep(33)` mỗi vòng, vì mỗi lần ngủ lệch vài ms sẽ cộng dồn. Em tính mốc tick tiếp theo bằng
> `Stopwatch` rồi chỉ chờ phần còn thiếu.

### Áp INPUT (dòng 288-318)

**Lý thuyết:** [L13 Số thứ tự, bù mất gói](00-ly-thuyet.md#lt-seq) · [L30 Prediction, reconcile](00-ly-thuyet.md#lt-prediction) · [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate)

- Mỗi gói INPUT mang input mới nhất **và tối đa 3 input cũ**. Mất một gói thì gói sau vẫn mang bước đi đó.
- `QueueInputs`: `seq <= lastProcessedSeq` thì bỏ (trùng hoặc tới muộn). Hàng chờ tối đa 32.
- `ApplyInputs`: mỗi tick tối đa 4 input/người. Gửi dồn 60 input/giây cũng không chạy nhanh hơn được.
- Không có input mới → đứng yên, server không đoán thay client.
- `SNAPSHOT` trả về `lastProcessedSeq` để client (phần của #2) biết bỏ input nào khi reconcile.

Test: `MatchTests.Inputs_are_applied_once_in_seq_order_and_reported_in_snapshot` (mất 1 gói, 1 gói trùng, vẫn đủ
10 input đúng thứ tự).

### J/K chỉ tính lúc vừa nhấn (dòng 324-345)

**Lý thuyết:** [L32 Bitmask, vừa nhấn](00-ly-thuyet.md#lt-bitmask)

```csharp
byte pressed = (byte)(e.Keys & ~p.PrevKeys);   // bit đang 1 mà lần trước là 0
```

> Giữ phím J nhiều tick thì mọi input đều có bit J, nhưng chỉ input đầu tiên mới tạo hành động nhặt.

---

## Bước 7. Hết giờ

**Lý thuyết:** [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state) · [L37 SQLite](00-ly-thuyet.md#lt-sqlite) · [L21 Thread, async/await](00-ly-thuyet.md#lt-async)

`Room.cs:415-444`

```csharp
p.Session.LeaveMatch(this);           // về LoggedIn TRƯỚC khi gửi
if (!p.Left) p.Session.Send(end);     // MATCH_END qua TCP
_ = Task.Run(() => _ctx.Users.SaveMatchResult(record));   // ghi SQLite ở Task nền
```

> Đổi trạng thái trước khi gửi `MATCH_END`, để client nhận xong bấm tìm trận ngay vẫn hợp lệ. Ghi SQLite ở Task nền
> để vòng tick không phải chờ đĩa và chờ lock DB, vì đăng nhập cũng dùng lock đó. Ghi lỗi thì chỉ mất một dòng lịch
> sử, người chơi vẫn biết kết quả.

---

## Bước 8. An thoát, hoặc chết giữa chừng

**Lý thuyết:** [L8 FIN, RST, đóng kết nối](00-ly-thuyet.md#lt-fin-rst) · [L9 Timeout, PING](00-ly-thuyet.md#lt-timeout) · [L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu)

`ClientSession.cs:57-88` (`RunAsync`), `ClientSession.cs:356-372` (`Cleanup`)

Mọi kiểu kết thúc đều đi vào cùng một chỗ dọn dẹp:

| Kiểu | Server nhận biết |
| --- | --- |
| Đóng bình thường / tiến trình bị kill | `ReadAsync` trả 0 (FIN) → `peer closed` |
| Mạng reset | `IOException` / `SocketException` → `connection reset` |
| Im lặng (treo, rút dây) | 15 s không có frame → `idle timeout` (dòng 139-149) |
| Trong trận mất UDP | 5 s không có gói UDP → phòng xử thua (`Room.cs:372`) |
| Gửi rác | độ dài sai → đóng ngay; nội dung sai 3 lần → đóng |

`Cleanup` làm 3 việc: đang chờ ghép thì gỡ khỏi hàng; đang trong trận thì `Post(PlayerLeft, force: true)`; gỡ khỏi
`SessionRegistry`. Sau đó cho vòng gửi tối đa 1 s gửi nốt frame cuối, rồi đóng "êm".

### Đóng êm (`GracefulCloseAsync`, dòng 333-352)

**Lý thuyết:** [L8 FIN, RST, đóng kết nối](00-ly-thuyet.md#lt-fin-rst)

> Lỗi này em chỉ thấy khi chạy thật trên Windows: server gửi `ERROR` rồi đóng socket ngay, trong khi bộ đệm nhận
> còn byte chưa đọc, hệ điều hành gửi RST thay vì FIN, và client mất luôn frame `ERROR`. Cách sửa: `Shutdown(Send)`
> để gửi FIN, đọc bỏ phần client còn gửi tối đa 500 ms / 64 KB, rồi mới đóng.

### Gửi qua hàng đợi (`Send`, dòng 99-104; `SendLoopAsync`, dòng 316-328)

**Lý thuyết:** [L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu) · [L26 Hàng đợi có giới hạn](00-ly-thuyet.md#lt-backpressure)

> Nhiều luồng cùng gửi cho một client: vòng đọc của chính nó, Matchmaker gửi `MATCH_FOUND`, phòng gửi `MATCH_END`.
> Nếu cả ba cùng ghi socket thì byte của hai frame có thể xen nhau. Nên mọi frame vào một `Channel`, chỉ một vòng
> gửi ghi ra socket. Hàng đợi đầy 256 frame nghĩa là client không đọc kịp: em đóng phiên đó, không để bộ nhớ server
> phình vì một client chậm.
