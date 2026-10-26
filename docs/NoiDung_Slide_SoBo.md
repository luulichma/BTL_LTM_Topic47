# Nội dung sơ bộ slide T47: Client Prediction & Lag Compensation

> **Bản nháp cũ, sẽ dựng lại từ [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md).**
>
> Bản nháp nội dung cho 13 slide theo dàn ý ở [`KeHoach_ThuyetTrinh_DemoSoBo.md`](KeHoach_ThuyetTrinh_DemoSoBo.md) mục 3.
> Ví dụ minh hoạ lấy từ dự án tham khảo **Game xếp đơn hàng (Nhóm 06)**: [`reference/BaoCao_GameXepDonHang_Nhom06.md`](reference/BaoCao_GameXepDonHang_Nhom06.md).
>
> Mỗi slide gồm 3 phần:
> - **Trên slide:** chữ đưa lên slide (giữ ngắn, tối đa ~5 dòng).
> - **Hình:** sơ đồ cần vẽ. Slide 5–11 dùng cùng một kiểu **timeline hai trục** (Client trên, Server dưới, thời gian chạy sang phải, mũi tên chéo là gói tin).
> - **Lời nói:** ý chính người trình bày cần nói, không đọc nguyên văn.
>
> Số liệu đánh dấu *(cần đo lại)* là giá trị tham khảo, nhóm nên tự đo bằng `ping` trước khi đưa lên slide.

---

## Slide 1 — Tiêu đề

**Trên slide**

- **Real-time Multiplayer Game Networking: Client Prediction**
- Đề tài T47 · Môn Lập trình mạng · GV: Đặng Ngọc Hùng
- Nhóm: Nguyễn Thế Chiến (B23DCCN095, trưởng nhóm) · Phạm Quang Anh (B23DCCN044) · *TBD*

**Lời nói** (#1)

- Một câu mở: "Làm sao để game online cảm giác như chơi offline, dù gói tin mất 100–200 ms mới tới server?"

---

## Slide 2 — Bài toán

**Trên slide**

- Game real-time cần 2 thứ **mâu thuẫn nhau**:
  1. **Phản hồi tức thì:** bấm phím là nhân vật phải chạy ngay (người chơi nhận ra độ trễ từ khoảng 100 ms).
  2. **Một thế giới chung:** mọi người chơi thấy cùng một trạng thái, không ai gian lận được.
- Nhưng ánh sáng trong cáp quang cũng cần thời gian, và mạng thật còn tệ hơn thế.

| Kết nối | RTT điển hình *(cần đo lại)* |
|---|---|
| LAN / cùng máy | < 1 ms |
| Wi-Fi gia đình → server trong nước | 10–40 ms |
| 4G | 30–80 ms |
| Việt Nam → Singapore | 30–60 ms |
| Việt Nam → Mỹ | 180–250 ms |

**Hình:** bản đồ nhỏ với 2 người chơi và 1 server, ghi RTT trên mỗi đường nối.

**Lời nói** (#1, ~1')

- RTT (round-trip time) là thời gian đi và về. Với 200 ms, nếu chờ server xác nhận mới cho nhân vật chạy thì người chơi thấy rõ là "ì".
- Không thể làm mạng nhanh hơn, nên phải **che giấu** độ trễ bằng thuật toán. Đó là toàn bộ nội dung của đề tài.

---

## Slide 3 — Mạng không hoàn hảo

**Trên slide**

- **Latency:** gói tin mất thời gian để tới nơi.
- **Jitter:** độ trễ dao động, gói sau có thể đến sớm hơn gói trước.
- **Packet loss:** gói bị mất (Wi-Fi, 4G: thường 0,5–2%).
- **Out-of-order:** gói đến sai thứ tự.

| | TCP | UDP |
|---|---|---|
| Đảm bảo tới nơi, đúng thứ tự | Có | Không |
| Mất 1 gói | **Chặn mọi gói sau** cho tới khi gửi lại xong (head-of-line blocking) | Gói sau vẫn đến bình thường |
| Phù hợp | Đăng nhập, ghép trận, sự kiện quan trọng | Input, snapshot vị trí (dữ liệu cũ thì vô giá trị) |

**Hình:** hai đường thời gian TCP và UDP cùng gửi gói 1–2–3–4, gói 2 bị mất. Với TCP, gói 3 và 4 phải đợi gói 2 được gửi lại. Với UDP, gói 3 và 4 vẫn tới đúng hạn.

**Lời nói** (#1, ~1')

- Vị trí cũ 200 ms thì không còn giá trị, chờ gửi lại nó là vô ích. Vì vậy dữ liệu chuyển động đi bằng UDP.
- Vì UDP không đảm bảo gì, nhóm tự thêm **sequence number** để phát hiện mất gói và gói sai thứ tự. Đây cũng là chỗ dự án tham khảo còn thiếu.

---

## Slide 4 — Cách ngây thơ: client-authoritative

**Trên slide**

- Ví dụ thật: **Game xếp đơn hàng (Nhóm 06)**, Unity + .NET, TCP + UDP.
- *"Client ấn WASD sẽ liên tục gửi **vị trí** và trạng thái hiện tại tới server bằng UDP, server nhận được và trả về cho đối thủ."*
- *"Client sẽ **tự xử lý** xem đồ vật có trùng khớp với yêu cầu khách hàng, gửi thông tin đúng sai đến server."*
- Hậu quả:
  - **Gian lận:** sửa gói tin là teleport, chạy nhanh gấp đôi, hoặc báo "giao đúng" để được cộng điểm.
  - **Giật hình:** đối thủ được vẽ theo gói mới nhất, không có tick hay seq, nên jitter và mất gói hiện ngay lên màn hình.
  - **Không phân xử:** hai người cùng nhặt một món đồ thì không ai quyết định ai được.

**Hình:** Client A → (gửi `pos=(3,5)`) → Server → (chuyển tiếp) → Client B. Đánh dấu đỏ chữ "pos" kèm chú thích "server tin luôn".

**Lời nói** (#1, ~1')

- Nhấn mạnh: dự án này vẫn **chạy được** và đủ cho một bài tập lớn. Nhóm chọn nó làm baseline vì nó cho thấy đúng các vấn đề của mô hình đơn giản nhất.
- Nguyên tắc rút ra: **không bao giờ tin trạng thái do client gửi lên.**

---

## Slide 5 — Server authoritative + fixed tick

**Trên slide**

- Client **chỉ gửi input** (phím đang bấm), kèm `seq` (số thứ tự input).
- Server là **nguồn sự thật duy nhất**: chạy mô phỏng với bước cố định (tick 30 Hz, mỗi tick ≈ 33 ms) và gửi **snapshot** trạng thái về cho mọi client.
- Server kiểm tra được mọi thứ: tốc độ tối đa, va chạm, món đồ có thật trong tay không.
- Cái giá: **bấm phím xong phải chờ ≈ 1 RTT** mới thấy nhân vật chạy.

**Hình (timeline):**

```
Client A ──●──────────────────────────────●────────▶ t
           │ bấm D (seq=1)                ▲ thấy nhân vật bắt đầu chạy
           ╲                             ╱
            ╲  ~RTT/2                   ╱  ~RTT/2
             ▼                         ╱
Server ───────●── tick 41: áp input ──●── gửi snapshot ─────▶ t
              └────────── tổng cộng ≈ RTT + ≤1 tick ─────────┘
```

**Lời nói** (#1, ~1')

- Vì sao fixed tick: mô phỏng phải cho ra cùng kết quả bất kể máy nhanh hay chậm, và đó là điều kiện để client mô phỏng lại được ở slide sau.
- Với RTT 200 ms, độ trễ điều khiển khoảng 200–250 ms. Chống gian lận được nhưng chơi rất khó chịu. Cảnh 1 của demo sẽ cho thấy điều này.

---

## Slide 6 — Client-side prediction

**Trên slide**

- Ý tưởng: thường thì server **sẽ đồng ý** với input của mình, vậy client tự áp input ngay, không chờ.
- Client và server chạy **cùng một hàm mô phỏng** `Simulation.Step(state, input, dt)`, viết chung bằng C#.
- Kết quả: phản hồi tức thì như chơi offline. Server vẫn là bên quyết định cuối cùng.

**Hình (timeline):**

```
Client A ──●══ chạy ngay (dự đoán) ═══════════════════▶ t
           │ seq=1
            ╲
Server ──────●── xác nhận seq=1 ──────────────────────▶ t
```

**Lời nói** (#2, ~1')

- Prediction chỉ đúng khi mô phỏng **giống hệt** ở hai phía. Đó là lý do nhóm giữ stack Unity + .NET: một file C# thuần dùng cho cả hai, không phụ thuộc Unity Physics.
- Câu hỏi treo cho slide sau: khi snapshot của server tới, ta làm gì với nó?

---

## Slide 7 — Prediction một mình chưa đủ

**Trên slide**

- Snapshot mang trạng thái **của quá khứ** (cách đây ≈ RTT/2 + thời gian chờ tick).
- Nếu client ghi đè vị trí theo snapshot, nhân vật **bị kéo giật lùi** về chỗ cũ rồi lại chạy tiếp (*rubber-banding*).

**Hình:** trục x là vị trí. Ô xanh (dự đoán) đang ở x=10; snapshot tới báo x=6 (vị trí server thấy cách đây 200 ms); mũi tên đỏ kéo ô xanh lùi về 6.

**Lời nói** (#2, ~0,5')

- Server **không sai**: x=6 đúng là vị trí tại thời điểm server xử lý tới input seq=4. Vấn đề là client đã đi thêm các input seq=5…10 mà server chưa nhận.
- Cảnh 2 của demo: bật prediction, nhấp nhanh một phím là thấy ô nhảy lên rồi bị kéo về chỗ cũ.

---

## Slide 8 — Server reconciliation

**Trên slide**

- Client giữ **buffer các input chưa được xác nhận** (có seq).
- Snapshot gửi kèm `lastProcessedSeq` cho từng client: "tôi đã xử lý tới input số N của bạn".
- Khi nhận snapshot: **(1)** đặt lại trạng thái theo server, **(2)** bỏ input có seq ≤ N, **(3)** **replay** các input còn lại.
- Số input đang chờ ≈ RTT / tick (200 ms / 33 ms ≈ 6).

```csharp
// Mỗi tick ở client
var input = ReadInput(); input.Seq = ++seq;
pending.Add(input);
transport.Send(input);
state = Simulation.Step(state, input, Dt);       // prediction

// Khi nhận snapshot từ server
void OnSnapshot(Snapshot s) {
    state = s.MyState;                            // (1) tin server
    pending.RemoveAll(i => i.Seq <= s.LastProcessedSeq);   // (2)
    foreach (var i in pending)                    // (3) replay
        state = Simulation.Step(state, i, Dt);
}
```

**Hình (timeline):** client gửi seq 1…6; snapshot "x=6, last=4" quay về; khung bên cạnh vẽ buffer `[1 2 3 4 | 5 6]`, gạch bỏ 1–4, mũi tên "replay 5, 6 → x=10".

**Lời nói** (#2, ~1,5')

- Nếu mô phỏng khớp thì sau replay vị trí trùng với vị trí đang dự đoán, nên người chơi **không thấy gì**. Nếu khác (ví dụ server chặn vì va tường mà client chưa biết), client tự sửa về đúng.
- Lệch nhỏ thì có thể làm mượt (blend vài frame) thay vì nhảy ngay.
- Overlay trong demo hiển thị số input chờ và "bóng" vị trí server để khán giả thấy cơ chế này.

---

## Slide 9 — Entity interpolation

**Trên slide**

- Đối thủ thì **không đoán được** (không biết họ sắp bấm gì).
- Snapshot đến thưa (10–20 Hz) và không đều (jitter), vẽ theo gói mới nhất thì đối thủ **giật từng nấc**.
- Giải pháp: vẽ đối thủ **lùi lại một khoảng** `renderTime = now − interpDelay`, nội suy giữa 2 snapshot bao quanh thời điểm đó.
- Chọn delay: `interpDelay ≥ 2 × chu kỳ snapshot + jitter`, để mất 1 gói vẫn còn dữ liệu.
  - Ví dụ: snapshot 20 Hz (50 ms), jitter 0 thì delay = 100 ms (giá trị mặc định của Source Engine).
- **Đánh đổi:** ta luôn thấy đối thủ ở **quá khứ** (≈ interpDelay + RTT/2).

```csharp
float renderTime = clock.Now - InterpDelay;
// tìm s0, s1 trong buffer sao cho s0.Time <= renderTime < s1.Time
float t = (renderTime - s0.Time) / (s1.Time - s0.Time);
opponent.Position = Vector2.Lerp(s0.Pos, s1.Pos, t);
```

**Hình:** trục thời gian với các snapshot S1…S5 là chấm không đều; vạch "now" ở bên phải, vạch "renderTime" lùi về giữa S3 và S4; ô đối thủ nằm trên đoạn nối S3–S4.

**Lời nói** (#2, ~1,5')

- Interpolation chỉ áp cho **người khác**; nhân vật của mình vẫn dùng prediction. Vì thế trên màn hình mỗi người, hai nhân vật đang ở **hai mốc thời gian khác nhau**: mình ở hiện tại (dự đoán), đối thủ ở quá khứ. Điều này dẫn thẳng sang vấn đề của slide 10.
- Phương án khác là extrapolation / dead reckoning (đoán tiếp theo vận tốc): ít trễ hơn nhưng sai khi đối thủ đổi hướng.

---

## Slide 10 — Lag compensation

**Trên slide**

- Vấn đề: khi tôi bấm J nhặt món đồ, tôi đang nhìn đối thủ ở **quá khứ** (slide 9). Server mà dùng vị trí hiện tại để phân xử thì **người ping thấp luôn thắng**.
- Server lưu **lịch sử vị trí theo tick** (ring buffer, khoảng 1 giây = 30 tick).
- Mỗi hành động gửi kèm **tick mà client đang nhìn thấy** (renderTime ở slide 9).
- Server **tua lại** thế giới về tick đó, kiểm tra hành động, rồi trả lại hiện tại.
- Giới hạn tua tối đa (ví dụ 200–250 ms) để chống client gửi tick giả.

**Hình (timeline):** 2 client có RTT khác nhau (A: 40 ms, B: 200 ms) cùng bấm J trong khung nhìn của mình. Gói của B tới server sau gói của A. Không có lag compensation thì A thắng vì gói tới trước. Có lag compensation thì server tua về tick của từng người và thấy B chạm tới trước, nên B thắng.

**Lời nói** (#3, ~1,5')

- Ví dụ kinh điển là game bắn súng: "tôi bắn trúng đầu trên màn hình của tôi thì phải tính là trúng".
- Áp vào game của nhóm: món đồ hiếm chỉ có 1 cái, hai người cùng bấm J. Server dùng lag compensation để xác định ai chạm tới trước **theo góc nhìn của từng người**.
- **Cái giá:** công bằng cho người bắn/nhặt thì người kia chịu thiệt. Đây là hiện tượng "đã chạy đi rồi vẫn bị bắt": trên màn hình của mình đã rời đi, nhưng ở quá khứ mà đối thủ nhìn thấy thì mình vẫn đứng đó.

---

## Slide 11 — Rollback

**Trên slide**

- Client dự đoán cả **hành động** (nhặt món đồ hiện vào tay ngay), không chỉ di chuyển.
- Server từ chối (người kia được món đồ) thì client **quay về trạng thái đã xác nhận** rồi **mô phỏng lại**, và món đồ "bật ra khỏi tay".
- So với reconciliation: reconciliation là **trường hợp riêng** của rollback, chỉ áp cho di chuyển của chính mình. Rollback tổng quát áp cho **toàn bộ trạng thái**, kể cả input của người khác.
- **GGPO** (game đối kháng: Street Fighter, Mortal Kombat): đoán input đối thủ = input lần trước. Khi input thật tới mà khác, rollback N frame rồi mô phỏng lại trong 1 frame.
- Điều kiện bắt buộc: **determinism**. Cùng input thì phải ra cùng kết quả: cẩn thận float khác nhau giữa CPU, thứ tự xử lý entity, số ngẫu nhiên phải có seed chung.

**Hình (timeline):** client: tick 50 bấm J, món đồ vào tay (nét đứt = dự đoán); tick 56 nhận "REJECT"; mũi tên vòng ngược về tick 50, nhánh mới (nét liền) không có món đồ; mô phỏng lại 50 → 56.

**Lời nói** (#3, ~1')

- Bản game của nhóm có server authoritative nên rollback chỉ xảy ra phía client, cho các hành động đã dự đoán. GGPO là peer-to-peer, không có server, nên rollback là cơ chế chính.
- Trong game, hiệu ứng "bật ra" nên có animation để người chơi hiểu là bị tranh mất chứ không phải lỗi.

---

## Slide 12 — Tổng kết kỹ thuật

**Trên slide**

| Kỹ thuật | Giải quyết | Đánh đổi |
|---|---|---|
| Server authoritative + tick | Gian lận, không đồng bộ | Điều khiển trễ 1 RTT |
| Client-side prediction | Điều khiển trễ | Có thể dự đoán sai |
| Server reconciliation | Rubber-banding khi dự đoán sai | Phải lưu buffer và replay input; cần mô phỏng giống nhau |
| Entity interpolation | Đối thủ giật do snapshot thưa, jitter | Thấy đối thủ ở quá khứ |
| Lag compensation | Người ping cao luôn thua khi tranh chấp | Server tốn bộ nhớ lịch sử; người kia chịu thiệt ("đã chạy vẫn bị bắt") |
| Rollback | Dự đoán hành động sai | Cần determinism; tốn CPU mô phỏng lại |

**Lời nói** (#3, ~0,5')

- Không có kỹ thuật nào xoá được độ trễ. Mỗi kỹ thuật chỉ **chuyển** độ trễ sang chỗ người chơi ít để ý hơn.

---

## Slide 13 — Hướng áp dụng của nhóm

**Trên slide**

- Bối cảnh: game xếp đơn hàng 2 người (theo ý tưởng của Nhóm 06): WASD di chuyển, J nhặt / giao, K thả; mỗi khách yêu cầu 3 món, giao đúng thứ tự +5 điểm; trận 2 phút 30 giây.

| | Nhóm 06 (baseline) | T47 |
|---|---|---|
| Client gửi | Vị trí | Input + `seq` |
| Ai kiểm tra giao đồ | Client | Server |
| Nhân vật mình | Vẽ theo local | Prediction + reconciliation |
| Đối thủ | Gói mới nhất | Interpolation (~100 ms) |
| Tranh chấp | Không có | **Món đồ hiếm** ở kệ giữa: lag compensation + rollback |

- Stack: Unity (C#) + .NET server, TCP cho đăng nhập/ghép trận, UDP cho input/snapshot, SQLite.
- → **Chuyển sang demo:** 2 ô vuông, 3 khung nhìn (Client A · Server · Client B), mạng giả lập chỉnh được.

**Lời nói** (#3, ~0,5')

- Giữ ngắn, không giới thiệu tính năng game. Chỉ nói vì sao game này hợp để minh hoạ: có di chuyển liên tục (prediction, interpolation) và có tranh chấp tài nguyên (lag compensation, rollback).

---

## Phụ lục (slide dự phòng cho Q&A, không trình bày)

### P1 — Định dạng gói tin (nháp)

```
Input    (UDP, client → server):  type | seq:u32 | clientTick:u32 | keys:u8 (bitmask WASD/J/K)
Snapshot (UDP, server → client):  type | serverTick:u32 | lastProcessedSeq:u32 | n | entities[n]{id, x, y, holding}
Event    (TCP):                    LOGIN, MATCH_FOUND, ITEM_GRANTED/REJECTED, MATCH_END
```

- Client gửi kèm vài input gần nhất trong mỗi gói (redundancy), nên mất 1 gói UDP thì server vẫn có đủ input.
- Snapshot có `serverTick` nên client bỏ được gói đến muộn (tick nhỏ hơn gói đã nhận).

### P2 — Vì sao interpolation delay không thể nhỏ tuỳ ý

- Snapshot 10 Hz (100 ms) mà delay chỉ 100 ms thì mất 1 gói là hết dữ liệu để nội suy, đối thủ đứng khựng.
- Công thức `≥ 2 × chu kỳ + jitter`: 10 Hz, jitter 50 ms thì cần ~250 ms; 20 Hz thì ~100–150 ms.

### P3 — Mạng giả lập trong demo có đáng tin không

- UDP trên localhost có độ trễ ≈ 0, nên dù dùng socket thật cũng phải giả lập trễ. Bộ giả lập đưa gói vào hàng đợi, trả ra sau `latency ± jitter`, bỏ ngẫu nhiên theo tỉ lệ loss.
- Lớp truyền tin là interface `ITransport`, bản cuối thay bằng UDP thật mà không đổi code netcode.

---

## Việc cần làm tiếp với nội dung này

- [ ] Mỗi người đọc lại slide phần mình, sửa lời nói theo cách nói của mình.
- [ ] Đo RTT thật (slide 2) bằng `ping` tới vài server trong nước / Singapore.
- [ ] Vẽ 6 sơ đồ timeline (slide 5–11) theo cùng một mẫu.
- [ ] Chốt thông số demo cảnh 4–5 cho khớp phụ lục P2 (snapshot 10 Hz thì delay ~250 ms, hoặc tăng snapshot lên 20 Hz để giữ delay 100 ms).
