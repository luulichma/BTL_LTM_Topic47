# T47 — Nội dung thuyết trình

> Rút gọn từ [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md) cho đúng quỹ thời gian: **phần kỹ thuật ~12 phút (13 slide)**, demo 5–6 phút, Q&A 3–5 phút.
>
> Mỗi slide gồm:
>
> - **Trên slide:** chữ đưa lên slide (ngắn, tối đa ~5 dòng).
> - **Hình:** hình cần vẽ.
> - **Lời nói:** lời thuyết trình mẫu. Mỗi người tự sửa theo giọng của mình, không đọc thuộc lòng.
>
> Vai: **#1** server & giao thức · **#2** client · **#3** tương tác & demo.
> Khi bị hỏi sâu, tra trong tài liệu chi tiết và các slide dự phòng ở cuối file.

## Phân bổ thời gian

| # | Slide | Người | Thời gian | Cộng dồn |
|---|---|---|---|---|
| 1 | Tiêu đề | #1 | 0,25' | 0,25' |
| 2 | Bài toán: hai yêu cầu mâu thuẫn | #1 | 0,75' | 1' |
| 3 | Mạng không hoàn hảo | #1 | 1' | 2' |
| 4 | Chuỗi 5 vấn đề con | #1 | 0,5' | 2,5' |
| 5 | VĐ1: ai giữ quyền quyết định? | #1 | 1' | 3,5' |
| 6 | VĐ1: server quyết định, nhịp cố định, kênh truyền | #1 | 1' | 4,5' |
| 7 | VĐ2: điều khiển bị ì → dự đoán | #2 | 1,25' | 5,75' |
| 8 | VĐ2: đối chiếu với server | #2 | 1,25' | 7' |
| 9 | VĐ3: nội suy thực thể | #2 | 1,5' | 8,5' |
| 10 | VĐ4: bù trễ | #3 | 1,5' | 10' |
| 11 | VĐ5: quay lui | #3 | 1' | 11' |
| 12 | Tổng kết | #3 | 0,5' | 11,5' |
| 13 | Áp dụng: game xếp đơn hàng → demo | #3 | 0,5' | **12'** |
| — | Demo | #3 (+#2 điều khiển client B) | 5–6' | 17–18' |
| — | Q&A | Cả nhóm | 3–5' | 20–23' |

> Nếu tập thấy quá 20 phút, cắt trước tiên ở slide 4 (gộp vào slide 3) và slide 12 (gộp vào slide 13).

---

## Slide 1 — Tiêu đề (#1, 0,25')

**Trên slide**

- **Kỹ thuật mạng cho game nhiều người chơi thời gian thực: Dự đoán phía client**
- Đề tài T47 · Môn Lập trình mạng · GV: Đặng Ngọc Hùng
- Nguyễn Thế Chiến (B23DCCN095) · Phạm Quang Anh (B23DCCN044) · *TBD*

**Lời nói**

> Chào thầy và các bạn. Nhóm em trình bày đề tài T47 về kỹ thuật mạng cho game nhiều người chơi thời gian thực. Câu hỏi nhóm em muốn trả lời là: làm sao để chơi game qua mạng mà vẫn có cảm giác như chơi trên máy mình, khi mỗi gói tin mất cả trăm mili giây mới tới server.

---

## Slide 2 — Bài toán: hai yêu cầu mâu thuẫn (#1, 0,75')

**Trên slide**

- **Độ phản hồi:** bấm phím là nhân vật phản ứng ngay (từ ~100 ms đã thấy ì)
- **Tính nhất quán:** mọi người thấy cùng một thế giới, không ai gian lận được
- Có mạng ở giữa thì: muốn nhất quán phải **chờ**, muốn phản hồi ngay thì **không được chờ**

**Hình:** cái cân. Một đĩa ghi "Phản hồi tức thì", đĩa kia ghi "Nhất quán". Ở giữa là đám mây "Mạng: 100–200 ms".

**Lời nói**

> Game thời gian thực phải thoả hai yêu cầu cùng lúc. Một là độ phản hồi: bấm phím là nhân vật chạy ngay; trễ khoảng 100 mili giây là người chơi đã thấy ì. Hai là tính nhất quán: mọi người phải thấy cùng một thế giới, và không ai được tự ý sửa nó.
>
> Trên một máy thì hai yêu cầu này không mâu thuẫn. Nhưng khi có mạng ở giữa, muốn nhất quán thì phải chờ thông tin đi hết đường, còn muốn phản hồi ngay thì không được chờ. Toàn bộ đề tài là cách dung hoà hai yêu cầu đó.

---

## Slide 3 — Mạng không hoàn hảo (#1, 1')

**Trên slide**

- **Độ trễ:** có giới hạn vật lý. Hà Nội – Singapore, RTT tối thiểu ≈ 22 ms; thực tế 30–60 ms
- **Dao động độ trễ:** gửi đều 50 ms, nhận cách nhau 30, 80, 45 ms…
- **Mất gói, sai thứ tự:** Wi-Fi / 4G mất 0,5–2% *(cần đo lại)*
- **TCP:** mất 1 gói thì chặn mọi gói sau (**nghẽn đầu hàng**) · **UDP:** không chặn, nhưng không đảm bảo gì

**Hình:** hai dòng thời gian TCP và UDP cùng gửi gói 1–2–3–4, gói 2 bị mất. Với TCP, gói 3 và 4 phải chờ gói 2 được gửi lại. Với UDP, gói 3 và 4 vẫn đến đúng hạn.

**Lời nói**

> Vấn đề gốc nằm ở mạng. Thứ nhất là độ trễ, và nó có giới hạn vật lý: tín hiệu trong cáp quang đi khoảng 200 nghìn km mỗi giây, nên từ Hà Nội sang Singapore đi rồi về tối thiểu cũng 22 mili giây; đo thực tế thường 30 đến 60. Thứ hai, độ trễ không đều: gói gửi đều đặn nhưng đến lúc nhanh lúc chậm, gọi là dao động độ trễ. Thứ ba, gói có thể mất hoặc đến sai thứ tự.
>
> Hai giao thức xử lý chuyện này khác nhau. TCP đảm bảo thứ tự, nên khi mất một gói nó giữ lại mọi gói phía sau để chờ gửi lại; hiện tượng này gọi là nghẽn đầu hàng. UDP thì gói nào mất là mất, các gói sau vẫn đến bình thường.
>
> Kết luận là những hiện tượng này không thể loại bỏ, chỉ có thể che giấu.

---

## Slide 4 — Chuỗi 5 vấn đề con (#1, 0,5')

**Trên slide**

```
VĐ1 Ai giữ quyền quyết định? → VĐ2 Điều khiển bị ì → VĐ3 Đối thủ giật
        → VĐ4 Tương tác không công bằng → VĐ5 Dự đoán hành động bị sai
```

- Giải pháp của vấn đề trước **làm lộ ra** vấn đề sau
- Tiêu chí chọn: **phản hồi** và **nhất quán** (ưu tiên), chịu mạng xấu, độ phức tạp, phù hợp loại game

**Hình:** 5 ô nối bằng mũi tên. Trên mỗi mũi tên ghi hệ quả: "phải chờ server", "chỉ đoán được mình", "đối thủ ở quá khứ", "server có thể quyết khác".

**Lời nói**

> Nhóm em chia vấn đề gốc thành 5 vấn đề con, nối thành chuỗi: giải quyết xong vấn đề trước thì lộ ra vấn đề sau. Với mỗi vấn đề, nhóm em xét vài hướng giải pháp, so sánh, rồi chọn. Hai tiêu chí quan trọng nhất là độ phản hồi và tính nhất quán, đúng hai yêu cầu vừa nói. Ngoài ra còn xét khả năng chịu mạng xấu, độ phức tạp và độ phù hợp với loại game.

---

## Slide 5 — VĐ1: Ai giữ quyền quyết định trạng thái? (#1, 1')

**Trên slide**

| Hướng | Phản hồi | Chống gian lận | Ghi chú |
|---|---|---|---|
| Client tự quyết, server chuyển tiếp | ● | ○ | Sửa gói tin là dịch chuyển, hack tốc độ, tự cộng điểm |
| Đồng bộ theo bước (ngang hàng) | ○ | ◐ | Chờ người chậm nhất; cần tất định tuyệt đối |
| **Server quyết định** | ○ (giải ở VĐ2) | ● | Client chỉ gửi **lệnh điều khiển** |

**Hình:** hai sơ đồ nhỏ đặt cạnh nhau.
- Bên trái: Client gửi `vị trí = (3,5)`, server chuyển tiếp thẳng, kèm chú thích đỏ "server tin luôn".
- Bên phải: Client gửi `phím D`, server tự tính, rồi gửi về ảnh trạng thái.

**Lời nói**

> Vấn đề đầu tiên: khi mỗi máy giữ một bản sao thế giới, ai được quyền quyết định bản nào đúng?
>
> Cách đơn giản nhất là cho client tự quyết rồi server chuyển tiếp. Phản hồi rất tốt, nhưng client chạy trên máy người chơi, nên chỉ cần sửa gói tin là dịch chuyển được, chạy nhanh gấp đôi được, hoặc báo "tôi giao đúng hàng" để được cộng điểm.
>
> Cách thứ hai là đồng bộ theo bước, kiểu game chiến thuật: các máy chỉ gửi lệnh cho nhau rồi cùng mô phỏng. Nhưng cách này phải chờ người chậm nhất, và mô phỏng phải giống nhau đến từng bit.
>
> Nhóm em chọn cách thứ ba: server quyết định. Client chỉ được nói "tôi bấm phím gì", còn kết quả do server tính.

---

## Slide 6 — VĐ1: Server quyết định, nhịp cố định, kênh truyền (#1, 1')

**Trên slide**

- Server mô phỏng theo **nhịp cố định 30 lần/giây**, gửi **ảnh trạng thái** 20 lần/giây
- Mỗi nhịp xử lý tối đa 1 lệnh/người, nên gửi nhiều lệnh cũng không chạy nhanh hơn
- **TCP:** đăng nhập, ghép trận, kết quả quan trọng · **UDP:** lệnh, ảnh trạng thái
- Trên UDP tự thêm: **số thứ tự**, **xác nhận** (`lastProcessedSeq`), **gửi dư** 2–3 lệnh trước

**Hình:** dòng thời gian Client/Server. Client bấm D, mũi tên chéo lên server, server áp lệnh ở nhịp 41, mũi tên chéo về client. Ghi chú: "chờ ≈ 1 RTT".

**Lời nói**

> Server chạy mô phỏng theo nhịp cố định, 30 nhịp mỗi giây. Nhịp cố định giúp mô phỏng lặp lại được và cho client với server một đồng hồ chung là số nhịp. Mỗi nhịp server chỉ xử lý tối đa một lệnh cho mỗi người, nên dù client gửi dồn dập cũng không chạy nhanh hơn được.
>
> Về giao thức, nhóm em dùng cả hai. Dữ liệu chuyển động thì dùng UDP, vì vị trí cũ 200 mili giây thì gửi lại cũng vô ích. Đăng nhập, ghép trận, kết quả phân xử thì dùng TCP, vì bắt buộc phải tới nơi. Trên UDP, nhóm em tự thêm số thứ tự, xác nhận, và mỗi gói lệnh gửi dư vài lệnh trước để bù mất gói.
>
> Nhưng có một cái giá: nhìn dòng thời gian thì thấy, bấm phím xong phải chờ khoảng một RTT mới thấy nhân vật chạy. Phần này bạn [#2] trình bày tiếp.

---

## Slide 7 — VĐ2: Điều khiển bị ì → dự đoán phía client (#2, 1,25')

**Trên slide**

- Độ trễ điều khiển ≈ **RTT + tối đa 83 ms**. Với RTT 200 ms là **200–280 ms**
- Các hướng:
  - chờ server ○
  - che bằng hoạt ảnh ◐
  - **dự đoán phía client** ●
- Dự đoán: client **tự áp lệnh ngay**, server vẫn là người quyết định cuối
- Điều kiện: **dùng chung code mô phỏng** `Simulation.Step` (C# cho cả Unity và .NET)
- Nhưng chỉ dự đoán thôi thì bị **giật ngược**

**Hình:** trục vị trí. Ô xanh (dự đoán) ở x = 2,0. Ảnh trạng thái về báo x = 0,8. Mũi tên đỏ kéo ô xanh lùi về 0,8.

**Lời nói**

> Với RTT 200 mili giây, mỗi lần bấm phím phải chờ 200 đến gần 300 mili giây. Gian lận đã bị chặn, nhưng game không chơi được.
>
> Có thể chấp nhận chờ, hoặc che bằng hoạt ảnh lấy đà, nhưng di chuyển bằng WASD cần phản hồi ngay. Nhóm em chọn dự đoán phía client. Hầu như lúc nào server cũng đồng ý với lệnh của mình, nên client cứ tự áp lệnh luôn, không chờ. Điều kiện là client phải tính ra đúng kết quả như server, nên hai bên dùng chung một hàm mô phỏng. Đây là lý do nhóm em dùng C# cho cả Unity lẫn server .NET.
>
> Nhưng nếu chỉ dự đoán thì có vấn đề. Ảnh trạng thái từ server phản ánh quá khứ. Client đang ở x bằng 2, server báo 0,8; ghi đè vào thì nhân vật bị kéo giật lùi. Hiện tượng này gọi là giật ngược.

---

## Slide 8 — VĐ2: Đối chiếu với server (#2, 1,25')

**Trên slide**

1. Client giữ **bộ đệm** các lệnh chưa được xác nhận
2. Ảnh trạng thái kèm `lastProcessedSeq = N`
3. Nhận ảnh: **đặt lại** theo server → **bỏ** lệnh ≤ N → **chạy lại** lệnh còn lại

```csharp
state = s.MyState;
pending.RemoveAll(i => i.Seq <= s.LastProcessedSeq);
foreach (var i in pending) state = Simulation.Step(state, i, Dt);
```

**Hình:** bộ đệm `[1 2 3 4 | 5 6 7 8 9 10]`, gạch bỏ 1–4. Mũi tên "chạy lại 5–10: 0,8 + 6 × 0,2 = 2,0 ✓".

**Lời nói**

> Cách sửa là đối chiếu với server. Client giữ lại mọi lệnh đã gửi mà server chưa xác nhận. Server gửi ảnh trạng thái kèm số thứ tự lệnh cuối cùng nó đã xử lý.
>
> Ví dụ: client đã gửi 10 lệnh, mỗi lệnh đi 0,2 đơn vị, đang dự đoán ở x bằng 2. Server báo "x bằng 0,8, đã xử lý tới lệnh 4". Client đặt lại x bằng 0,8, bỏ lệnh 1 đến 4, chạy lại 6 lệnh còn lại, và được đúng 2,0. Người chơi không thấy gì.
>
> Chỉ khi dự đoán thật sự sai, ví dụ server thấy có va chạm mà client không biết, thì vị trí mới thay đổi; lệch nhỏ thì làm mượt trong vài khung hình. Trong demo, các bạn sẽ thấy số lệnh trong bộ đệm luôn xấp xỉ RTT chia độ dài một nhịp.

---

## Slide 9 — VĐ3: Đối thủ hiển thị giật → nội suy thực thể (#2, 1,5')

**Trên slide**

- Đối thủ thì **không đoán được**. Ảnh trạng thái thưa (10–20 lần/giây), không đều, có thể mất
- Các hướng:
  - vẽ ảnh mới nhất: giật ○
  - ngoại suy: sai khi đổi hướng ○
  - **nội suy** ●
- **Vẽ đối thủ lùi lại** một khoảng, nội suy giữa 2 ảnh trạng thái bao quanh
- Độ trễ nội suy ≥ **2 × chu kỳ gửi ảnh + dao động độ trễ** (20 lần/giây thì ~100 ms)
- Đánh đổi: **mình ở hiện tại, đối thủ ở quá khứ**

**Hình:** trục thời gian với các ảnh S1…S5 là các chấm đặt không đều. Vạch "hiện tại" ở bên phải, vạch "thời điểm vẽ" lùi về giữa S3 và S4. Ô đối thủ nằm trên đoạn nối S3–S4.

**Lời nói**

> Dự đoán chỉ dùng được cho chính mình, vì mình không biết đối thủ sắp bấm gì. Thông tin về đối thủ chỉ đến qua ảnh trạng thái, mà ảnh thì thưa, không đều và có thể mất. Vẽ theo ảnh mới nhất thì đối thủ đứng yên rồi nhảy cóc. Ngoại suy theo vận tốc thì sai mỗi khi đối thủ đổi hướng.
>
> Nhóm em chọn nội suy: cố ý vẽ đối thủ lùi lại một khoảng, ví dụ 100 mili giây. Nhờ lùi lại, ta luôn có hai ảnh trạng thái kẹp hai bên thời điểm vẽ, và chỉ cần nội suy đường thẳng giữa chúng. Khoảng lùi phải ít nhất bằng hai chu kỳ gửi ảnh cộng dao động độ trễ, để mất một gói vẫn còn dữ liệu.
>
> Cái giá là ta luôn thấy đối thủ ở quá khứ. Như vậy trên màn hình mỗi người có hai mốc thời gian: mình ở hiện tại, đối thủ ở quá khứ. Khi hai người tranh cùng một thứ thì sao? Bạn [#3] trình bày tiếp.

---

## Slide 10 — VĐ4: Tương tác không công bằng → bù trễ (#3, 1,5')

**Trên slide**

- A (RTT 40 ms) và B (RTT 200 ms) cùng nhặt một vật; trên màn hình, **B chạm trước**
- Phân xử theo thứ tự gói đến thì **A luôn thắng**: người trễ thấp luôn được lợi
- Các hướng:
  - thứ tự gói đến ○
  - client tự quyết ○ (gian lận)
  - **bù trễ** ●
- **Bù trễ:** server lưu lịch sử 1 giây, **tua lại** về thời điểm người chơi nhìn thấy rồi mới phân xử
- Giới hạn tua tối đa ~200–250 ms · Đánh đổi: **"đã chạy đi rồi vẫn bị bắt"**

**Hình:** dòng thời gian có hai client với độ dài mũi tên khác nhau.
- Gói của B đến server sau gói của A.
- Không bù trễ: A thắng.
- Có bù trễ: server tua về thời điểm của từng người, thấy B trước nên B thắng.

**Lời nói**

> Ví dụ kinh điển là game bắn súng: trên màn hình tôi ngắm trúng đầu, nhưng thứ tôi thấy là đối thủ của 200 mili giây trước. Lệnh bắn tới server thì đối thủ đã đi chỗ khác.
>
> Với vật dùng chung cũng vậy. A ping 40, B ping 200, trên màn hình B chạm vật trước. Nhưng gói của A tới server nhanh hơn, nên nếu xử theo thứ tự gói đến thì A luôn thắng. Để client tự quyết thì quay lại chuyện gian lận.
>
> Giải pháp là bù trễ. Server lưu lịch sử trạng thái khoảng một giây. Mỗi hành động gửi kèm thời điểm người chơi đang nhìn thấy. Server tua lại về đúng lúc đó, kiểm tra, rồi mới phân xử. Để chống gửi thời điểm giả, server giới hạn chỉ tua tối đa khoảng 200 mili giây.
>
> Đánh đổi là công bằng cho người hành động thì người kia chịu thiệt: trên màn hình mình đã chạy đi rồi, vẫn bị tính là bị bắt. Không cách nào làm cả hai cùng thấy công bằng.

---

## Slide 11 — VĐ5: Dự đoán hành động bị sai → quay lui (#3, 1')

**Trên slide**

- Client đã hiển thị "nhặt được", server báo **từ chối**
- Các hướng:
  - không dự đoán: chờ 1 RTT ○
  - **dự đoán + quay lui cục bộ** ●
  - quay lui toàn phần kiểu GGPO: cho mô hình ngang hàng ○
- **Quay lui:** về trạng thái đã xác nhận → mô phỏng lại → vật "bật ra khỏi tay"
- Đối chiếu (VĐ2) là **trường hợp riêng** của quay lui
- Quay lui ngang hàng cần **tính tất định tuyệt đối**; mô hình server quyết định chỉ cần "gần giống"

**Hình:** dòng thời gian client. Nhịp 50 bấm nhặt (nét đứt: dự đoán); nhịp 56 nhận "TỪ CHỐI"; mũi tên vòng ngược về nhịp 50; nhánh mới (nét liền) không cầm vật.

**Lời nói**

> Kết quả phân xử về muộn, mà client đã cho người chơi cầm vật từ trước, vì không muốn bắt họ chờ. Nếu thua thì sao? Client quay về trạng thái đã được xác nhận, mô phỏng lại với thông tin đúng, và người chơi thấy vật bật ra khỏi tay. Đó là quay lui.
>
> Đối chiếu ở VĐ2 thực chất là trường hợp riêng của quay lui, chỉ áp cho di chuyển của mình. Game đối kháng như Street Fighter dùng quay lui toàn phần theo mô hình GGPO, chạy ngang hàng. Mô hình đó đòi hỏi mô phỏng giống nhau đến từng bit giữa các máy. Nhóm em có server sửa định kỳ, nên chỉ cần gần giống.

---

## Slide 12 — Tổng kết (#3, 0,5')

**Trên slide**

| Vấn đề | Giải pháp | Đánh đổi |
|---|---|---|
| Ai quyết định | Server quyết định | Trễ 1 RTT |
| Điều khiển ì | Dự đoán + đối chiếu | Cần mô phỏng chung |
| Đối thủ giật | Nội suy | Thấy đối thủ ở quá khứ |
| Tranh chấp không công bằng | Bù trễ | "Chạy rồi vẫn bị bắt" |
| Dự đoán sai | Quay lui | Thỉnh thoảng thấy hành động bị huỷ |

- **Không kỹ thuật nào xoá được độ trễ, chỉ chuyển nó sang chỗ ít bị để ý hơn**

**Lời nói**

> Tóm lại, không có kỹ thuật nào xoá được độ trễ. Mỗi kỹ thuật chuyển độ trễ sang chỗ người chơi ít để ý hơn: mình đi trước nhờ dự đoán, đối thủ đi sau nhờ nội suy, và server bù lại sự lệch thời gian khi hai bên tương tác.

---

## Slide 13 — Áp dụng: game xếp đơn hàng siêu thị → demo (#3, 0,5')

**Trên slide**

- Game 2 người, mỗi người là nhân viên siêu thị: WASD di chuyển, J nhặt / giao, K thả
- Mỗi khách cần 3 món theo thứ tự, giao đúng +5 điểm, trận 2 phút 30 giây
- **Món hàng hiếm** chỉ có 1 cái ở kệ giữa → tranh chấp → bù trễ + quay lui
- Unity (C#) + .NET server · TCP + UDP · SQLite
- **Demo hôm nay:** bản chứng minh ý tưởng, 2 ô vuông, 3 khung nhìn, mạng giả lập

**Hình:** ảnh phác bản đồ siêu thị: hai quầy khách hai bên, kệ hàng, món hàng hiếm phát sáng ở giữa.

**Lời nói**

> Nhóm em sẽ áp dụng các kỹ thuật này vào game xếp đơn hàng siêu thị cho 2 người. Game có di chuyển liên tục, nên cần dự đoán và nội suy. Món hàng hiếm chỉ có một cái, hai người phải tranh nhau, nên cần bù trễ và quay lui. Hôm nay nhóm em demo bản chứng minh ý tưởng cho phần di chuyển.

---

## Demo (#3 dẫn, #2 điều khiển client B; 5–6')

Màn hình chia 3 khung: **Client A · Server · Client B**. Thanh trượt chỉnh độ trễ, dao động độ trễ, mất gói, tần số gửi ảnh.

| Cảnh | Cấu hình | Thao tác | Lời dẫn |
|---|---|---|---|
| 0 | Client tự quyết, trễ 100 ms | Bấm nút "dịch chuyển" | "Đây là cách client tự quyết. Em giả lập sửa gói tin, ô của A nhảy sang đây, và server lẫn B đều chấp nhận." |
| 1 | Server quyết định, trễ 200 ms, tắt hết | A chạy qua lại; bấm lại "dịch chuyển" | "Chuyển sang server quyết định: dịch chuyển hết tác dụng. Nhưng thầy để ý, em bấm phím khoảng 0,2 giây sau ô mới chạy." |
| 2 | Bật dự đoán | Chạy rồi dừng đột ngột | "Bật dự đoán: phản hồi ngay. Nhưng khi dừng lại, ô bị kéo giật lùi, đó là giật ngược." |
| 3 | Bật đối chiếu | Lặp lại | "Bật đối chiếu: mượt và khớp với bóng xám là vị trí theo server. Bộ đệm đang có khoảng 6 lệnh, đúng bằng RTT chia nhịp." |
| 4 | Nhìn khung B; gửi ảnh 10 lần/giây, dao động 50 ms | A chạy vòng tròn | "Giờ nhìn sang màn hình của B: ô của A giật từng nấc vì ảnh trạng thái đến thưa và không đều." |
| 5 | Bật nội suy (độ trễ nội suy tự tính ≈ 250 ms); sau đó tăng lên 20 lần/giây | Lặp lại | "Bật nội suy: mượt, nhưng so với khung server thì ô của A trễ hơn, đó là đánh đổi. Tăng tần số gửi ảnh lên 20 thì độ trễ giảm còn khoảng 150 mili giây." |

**Phương án dự phòng:** quay sẵn video toàn bộ 6 cảnh. Demo lỗi thì chuyển sang video ngay, không sửa lỗi trên sân khấu.

---

## Slide dự phòng cho Q&A (không trình bày)

Chỉ mở khi bị hỏi. Nội dung lấy từ [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md).

| # | Slide | Dùng khi bị hỏi | Nguồn |
|---|---|---|---|
| D1 | Thành phần của độ trễ (lan truyền, truyền, xếp hàng, xử lý) | "Độ trễ đến từ đâu, giảm được không?" | Mục 2.1 |
| D2 | TCP chi tiết: thời gian chờ gửi lại, thuật toán Nagle | "Sao không dùng TCP cho tất cả, tắt Nagle là xong?" | Mục 2.3 |
| D3 | Bộ tích luỹ thời gian, tần số mô phỏng và tần số gửi ảnh | "Chọn 30 nhịp/giây thế nào? Client 144 Hz thì sao?" | VĐ1 |
| D4 | Hàng đợi lệnh ở server: thiếu lệnh, dồn lệnh, chống hack tốc độ | "Lệnh đến không đều thì server làm gì?" | VĐ1 |
| D5 | Bảng giao thức (`INPUT`, `SNAPSHOT`, `ITEM_*`) | "Định dạng gói tin thế nào?" | Mục 5 |
| D6 | Bảng chọn độ trễ nội suy theo tần số gửi ảnh, ngoại suy dự phòng | "Độ trễ nội suy chọn bao nhiêu? Mất nhiều gói thì sao?" | VĐ3 |
| D7 | Cửa sổ phân xử cho món hàng hiếm, kiểm tra số nhịp giả | "Hai người cùng nhặt thì xử thế nào? Gửi nhịp giả thì sao?" | VĐ4 |
| D8 | Các nguồn phá tính tất định (số thực, thứ tự duyệt, số ngẫu nhiên) | "Tính tất định ảnh hưởng thế nào?" | VĐ5 |
| D9 | So sánh đồng bộ theo bước / GGPO với mô hình của nhóm | "Sao không làm ngang hàng?" | VĐ1, VĐ5 |
| D10 | Mạng giả lập: `ITransport`, cách tạo độ trễ, mất gói | "Demo có phản ánh mạng thật không?" | Mục 7.2 |

Câu trả lời mẫu: [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md), phụ lục A. Bảng thuật ngữ Việt – Anh: phụ lục B.

---

## Ghi chú tập luyện

- **Chuyển người nói** ở cuối slide 6 (#1 → #2) và cuối slide 9 (#2 → #3). Câu chuyển là câu hỏi dẫn sang vấn đề tiếp theo, đã viết sẵn trong lời nói.
- **Bấm giờ theo cột cộng dồn.** Đến slide 9 mà đã quá 9 phút thì nói nhanh slide 10–11, bỏ slide 12.
- **Mỗi người tự trả lời được** các câu Q&A thuộc vấn đề con của mình (phụ lục A có gắn nhãn VĐ).
- Số liệu *(cần đo lại)* ở slide 3 phải thay bằng số thật trước buổi trình bày.
