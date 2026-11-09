# 04 - Sửa tại chỗ (ôn C3 Live change)

> Chưa nắm khái niệm nào thì mở [00-ly-thuyet.md](00-ly-thuyet.md). Dòng **Lý thuyết** dưới mỗi mục chỉ tới đúng phần cần đọc trước.

Đề gợi ý đúng 2 bài: **thêm một lệnh mới vào giao thức** và **đổi kích thước message tối đa**. Phải tự tay làm
thật ít nhất một lần trước 10/11. Làm xong trên máy tập thì bỏ thay đổi:

```powershell
git checkout .
git status   # phải sạch
```

Lệnh chạy test (ở gốc repo classroom):

```powershell
dotnet test T47.sln                                        # toàn bộ, khoảng 1 phút
dotnet test shared/T47.Shared.Tests                        # chỉ giao thức
dotnet test server/tests/T47.Server.Tests --filter Session # chỉ test phiên
```

Khi làm trước mặt thầy: **nói trước mình sẽ sửa những chỗ nào**, rồi mới gõ. Danh sách chỗ phải sửa đã ghi sẵn ở
`docs/protocol-design.md` §9, mở ra đọc cũng được.

---

## Bài 1. Đổi kích thước frame tối đa 4096 → 8192 (~2 phút)

**Lý thuyết:** [L5 Tách frame TCP](00-ly-thuyet.md#lt-framing) · [L15 Byte, hex](00-ly-thuyet.md#lt-byte) · [L16 Little-endian](00-ly-thuyet.md#lt-endian) · [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate) · [L12 MTU](00-ly-thuyet.md#lt-mtu) · [L38 Các loại test](00-ly-thuyet.md#lt-test)

**Nói trước:**

> Giới hạn nằm ở một hằng số dùng chung cho client và server, nên em chỉ sửa một chỗ, rồi cập nhật tài liệu.

**Sửa** `shared/T47.Shared/Protocol/ProtocolConstants.cs:8`:

```csharp
public const int MaxFrameLength = 8192;
```

**Chạy:**

```powershell
dotnet test shared/T47.Shared.Tests --filter FrameReader
```

**Sẽ có 1 test hỏng, đây là chỗ ghi điểm.** Đã thử thật ngày 09/11 trên bản sao: `MaxLength_IsAccepted` qua (nó dùng
chính hằng số), nhưng `InvalidLength_ReturnsTooLarge_WithoutAllocating(header: [1, 16, 0, 0])` hỏng: mong `TooLarge`,
nhận `Closed`. Lý do: test ghi cứng 4 byte `01 10 00 00` là số 4097 (little-endian: 0x1001). Giờ 4097 đã hợp lệ, nên
reader chờ đọc tiếp 4097 byte, luồng hết, trả `Closed`.

> Test này ghi cứng giá trị cũ ngay trên giới hạn. Em sửa thành 8193, tức 0x2001, viết little-endian là `01 20 00 00`.

Sửa `shared/T47.Shared.Tests/FrameReaderTests.cs:43`:

```csharp
[InlineData(new byte[] { 0x01, 0x20, 0x00, 0x00 })] // 8193
```

Chạy lại: 9/9 test `FrameReader` qua, test phiên phía server (`--filter Session`) 10/10 qua (đã thử thật).

**Nói thêm (để được điểm trọn):**

- `FrameReader` kiểm `length > MaxFrameLength` (dòng 52), `MessageCodec.EncodeFrame` cũng kiểm khi gửi (dòng 20).
  Hai phía dùng chung hằng nên luôn khớp.
- Unity dùng DLL build từ `shared/`, nên phải chạy lại `.\client\unity\Tools\Build-Shared.ps1`. Nếu quên, client cũ
  vẫn từ chối gửi frame > 4096, nhưng không sai giao thức vì hiện chưa message nào lớn tới mức đó (lớn nhất là
  `MATCH_FOUND` khoảng 100 byte).
- Sửa bảng §7 của `protocol-design.md`.
- Đánh đổi: giới hạn càng lớn thì một kết nối rác bắt server cấp phát càng nhiều bộ nhớ mỗi frame.

**Nếu thầy bảo đổi giới hạn UDP 1200:** `MaxDatagramSize` ở dòng 15. Nói thêm: tăng quá khoảng 1400 thì gói vượt
MTU, bị phân mảnh IP, mất một mảnh là mất cả gói.

---

## Bài 2. Thêm lệnh mới `GET_ONLINE` → `ONLINE_COUNT` (~10 phút)

**Lý thuyết:** [L17 Nhị phân, codec](00-ly-thuyet.md#lt-codec) · [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state) · [L20 Mã lỗi](00-ly-thuyet.md#lt-status) · [L23 Race condition, lock](00-ly-thuyet.md#lt-race) · [L38 Các loại test](00-ly-thuyet.md#lt-test)

Lệnh: client hỏi "server đang có bao nhiêu kết nối", server trả một số `u16`. Đơn giản, đi qua đủ mọi tầng.

| Type | Message | Hướng | Payload |
| :-: | --- | :-: | --- |
| `0x0F` | `GET_ONLINE` | C → S | (trống) |
| `0x10` | `ONLINE_COUNT` | S → C | `u16 count` |

**Nói trước:**

> Thêm lệnh TCP mới em sửa 5 chỗ: enum type, lớp message, một dòng trong bộ giải mã, nhánh xử lý ở server, và test.
> Số type em chọn 0x0F, 0x10 vì TCP dùng khoảng 0x01 đến 0x3F và 0x0E là số cuối đang dùng. Lệnh này cần đăng nhập,
> theo quy tắc mọi lệnh trừ REGISTER, LOGIN, PING đều cần đăng nhập.

### Bước 1. `shared/T47.Shared/Protocol/MessageType.cs` (sau dòng `Error = 0x0E,`)

**Lý thuyết:** [L15 Byte, hex](00-ly-thuyet.md#lt-byte)

```csharp
GetOnline = 0x0F,
OnlineCount = 0x10,
```

### Bước 2. `shared/T47.Shared/Protocol/TcpMessages.cs` (thêm cuối phần TCP, cạnh `Pong`)

**Lý thuyết:** [L17 Nhị phân, codec](00-ly-thuyet.md#lt-codec)

```csharp
public sealed class GetOnline : Message
{
    public override MessageType Type => MessageType.GetOnline;
    internal override void Write(PacketWriter w) { }
}

public sealed class OnlineCount : Message
{
    public ushort Count { get; set; }
    public override MessageType Type => MessageType.OnlineCount;
    internal override void Write(PacketWriter w) => w.U16(Count);
    internal static OnlineCount Read(ref PacketReader r) => new OnlineCount { Count = r.U16() };
}
```

> `Write` ghi payload, `Read` đọc lại theo đúng thứ tự đó. Lệnh trống thì không cần `Read`, giống `FindMatch`.

### Bước 3. `shared/T47.Shared/Protocol/MessageCodec.cs`, hàm `ReadPayload` (dòng 78)

**Lý thuyết:** [L17 Nhị phân, codec](00-ly-thuyet.md#lt-codec) · [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate)

```csharp
case MessageType.GetOnline: return new GetOnline();
case MessageType.OnlineCount: return OnlineCount.Read(ref r);
```

> Thiếu dòng này thì server coi type 0x0F là type lạ và trả `BAD_FRAME`.

### Bước 4. `server/src/T47.Server/ClientSession.cs`, hàm `Dispatch` (dòng 172), thêm trước `default:`

**Lý thuyết:** [L19 Stateful, máy trạng thái](00-ly-thuyet.md#lt-state) · [L20 Mã lỗi](00-ly-thuyet.md#lt-status) · [L23 Race condition, lock](00-ly-thuyet.md#lt-race)

```csharp
case GetOnline:
    if (State == SessionState.Connected)
        Send(new Error { Code = StatusCode.NotLoggedIn, Detail = "GET_ONLINE cần đăng nhập" });
    else
        Send(new OnlineCount { Count = (ushort)Math.Min(_ctx.Registry.Count, ushort.MaxValue) });
    return false;
```

> `Registry.Count` đọc từ `ConcurrentDictionary`, an toàn khi nhiều luồng cùng đọc. `Math.Min` để không tràn `u16`.

### Bước 5. Test

**Lý thuyết:** [L38 Các loại test](00-ly-thuyet.md#lt-test)

`shared/T47.Shared.Tests/MessageCodecTests.cs`, thêm vào danh sách `TcpSamples()` (cạnh `new Pong {...}`):

```csharp
new GetOnline(),
new OnlineCount { Count = 3 },
```

Test có sẵn sẽ mã hoá rồi giải mã lại từng mẫu. Thêm một test chạy qua socket thật vào
`server/tests/T47.Server.Tests/SessionTests.cs`:

```csharp
[Fact]
public async Task Get_online_returns_session_count()
{
    await using var server = TestServer.Start();
    await using var a = await TestClient.ConnectAsync(server);
    await using var b = await TestClient.ConnectAsync(server);

    await a.SendAsync(new GetOnline());
    Assert.Equal(StatusCode.NotLoggedIn, (await a.ReceiveAsync<Error>()).Code);

    await a.RegisterAndLoginAsync("online_a");
    await a.SendAsync(new GetOnline());
    Assert.Equal(2, (await a.ReceiveAsync<OnlineCount>()).Count);
}
```

```powershell
dotnet test shared/T47.Shared.Tests --filter MessageCodec
dotnet test server/tests/T47.Server.Tests --filter Get_online
```

Đã thử thật ngày 09/11 trên bản sao repo: `MessageCodec` 75/75 qua (gồm cả các test cắt cụt và thừa byte chạy trên 2
mẫu mới), `Session` + `Get_online` 10/10 qua.

> Nếu lúc làm trước thầy test đếm 2 bị lệch: vòng accept thêm phiên vào registry ngay khi nhận kết nối, nhưng có thể
> chậm hơn lúc `ConnectAsync` phía test trả về. Đổi thành `Assert.True(count >= 1)` và giải thích như vậy.

### Bước 6. Tài liệu

`docs/protocol-design.md`: thêm 2 dòng vào bảng §4.1; thêm `GET_ONLINE` vào danh sách lệnh cần đăng nhập ở §7.

**Hết thời gian?** Đề cho phép "nói cách làm". Khi đó mở `protocol-design.md` §9, đọc 6 bước, chỉ vào từng file.

---

## Bài 3. Các đổi nhỏ khác có thể bị yêu cầu

**Lý thuyết:** [L9 Timeout, PING](00-ly-thuyet.md#lt-timeout) · [L28 Tick](00-ly-thuyet.md#lt-tick) · [L35 Docker](00-ly-thuyet.md#lt-docker)

| Yêu cầu | Sửa ở đâu | Kiểm thế nào | Chú ý |
| --- | --- | --- | --- |
| Đổi thời gian trận | `.env`: `MATCH_SECONDS=60` | `docker compose up -d --force-recreate`, vào trận, xem đồng hồ | Không cần build lại, chứng minh cấu hình qua `.env` |
| Đổi idle timeout 15 s | `ServerConfig.cs:16` `IdleTimeout` | `--filter Silent_client` | Không có biến môi trường cho mục này; client PING mỗi 5 s nên không được để dưới 5 s |
| Đổi thời gian rớt UDP 5 s | `ServerConfig.cs:17` `UdpSilenceTimeout` | `--filter Player_without_udp` | Quá ngắn thì mạng chập chờn là bị xử thua |
| Đổi số lỗi frame cho phép 3 | `ProtocolConstants.cs:9` `MaxBadFrames` | `--filter Third_bad_frame` | Test gửi 3 frame lỗi rồi chờ bị đóng, nên có thể phải sửa test theo (chưa thử) |
| Đổi số input/tick 4 | `Room.cs:21` `MaxInputsPerTick` | `--filter Inputs_are_applied` | Đặt 1 thì mạng giật làm input dồn, nhân vật chậm dần |
| Đổi tick rate | `.env`: `TICK_RATE` | | Client đang cố định 30, xem `03-hoi-dap.md` E1. Nói rõ giới hạn này thay vì đổi |

---

## Bài 4. Tìm lỗi trong code của mình

**Lý thuyết:** [L38 Các loại test](00-ly-thuyet.md#lt-test) · [L23 Race condition, lock](00-ly-thuyet.md#lt-race) · [L18 Kiểm dữ liệu đầu vào](00-ly-thuyet.md#lt-validate) · [L5 Tách frame TCP](00-ly-thuyet.md#lt-framing) · [L13 Số thứ tự, bù mất gói](00-ly-thuyet.md#lt-seq) · [L8 FIN, RST, đóng kết nối](00-ly-thuyet.md#lt-fin-rst)

Thầy có thể cố ý gài lỗi rồi bảo tìm. Cách làm: chạy test → đọc test nào hỏng → mở hàm mà test đó kiểm.

| Nếu ai đó sửa thành | Hậu quả | Test bắt được |
| --- | --- | --- |
| `Matchmaker`: tách kiểm `Count >= 2` và lấy người ra thành 2 lần lock | Một người vào 2 phòng | `Concurrent_find_match_pairs_everyone_exactly_once` |
| `FrameReader`: bỏ kiểm `length > MaxFrameLength` | `FF FF FF FF` làm server cấp phát 4 GB | `InvalidLength_ReturnsTooLarge_WithoutAllocating`, `Oversized_frame_closes_only_that_connection` |
| `FrameReader`: gọi `ReadAsync` 1 lần thay vì `ReadExactAsync` | Frame bị chia nhỏ thì đọc sai | `SplitAndMergedFrames_AreSeparatedCorrectly`, `Frame_split_across_many_writes_is_reassembled` |
| `Room.QueueInputs`: bỏ `if (e.Seq <= p.LastProcessedSeq) continue;` | Input gửi kèm bị áp 2 lần, nhân vật đi nhanh gấp đôi | `Inputs_are_applied_once_in_seq_order_and_reported_in_snapshot` |
| `ClientSession.Cleanup`: bỏ `room?.Post(PlayerLeft...)` | Người còn lại kẹt trong trận tới hết giờ | `Disconnect_mid_match_gives_the_win_to_the_opponent` |
| `MessageCodec.TryDecodeTcp`: bỏ `r.Remaining != 0` | Frame thừa byte vẫn được chấp nhận | `Tcp_TrailingByte_ReturnsFalse` |

Nói khi tìm ra:

> Test này kiểm [điều gì]. Nó hỏng ở [dòng assert]. Hàm liên quan là [hàm], dòng [x] đã bị đổi thành [y], nên
> [hậu quả]. Sửa lại là [z], chạy lại test thì qua.
