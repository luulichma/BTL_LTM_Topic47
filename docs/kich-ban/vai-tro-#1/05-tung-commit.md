# 05 - Thầy hỏi từng commit (ôn C1 Ownership)

> Chưa nắm khái niệm nào thì mở [00-ly-thuyet.md](00-ly-thuyet.md). Dòng **Lý thuyết** dưới mỗi mục chỉ tới đúng
> phần cần đọc trước.

Thầy có thể mở `git log` của repo classroom, chỉ vào một commit mang tên em và hỏi: *"commit này làm gì? mở ra giải
thích xem"*. README mục Contribution có liệt kê hash, nên khả năng thầy chọn các commit đó là cao nhất.

Số liệu: trên `main` có **46 commit thường + 23 commit merge** mang email của em (44 commit tên `chiennt`, 2 commit
tên `luulichma`: `3255593` và `cee52ca`). Dữ liệu lấy bằng lệnh:

```powershell
git log main --no-merges --author="chiennguyenthe659" --reverse --format="%h %ad %s" --date=short
```

**Lý thuyết:** [L38 Các loại test](00-ly-thuyet.md#lt-test)

---

## A. Cách trả lời một câu "commit này làm gì"

### Tự mở commit trước mặt thầy

```powershell
git show --stat 9d30a0d            # commit sửa những file nào, bao nhiêu dòng
git show 9d30a0d -- server/src/T47.Server/Matchmaker.cs   # xem diff đúng một file
git log --oneline -- server/src/T47.Server/Room.cs        # file này qua những commit nào
```

Trong VS Code: mở file, bật **Timeline** (góc dưới thanh Explorer) hoặc GitLens để thấy commit nào sửa dòng nào.

### Khuôn trả lời 4 ý (nói trong khoảng 30 giây)

1. **Mục tiêu:** commit thuộc task nào (mã task S1, S2, G3, ...) và giải quyết vấn đề gì.
2. **File chính:** 1-3 file quan trọng nhất, không cần đọc hết danh sách.
3. **Một điểm kỹ thuật đáng nói:** quyết định khó nhất trong commit đó, có đánh đổi.
4. **Kiểm chứng:** test nào chứng minh nó chạy, hoặc em đã chạy thử thế nào.

Ví dụ với `9d30a0d`:

> Commit này là server đầu tiên, gồm task S1, R1, S3 đến S6. File chính là `ClientSession`, `Room` và `UdpGateway`.
> Điểm quan trọng nhất là chỉ vòng tick của phòng được sửa state, luồng khác phải bỏ tin vào hộp thư. Commit ngay
> sau đó, `d0afc1f`, thêm 18 test tích hợp chạy socket thật để kiểm chính commit này.

### Bị hỏi commit không nhớ

Đừng đoán. Nói: *"Em mở diff ra xem ạ"*, chạy `git show --stat <hash>`, đọc dòng tiêu đề và thân commit message
(commit của em phần lớn có thân message liệt kê việc đã làm), rồi trả lời theo khuôn trên.

---

## B. Bản đồ toàn bộ commit

Ký hiệu: ★ commit code chính (chắc chắn bị hỏi), ◆ commit UI client, ○ tài liệu, ⚠ commit cần chuẩn bị câu trả lời
riêng (xem mục E).

| Ngày | Hash | Loại | Việc chính | Chi tiết |
| --- | --- | :-: | --- | --- |
| 20/10 | `3255593` | ⚠ | Nhập template starter của môn | [E1](#e1) |
| 28/10 | `cee52ca` | ⚠ | Sửa README (tên tác giả `luulichma`) | [E2](#e2) |
| 02/11 | `59b9561`, `18f8a56` | ○ | README: thông tin nhóm, đề tài | [D](#d) |
| 02/11 | `1aeae2e` | ⚠ | Đăng bù tài liệu giao thức và kiến trúc | [E3](#e3) |
| 02/11 | `0801d39` | ○ | Đánh dấu bản nháp để #2, #3 viết lại | [D](#d) |
| 02/11 | `53af939` | ★ | T1: khung repo .NET | [C1](#c1) |
| 02/11 | `1caf7f1`, `56ee07f` | ○ | AI log T1; thống nhất giao diện `Simulation` | [D](#d) |
| 04/11 | `b9129b7` | ★ | S2: thư viện encode/decode giao thức | [C2](#c2) |
| 04/11 | `9d30a0d` | ★ | S1, R1, S3-S6: server | [C3](#c3) |
| 04/11 | `d0afc1f` | ★ | 18 test tích hợp socket thật | [C4](#c4) |
| 04/11 | `cdf426b`, `f941bea` | ○ | Cập nhật tài liệu, ghi mốc test | [D](#d) |
| 06/11 | `8490442` | ★ | G3: luật nhặt/thả/giao ở server | [C5](#c5) |
| 07/11 | `569ad29` | ★ | S7, R5: relay và `DEMO_CONFIG` | [C6](#c6) |
| 07/11 | `37faa1e` | ★ | R2: bot headless | [C7](#c7) |
| 07/11 | `5fa8e52` | ★ | S8: lưu kết quả trận; R3 bằng chứng | [C8](#c8) |
| 07/11 | `657cd89`, `032b5d1` | ⚠ | Cắt phạm vi rồi khôi phục lại | [E4](#e4) |
| 07/11 | `c6e66c0` | ★ | G4, G5: món hiếm, lag compensation | [C9](#c9) |
| 07/11 | `44c9ff4` | ⚠ | C7, C8: làm mượt reconcile, camera, HUD | [E5](#e5) |
| 07/11 | `98ff410` | ⚠ | `fix_docs`: thêm Radmin, **xoá thư mục bằng chứng R4** | [E6](#e6) |
| 08/11 | `2615e9c`, `dac2fb4`, `8fc68e6`, `d229f7c`, `106edd0` | ○ | Bằng chứng test, README, kiến trúc, AI log | [D](#d) |
| 08/11 | `3d10e6c` → `760f96c`, `e0da3b4` | ◆ | UI Unity (8 commit) | [C10](#c10) |
| 08/11-09/11 | `b87c4ab`, `fb76ba0`, `241b5db`, `cf1a356`, `ecc9314`, `08be64b`, `1f0b909`, `56b287a` | ○ | Sửa định dạng, Contribution, Radmin, bỏ giọng văn AI | [D](#d) |
| 02/11-09/11 | 23 commit `Merge ...` | - | Gộp nhánh của cả nhóm vào `main` | [F](#f) |

---

## C. Các commit code chính

<a id="c1"></a>

### C1. `53af939` chore: khung repo .NET (T1)

**Lý thuyết:** [L1 Client, server, giao thức](00-ly-thuyet.md#lt-client-server) · [L35 Docker](00-ly-thuyet.md#lt-docker)

**Làm gì:** tạo `T47.sln` gồm 4 project: `shared/T47.Shared` (thư viện dùng chung), `shared/T47.Shared.Tests`
(xUnit), `server/src/T47.Server`, `client/bot/T47.Bot`. Thêm `ProtocolConstants.cs` và chỗ trống `Simulation/` cho #2.

**Nói:**

> Commit này dựng khung để cả nhóm làm song song. `T47.Shared` để `netstandard2.1` và C# 9 vì Unity chỉ đọc được
> mức đó, nên cùng một code giao thức chạy được ở cả server .NET 8 lẫn client Unity. Server và bot đọc cấu hình từ
> biến môi trường ngay từ đầu, không ghi cứng cổng.

**Hay bị hỏi:** *"Dùng chung `shared/` có vi phạm client, server độc lập không?"* Xem [03 F3](03-hoi-dap.md).

---

<a id="c2"></a>

### C2. `b9129b7` feat: thư viện encode/decode giao thức (S2)

**Lý thuyết:** [L5 Tách frame TCP](00-ly-thuyet.md#lt-framing) · [L15 Byte, hex](00-ly-thuyet.md#lt-byte) ·
[L16 Little-endian](00-ly-thuyet.md#lt-endian) · [L17 Nhị phân, codec](00-ly-thuyet.md#lt-codec) ·
[L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate)

**Làm gì:** toàn bộ thư mục `shared/T47.Shared/Protocol/`: `MessageType`, `Codes`, `PacketWriter`/`PacketReader`,
các lớp message TCP và UDP, `MessageCodec`, `FrameReader`. Kèm **79 unit test**. Cập nhật `protocol-design.md`
(§3.3 đánh số type, §7 giới hạn, §9 các bước thêm message).

**File để mở:** `FrameReader.cs` (tách frame), `MessageCodec.cs` (chọn lớp theo type), `PacketReader.cs`.

**Nói:**

> Đây là giao thức T47GP/1 viết thành code. Mỗi message là một lớp có `Write` để ghi payload và `Read` để đọc lại
> theo đúng thứ tự. `PacketReader` không ném exception khi thiếu byte mà bật cờ `Failed`, vì dữ liệu mạng sai là
> chuyện bình thường, không phải lỗi bất ngờ; dùng exception cho việc đó vừa chậm vừa dễ quên bắt.

**Hay bị hỏi:**

- *79 test kiểm gì?* Mã hoá rồi giải mã lại mọi message phải ra như cũ; cắt cụt 1 byte phải bị từ chối; thừa 1 byte
  cũng bị từ chối; `FrameReader` nhận luồng bị cắt thành mẩu 1, 2, 3 byte vẫn tách đúng.
- *Vì sao thừa byte cũng từ chối?* Thừa byte nghĩa là hai bên hiểu khác nhau về cấu trúc message; chấp nhận thì lỗi
  bị giấu đi.
- Chi tiết từng byte: [02 Bước 2](02-di-theo-mot-goi-tin.md).

---

<a id="c3"></a>

### C3. `9d30a0d` feat: server TCP/UDP, đăng nhập SQLite, ghép trận, tick + SNAPSHOT (S1, R1, S3-S6)

**Lý thuyết:** [L21 Thread, async/await](00-ly-thuyet.md#lt-async) · [L23 Race condition, lock](00-ly-thuyet.md#lt-race) ·
[L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu) · [L28 Tick](00-ly-thuyet.md#lt-tick) ·
[L33 Băm mật khẩu](00-ly-thuyet.md#lt-pbkdf2) · [L8 FIN, RST](00-ly-thuyet.md#lt-fin-rst) ·
[L35 Docker](00-ly-thuyet.md#lt-docker)

**Làm gì:** commit lớn nhất (18 file, ~1.400 dòng). Commit message đã chia sẵn theo task, nói theo đúng thứ tự đó:

| Task | Nội dung | File |
| --- | --- | --- |
| S1 | Nhận kết nối, vòng đọc/gửi async, idle timeout 15 s, xử lý frame hỏng, đóng êm | `ServerApp.cs`, `ClientSession.cs` |
| R1 | Dockerfile 2 tầng, compose, volume `./data`, biến `.env` | `server/Dockerfile`, `docker-compose.yml`, `.env.example` |
| S3 | Đăng ký/đăng nhập SQLite, PBKDF2 ngoài lock, chặn đăng nhập 2 nơi | `UserStore.cs`, `SessionRegistry.cs` |
| S4 | Ghép 2 người trong cùng một lock, tạo phòng | `Matchmaker.cs`, `RoomManager.cs` |
| S5/S6 | Vòng tick 30 Hz, hộp thư, INPUT theo seq, UDP token, SNAPSHOT 20 Hz, kết thúc trận | `Room.cs`, `UdpGateway.cs` |

**Nói:** dùng lại câu phải thuộc số 1 trong [README](README.md), rồi kể theo [02 Bước 1-8](02-di-theo-mot-goi-tin.md).

**Hay bị hỏi:** *"Commit to vậy sao không chia nhỏ?"*

> Dạ, đúng ra nên chia theo từng task. Lúc đó các phần phụ thuộc nhau: ghép trận cần đăng nhập, phòng cần ghép trận,
> nên em làm chung trên nhánh `feature/s1-s6-server` rồi commit một lần. Bù lại, commit message ghi rõ từng task, và
> commit test ngay sau đó kiểm từng phần.

(Câu này chỉ nói nếu đúng với cách em đã làm. Nhánh `feature/s1-s6-server` có thật, merge ở `f917f83`.)

---

<a id="c4"></a>

### C4. `d0afc1f` test: 18 test tích hợp server với socket thật

**Lý thuyết:** [L38 Các loại test](00-ly-thuyet.md#lt-test) · [L13 Số thứ tự, bù mất gói](00-ly-thuyet.md#lt-seq) ·
[L23 Race condition, lock](00-ly-thuyet.md#lt-race)

**Làm gì:** project `server/tests/T47.Server.Tests`: `TestHelpers.cs` (khởi động server thật trên `127.0.0.1`, cổng
ngẫu nhiên, SQLite tạm), `SessionTests.cs`, `MatchTests.cs`.

**Nói:**

> Đây không phải unit test gọi hàm trực tiếp, mà mở server thật và nói chuyện qua TCP và UDP thật. Có test 30 client
> song song, 20 người ghép trận cùng lúc để bắt race ở Matchmaker, INPUT bị trùng và mất gói nhưng vị trí vẫn khớp
> từng bit với `Simulation.Step`, UDP rác, rớt mạng giữa trận, hết giờ.

**Hay bị hỏi:**

- *Cổng ngẫu nhiên để làm gì?* Test đặt cổng 0 nghĩa là để hệ điều hành tự chọn một cổng trống
  (`TestHelpers.cs`). Để nhiều test chạy song song không giành cùng một cổng, và không đụng server đang chạy
  thật trên 5000.
- *"Khớp từng bit" nghĩa là sao?* Test tự chạy `Simulation.Step` với cùng chuỗi input rồi so với vị trí trong
  SNAPSHOT bằng phép so bằng tuyệt đối, không cho sai số. Được vì client và server dùng chung một hàm.

---

<a id="c5"></a>

### C5. `8490442` feat(g3): luật nhặt/thả/giao ở server, đồng bộ đơn hàng trong SNAPSHOT

**Lý thuyết:** [L27 Authoritative, relay](00-ly-thuyet.md#lt-authoritative) ·
[L32 Bitmask, vừa nhấn](00-ly-thuyet.md#lt-bitmask) · [L29 Snapshot](00-ly-thuyet.md#lt-snapshot) ·
[L26 Hàng đợi có giới hạn](00-ly-thuyet.md#lt-backpressure)

**Làm gì:** `MatchItems.cs` mới: server kiểm tra phím J/K (nhặt từ bàn hoặc đất, thả, giao ở quầy của mình; giao sai
món thì báo `WRONG_ORDER` và giữ món; đủ đơn 3 món được +5; tối đa 16 món nằm dưới đất). J/K chỉ tính lúc vừa nhấn.
SNAPSHOT thêm danh sách món và đơn hàng của từng quầy (10 byte mỗi quầy).

**Nói:**

> G3 là phần của #3, em làm phía server: client chỉ báo "em bấm J", còn có nhặt được hay không là server quyết. Giới
> hạn 16 món dưới đất để SNAPSHOT không phình: mỗi món 12 byte, tối đa 25 món, còn xa mức 91 món mà một gói UDP
> 1200 byte chứa được ([L12](00-ly-thuyet.md#lt-mtu)).

**Hay bị hỏi:** *Sửa giao thức thì phải làm gì thêm?* Commit này sửa `UdpMessages.cs` nên phải build lại
`T47.Shared.dll` cho Unity (có trong commit) và cập nhật `protocol-design.md` §4.2, §4.4, §7 (cũng có).

---

<a id="c6"></a>

### C6. `569ad29` feat(s7,r5): chế độ naive relay và `DEMO_CONFIG` ở server

**Lý thuyết:** [L27 Authoritative, relay](00-ly-thuyet.md#lt-authoritative) ·
[L25 Hàng đợi, hộp thư](00-ly-thuyet.md#lt-hop-thu) · [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate)

**Làm gì:** chế độ relay lấy vị trí từ `POS_REPORT` của client (không chạy `Step`, không va chạm, không kẹp biên
map). Chế độ authoritative bỏ qua `POS_REPORT`. `DEMO_CONFIG` đổi chế độ, được áp ở đầu tick qua hộp thư. Codec từ
chối `roomMode` lạ và toạ độ `NaN`/vô cực. `DemoConfigTests`.

**Nói:**

> Relay là bản đối chứng cố ý làm sai, để demo cho thấy tin client thì gian lận được. `DEMO_CONFIG` không sửa phòng
> trực tiếp mà đi qua hộp thư, để giữ nguyên tắc chỉ vòng tick đụng vào state. Relay em cố ý không kẹp biên map, đó là
> quyết định của em, để cảnh gian lận nhìn rõ.

**Hay bị hỏi:** *Vì sao phải chặn `NaN`?* Một toạ độ `NaN` lan sang mọi phép tính (so sánh với `NaN` luôn sai), làm
hỏng cả phòng. Kiểm ở codec thì không vào được tới phòng. Chỉ cho bật khi `DEMO_MODE=true` ([01 mục F](01-demo-va-mo-dau.md)).

---

<a id="c7"></a>

### C7. `37faa1e` feat(r2): bot headless để thử tải

**Lý thuyết:** [L21 Thread, async/await](00-ly-thuyet.md#lt-async) · [L38 Các loại test](00-ly-thuyet.md#lt-test) ·
[L35 Docker](00-ly-thuyet.md#lt-docker)

**Làm gì:** `client/bot/T47.Bot`: N bot chạy thành N `Task`, mỗi bot đăng ký, đăng nhập, tìm trận, `UDP_HELLO`, gửi
INPUT ngẫu nhiên 30 lần/giây, đo RTT bằng PING, in thống kê mỗi 5 s. Cấu hình qua `BOT_COUNT`, `BOT_DURATION`, ...
`client/Dockerfile` build bot. `testing-guide.md` §2.0b ghi kết quả chạy thật 20 và 50 bot.

**Nói:**

> Bot là client thật thứ hai, đi đúng giao thức như Unity nhưng không có giao diện, để chứng minh server chịu nhiều
> client cùng lúc. 50 bot là 25 phòng, server vẫn giữ 30 tick mỗi giây.

**Hay bị hỏi:** *Bot có phát hiện lỗi gì không?* Có, bot gửi thừa INPUT, chỉ lộ ra khi chạy trong Docker (xem
[03 C4](03-hoi-dap.md)).

---

<a id="c8"></a>

### C8. `5fa8e52` feat(s8): lưu kết quả trận vào SQLite; bằng chứng R3 trong Docker

**Lý thuyết:** [L37 SQLite](00-ly-thuyet.md#lt-sqlite) · [L21 Thread, async/await](00-ly-thuyet.md#lt-async) ·
[L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate) · [L8 FIN, RST](00-ly-thuyet.md#lt-fin-rst)

**Làm gì:** bảng `MatchResult`; phòng gửi `MATCH_END` trước rồi mới ghi DB ở Task nền. `MatchResultTests`.
`scripts/r3-abuse.ps1` gửi frame quá lớn, frame hỏng, byte ngẫu nhiên, UDP rác, client im lặng.
`testing-guide.md` §2.0c ghi kết quả chạy thật (`docker kill` giữa trận, script rác).

**Nói:**

> Ghi SQLite ở Task nền vì vòng tick không được chờ đĩa, và lock DB cũng là lock mà đăng nhập dùng. Script R3 là để
> chứng minh yêu cầu "server sống sót khi nhận dữ liệu hỏng" bằng chạy thật chứ không chỉ bằng test.

Chi tiết: [02 Bước 7](02-di-theo-mot-goi-tin.md), [01 mục E](01-demo-va-mo-dau.md).

---

<a id="c9"></a>

### C9. `c6e66c0` feat(g4,g5): món hiếm và lag compensation ở server

**Lý thuyết:** [L31 Lag compensation](00-ly-thuyet.md#lt-lag-comp) · [L28 Tick](00-ly-thuyet.md#lt-tick) ·
[L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate)

**Làm gì:** một món hiếm (id 6) ở kệ giữa: giao đúng quầy được +5, 5 s sau mọc lại. Lag comp tắt: input nào server xử
lý trước thắng, người kia nhận `ITEM_TAKEN`. Lag comp bật: lượt giành ghi theo `viewTick` (kẹp trong
`[tick-30, tick]`), chờ tối đa 12 tick, ai có `viewTick` sớm hơn thắng, người kia nhận `LOST_TO_OPPONENT`; log in
cách phân xử. `RareItemTests` (8 test), `LagCompTests` (2 client qua socket thật).

**Nói:** xem [03 E2](03-hoi-dap.md) và cách đọc log ở [01 mục D](01-demo-va-mo-dau.md). Quyết định của em: chỉ tua thời
điểm, không tua vị trí, vì kệ hiếm đứng yên.

---

<a id="c10"></a>

### C10. 8 commit UI Unity (`3d10e6c` → `760f96c`, `e0da3b4`)

**Lý thuyết:** [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state) · [L8 FIN, RST](00-ly-thuyet.md#lt-fin-rst)

Phần này ít liên quan lập trình mạng; thầy hỏi thì nói ngắn, rồi kéo về chỗ nó dính tới giao thức.

| Hash | Làm gì | Chỗ dính tới mạng (nên nói) |
| --- | --- | --- |
| `3d10e6c` | `UiScale` (co giãn giao diện từ khung 1280×720), `UiTheme`, `UiWidgets` | Không |
| `c56ebea` | Tách màn đăng ký thành hộp riêng; kiểm "nhập lại mật khẩu" ở client; đăng ký xong không tự đăng nhập | Kiểm ở client chỉ để tiện người dùng; server vẫn tự kiểm định dạng, vì không tin client ([L18](00-ly-thuyet.md#lt-validate)). Giao thức không đổi |
| `71a39ac` | Network Panel thành ngăn kéo bên phải, có tab, ẩn mặc định (F1) | Các chỉ số mạng và preset 1-6 vẫn giữ |
| `fd42d7f` | HUD trận và hộp kết quả co giãn | Hộp kết quả hiện khi nhận `MATCH_END` |
| `3112925` | Lobby có 2 trạng thái: rảnh / đang tìm | Trạng thái **chỉ** đổi khi nhận `QUEUE_STATUS`, `MATCH_FOUND`, `ERROR` từ server; nút bị khoá khi đang chờ trả lời, để không gửi `FIND_MATCH` hai lần |
| `79a42ec` | Thông báo preset; băng đỏ "mất kết nối" | Băng đỏ hiện khi kết nối TCP rớt ([L8](00-ly-thuyet.md#lt-fin-rst)) |
| `24ac1dc` | Áp `UiScale` và theme cho scene offline | Không |
| `760f96c` | Sửa câu ghi chú ở màn kết quả: "server lưu trận" | Đúng với S8: client không lưu gì |
| `e0da3b4` | Thu gọn HUD, chừa dải trên cùng cho HUD | Không |

**Nói:**

> Giao diện Lobby là một máy trạng thái nhỏ phía client, chỉ chuyển trạng thái theo tin của server, giống máy trạng
> thái phiên ở server. Nhờ vậy client không bao giờ tự cho mình là "đang tìm trận" khi server chưa xác nhận.

---

<a id="d"></a>

## D. Commit tài liệu

**Lý thuyết:** không cần. Ý chính khi bị hỏi: đề bắt buộc tài liệu khớp với code, nên mỗi lần code đổi em cập nhật
tài liệu, có lúc chung commit, có lúc commit riêng ngay sau.

| Hash | Làm gì, nói 1 câu |
| --- | --- |
| `59b9561`, `18f8a56` | Điền thông tin nhóm, đề tài, mô tả bài toán vào README |
| `0801d39` | Đánh dấu architecture §2.1 và protocol-design §8.7 là bản nháp, để #2 và #3 tự viết phần của họ |
| `1caf7f1` | Ghi AI log cho T1 |
| `56ee07f` | Thống nhất với #2 giao diện `Simulation`: `Step` chỉ lo di chuyển, va chạm hai người tách ra `ResolvePlayers` chỉ server gọi; đơn vị và hằng số chung |
| `cdf426b`, `f941bea` | Sau khi có server: cập nhật giao thức, kiến trúc, bằng chứng test; ghi lại kết quả chạy lại test trên `main` |
| `2615e9c` | Bằng chứng unit test G6/C7, chạy toàn bộ test, bảng chạy Unity |
| `dac2fb4`, `8fc68e6` | README liệt kê tính năng G6, C7, C8, G8; kiến trúc ghi trạng thái G5, G6, preset G8 |
| `d229f7c` | Chạy lại từ bản clone sạch với 20 bot trong Docker (đúng yêu cầu "chạy từ đầu sạch") |
| `106edd0` | Bản nháp AI log mục 28-29, để trống phần "giữ lại / kiểm chứng" cho từng người tự viết |
| `8634ee0` | Kiến trúc: mô tả ngăn kéo Network Panel, `UiScale` |
| `b87c4ab`, `fb76ba0`, `241b5db`, `cf1a356`, `ecc9314` | Sửa định dạng tài liệu, hoàn thiện AI log, gọn `testing-guide` |
| `08be64b`, `1f0b909` | Điền phần đóng góp của em trong Contribution; đổi cách demo 2 máy sang Radmin VPN |
| `56b287a` | Bỏ giọng văn AI và các ghi chú còn sót của template |

---

## E. Commit cần chuẩn bị câu trả lời riêng

Những commit dưới đây có điểm thầy dễ hỏi vặn. Phần **Sự thật** là thông tin đọc được từ git. Phần **Em cần tự
chốt** là chỗ chỉ em biết câu trả lời thật; đừng dùng lời giải thích nào không đúng với điều đã xảy ra.

<a id="e1"></a>

### E1. `3255593` chore: import network-programming-starter (31 file, 1.611 dòng)

**Sự thật:** commit đầu tiên, nhập nguyên template của môn (`INSTRUCTION.md`, `AGENTS.md`, khung `docs/`, Dockerfile
mẫu, ...). Không phải code của nhóm.

**Nói:**

> Đây là template thầy cung cấp, em nhập vào để làm khung. Phần code của nhóm bắt đầu từ `53af939`.

<a id="e2"></a>

### E2. `cee52ca` update_readme_luulichma (và `3255593`)

**Sự thật:** hai commit đầu (`3255593`, `cee52ca`) có tác giả hiển thị là `luulichma` nhưng email là email của em.

**Em cần tự chốt:** `luulichma` là tên cấu hình git cũ trên một máy của em, hay là ai khác dùng máy em? Nếu là em thì
nói: *"Đây là em, lúc đó máy em cấu hình `user.name` khác, email vẫn là của em."*

<a id="e3"></a>

### E3. `1aeae2e` docs: đăng bù tài liệu thiết kế giao thức và kiến trúc (625 dòng thêm)

**Sự thật:** commit message ghi rõ tài liệu đã soạn trước ở workspace riêng `BTL_LTM_Topic47`, nay đưa lên một lần;
và AI log có khai dùng Claude soạn bản nháp.

**Nói:**

> Nhóm em soạn thiết kế ở một repo riêng trước để không làm bẩn repo chấm điểm, rồi đưa lên một lần. Em ghi rõ trong
> commit message và AI log. Đây là thiết kế viết trước khi code, sau đó cập nhật dần theo code ở các commit `cdf426b`,
> `032b5d1`, ...

<a id="e4"></a>

### E4. `657cd89` cắt phạm vi → `032b5d1` khôi phục

**Sự thật:** `657cd89` đánh dấu cắt G4/G5, G6, C7/C8, T4, demo LAN vì sợ không kịp hạn 08/11. Cùng ngày, `c6e66c0`
làm xong G4/G5 ở server, và `032b5d1` gỡ các ghi chú "cắt", khôi phục nguyên văn phần của #3.

**Nói:**

> Em là trưởng nhóm nên phải chốt phạm vi trước hạn. Lúc đó em ghi rõ phần nào cắt để tài liệu không hứa thứ chưa
> làm. Sau đó các phần đó làm kịp, nên em khôi phục lại và ghi phần còn dang dở kèm người phụ trách trong
> `testing-guide`.

Ý để ghi điểm: tài liệu luôn nói đúng trạng thái thật của code, kể cả khi phải ghi "chưa làm".

<a id="e5"></a>

### E5. `44c9ff4` feat(c7,c8): làm mượt reconcile, camera, animation, HUD

**Sự thật:**

- Commit mang tên em, sửa code client netcode (`CorrectionSmoother.cs`, `NetClient.cs`) và gameplay Unity
  (`CameraFollow`, `PlayerAnimator`, `MatchHud`). Merge vào `main` ở `358faae`.
- README Contribution của em **không** liệt kê commit này, và ghi *"Netcode client do #2, hiển thị Unity G1-G8 do
  #3"*. Phần của #2 và #3 trong README cũng không liệt kê C7/C8.
- Nghĩa là hiện **không ai nhận** C7/C8 trong bảng Contribution, trong khi git ghi tên em.

**Em cần tự chốt** (trước 10/11, nếu được thì sửa README cho khớp):

- Nếu em làm: thêm `44c9ff4` vào cột commit của em trong README và học phần này (C7: sau mỗi lần reconcile giữ một
  độ lệch hiển thị rồi giảm dần 20% mỗi khung hình ở 60 fps; lệch quá 1 m thì nhảy thẳng; chỉ vị trí vẽ đổi, state
  logic và giao thức không đổi).
- Nếu em commit hộ bạn: nói đúng như vậy và nói rõ ai làm, và người đó nên ghi vào phần của họ.

Đừng để thầy tự phát hiện chỗ lệch này.

<a id="e6"></a>

### E6. `98ff410` fix_docs: thêm phần Radmin VPN, **xoá thư mục bằng chứng R4**

**Sự thật:**

- Message chỉ ghi *"bổ sung phần kết nối qua radmin vpn"* (thêm `architecture.md` §5.3, 40 dòng).
- Nhưng commit còn **xoá 14 file** trong `docs/asset/r4-2026-11-07/` (log, TRX của 5 lượt test, `runs.json`, ...,
  523 dòng). Thư mục này do #3 thêm ở `84f0846`.
- Hiện **vẫn còn 2 chỗ trỏ tới thư mục đã xoá**: `docs/architecture.md` dòng 595 và `docs/ai-log.md` mục 26. Thầy
  bấm vào sẽ thấy link hỏng, tức là bằng chứng test 7/9 không còn.

**Em cần tự chốt:** việc xoá là cố ý hay vô tình (ví dụ do `git add -A` sau khi dọn thư mục)?

- Vô tình: khôi phục trước khi nộp bằng
  `git checkout 98ff410~1 -- docs/asset/r4-2026-11-07` rồi commit (báo #3 biết vì đây là bằng chứng của bạn ấy).
- Cố ý: sửa 2 chỗ trỏ tới cho khớp, và chuẩn bị lý do.

Bài học nên nói nếu bị hỏi: *"Commit message của em không mô tả hết thay đổi; đúng ra phải xem `git status` trước
khi commit."*

### E7. Commit message đặt tên chưa chuẩn

`UPDATE_README`, `fix_docs`, `update_documents`, `update_testing-guide`, ... không theo quy ước `loại: mô tả` như các
commit code. Nếu bị nhắc:

> Dạ, các commit code em theo quy ước `feat:`, `test:`, `docs:` và có thân message liệt kê việc đã làm. Mấy commit sửa
> tài liệu cuối em đặt vội. Em nhận đây là điểm cần sửa.

---

<a id="f"></a>

## F. 23 commit merge

**Lý thuyết:** không cần.

**Sự thật:** em là người gộp nhánh của cả nhóm vào `main` (README ghi "merge nhánh vào `main`"). Mỗi task một nhánh:
`feature/s2-protocol`, `feature/c1-shared-simulation`, `feature/g7-network-simulator`, `feature/s1-s6-server`, ...

**Hay bị hỏi:**

- *Vì sao làm nhánh riêng?* Mỗi người làm phần mình không đụng `main`; `main` luôn build được và chạy được. Gộp
  xong em chạy lại toàn bộ test trên `main` (ví dụ `f941bea` ghi kết quả chạy lại sau merge).
- *Có bị xung đột không, xử lý thế nào?* **Em cần tự nhớ** một lần xung đột thật (file nào, giữ bên nào). Nếu không
  nhớ thì nói thật là không nhớ cụ thể, rồi giải thích cách làm chung: mở file xung đột, đọc hai phía, giữ phần đúng
  với thiết kế đã chốt, build và chạy test trước khi commit merge.
- *Merge commit của em có viết code không?* Bình thường là không, merge chỉ gộp. Nếu bị chỉ vào một merge có sửa tay
  để giải xung đột, chạy `git show <hash>` để xem phần sửa đó.

---

## G. Ôn nhanh trước giờ vấn đáp

Thuộc 10 hash trong README Contribution và mỗi hash một câu:

| Hash | Một câu |
| --- | --- |
| `53af939` | Khung repo: 4 project, `shared` chạy được cả .NET 8 lẫn Unity |
| `b9129b7` | Giao thức thành code: codec, `FrameReader`, 79 unit test |
| `9d30a0d` | Server: kết nối, đăng nhập, ghép trận, vòng tick, UDP, Docker |
| `d0afc1f` | 18 test tích hợp với socket thật |
| `8490442` | Luật nhặt/thả/giao do server quyết, J/K chỉ tính lúc vừa nhấn |
| `569ad29` | Relay cố ý tin client để đối chứng; `DEMO_CONFIG` qua hộp thư |
| `37faa1e` | Bot headless: 50 bot, 25 phòng, vẫn 30 tick/giây |
| `5fa8e52` | Lưu kết quả trận ở Task nền; script gửi rác R3 |
| `c6e66c0` | Món hiếm, lag comp theo `viewTick`, chỉ tua thời điểm |
| `3d10e6c`..`e0da3b4` | UI Unity; Lobby chỉ đổi trạng thái theo tin của server |
