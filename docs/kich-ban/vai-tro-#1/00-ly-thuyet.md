# 00 - Lý thuyết nền (đọc trước file 01-04)

Trang này giải thích từ đầu mọi khái niệm mà các file kịch bản dùng tới. Viết cho người chưa biết gì về lập trình
mạng. Mỗi mục có mã `L1`, `L2`, ... và các file 01-04 trích dẫn đúng mã đó, ví dụ "Lý thuyết: [L5](#lt-framing)".

Mỗi mục gồm 4 phần:

- **Hình dung**: so sánh với đời thường để nắm ý.
- **Chính xác**: định nghĩa đúng, dùng được khi thầy hỏi "định nghĩa là gì".
- **Trong T47**: khái niệm này nằm ở đâu trong code của nhóm.
- **Gặp ở**: file kịch bản nào dùng nó.

Cách đọc: đọc lần lượt phần 1 → 3 trước (mạng, TCP, UDP), vì phần sau dựa vào đó. Phần 6 (đồng thời) là phần khó
nhất và hay bị hỏi nhất, đọc 2 lần.

## Mục lục

| Phần | Mục |
| --- | --- |
| 1. Mạng cơ bản | [L1 Client, server, giao thức](#lt-client-server) · [L2 IP, cổng, socket](#lt-ip-port) · [L3 Các tầng mạng](#lt-tang) |
| 2. TCP | [L4 TCP](#lt-tcp) · [L5 Luồng byte và tách frame](#lt-framing) · [L6 Nghẽn đầu hàng](#lt-hol) · [L7 Nagle](#lt-nagle) · [L8 FIN, RST, đóng kết nối](#lt-fin-rst) · [L9 Timeout và PING](#lt-timeout) |
| 3. UDP | [L10 UDP](#lt-udp) · [L11 NAT và token](#lt-nat) · [L12 MTU](#lt-mtu) · [L13 Số thứ tự, bù mất gói](#lt-seq) · [L14 Vì sao bỏ gói rác im lặng](#lt-udp-im-lang) |
| 4. Mã hoá dữ liệu | [L15 Byte, hex, u8/u16/u32](#lt-byte) · [L16 Little-endian](#lt-endian) · [L17 Nhị phân, JSON, codec](#lt-codec) · [L18 Không tin dữ liệu đầu vào](#lt-validate) |
| 5. Giao thức ứng dụng | [L19 Stateful, máy trạng thái](#lt-state) · [L20 Mã lỗi](#lt-status) |
| 6. Đồng thời | [L21 Thread, async/await](#lt-async) · [L22 Các kiểu tổ chức server](#lt-mo-hinh-server) · [L23 Race condition, lock](#lt-race) · [L24 Deadlock](#lt-deadlock) · [L25 Hàng đợi, hộp thư](#lt-hop-thu) · [L26 Hàng đợi có giới hạn](#lt-backpressure) |
| 7. Game mạng | [L27 Authoritative và relay](#lt-authoritative) · [L28 Tick](#lt-tick) · [L29 Snapshot](#lt-snapshot) · [L30 Prediction, reconcile, nội suy](#lt-prediction) · [L31 Lag compensation](#lt-lag-comp) · [L32 Bitmask, "vừa nhấn"](#lt-bitmask) |
| 8. Bảo mật | [L33 Băm mật khẩu, salt, PBKDF2](#lt-pbkdf2) · [L34 Tấn công đo thời gian](#lt-timing) |
| 9. Triển khai | [L35 Docker](#lt-docker) · [L36 VPN, CGNAT, Radmin](#lt-vpn) · [L37 SQLite](#lt-sqlite) |
| 10. Kiểm thử | [L38 Các loại test](#lt-test) |

---

# Phần 1. Mạng cơ bản

<a id="lt-client-server"></a>

## L1. Client, server, giao thức

**Hình dung.** Quán ăn: khách (client) gọi món, bếp (server) nấu. Hai bên phải nói cùng một "ngôn ngữ gọi món":
gọi món thế nào, bếp trả lời "hết món" ra sao. Bộ quy tắc đó là **giao thức**.

**Chính xác.**

- **Server**: chương trình chạy liên tục, chờ người khác kết nối tới và phục vụ nhiều người cùng lúc.
- **Client**: chương trình chủ động kết nối tới server để yêu cầu việc gì đó.
- **Giao thức tầng ứng dụng**: quy định tin nhắn gồm những loại nào, mỗi loại có những trường gì, nằm ở byte nào,
  gửi theo thứ tự nào, lỗi thì trả gì. HTTP là một giao thức tầng ứng dụng có sẵn; T47GP/1 là giao thức nhóm tự
  thiết kế.

**Trong T47.** Client là game Unity (và bot không giao diện). Server là chương trình .NET trong Docker. Giao thức
T47GP/1 được đặc tả trong `docs/protocol-design.md`; code nằm ở `shared/T47.Shared/Protocol/`. Đề bắt buộc client
và server chỉ nói chuyện qua giao thức này.

**Gặp ở:** [01 mục B](01-demo-va-mo-dau.md), [03 F3](03-hoi-dap.md).

---

<a id="lt-ip-port"></a>

## L2. Địa chỉ IP, cổng (port), socket, `0.0.0.0`

**Hình dung.** IP là địa chỉ toà chung cư, cổng là số phòng. Thư tới đúng toà nhưng sai phòng thì không ai nhận.

**Chính xác.**

- **Địa chỉ IP**: số định danh một máy trên mạng, ví dụ `192.168.1.5`. `127.0.0.1` (localhost) là "chính máy này".
- **Cổng**: số 0-65535 chọn chương trình nào trên máy nhận dữ liệu. TCP và UDP có hai dải cổng riêng, nên TCP 5000
  và UDP 5001 không đụng nhau (thậm chí cùng số cũng được).
- **Socket**: "đầu cắm" mà chương trình mở ra để gửi/nhận qua mạng. Một kết nối TCP được xác định bởi 4 số:
  IP nguồn, cổng nguồn, IP đích, cổng đích.
- **Bind** (gắn): server nói với hệ điều hành "đưa cho tôi mọi dữ liệu tới cổng X trên địa chỉ Y".
  - Bind `127.0.0.1`: chỉ chương trình trên cùng máy kết nối được.
  - Bind `0.0.0.0`: nhận trên **mọi** card mạng (Wi-Fi, LAN, card ảo của Docker, card Radmin).
- **Listen / accept**: server TCP "lắng nghe" một cổng; mỗi lần có client gõ cửa thì `accept` tạo ra một socket mới
  riêng cho client đó.

**Trong T47.** Server bind `0.0.0.0`, TCP 5000, UDP 5001, đọc từ `.env`. Trong Docker bắt buộc `0.0.0.0`, vì với
container, `127.0.0.1` là chính container chứ không phải máy host.

**Gặp ở:** [01 mục A, B](01-demo-va-mo-dau.md), [02 Bước 1](02-di-theo-mot-goi-tin.md).

---

<a id="lt-tang"></a>

## L3. Các tầng mạng (vì sao "tầng ứng dụng")

**Hình dung.** Gửi bưu kiện: mình viết thư (nội dung), cho vào phong bì ghi tên người nhận (cổng), bưu điện ghi mã
tỉnh (IP), xe tải chở đi (cáp, sóng). Mỗi bên chỉ lo phần mình.

**Chính xác.** Mô hình TCP/IP rút gọn có 4 tầng:

| Tầng | Lo việc gì | Ví dụ |
| --- | --- | --- |
| Ứng dụng | Nội dung tin nhắn có nghĩa gì | HTTP, **T47GP/1** |
| Giao vận | Đưa dữ liệu tới đúng chương trình (cổng), có đảm bảo hay không | **TCP**, **UDP** |
| Mạng | Đưa gói tới đúng máy qua nhiều router | IP |
| Liên kết | Truyền bit trên dây/sóng | Ethernet, Wi-Fi |

Nhóm chỉ viết **tầng ứng dụng**, và chọn dùng TCP hay UDP ở tầng giao vận. Mọi thứ bên dưới hệ điều hành lo.

**Gặp ở:** [03 A1](03-hoi-dap.md).

---

# Phần 2. TCP

<a id="lt-tcp"></a>

## L4. TCP là gì

**Hình dung.** Gọi điện thoại: phải bấm số, chờ bắt máy (thiết lập kết nối), rồi nói. Nói câu nào bên kia nghe câu
đó, đúng thứ tự. Nghe không rõ thì "anh nói lại được không" (gửi lại).

**Chính xác.** TCP (Transmission Control Protocol):

- **Hướng kết nối**: hai bên bắt tay 3 bước (SYN, SYN-ACK, ACK) trước khi gửi dữ liệu.
- **Tin cậy**: mỗi đoạn dữ liệu được bên nhận xác nhận (ACK); không thấy ACK thì bên gửi tự gửi lại.
- **Đúng thứ tự**: bên nhận sắp xếp lại, chương trình luôn đọc ra theo đúng thứ tự đã gửi.
- **Luồng byte**: xem [L5](#lt-framing), đây là điểm quan trọng nhất.
- **Điều khiển luồng và tắc nghẽn**: tự giảm tốc khi bên nhận hoặc mạng quá tải.

Cái giá: chậm hơn khi mất gói (phải chờ gửi lại, xem [L6](#lt-hol)), và tốn công bắt tay.

**Trong T47.** TCP dùng cho REGISTER, LOGIN, FIND_MATCH, MATCH_FOUND, MATCH_END, PING, ERROR: những tin **phải tới**
và gửi ít.

**Gặp ở:** [02 Bước 1-3](02-di-theo-mot-goi-tin.md), [03 A1](03-hoi-dap.md).

---

<a id="lt-framing"></a>

## L5. TCP là luồng byte → phải tự tách tin nhắn (framing, length-prefix)

**Hình dung.** Một ống nước. Mình đổ vào 3 cốc nước riêng biệt, đầu kia nhận một dòng nước liên tục, không biết
đâu là ranh giới giữa các cốc. Muốn tách lại, mỗi cốc phải có nhãn "cốc này 200 ml".

**Chính xác.** TCP chỉ đảm bảo **chuỗi byte** tới đúng và đủ, **không giữ ranh giới** giữa các lần gửi. Client gọi
`send` 2 lần, mỗi lần 13 byte, thì server có thể:

- nhận 26 byte trong một lần đọc (**dính frame**, sticky packet),
- hoặc nhận 5 byte rồi 21 byte (**nửa frame**, partial read),
- hoặc bất kỳ cách chia nào khác.

Nên tầng ứng dụng phải tự quy định ranh giới, gọi là **framing**. Có 3 cách phổ biến:

| Cách | Ví dụ | Ưu | Nhược |
| --- | --- | --- | --- |
| Ký tự phân cách | mỗi tin kết thúc bằng `\n` | dễ đọc bằng mắt | nội dung có `\n` thì phải escape; không biết trước tin dài bao nhiêu |
| Độ dài cố định | mọi tin đúng 64 byte | rất đơn giản | phí chỗ, tin dài không vừa |
| **Tiền tố độ dài** (length-prefix) | 4 byte đầu ghi độ dài | biết trước phải đọc bao nhiêu; chặn được tin quá lớn | không đọc bằng mắt được |

Thuật toán đọc với length-prefix:

```text
1. đọc cho ĐỦ 4 byte  → ra số length
2. kiểm length hợp lệ (0 < length ≤ giới hạn); sai thì báo lỗi, dừng
3. đọc cho ĐỦ length byte → đó là một frame
4. quay lại bước 1
```

"Đọc cho đủ" nghĩa là gọi `read` lặp đi lặp lại tới khi gom đủ số byte, vì một lần `read` có thể trả ít hơn.

**Trong T47.** Frame TCP: `[u32 length][u8 type][payload]`, `length ≤ 4096`. Code ở `FrameReader.cs:45-73`, hàm
`ReadExactAsync` là "đọc cho đủ".

**Gặp ở:** [02 Bước 2](02-di-theo-mot-goi-tin.md), [03 A2, A3](03-hoi-dap.md), [04 Bài 1, Bài 4](04-sua-tai-cho.md).

---

<a id="lt-hol"></a>

## L6. Nghẽn đầu hàng (head-of-line blocking)

**Hình dung.** Xếp hàng mua vé, người đầu hàng quên ví quay về lấy. Cả hàng phải đứng chờ, dù người phía sau đã
cầm sẵn tiền.

**Chính xác.** Vì TCP cam kết đúng thứ tự, khi gói số 5 bị mất, các gói 6, 7, 8 dù đã tới vẫn bị hệ điều hành giữ
lại, chưa đưa cho chương trình, cho tới khi gói 5 được gửi lại thành công (mất thêm ít nhất một vòng đi-về, thường
vài chục tới vài trăm ms).

Với game: gói 6, 7, 8 chứa vị trí mới hơn, đáng giá hơn gói 5. Chờ gói 5 là vô ích, nhân vật đứng hình rồi nhảy cóc.
Đây là lý do chính dùng UDP cho dữ liệu thời gian thực.

**Gặp ở:** [03 A1](03-hoi-dap.md).

---

<a id="lt-nagle"></a>

## L7. Thuật toán Nagle và `NoDelay`

**Hình dung.** Shipper chờ gom đủ nhiều đơn rồi mới chạy một chuyến, tiết kiệm xăng nhưng đơn đầu phải chờ.

**Chính xác.** Nagle là cơ chế của TCP: khi đang có dữ liệu chưa được ACK, các mẩu dữ liệu nhỏ mới sẽ bị giữ lại để
gom thành một gói lớn. Tiết kiệm băng thông, nhưng thêm độ trễ cho tin nhỏ. `NoDelay = true` (tên chuẩn
`TCP_NODELAY`) tắt Nagle: ghi bao nhiêu gửi bấy nhiêu ngay.

Phân biệt với [L6](#lt-hol): Nagle làm trễ **lúc gửi**; nghẽn đầu hàng xảy ra **lúc nhận** khi mất gói. Tắt Nagle
không chữa được nghẽn đầu hàng.

**Trong T47.** `client.NoDelay = true` ở `ServerApp.cs`, vì frame của nhóm nhỏ (vài chục byte).

**Gặp ở:** [02 Bước 1](02-di-theo-mot-goi-tin.md), [03 A1](03-hoi-dap.md).

---

<a id="lt-fin-rst"></a>

## L8. Đóng kết nối TCP: FIN, RST, `read` trả 0, "đóng êm"

**Hình dung.** FIN là nói "chào nhé" rồi cúp máy. RST là giật đứt dây điện thoại giữa chừng. Rút dây mạng là
người kia đột nhiên im lặng, mình không biết họ còn đó hay không.

**Chính xác.**

- **FIN**: tín hiệu "tôi không gửi thêm nữa". Bên nhận thấy hàm `read` trả về **0 byte**. Đây là cách duy nhất biết
  bên kia đã đóng một cách bình thường.
- Khi một **tiến trình bị kill** (`kill -9`, tắt cửa sổ, `docker kill`), hệ điều hành vẫn đóng các socket của nó và
  thường gửi FIN. Nên server vẫn thấy `read` trả 0.
- **RST** (reset): đóng đột ngột, dữ liệu đang trên đường có thể bị huỷ. Bên nhận gặp lỗi kiểu
  `connection reset` (`IOException` / `SocketException`). Một trường hợp hay gặp: gọi `close()` khi **bộ đệm nhận
  còn byte chưa đọc**, hệ điều hành gửi RST thay vì FIN, và bên kia có thể mất cả những byte mình vừa gửi.
- **Đóng êm** (graceful close): `Shutdown(Send)` để gửi FIN trước, đọc bỏ phần bên kia còn gửi, rồi mới `close()`.
- **Rút dây mạng / mất Wi-Fi**: không có FIN, không có RST. `read` cứ chờ mãi. Chỉ phát hiện được bằng **timeout**
  ([L9](#lt-timeout)).

**Trong T47.** `FrameReader` báo `Closed` khi đọc được 0 byte; log ghi `peer closed`. `GracefulCloseAsync` ở
`ClientSession.cs:333-352` là cách đóng êm (đọc bỏ tối đa 500 ms / 64 KB).

**Gặp ở:** [01 mục E](01-demo-va-mo-dau.md), [02 Bước 8](02-di-theo-mot-goi-tin.md), [03 C1, C2, C4](03-hoi-dap.md).

---

<a id="lt-timeout"></a>

## L9. Timeout, PING / heartbeat

**Hình dung.** Gọi điện mà đầu bên kia im 15 giây, mình hỏi "alo?" rồi cúp. Còn nếu đang nghĩ, họ nói "ừ ừ" để báo
"tôi vẫn đây".

**Chính xác.** **Idle timeout**: quá một khoảng thời gian không nhận được gì thì coi kết nối đã chết và đóng, để giải
phóng tài nguyên. **Heartbeat** (PING/PONG): khi rảnh, client gửi tin nhỏ định kỳ để server không đóng nhầm. Khoảng
gửi PING phải ngắn hơn timeout.

**Trong T47.** TCP: 15 s không có frame thì đóng (`ServerConfig.IdleTimeout`), client PING mỗi 5 s. UDP trong trận:
5 s không có gói thì xử rớt mạng (`UdpSilenceTimeout`).

**Gặp ở:** [01 mục E](01-demo-va-mo-dau.md), [02 Bước 8](02-di-theo-mot-goi-tin.md), [03 C2](03-hoi-dap.md),
[04 Bài 3](04-sua-tai-cho.md).

---

# Phần 3. UDP

<a id="lt-udp"></a>

## L10. UDP là gì

**Hình dung.** Gửi bưu thiếp không đăng ký: bỏ vào thùng là xong. Có thể mất, có thể tới hai lần, tấm gửi sau có thể
tới trước. Nhưng rất nhanh, không cần hẹn trước.

**Chính xác.** UDP (User Datagram Protocol):

- **Không kết nối**: không bắt tay, cứ gửi tới IP:cổng.
- **Không tin cậy**: gói có thể **mất**, **trùng**, **đảo thứ tự**. Không có gửi lại.
- **Giữ ranh giới**: mỗi lần gửi là một **datagram** riêng; bên nhận đọc ra đúng nguyên gói đó (hoặc không có gì).
  Nên UDP **không cần framing** như TCP.
- **Không nghẽn đầu hàng**: gói mất thì thôi, gói sau tới vẫn được đọc ngay.

Hợp với dữ liệu gửi liên tục mà **gói mới thay thế gói cũ**: phím bấm, vị trí, voice, video.

**Trong T47.** UDP cho `UDP_HELLO`, `INPUT` (30 lần/giây), `SNAPSHOT` (20 lần/giây). Header 8 byte:
`[u16 magic 0x4737][u8 version][u8 type][u32 token]`. Magic là "số nhận dạng" để loại nhanh gói lạc không phải của
T47.

**Gặp ở:** [02 Bước 5, 6](02-di-theo-mot-goi-tin.md), [03 A1](03-hoi-dap.md).

---

<a id="lt-nat"></a>

## L11. NAT, học endpoint, token định danh

**Hình dung.** Cả toà văn phòng dùng chung một số điện thoại tổng đài. Gọi ra ngoài, người nhận chỉ thấy số tổng đài
chứ không thấy số máy lẻ. Muốn biết ai gọi thì người gọi phải tự xưng tên (token).

**Chính xác.**

- **NAT** (Network Address Translation): router (hoặc Docker) đổi IP:cổng nguồn của gói khi đi ra ngoài. Client
  nghĩ mình gửi từ cổng 60000, server lại thấy gói đến từ cổng 41234.
- Vì vậy server **không tin** cổng client tự khai, mà **học endpoint**: lấy IP:cổng thật từ chính gói UDP vừa tới, và
  gửi trả về đúng địa chỉ đó (NAT sẽ chuyển ngược lại cho client).
- UDP không có kết nối, nên server cần biết gói là **của ai**. Cách làm: lúc đăng nhập qua TCP (đã tin được), server
  cấp một **token** ngẫu nhiên; mọi gói UDP mang token ở header. Tra token → ra phiên người chơi.
- Không dùng "gói đầu tiên từ IP này là của người đó", vì ai gửi trước thì chiếm được.

Giới hạn: token gửi không mã hoá, người nghe lén cùng mạng đọc được.

**Trong T47.** Token 4 byte cấp trong `LOGIN_RESULT` (`SessionRegistry.cs`). Client gửi `UDP_HELLO` mỗi 200 ms tới
khi nhận `UDP_HELLO_ACK`; server học endpoint trong `Room.HandleUdp`.

**Gặp ở:** [02 Bước 3, 5](02-di-theo-mot-goi-tin.md), [03 A6](03-hoi-dap.md).

---

<a id="lt-mtu"></a>

## L12. MTU và phân mảnh

**Hình dung.** Cửa thang máy chỉ vừa thùng 1 m. Thùng to hơn phải tháo ra chở từng phần; mất một phần là cả thùng
vô dụng.

**Chính xác.** **MTU** (Maximum Transmission Unit) là kích thước gói lớn nhất một đường truyền chở được, Ethernet
thường 1500 byte (trừ header IP/UDP còn khoảng 1472 byte dữ liệu; qua VPN còn ít hơn). Datagram lớn hơn bị IP **phân
mảnh**; với UDP, mất một mảnh là mất cả datagram. Nên game giữ gói UDP nhỏ, thường ≤ 1200 byte để an toàn.

**Trong T47.** `MaxDatagramSize = 1200` (`ProtocolConstants.cs`).

**Gặp ở:** [04 Bài 1](04-sua-tai-cho.md).

---

<a id="lt-seq"></a>

## L13. Số thứ tự (seq) và bù mất gói ở tầng ứng dụng

**Hình dung.** Nhắn tin cho bạn: "1. mua trứng", "2. mua sữa". Sợ tin mất, mỗi tin mới mình chép lại luôn 3 tin
trước. Bạn đánh dấu "đã làm tới số mấy" để không mua trứng hai lần.

**Chính xác.** UDP không tự xử lý mất/trùng/đảo thứ tự, nên tầng ứng dụng tự làm:

- **Số thứ tự (seq)**: mỗi input có một số tăng dần. Bên nhận nhớ `lastProcessedSeq`; gói có `seq` nhỏ hơn hoặc bằng
  số đó là cũ hoặc trùng → bỏ. Nhờ vậy chống được cả trùng lẫn đảo thứ tự.
- **Gửi dư (redundancy)**: mỗi gói mang input mới nhất **kèm vài input cũ** chưa được xác nhận. Mất một gói thì gói
  sau vẫn mang bước đi đó.
- **Gửi trạng thái đầy đủ** thay vì "thay đổi": nếu mỗi SNAPSHOT là toàn bộ trạng thái, mất một cái thì cái sau thay
  thế hoàn toàn, không cần gửi lại.
- **ACK bằng dữ liệu**: SNAPSHOT trả `lastProcessedSeq` để client biết input nào server đã nhận.

**Trong T47.** Mỗi INPUT tối đa 4 input (1 mới + 3 cũ), `Room.QueueInputs` bỏ `seq <= lastProcessedSeq`.

**Gặp ở:** [02 Bước 6](02-di-theo-mot-goi-tin.md), [03 A1](03-hoi-dap.md), [04 Bài 4](04-sua-tai-cho.md).

---

<a id="lt-udp-im-lang"></a>

## L14. Vì sao gói UDP rác thì bỏ im lặng (spoofing, phản xạ)

**Hình dung.** Ai đó gửi thư mạo danh địa chỉ nhà hàng xóm tới mình. Nếu mình lịch sự trả lời từng lá, hàng xóm
nhận một núi thư mà không biết vì sao.

**Chính xác.** Với UDP, kẻ xấu có thể giả địa chỉ nguồn (**IP spoofing**). Nếu server trả lời mọi gói sai, nó vô tình
dội lưu lượng vào máy nạn nhân (**tấn công phản xạ**; nếu trả lời to hơn gói nhận là **khuếch đại**). Cách an toàn:
gói sai thì **bỏ, không trả lời**, chỉ đếm. Log cũng không ghi từng gói, để flood không làm đầy đĩa.

**Trong T47.** `UdpGateway.Handle`: sai magic/version/token → bỏ, tăng bộ đếm; log gói đầu và mỗi 1000 gói.

**Gặp ở:** [02 Bước 5](02-di-theo-mot-goi-tin.md), [03 A8](03-hoi-dap.md).

---

# Phần 4. Mã hoá dữ liệu

<a id="lt-byte"></a>

## L15. Byte, hệ hex, `u8` / `u16` / `u32`

**Chính xác.**

- **Byte** = 8 bit, giá trị 0-255. Viết bằng **hex** (hệ 16) cho gọn: mỗi byte đúng 2 ký tự, `00` tới `FF`.
  Ví dụ `0D` = 13, `61` = 97 = chữ `a` trong ASCII.
- **`u8`** = số không dấu 1 byte (0-255). **`u16`** = 2 byte (0-65535). **`u32`** = 4 byte (0 tới ~4,29 tỉ).
- **Chuỗi** trong T47: `u16` độ dài + các byte UTF-8. `"an"` → `02 00 61 6E`.

Tự tính ví dụ LOGIN trong file 02: `0D 00 00 00 | 03 | 02 00 61 6E | 06 00 61 62 63 31 32 33`

- `0D 00 00 00` = `u32` 13 (độ dài phần sau),
- `03` = type LOGIN,
- `02 00` + `61 6E` = chuỗi dài 2, "an",
- `06 00` + `61 62 63 31 32 33` = chuỗi dài 6, "abc123".
- Kiểm: 1 + (2+2) + (2+6) = 13.

**Gặp ở:** [02 Bước 2](02-di-theo-mot-goi-tin.md), [04 Bài 1](04-sua-tai-cho.md).

---

<a id="lt-endian"></a>

## L16. Little-endian (thứ tự byte)

**Hình dung.** Viết ngày tháng: Việt Nam viết 09/11, Mỹ viết 11/09. Cùng một ngày nhưng thứ tự khác nhau; hai bên
phải thống nhất trước, không thì hiểu sai.

**Chính xác.** Một số nhiều byte phải chọn byte nào đứng trước.

- **Little-endian**: byte **thấp** trước. Số 13 kiểu `u32` → `0D 00 00 00`. Số 4097 = `0x1001` → `01 10 00 00`.
- **Big-endian** ("network byte order"): byte **cao** trước. Số 13 → `00 00 00 0D`.

Không bên nào "đúng" hơn. Quan trọng là giao thức ghi rõ một thứ tự cố định và cả hai phía làm theo, không phụ thuộc
loại CPU.

**Trong T47.** Little-endian, dùng `BinaryPrimitives.ReadUInt32LittleEndian` / `Write...LittleEndian`.

**Gặp ở:** [02 Bước 2](02-di-theo-mot-goi-tin.md), [03 A5](03-hoi-dap.md), [04 Bài 1](04-sua-tai-cho.md) (đổi
4097 → 8193 = `0x2001` → `01 20 00 00`).

---

<a id="lt-codec"></a>

## L17. Nhị phân hay văn bản (JSON, Protobuf); codec, encode/decode

**Chính xác.**

- **Encode** (mã hoá, serialize): đổi đối tượng trong code thành dãy byte để gửi. **Decode**: ngược lại.
  **Codec** = cặp encode + decode.
- **Văn bản/JSON**: `{"type":"login","user":"an"}`. Dễ đọc, dễ debug, nhưng to (lặp lại tên trường), chậm phân tích.
- **Nhị phân tự viết**: mỗi trường ở vị trí cố định. Nhỏ, nhanh, giải thích được từng byte. Nhưng phải tự viết
  encoder/decoder, dễ lệch nhau → cần test "mã hoá rồi giải mã lại phải ra như cũ" (round-trip test).
- **Protobuf**: nhị phân có công cụ sinh code. Gọn, nhưng thêm thư viện và khó giải thích từng byte.

**Trong T47.** Nhị phân tự viết. Mỗi message là một lớp có `Write` (encode) và `Read` (decode)
(`TcpMessages.cs`); `MessageCodec.cs` chọn lớp theo byte `type`.

**Gặp ở:** [02 Bước 2](02-di-theo-mot-goi-tin.md), [03 A3, A4](03-hoi-dap.md), [04 Bài 2](04-sua-tai-cho.md).

---

<a id="lt-validate"></a>

## L18. Không tin dữ liệu đầu vào (input validation)

**Hình dung.** Bảo vệ toà nhà kiểm thẻ **trước** khi mở cửa, không phải sau khi khách đã vào.

**Chính xác.** Mọi byte tới từ mạng có thể sai, cố ý hoặc vô tình. Server phải kiểm trước khi dùng:

- **Kiểm độ dài trước khi cấp phát bộ nhớ**: nếu tin ngay số `FF FF FF FF` (4 tỉ) rồi `new byte[length]`, chỉ một
  frame là server hết RAM. Đây là một dạng **tấn công từ chối dịch vụ (DoS)**.
- **Kiểm đúng cấu trúc**: type có tồn tại, payload đủ byte, **không thừa byte**, chuỗi không quá dài.
- **Cô lập lỗi**: client gửi rác chỉ làm hỏng kết nối của chính nó.
- **Kẹp giá trị** (clamp): giá trị client khai (như `viewTick`) bị ép vào khoảng hợp lý.

**Trong T47.** `length` 0 hoặc > 4096 → đóng ngay (không biết frame sau bắt đầu ở đâu nữa). Nội dung sai nhưng độ
dài đúng → `BAD_FRAME`, cho sai tối đa 3 lần.

**Gặp ở:** [02 Bước 2](02-di-theo-mot-goi-tin.md), [03 A8, D3](03-hoi-dap.md), [04 Bài 4](04-sua-tai-cho.md).

---

# Phần 5. Giao thức ứng dụng

<a id="lt-state"></a>

## L19. Stateful / stateless, máy trạng thái của phiên

**Hình dung.** Quầy ngân hàng: phải lấy số → được gọi → mới giao dịch. Chưa lấy số mà đòi rút tiền thì bị từ chối.

**Chính xác.**

- **Stateless**: server không nhớ gì giữa hai yêu cầu, mỗi yêu cầu tự mang đủ thông tin (HTTP thuần).
- **Stateful**: server nhớ mỗi kết nối đang ở bước nào. Lệnh được chấp nhận hay không tuỳ bước hiện tại.
- **Máy trạng thái** (state machine): liệt kê các trạng thái và lệnh nào chuyển từ trạng thái nào sang trạng thái
  nào. Lệnh không hợp lệ ở trạng thái hiện tại → trả lỗi.

**Trong T47.** `Connected → LoggedIn → InQueue → InMatch → (hết trận) LoggedIn`. FIND_MATCH khi chưa đăng nhập →
`NOT_LOGGED_IN`; khi đang trong trận → `INVALID_STATE`. Cái giá của stateful: khi kết nối chết phải dọn trạng thái
([L8](#lt-fin-rst)).

**Gặp ở:** [02 Bước 3, 7](02-di-theo-mot-goi-tin.md), [03 A7](03-hoi-dap.md), [04 Bài 2](04-sua-tai-cho.md).

---

<a id="lt-status"></a>

## L20. Mã trạng thái / mã lỗi

**Chính xác.** Giống HTTP 200/404/500: giao thức định nghĩa sẵn một bảng mã, client đọc mã để biết chuyện gì xảy ra
mà không phải đoán từ câu chữ. Một nguyên tắc bảo mật: đăng nhập sai username hay sai mật khẩu đều trả **cùng một
mã**, để không lộ username nào tồn tại.

**Trong T47.** `StatusCode`: `OK`, `BAD_FRAME`, `FRAME_TOO_LARGE`, `NOT_LOGGED_IN`, `INVALID_STATE`,
`ALREADY_LOGGED_IN`, `INVALID_CREDENTIAL_FORMAT`, ... (bảng đầy đủ trong `protocol-design.md`).

**Gặp ở:** [02 Bước 3](02-di-theo-mot-goi-tin.md), [03 A8](03-hoi-dap.md).

---

# Phần 6. Đồng thời (concurrency)

<a id="lt-async"></a>

## L21. Thread, thread pool, `async` / `await`

**Hình dung.** Nhà hàng có 8 phục vụ (thread pool). Cách dở: mỗi bàn giữ riêng một phục vụ đứng chờ khách chọn món
(thread-per-client). Cách tốt: phục vụ ghi order, đưa vào bếp, **đi phục vụ bàn khác**, món xong ai rảnh thì bưng ra
(`await`).

**Chính xác.**

- **Thread** (luồng): một dòng chạy lệnh. Nhiều thread chạy song song. Mỗi thread tốn bộ nhớ (stack khoảng 1 MB) và
  hệ điều hành tốn công chuyển qua lại.
- **Thread pool**: một nhóm thread có sẵn, việc nào cần chạy thì mượn tạm một thread.
- **I/O chặn** (blocking): `Read()` làm thread đứng chờ tới khi có dữ liệu, không làm được gì khác.
- **`async` / `await`**: `await stream.ReadAsync(...)` nghĩa là "khi nào có dữ liệu thì chạy tiếp đoạn sau". Trong lúc
  chờ, thread **được trả về pool** để làm việc khác. 1.000 client đang chờ dữ liệu không chiếm 1.000 thread.
- **`Task`**: một việc đang chạy hoặc sẽ xong trong tương lai. `Task.Run(...)` giao việc cho thread pool.

**Trong T47.** Vòng accept, vòng đọc/gửi của mỗi phiên, vòng nhận UDP, vòng tick của mỗi phòng đều là `async`.

**Gặp ở:** [02 Bước 1](02-di-theo-mot-goi-tin.md), [03 B2, B3](03-hoi-dap.md); câu phải thuộc số 1 trong
[README](README.md).

---

<a id="lt-mo-hinh-server"></a>

## L22. Các kiểu tổ chức server đồng thời

| Kiểu | Cách làm | Ưu | Nhược |
| --- | --- | --- | --- |
| Lặp (iterative) | phục vụ xong client này mới tới client khác | đơn giản nhất | một client chậm chặn tất cả; **không đạt yêu cầu đề** |
| Thread-per-client | mỗi client một thread chặn | dễ viết, dễ hiểu | tốn thread; dữ liệu chung bị nhiều thread sửa → lock khắp nơi |
| Async / event-driven | ít thread, `await` khi chờ I/O | chịu được nhiều kết nối | khó debug hơn |
| Một game loop chung | một vòng lặp chạy mọi phòng | không cần lock | phòng chậm kéo tụt mọi phòng |
| **T47: async + mỗi phòng một vòng tick** | I/O async; state phòng chỉ vòng tick của phòng đó đụng vào | phòng độc lập, state game không cần lock | phải chuyển tin qua hộp thư ([L25](#lt-hop-thu)) |

**Gặp ở:** [03 B3, B4](03-hoi-dap.md).

---

<a id="lt-race"></a>

## L23. Race condition, vùng găng, lock, thao tác nguyên tử

**Hình dung.** Còn 1 vé cuối. Hai nhân viên cùng nhìn màn hình thấy "còn 1", cùng bán. Kết quả bán 2 vé cho 1 ghế.

**Chính xác.**

- **Race condition**: kết quả sai tuỳ vào thứ tự chạy của các thread, khi nhiều thread cùng đọc/ghi một dữ liệu và
  ít nhất một thread ghi. Lỗi kiểu **"kiểm tra rồi hành động"** (check-then-act): kiểm "còn vé" và "lấy vé" là hai
  bước tách rời, thread khác chen vào giữa.
- **Vùng găng** (critical section): đoạn code mà mỗi lúc chỉ một thread được chạy.
- **Lock** (khoá, `lock(obj)` trong C#): thread nào vào trước giữ khoá, thread sau đứng chờ. Mọi bước "kiểm" và
  "hành động" phải nằm **trong cùng một lần giữ khoá**.
- **Thao tác nguyên tử** (atomic): một thao tác không thể bị chen ngang giữa chừng, ví dụ
  `ConcurrentDictionary.TryAdd` (thêm nếu chưa có, đúng một bên thắng).
- Nguyên tắc: giữ lock **càng ngắn càng tốt**, không làm việc chậm (mạng, đĩa, băm mật khẩu) khi đang giữ lock.

**Trong T47.** Matchmaker: "đủ 2 người" và "lấy 2 người ra" trong cùng `lock (_queueLock)`; tạo phòng và gửi
`MATCH_FOUND` sau khi nhả lock. Đăng nhập hai nơi: `_players.TryAdd`. PBKDF2 tính ngoài lock DB.

**Gặp ở:** [02 Bước 3, 4](02-di-theo-mot-goi-tin.md), [03 B1](03-hoi-dap.md), [04 Bài 4](04-sua-tai-cho.md).

---

<a id="lt-deadlock"></a>

## L24. Deadlock

**Hình dung.** Hai xe đối đầu trên cầu một làn, xe nào cũng chờ xe kia lùi. Mãi mãi.

**Chính xác.** Deadlock xảy ra khi đủ **4 điều kiện** cùng lúc (điều kiện Coffman):

1. **Loại trừ lẫn nhau**: tài nguyên chỉ một thread dùng được một lúc (lock).
2. **Giữ và chờ**: thread đang giữ một lock lại chờ lấy thêm lock khác.
3. **Không tước đoạt**: không ai giật được lock từ tay thread khác.
4. **Chờ vòng tròn**: A chờ B, B chờ A.

Phá được **một** điều kiện là không còn deadlock. Cách phổ biến: không bao giờ giữ hai lock lồng nhau (phá điều kiện
2), hoặc luôn lấy các lock theo cùng một thứ tự (phá điều kiện 4), hoặc gửi tin nhắn không chờ trả lời.

**Trong T47.** Không chỗ nào giữ hai lock cùng lúc; không làm I/O mạng trong lock; phiên và phòng nói chuyện qua hộp
thư, gửi xong không chờ.

**Gặp ở:** [03 B1](03-hoi-dap.md).

---

<a id="lt-hop-thu"></a>

## L25. Hàng đợi an toàn luồng, producer-consumer, mô hình "hộp thư" (actor)

**Hình dung.** Bếp chỉ có một đầu bếp chính được đụng vào nồi. Phục vụ không tự nêm nếm, mà ghi phiếu cắm lên bảng.
Đầu bếp lấy từng phiếu ra làm. Không ai tranh nhau cái nồi.

**Chính xác.**

- **Producer-consumer**: nhiều bên sinh việc (producer) bỏ vào hàng đợi, một hoặc vài bên lấy ra xử lý (consumer).
- **Hàng đợi an toàn luồng**: nhiều thread cùng ghi/đọc mà không hỏng dữ liệu. Trong .NET: `ConcurrentQueue`,
  `Channel`.
- **Actor / hộp thư**: một đối tượng có trạng thái riêng và **chỉ một luồng** của chính nó đụng vào trạng thái đó.
  Bên ngoài muốn tác động thì gửi tin nhắn vào hộp thư. Không cần lock cho trạng thái, vì chỉ có một người đọc/ghi.
- **Một người ghi socket**: nếu nhiều thread cùng ghi một socket TCP, byte của hai frame có thể xen kẽ, hỏng framing.
  Cách chữa: mọi frame vào một hàng đợi, **một vòng gửi duy nhất** ghi ra socket.

**Trong T47.**

- Phòng là actor: `Room.Post` bỏ sự kiện vào `ConcurrentQueue`, vòng tick rút ra đầu mỗi tick (`DrainInbox`).
- Mỗi phiên có `Channel` gửi riêng + `SendLoopAsync`. UDP có một `Channel` gửi chung + một vòng gửi.

**Gặp ở:** [01 mục E](01-demo-va-mo-dau.md), [02 Bước 5, 6, 8](02-di-theo-mot-goi-tin.md), [03 B1](03-hoi-dap.md);
câu phải thuộc số 1 trong [README](README.md).

---

<a id="lt-backpressure"></a>

## L26. Hàng đợi có giới hạn (bounded queue, backpressure)

**Hình dung.** Phễu rót nước: rót nhanh hơn nước chảy xuống thì tràn. Phải chọn: ngừng rót (chờ), đổ bớt (bỏ), hoặc
đuổi người rót.

**Chính xác.** Hàng đợi không giới hạn + bên đọc chậm = bộ nhớ phình mãi. Nên đặt **sức chứa tối đa** và chọn cách xử
lý khi đầy:

| Cách | Khi nào dùng |
| --- | --- |
| Chờ cho có chỗ | khi chờ không làm hại ai |
| **Bỏ tin mới** (drop) | dữ liệu chịu được mất, như gói UDP |
| **Đóng bên gây đầy** | client không đọc kịp, chờ thì kéo chậm cả phòng |

**Trong T47.** Hàng gửi mỗi phiên 256 frame, đầy → đóng phiên (`TryWrite`, không bao giờ chờ). Hàng gửi UDP 4096,
đầy → `DropWrite`. Hộp thư phòng 1024 sự kiện; riêng `PlayerLeft` được ép vào (`force: true`) vì không được mất. Hàng
input mỗi người 32.

**Gặp ở:** [02 Bước 5, 6, 8](02-di-theo-mot-goi-tin.md), [03 B5](03-hoi-dap.md).

---

# Phần 7. Game mạng

<a id="lt-authoritative"></a>

## L27. Server authoritative và relay

**Hình dung.** Authoritative: trọng tài xem trận và quyết định bàn thắng. Relay: mỗi đội tự báo tỉ số, trọng tài chỉ
chuyển lời.

**Chính xác.**

- **Authoritative server**: client chỉ gửi **ý định** (phím bấm). Server tự tính vị trí, điểm, ai nhặt được món, rồi
  báo kết quả. Client sửa gói để "dịch chuyển" cũng vô ích.
- **Relay**: server chỉ chuyển tiếp những gì client báo (kể cả vị trí). Đơn giản, nhưng tin client → dễ gian lận.

**Trong T47.** Chế độ chính là authoritative (`Simulation.Step` chạy ở server). Relay được giữ lại **cố ý** để demo
so sánh: gói `POS_REPORT` bị bỏ qua ở authoritative, nhưng được tin ở relay.

**Gặp ở:** [01 mục B](01-demo-va-mo-dau.md), [03 D1, D2](03-hoi-dap.md).

---

<a id="lt-tick"></a>

## L28. Tick, tick rate, hẹn giờ không trôi

**Hình dung.** Đồng hồ cờ vua của trọng tài: cứ mỗi nhịp thì cập nhật thế giới một lần, không phải mỗi khi có người
bấm.

**Chính xác.**

- **Tick**: một lần server cập nhật thế giới game: lấy input, tính di chuyển, phân xử, gửi kết quả.
- **Tick rate**: số tick mỗi giây. 30 Hz → mỗi tick cách nhau `dt = 1/30 ≈ 33,3 ms`.
- **Fixed timestep**: mỗi tick dùng cùng một `dt`, nên client và server tính ra cùng kết quả nếu cùng input.
- **Hẹn giờ trôi** (timer drift): `Sleep(33)` mỗi vòng thì mỗi lần lệch vài ms (thời gian chạy tick + hệ điều hành
  đánh thức trễ), cộng dồn sau vài phút lệch hàng giây. Cách đúng: tính **mốc tuyệt đối** `next += interval` theo
  đồng hồ `Stopwatch`, chỉ chờ phần còn thiếu. Chậm quá nhiều thì bỏ phần trễ thay vì chạy dồn.

**Trong T47.** 30 tick/giây (`TICK_RATE` trong `.env`), vòng tick ở `Room.cs:111-200`, chậm quá 5 tick thì reset mốc.

**Gặp ở:** [02 Bước 6](02-di-theo-mot-goi-tin.md), [03 E1](03-hoi-dap.md), [04 Bài 3](04-sua-tai-cho.md).

---

<a id="lt-snapshot"></a>

## L29. Snapshot

**Chính xác.** **Snapshot** = ảnh chụp trạng thái thế giới tại một tick (vị trí mọi người, món, điểm, tick số mấy,
`lastProcessedSeq` của mỗi người). Gửi **đầy đủ** mỗi lần nên mất một cái không sao ([L13](#lt-seq)). Không cần gửi
mỗi tick, vì client nội suy giữa hai snapshot ([L30](#lt-prediction)).

**Trong T47.** Gửi 2 trên 3 tick → khoảng 20 snapshot/giây.

**Gặp ở:** [02 Bước 6](02-di-theo-mot-goi-tin.md), [03 E1](03-hoi-dap.md).

---

<a id="lt-prediction"></a>

## L30. Prediction, reconciliation, interpolation (phần của #2, chỉ cần hiểu ý)

**Hình dung.** Mạng trễ 100 ms. Bấm phím mà chờ server trả lời mới thấy nhân vật đi thì rất "đơ".

**Chính xác.**

- **Client-side prediction**: client tự chạy luôn bước di chuyển ngay khi bấm, không chờ server.
- **Reconciliation**: khi snapshot về, client so vị trí server với vị trí mình đoán. Lệch thì lấy vị trí server, bỏ
  các input có `seq ≤ lastProcessedSeq`, chạy lại các input chưa được xác nhận.
- **Interpolation** (nội suy): người chơi khác được vẽ hơi lùi về quá khứ, trượt mượt giữa hai snapshot đã nhận.

Phía server chỉ cần: áp input theo đúng `seq`, dùng cùng `dt` với client, gửi `lastProcessedSeq` trong snapshot.
Nếu tick rate server khác client, đoán và thực tế lệch liên tục (xem 03 E1).

**Gặp ở:** [02 Bước 6](02-di-theo-mot-goi-tin.md), [03 E1, Mẹo](03-hoi-dap.md).

---

<a id="lt-lag-comp"></a>

## L31. Lag compensation

**Hình dung.** Hai người cùng giơ tay giành món. Một người mạng chậm nên tin "giơ tay" tới server sau, dù trên màn
hình của họ đã giơ trước. Lag compensation là trọng tài xem lại băng: "lúc người đó nhìn thấy, họ giơ trước".

**Chính xác.** Mỗi input mang **`viewTick`**: tick của thế giới mà người chơi **đang nhìn thấy** trên màn hình lúc
bấm. Server phân xử theo `viewTick` thay vì theo "gói nào tới trước". Bản đầy đủ (game bắn súng) còn lưu lịch sử vị
trí và "tua" thế giới về `viewTick`. Chống gian lận: kẹp `viewTick` trong khoảng lùi tối đa cho phép.

**Trong T47.** Chỉ "tua thời điểm", không tua vị trí, vì kệ hiếm đứng yên. Chờ tối đa 12 tick (400 ms) gom đủ lượt
giành; `viewTick` kẹp trong `[tick - 30, tick]`.

**Gặp ở:** [01 mục D](01-demo-va-mo-dau.md), [03 D3, E2](03-hoi-dap.md).

---

<a id="lt-bitmask"></a>

## L32. Bitmask và phát hiện "vừa nhấn" (edge detection)

**Chính xác.** Nhiều phím bật/tắt được gói vào **một byte**, mỗi phím một bit (bitmask). Ví dụ bit 4 = J.

- `a & b`: bit nào cả hai đều 1. `~a`: đảo mọi bit.
- **Vừa nhấn** = bit đang 1 bây giờ **và** lần trước là 0: `pressed = keys & ~prevKeys`.

Giữ phím J 10 tick thì cả 10 input có bit J, nhưng `pressed` chỉ có J ở input đầu tiên → chỉ nhặt một lần.

**Trong T47.** `Room.cs:324-345`.

**Gặp ở:** [02 Bước 6](02-di-theo-mot-goi-tin.md).

---

# Phần 8. Bảo mật

<a id="lt-pbkdf2"></a>

## L33. Băm mật khẩu: hash, salt, PBKDF2

**Hình dung.** Không giữ chìa khoá của khách, chỉ giữ **dấu vân tay** của chìa. Khách đưa chìa, mình in vân tay ra so.
Kẻ trộm lấy được sổ vân tay cũng không làm lại được chìa.

**Chính xác.**

- **Hàm băm** (hash): biến dữ liệu thành chuỗi cố định, **một chiều** (không tính ngược ra mật khẩu). Lưu hash, không
  lưu mật khẩu thô.
- **Salt**: chuỗi ngẫu nhiên riêng cho mỗi user, trộn vào trước khi băm. Hai người cùng mật khẩu `123456` vẫn ra hash
  khác nhau, và **bảng tra sẵn** (rainbow table) trở nên vô dụng.
- **Hàm băm chậm có chủ đích** (PBKDF2, bcrypt, Argon2): lặp lại hàng chục nghìn vòng. Người dùng thật chỉ chờ vài
  chục ms một lần; kẻ đoán thử hàng tỉ mật khẩu thì chậm hơn hàng trăm nghìn lần.
- Giới hạn: băm chỉ bảo vệ **khi lưu**. Mật khẩu **trên đường truyền** vẫn lộ nếu không có TLS (mã hoá kênh).

**Trong T47.** PBKDF2-SHA256, 100.000 vòng, salt 16 byte (`UserStore.cs:150-179`). Không làm TLS.

**Gặp ở:** [02 Bước 3](02-di-theo-mot-goi-tin.md), [03 D4](03-hoi-dap.md).

---

<a id="lt-timing"></a>

## L34. Tấn công đo thời gian, `FixedTimeEquals`, băm giả

**Hình dung.** Khoá số mà sai số đầu thì kêu "tạch" ngay, đúng số đầu thì kêu chậm hơn. Nghe tiếng là đoán từng số.

**Chính xác.** **Timing attack**: đo thời gian server trả lời để suy ra thông tin bí mật.

- So sánh thường dừng ngay ở byte sai đầu tiên → càng đúng nhiều byte càng lâu. **So sánh thời gian cố định**
  (`CryptographicOperations.FixedTimeEquals`) luôn so hết mọi byte.
- Username không tồn tại mà trả lời ngay (không băm) thì nhanh hơn hẳn username có thật → lộ danh sách username.
  Cách chữa: vẫn băm với một salt giả (`DummySalt`) để thời gian như nhau.

**Gặp ở:** [02 Bước 3](02-di-theo-mot-goi-tin.md), [03 D4](03-hoi-dap.md).

---

# Phần 9. Triển khai

<a id="lt-docker"></a>

## L35. Docker: image, container, multi-stage, port mapping, volume, `.env`

**Hình dung.** Image là hộp cơm đóng sẵn đủ món; container là hộp cơm đang được ăn. Ở máy nào mở ra cũng giống nhau.

**Chính xác.**

- **Image**: gói chứa chương trình + mọi thứ nó cần để chạy. **Container**: một lần chạy của image, tách biệt với máy.
- **Dockerfile**: công thức build image. **Multi-stage build**: tầng 1 dùng image có trình biên dịch (`sdk`) để build,
  tầng 2 chỉ chép kết quả sang image nhẹ chỉ có runtime → image cuối nhỏ.
- **Build context**: thư mục Docker được phép đọc khi build. T47 đặt ở gốc repo để thấy `shared/`.
- **Port mapping** `5000:5000/tcp`, `5001:5001/udp`: nối cổng máy host vào cổng container. UDP phải ghi rõ `/udp`.
- **Volume** `./data`: thư mục của máy host gắn vào container. Xoá container không mất file SQLite.
- **docker compose**: chạy nhiều container theo file `docker-compose.yml`. **`.env`**: file biến môi trường, cấu hình
  không ghi cứng trong code.
- `docker kill` gửi tín hiệu dừng ngay tiến trình trong container, giống `kill -9` ([L8](#lt-fin-rst)).

**Gặp ở:** [01 mục A, F](01-demo-va-mo-dau.md), [03 F1](03-hoi-dap.md), [04 Bài 3](04-sua-tai-cho.md).

---

<a id="lt-vpn"></a>

## L36. Vì sao hai máy ở xa không gọi thẳng nhau được: NAT, CGNAT, VPN

**Chính xác.**

- Máy ở nhà nằm sau router NAT ([L11](#lt-nat)). Từ ngoài gửi vào, router không biết chuyển cho máy nào → bị chặn.
- **Mở port (port forwarding)** trên router thì giải quyết được, nhưng nhiều nhà mạng dùng **CGNAT** (NAT thêm một
  tầng ở nhà mạng), mình không có quyền mở.
- **Tunnel kiểu ngrok**: thường chỉ cho TCP/HTTP, và đổi cổng. Client T47 lấy cổng UDP từ `MATCH_FOUND`, cổng bị đổi
  thì UDP đi sai chỗ.
- **VPN kiểu LAN ảo (Radmin)**: mỗi máy có thêm một IP ảo (`26.x.x.x`) như cùng mạng LAN, giữ nguyên cả TCP lẫn UDP và
  số cổng. Vì vậy không phải sửa giao thức.
- **Firewall** Windows có thể chặn cổng; rule phải áp cho mọi profile (`profile=any`), vì card Radmin thường bị coi là
  mạng "Public".

**Gặp ở:** [01 mục F](01-demo-va-mo-dau.md), [03 F2](03-hoi-dap.md).

---

<a id="lt-sqlite"></a>

## L37. SQLite và lock cơ sở dữ liệu

**Chính xác.** **SQLite** là cơ sở dữ liệu nằm gọn trong một file, không cần chạy server DB riêng. Một kết nối SQLite
không nên bị nhiều thread dùng cùng lúc, nên T47 bọc mọi truy cập bằng `lock (_dbLock)`. Hệ quả: việc nào đụng DB lâu
sẽ chặn việc khác (ví dụ đăng nhập). Nên: tính PBKDF2 **ngoài** lock; ghi kết quả trận ở **Task nền** để vòng tick
không phải chờ đĩa.

**Gặp ở:** [02 Bước 3, 7](02-di-theo-mot-goi-tin.md), [03 B1, B2](03-hoi-dap.md).

---

# Phần 10. Kiểm thử

<a id="lt-test"></a>

## L38. Các loại test

| Loại | Kiểm cái gì | Ví dụ T47 |
| --- | --- | --- |
| **Unit test** | một hàm/lớp nhỏ, không mạng | `FrameReaderTests` cắt luồng byte thành mẩu 1, 2, 3 byte; `MessageCodecTests` mã hoá rồi giải mã lại |
| **Round-trip test** | encode xong decode phải ra như cũ | mọi mẫu trong `TcpSamples()` |
| **Integration test** | nhiều phần ghép lại, **socket thật** | `SessionTests`, `MatchTests` mở server thật trên cổng thật |
| **Concurrency test** | nhiều client cùng lúc, tìm race | `Concurrent_find_match_pairs_everyone_exactly_once` |
| **Load test** | nhiều client giả lập chạy lâu | bot headless: 20-50 bot, đo tick/giây |
| **Abuse / fuzz test** | gửi dữ liệu rác, sai định dạng | `scripts/r3-abuse.ps1`, `Oversized_frame_closes_only_that_connection` |
| **Regression test** | lỗi đã sửa không quay lại | test bị hỏng khi đổi 4096 → 8192 ở 04 Bài 1 |

Lệnh chạy: `dotnet test T47.sln`; `--filter <tên>` để chỉ chạy test có tên chứa chuỗi đó.

**Gặp ở:** [01 mục C](01-demo-va-mo-dau.md), [02 Bước 2, 4, 6](02-di-theo-mot-goi-tin.md),
[03 A8, B2, G1](03-hoi-dap.md), [04 toàn bộ](04-sua-tai-cho.md).
