# T47 — Nội dung thuyết trình

> Bám theo slide [`slides/T47_ThuyetTrinh_Khung_New.pptx`](../slides/T47_ThuyetTrinh_Khung_New.pptx): **4 phần, 18 slide trình bày + 13 slide dự phòng**. Số slide trong file này trùng với số trang trên slide.
> Quỹ thời gian: **phần kỹ thuật ~11 phút** (kể cả 3 slide chuyển phần, mỗi slide ~5 giây), demo 5 phút, Q&A 3–5 phút.
> Nội dung gốc, đầy đủ hơn: [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md).
>
> Mỗi slide gồm:
>
> - **Trên slide:** chữ đang có trên slide.
> - **Hình:** hình minh hoạ trên slide.
> - **Lời nói:** lời thuyết trình mẫu (giống phần ghi chú của slide). Mỗi người tự sửa theo giọng của mình, không đọc thuộc lòng.
> - **Ghi chú:** việc cần nhớ, slide dự phòng cần mở khi bị hỏi.
>
> Vai: **#1** server & giao thức · **#2** client · **#3** tương tác & demo.

## Cấu trúc và phân bổ thời gian

| Slide | Nội dung | Người | Thời gian | Cộng dồn |
|---|---|---|---|---|
| 1 | Tiêu đề | #1 | 0,25' | 0,25' |
| 2 | Mục lục | #1 | 0,25' | 0,5' |
| **3** | **Phần 1 — Vấn đề gốc: bài toán đường truyền mạng** | #1 | ~5" | |
| 4 | a. Bài toán: chơi game thời gian thực qua mạng | #1 | 0,5' | 1' |
| 5 | b. Hai yêu cầu mâu thuẫn | #1 | 0,5' | 1,5' |
| 6 | c. Mạng không hoàn hảo (+ chia 5 vấn đề con) | #1 | 1' | 2,5' |
| **7** | **Phần 2 — Các vấn đề con và giải pháp** | #1 | ~5" | |
| 8 | VĐ1: Ai giữ quyền quyết định trạng thái? | #1 | 0,75' | 3,25' |
| 9 | VĐ1: Server quyết định, nhịp cố định, kênh truyền | #1 | 0,75' | 4' |
| 10 | VĐ2: Điều khiển bị ì → dự đoán phía client | #2 | 1,25' | 5,25' |
| 11 | VĐ2: Đối chiếu với server | #2 | 1,25' | 6,5' |
| 12 | VĐ3: Đối thủ hiển thị giật → nội suy thực thể | #2 | 1,5' | 8' |
| 13 | VĐ4: Tương tác không công bằng → bù trễ | #3 | 1,5' | 9,5' |
| 14 | VĐ5: Dự đoán hành động bị sai → quay lui | #3 | 0,75' | 10,25' |
| **15** | **Phần 3 — Áp dụng và demo** | #3 | ~5" | |
| 16 | Game xếp đơn hàng siêu thị → demo | #3 | 0,5' | 10,75' |
| 17 | Demo: bản chứng minh ý tưởng, phần di chuyển | #3 (+#2 điều khiển client B) | 5' | ~16' |
| **18** | **Phần 4 — Q&A** | Cả nhóm | 3–5' | 19–21' |
| 19–31 | Phụ lục: slide dự phòng (không trình bày) | | | |

Chuyển người nói ở cuối slide 9 (#1 → #2) và cuối slide 12 (#2 → #3).

---

## Mở đầu (#1)

### Slide 1 — Tiêu đề (#1, 0,25')

**Trên slide**

- Đề tài T47 · Môn Lập trình mạng
- **Kỹ thuật mạng cho game nhiều người chơi thời gian thực** · Dự đoán phía client
- GV: TS. Đặng Ngọc Hùng
- Nguyễn Thế Chiến (B23DCCN095) · Phạm Quang Anh (B23DCCN044) · Nguyễn Đức Anh (B23DCCN031)

**Hình:** sơ đồ Server nối với Client A và Client B, các gói tin đang đi trên đường truyền, chú thích "RTT 100–200 ms". Sơ đồ này lặp lại trên các slide chuyển phần.

**Lời nói**

> Chào thầy và các bạn. Nhóm em trình bày đề tài T47 về kỹ thuật mạng cho game nhiều người chơi thời gian thực. Câu hỏi nhóm em muốn trả lời là: làm sao để chơi game qua mạng mà vẫn có cảm giác như chơi trên máy mình, khi mỗi gói tin mất cả trăm mili giây mới tới server.

---

### Slide 2 — Mục lục (#1, 0,25')

**Trên slide:** 4 thẻ, mỗi thẻ ghi tên phần, các mục nhỏ và người trình bày.

| Phần | Mục nhỏ | Người |
|---|---|---|
| 01 · Vấn đề gốc: bài toán đường truyền mạng | a. Bài toán · b. Hai yêu cầu mâu thuẫn · c. Mạng không hoàn hảo | #1 |
| 02 · Các vấn đề con và giải pháp | a. VĐ1 Ai giữ quyền quyết định · b. VĐ2 Điều khiển bị ì · c. VĐ3 Đối thủ giật · d. VĐ4 Không công bằng · e. VĐ5 Dự đoán hành động sai | #1, #2, #3 |
| 03 · Áp dụng và demo | a. Game xếp đơn hàng siêu thị · b. Demo 3 khung nhìn | #3, #2 |
| 04 · Q&A | a. Hỏi & đáp · b. Slide dự phòng khi bị hỏi sâu | Cả nhóm |

**Lời nói**

> Bài trình bày của nhóm em gồm 4 phần: vấn đề gốc là bài toán đường truyền mạng; các vấn đề con và giải pháp; áp dụng và demo; cuối cùng là phần hỏi đáp.

---

## Phần 1 — Vấn đề gốc: bài toán đường truyền mạng (#1)

### Slide 3 — Chuyển phần 1 (~5")

**Trên slide**

- **PHẦN 1** · Vấn đề gốc: bài toán đường truyền mạng
- Mạng luôn có độ trễ, dao động độ trễ và mất gói
- Bài toán · Hai yêu cầu mâu thuẫn · Mạng không hoàn hảo · Trình bày: #1

**Lời nói**

> Đầu tiên là vấn đề gốc: bài toán đường truyền mạng.

---

### Slide 4 — a. Bài toán: chơi game thời gian thực qua mạng (#1, 0,5')

**Trên slide**

- **Trên một máy:** bấm phím là thấy ngay; mọi người nhìn chung một màn hình, chung một trạng thái
- **Qua mạng:** mỗi thao tác mất 100–200 ms mới tới máy khác; gói tin có thể đến trễ, sai thứ tự hoặc mất
- Bài toán: làm sao chơi qua mạng mà vẫn có cảm giác **như chơi trên cùng một máy**?

**Hình:** hai thẻ "Trên một máy" và "Qua mạng", ở giữa là khối "Mạng 100–200 ms".

**Lời nói**

> Game nhiều người chơi thời gian thực là game có nhiều người cùng chơi trong một thế giới chung, và trạng thái thay đổi liên tục theo thao tác của họ: bắn súng, đối kháng, đua xe. Khác với cờ vua trực tuyến, ở đây từng phần trăm giây đều có ý nghĩa.
>
> Nếu mọi người chơi trên cùng một máy thì bấm phím là thấy ngay. Nhưng qua mạng, mỗi thao tác mất 100 đến 200 mili giây mới tới máy khác, có lúc đến trễ, sai thứ tự hoặc mất hẳn. Bài toán của nhóm em là: làm sao chơi qua mạng mà vẫn có cảm giác như chơi trên cùng một máy.

---

### Slide 5 — b. Hai yêu cầu mâu thuẫn (#1, 0,5')

**Trên slide**

- **Độ phản hồi:** bấm phím là nhân vật phản ứng ngay (từ ~100 ms đã thấy ì)
- **Tính nhất quán:** mọi người thấy cùng một thế giới, không ai gian lận được
- Có mạng ở giữa thì: muốn nhất quán phải **chờ**, muốn phản hồi ngay thì **không được chờ**

**Hình:** hai thẻ "Độ phản hồi" và "Tính nhất quán", ở giữa là khối "Mạng 100–200 ms".

**Lời nói**

> Muốn vậy, game phải thoả hai yêu cầu cùng lúc. Một là độ phản hồi: bấm phím là nhân vật chạy ngay; trễ khoảng 100 mili giây là người chơi đã thấy ì. Hai là tính nhất quán: mọi người phải thấy cùng một thế giới, và không ai được tự ý sửa nó.
>
> Trên một máy thì hai yêu cầu này không mâu thuẫn. Nhưng khi có mạng ở giữa, muốn nhất quán thì phải chờ thông tin đi hết đường, còn muốn phản hồi ngay thì không được chờ. Toàn bộ đề tài là cách dung hoà hai yêu cầu đó.

---

### Slide 6 — c. Mạng không hoàn hảo (#1, 1')

**Trên slide**

- **Độ trễ:** không thể về 0 (VN → Singapore: 30–60 ms)
- **Dao động độ trễ:** gửi đều 50 ms, nhận cách nhau 30, 80, 45 ms…
- **Mất gói, sai thứ tự:** Wi-Fi / 4G mất 0,5–2% *(cần đo lại)*
- **TCP:** mất 1 gói thì chặn mọi gói sau (**nghẽn đầu hàng**)
- **UDP:** không chặn, nhưng không đảm bảo gì
- Chia thành 5 vấn đề con: **VĐ1** Ai quyết định → **VĐ2** Điều khiển ì → **VĐ3** Đối thủ giật → **VĐ4** Không công bằng → **VĐ5** Dự đoán sai. Giải pháp của vấn đề trước **làm lộ ra** vấn đề sau
- Câu chốt: *Không thể loại bỏ, chỉ có thể che giấu*

**Hình:** hai dòng thời gian TCP và UDP cùng gửi gói 1–4, gói 2 bị mất. TCP: gói 3, 4 chờ gói 2 gửi lại, ứng dụng nhận 1, 2, 3, 4. UDP: ứng dụng nhận ngay 1, 3, 4.

**Lời nói**

> Vấn đề gốc nằm ở mạng. Thứ nhất là độ trễ: nó có giới hạn vật lý nên không bao giờ về 0; từ Việt Nam sang Singapore đo thực tế khoảng 30 đến 60 mili giây. Thứ hai, độ trễ không đều: gói gửi đều đặn nhưng đến lúc nhanh lúc chậm, gọi là dao động độ trễ. Thứ ba, gói có thể mất hoặc đến sai thứ tự.
>
> Hai giao thức xử lý chuyện này khác nhau. TCP đảm bảo thứ tự, nên khi mất một gói nó giữ lại mọi gói phía sau để chờ gửi lại; hiện tượng này gọi là nghẽn đầu hàng. UDP thì gói nào mất là mất, các gói sau vẫn đến bình thường.
>
> Những hiện tượng này không thể loại bỏ, chỉ có thể che giấu. Nhóm em chia bài toán thành 5 vấn đề con nối thành chuỗi: giải quyết vấn đề trước thì lộ ra vấn đề sau. Với mỗi vấn đề, nhóm em so sánh vài hướng theo hai tiêu chí chính là độ phản hồi và tính nhất quán, rồi chọn.

**Ghi chú**

- **Việc cần làm:** thay số liệu mất gói *(cần đo lại)* bằng số đo thật trước buổi trình bày.
- Bị hỏi phép tính giới hạn vật lý: Hà Nội – Singapore ≈ 2.200 km, cáp quang ≈ 200.000 km/s → RTT tối thiểu ≈ 22 ms (slide dự phòng D1).
- Muốn giải thích lại mạch 5 vấn đề con và tiêu chí chọn giải pháp: mở slide dự phòng 20.

---

## Phần 2 — Các vấn đề con và giải pháp (#1 → #2 → #3)

### Slide 7 — Chuyển phần 2 (~5")

**Trên slide**

- **PHẦN 2** · Các vấn đề con và giải pháp
- Giải pháp của vấn đề trước làm lộ ra vấn đề sau
- a. VĐ1 Ai giữ quyền quyết định · b. VĐ2 Điều khiển bị ì · c. VĐ3 Đối thủ giật · d. VĐ4 Không công bằng · e. VĐ5 Dự đoán hành động sai

**Lời nói**

> Phần hai: các vấn đề con và giải pháp. Với mỗi vấn đề, nhóm em đi theo thứ tự: vấn đề, các hướng giải pháp, so sánh, rồi chọn.

---

### Slide 8 — VĐ1: Ai giữ quyền quyết định trạng thái? (#1, 0,75')

**Trên slide**

| Hướng | Phản hồi | Chống gian lận | Ghi chú |
|---|---|---|---|
| Client tự quyết, server chuyển tiếp | ● | ○ | Sửa gói tin là dịch chuyển, hack tốc độ, tự cộng điểm |
| Đồng bộ theo bước (ngang hàng) | ○ | ◐ | Chờ người chậm nhất |
| **Server quyết định** | ○ (giải ở VĐ2) | ● | **Client chỉ gửi lệnh điều khiển** |

**Hình:** hai sơ đồ đặt cạnh nhau. "Client tự quyết": client gửi `vị trí = (3,5)`, server chuyển tiếp thẳng cho client B, chú thích đỏ "Server tin luôn". "Server quyết định": client gửi `phím D`, server tự mô phỏng rồi gửi ảnh trạng thái cho cả hai client.

**Lời nói**

> Vấn đề đầu tiên: khi mỗi máy giữ một bản sao thế giới, ai được quyền quyết định bản nào đúng?
>
> Cho client tự quyết thì phản hồi rất tốt, nhưng client chạy trên máy người chơi: sửa gói tin là dịch chuyển được, chạy nhanh gấp đôi được, tự cộng điểm được. Đồng bộ theo bước kiểu game chiến thuật thì phải chờ người chậm nhất.
>
> Nhóm em chọn server quyết định: client chỉ được nói "tôi bấm phím gì", còn kết quả do server tính.

**Ghi chú:** bị hỏi sâu về đồng bộ theo bước / ngang hàng, tính tất định: mở D9.

---

### Slide 9 — VĐ1: Server quyết định, nhịp cố định, kênh truyền (#1, 0,75')

**Trên slide**

- **30** nhịp mô phỏng mỗi giây (cố định) · **20** ảnh trạng thái gửi mỗi giây
- Mỗi nhịp xử lý tối đa **1 lệnh/người**, nên gửi nhiều lệnh cũng không chạy nhanh hơn
- **TCP:** đăng nhập, ghép trận, kết quả quan trọng · **UDP:** lệnh, ảnh trạng thái

**Hình:** dòng thời gian Client/Server. Client bấm D, mũi tên "Gửi lệnh" chéo lên server; server áp lệnh ở nhịp 41; mũi tên "Ảnh trạng thái" chéo về client, lúc đó nhân vật mới hiển thị. Chú thích "Chờ ≈ 1 RTT".

**Lời nói**

> Server chạy mô phỏng theo nhịp cố định, 30 nhịp mỗi giây, nên mô phỏng lặp lại được và hai bên có chung một đồng hồ là số nhịp. Mỗi nhịp server chỉ xử lý tối đa một lệnh cho mỗi người, nên gửi dồn dập cũng không chạy nhanh hơn.
>
> Dữ liệu chuyển động đi bằng UDP, vì vị trí cũ 200 mili giây thì gửi lại cũng vô ích. Đăng nhập và kết quả quan trọng đi bằng TCP, vì bắt buộc phải tới nơi.
>
> Nhưng có một cái giá: nhìn dòng thời gian thì thấy, bấm phím xong phải chờ khoảng một RTT mới thấy nhân vật chạy. Phần này bạn [#2] trình bày tiếp.

**Ghi chú**

- Bị hỏi cơ chế bổ sung trên UDP (số thứ tự, xác nhận `lastProcessedSeq`, gửi dư 2–3 lệnh trước): mở D4, D5.
- **Chuyển người nói: #1 → #2.**

---

### Slide 10 — VĐ2: Điều khiển bị ì → dự đoán phía client (#2, 1,25')

**Trên slide**

- Độ trễ điều khiển ≈ **RTT + tối đa 83 ms**. Với RTT 200 ms là **200–280 ms**
- Các hướng: chờ server ○ · che bằng hoạt ảnh ◐ · **dự đoán phía client** ● (đã chọn)
- Dự đoán: client **tự áp lệnh ngay**, server vẫn là người quyết định cuối
- Điều kiện: **dùng chung code mô phỏng** `Simulation.step` (một file, chạy ở cả client và server)
- Nhưng chỉ dự đoán thôi thì bị **giật ngược**

**Hình:** "Giật ngược". Trục x: client dự đoán ở 2,0; ảnh trạng thái server báo x = 0,8; mũi tên đỏ "Ghi đè vị trí" kéo nhân vật lùi về 0,8. Chú thích "Ảnh cũ kéo nhân vật lùi lại".

**Lời nói**

> Với RTT 200 mili giây, mỗi lần bấm phím phải chờ 200 đến gần 300 mili giây. Gian lận đã bị chặn, nhưng game không chơi được.
>
> Có thể chấp nhận chờ, hoặc che bằng hoạt ảnh lấy đà, nhưng di chuyển bằng WASD cần phản hồi ngay. Nhóm em chọn dự đoán phía client. Hầu như lúc nào server cũng đồng ý với lệnh của mình, nên client cứ tự áp lệnh luôn, không chờ. Điều kiện là client phải tính ra đúng kết quả như server, nên hai bên dùng chung một hàm mô phỏng. Trong demo, đó là file `simulation.js` chạy chung cho server Node.js và trình duyệt; bản cuối dùng chung C# cho Unity và .NET.
>
> Nhưng nếu chỉ dự đoán thì có vấn đề. Ảnh trạng thái từ server phản ánh quá khứ. Client đang ở x bằng 2, server báo 0,8; ghi đè vào thì nhân vật bị kéo giật lùi. Hiện tượng này gọi là giật ngược.

---

### Slide 11 — VĐ2: Đối chiếu với server (#2, 1,25')

**Trên slide**

1. Client giữ **bộ đệm** các lệnh chưa được xác nhận
2. Ảnh trạng thái kèm `lastProcessedSeq = N`
3. Nhận ảnh: **đặt lại** theo server → **bỏ** lệnh ≤ N → **chạy lại** lệnh còn lại

```js
state = snap.me;
pending = pending.filter(i => i.seq > snap.lastProcessedSeq);
for (const i of pending) state = Simulation.step(state, i.keys, DT);
```

**Hình:** "Đối chiếu và chạy lại lệnh". Bộ đệm `[1 2 3 4 | 5 6 7 8 9 10]`, lệnh 1–4 bị gạch (đã xác nhận: bỏ), 5–10 chạy lại; `lastProcessedSeq = 4`; từ x = 0,8 chạy qua 6 lệnh được x = 2,0 ✓ (0,8 + 6 × 0,2 = 2,0).

**Lời nói**

> Cách sửa là đối chiếu với server. Client giữ lại mọi lệnh đã gửi mà server chưa xác nhận. Server gửi ảnh trạng thái kèm số thứ tự lệnh cuối cùng nó đã xử lý.
>
> Ví dụ: client đã gửi 10 lệnh, mỗi lệnh đi 0,2 đơn vị, đang dự đoán ở x bằng 2. Server báo "x bằng 0,8, đã xử lý tới lệnh 4". Client đặt lại x bằng 0,8, bỏ lệnh 1 đến 4, chạy lại 6 lệnh còn lại, và được đúng 2,0. Người chơi không thấy gì.
>
> Chỉ khi dự đoán thật sự sai, ví dụ server thấy có va chạm mà client không biết, thì vị trí mới thay đổi; lệch nhỏ thì làm mượt trong vài khung hình. Trong demo, các bạn sẽ thấy số lệnh trong bộ đệm luôn xấp xỉ RTT chia độ dài một nhịp.

**Ghi chú:** đoạn code trên slide là code thật của demo (hàm xử lý ảnh trạng thái ở client).

---

### Slide 12 — VĐ3: Đối thủ hiển thị giật → nội suy thực thể (#2, 1,5')

**Trên slide**

- Đối thủ thì **không đoán được**. Ảnh trạng thái thưa (10–20 lần/giây), không đều, có thể mất
- Các hướng: vẽ ảnh mới nhất: giật ○ · ngoại suy: sai khi đổi hướng ○ · **nội suy** ● (đã chọn)
- **Vẽ đối thủ lùi lại** một khoảng, nội suy giữa 2 ảnh trạng thái bao quanh
- Độ trễ nội suy ≥ **2 × chu kỳ gửi ảnh + dao động độ trễ** (20 lần/giây thì ~100 ms)
- Đánh đổi: **mình ở hiện tại, đối thủ ở quá khứ**

**Hình:** "Nội suy thực thể". Trục thời gian với các ảnh S1…S5; vạch "hiện tại" ở bên phải, vạch "thời điểm vẽ" lùi về giữa S3 và S4 (khoảng lùi là "độ trễ nội suy"). Vị trí vẽ: p = (1 − α)·p₃ + α·p₄, với 0 ≤ α ≤ 1.

**Lời nói**

> Dự đoán chỉ dùng được cho chính mình, vì mình không biết đối thủ sắp bấm gì. Thông tin về đối thủ chỉ đến qua ảnh trạng thái, mà ảnh thì thưa, không đều và có thể mất. Vẽ theo ảnh mới nhất thì đối thủ đứng yên rồi nhảy cóc. Ngoại suy theo vận tốc thì sai mỗi khi đối thủ đổi hướng.
>
> Nhóm em chọn nội suy: cố ý vẽ đối thủ lùi lại một khoảng, ví dụ 100 mili giây. Nhờ lùi lại, ta luôn có hai ảnh trạng thái kẹp hai bên thời điểm vẽ, và chỉ cần nội suy đường thẳng giữa chúng. Khoảng lùi phải ít nhất bằng hai chu kỳ gửi ảnh cộng dao động độ trễ, để mất một gói vẫn còn dữ liệu.
>
> Cái giá là ta luôn thấy đối thủ ở quá khứ. Như vậy trên màn hình mỗi người có hai mốc thời gian: mình ở hiện tại, đối thủ ở quá khứ. Khi hai người tranh cùng một thứ thì sao? Bạn [#3] trình bày tiếp.

**Ghi chú:** **chuyển người nói: #2 → #3.** Bị hỏi chọn độ trễ nội suy bao nhiêu, mất nhiều gói thì sao: mở D6.

---

### Slide 13 — VĐ4: Tương tác không công bằng → bù trễ (#3, 1,5')

**Trên slide**

- A (RTT 40 ms) và B (RTT 200 ms) cùng nhặt một vật; trên màn hình, **B chạm trước**
- Phân xử theo thứ tự gói đến thì **A luôn thắng**: người trễ thấp luôn được lợi
- Các hướng: thứ tự gói đến ○ · client tự quyết (gian lận) ○ · **bù trễ** ● (đã chọn)
- **Bù trễ:** server lưu lịch sử 1 giây, **tua lại** về thời điểm người chơi nhìn thấy rồi mới phân xử
- Tua tối đa ~200–250 ms · Đánh đổi: **"đã chạy đi rồi vẫn bị bắt"**

**Hình:** "Bù trễ và cửa sổ phân xử". Ba trục dọc B (RTT 200 ms), Server, A (RTT 40 ms). B nhặt lúc 100 ms (đi mất 100 ms), A nhặt lúc 150 ms (đi mất 20 ms); server nhận A lúc 170 ms, B lúc 200 ms, chờ gom 100 ms rồi "tua lại, kiểm tra hợp lệ". Kết quả: theo gói đến thì A thắng; theo lịch sử hợp lệ thì B thắng.

**Lời nói**

> Ví dụ kinh điển là game bắn súng: trên màn hình tôi ngắm trúng đầu, nhưng thứ tôi thấy là đối thủ của 200 mili giây trước. Lệnh bắn tới server thì đối thủ đã đi chỗ khác.
>
> Với vật dùng chung cũng vậy. A ping 40, B ping 200, trên màn hình B chạm vật trước. Nhưng gói của A tới server nhanh hơn, nên nếu xử theo thứ tự gói đến thì A luôn thắng. Để client tự quyết thì quay lại chuyện gian lận.
>
> Giải pháp là bù trễ. Server lưu lịch sử trạng thái khoảng một giây. Mỗi hành động gửi kèm thời điểm người chơi đang nhìn thấy. Server tua lại về đúng lúc đó, kiểm tra, rồi mới phân xử. Để chống gửi thời điểm giả, server giới hạn chỉ tua tối đa khoảng 200 mili giây.
>
> Đánh đổi là công bằng cho người hành động thì người kia chịu thiệt: trên màn hình mình đã chạy đi rồi, vẫn bị tính là bị bắt. Không cách nào làm cả hai cùng thấy công bằng.

**Ghi chú:** bị hỏi hai người cùng nhặt thì xử thế nào, gửi nhịp giả thì sao: mở D7.

---

### Slide 14 — VĐ5: Dự đoán hành động bị sai → quay lui (#3, 0,75')

**Trên slide**

- Client đã hiển thị "nhặt được", server báo **từ chối**
- Các hướng: không dự đoán: chờ 1 RTT ○ · **dự đoán + quay lui cục bộ** ● (đã chọn) · quay lui toàn phần kiểu GGPO (ngang hàng) ○
- **Quay lui:** về trạng thái đã xác nhận → mô phỏng lại → vật "bật ra khỏi tay"
- Đối chiếu (VĐ2) là **trường hợp riêng** của quay lui

**Hình:** "Quay lui cục bộ". Nhịp 50 bấm J nhặt (nét đứt: dự đoán cầm vật); nhịp 56 nhận `ITEM_REJECTED`, "từ chối"; mũi tên quay về nhịp 50; mô phỏng lại đến nhịp 56, nhánh mới (nét liền) A không cầm vật.

**Lời nói**

> Kết quả phân xử về muộn, mà client đã cho người chơi cầm vật từ trước, vì không muốn bắt họ chờ. Nếu thua thì sao? Client quay về trạng thái đã được xác nhận, mô phỏng lại với thông tin đúng, và người chơi thấy vật bật ra khỏi tay. Đó là quay lui.
>
> Đối chiếu ở VĐ2 thực chất là trường hợp riêng của quay lui, chỉ áp cho di chuyển của chính mình.

**Ghi chú:** bị hỏi GGPO / tính tất định: mở D8, D9. Ý chính: quay lui ngang hàng cần mô phỏng giống nhau đến từng bit giữa các máy; nhóm em có server sửa định kỳ nên chỉ cần gần giống.

---

## Phần 3 — Áp dụng và demo (#3, #2)

### Slide 15 — Chuyển phần 3 (~5")

**Trên slide**

- **PHẦN 3** · Áp dụng và demo
- Game xếp đơn hàng siêu thị · demo sơ bộ
- Trình bày: #3 · #2 điều khiển client B trong demo

**Lời nói**

> Phần ba: nhóm em áp dụng các kỹ thuật này vào một game cụ thể, rồi demo.

---

### Slide 16 — Game xếp đơn hàng siêu thị → demo (#3, 0,5')

**Trên slide**

- Game 2 người, mỗi người là nhân viên siêu thị: **WASD** di chuyển, **J** nhặt / giao, **K** thả
- **Món hàng hiếm** chỉ có 1 cái ở kệ giữa → tranh chấp → bù trễ + quay lui
- Bản cuối: Unity (C#) + .NET server · TCP + UDP · SQLite
- *Không kỹ thuật nào xoá được độ trễ, chỉ chuyển nó sang chỗ ít bị để ý hơn*
- **Demo hôm nay:** server Node.js thật + trình duyệt qua WebSocket, mạng xấu giả lập, 3 khung nhìn

**Hình:** "Phác thảo siêu thị". Quầy A và quầy B ở hai góc trên, các kệ trái cây, rau củ, thịt cá, đồ uống, đồ khô, sữa và trứng; ở giữa là kệ "Món hàng hiếm · 1 cái" phát sáng, hai đường nét đứt từ A và B cùng chạy tới.

**Lời nói**

> Nhóm em sẽ áp dụng các kỹ thuật này vào game xếp đơn hàng siêu thị cho 2 người. Game có di chuyển liên tục, nên cần dự đoán và nội suy. Món hàng hiếm chỉ có một cái, hai người phải tranh nhau, nên cần bù trễ và quay lui.
>
> Tóm lại, không kỹ thuật nào xoá được độ trễ, chỉ chuyển nó sang chỗ ít bị để ý hơn: mình đi trước nhờ dự đoán, đối thủ đi sau nhờ nội suy, và server bù lại sự lệch thời gian khi hai bên tương tác.
>
> Hôm nay nhóm em demo bản chứng minh ý tưởng cho phần di chuyển: server Node.js thật, trình duyệt kết nối qua WebSocket, còn mạng xấu được giả lập để thấy rõ hiệu ứng.

**Ghi chú**

- Bị hỏi vì sao demo dùng JavaScript mà bản cuối dùng C#: demo cần làm nhanh và chạy ngay trên trình duyệt. Kỹ thuật netcode không phụ thuộc ngôn ngữ, và ý tưởng dùng chung code mô phỏng vẫn giữ nguyên (`simulation.js` ↔ `Simulation.Step`).
- Bảng tổng kết "năm vấn đề, năm đánh đổi" đã chuyển xuống slide dự phòng 21; câu chốt nằm ở slide này.

---

### Slide 17 — Demo: bản chứng minh ý tưởng, phần di chuyển (#3 dẫn, #2 điều khiển client B; 5')

**Trên slide**

- 3 khung nhìn: **Client A** (dự đoán + đối chiếu) · **Server** (người quyết định) · **Client B** (nội suy ô của A)
- Thanh trượt: độ trễ · dao động độ trễ · mất gói · tần số gửi ảnh
- 6 cảnh: 0 Client tự quyết · 1 Server quyết định · 2 Bật dự đoán · 3 Bật đối chiếu · 4 Ảnh thưa, chưa nội suy · 5 Bật nội suy

**Cách chạy:** phím 0–5 chuyển ngay sang cấu hình của từng cảnh, không kéo thanh trượt trên sân khấu. Mỗi cảnh khoảng 45 giây.

| Cảnh | Cấu hình | Thao tác | Lời dẫn |
|---|---|---|---|
| 0 | Client tự quyết, RTT 100 ms | Bấm nút "dịch chuyển" | "Đây là cách client tự quyết. Em giả lập sửa gói tin, ô của A nhảy sang đây, và server lẫn B đều chấp nhận." |
| 1 | Server quyết định, RTT 200 ms, tắt hết | A chạy qua lại; bấm lại "dịch chuyển" | "Chuyển sang server quyết định: dịch chuyển hết tác dụng, khung server đếm số gói bị từ chối. Nhưng thầy để ý, em bấm phím khoảng 0,2 giây sau ô mới chạy." |
| 2 | Bật dự đoán | Chạy rồi dừng đột ngột | "Bật dự đoán: phản hồi ngay. Nhưng khi dừng lại, ô bị kéo giật lùi, đó là giật ngược." |
| 3 | Bật đối chiếu | Lặp lại | "Bật đối chiếu: mượt và khớp với bóng xám là vị trí theo server. Bộ đệm đang có khoảng 6 lệnh, đúng bằng RTT chia nhịp." |
| 4 | Nhìn khung B; gửi ảnh 10 lần/giây, dao động 50 ms | A chạy vòng tròn | "Giờ nhìn sang màn hình của B: ô của A giật từng nấc vì ảnh trạng thái đến thưa và không đều." |
| 5 | Bật nội suy (độ trễ nội suy tự tính ≈ 250 ms); sau đó tăng lên 20 lần/giây | Lặp lại | "Bật nội suy: mượt, nhưng so với khung server thì ô của A trễ hơn, đó là đánh đổi. Tăng tần số gửi ảnh lên 20 thì độ trễ giảm còn khoảng 150 mili giây." |

**Phương án dự phòng:** quay sẵn video toàn bộ 6 cảnh. Demo lỗi thì chuyển sang video ngay, không sửa lỗi trên sân khấu.

---

## Phần 4 — Q&A (cả nhóm)

### Slide 18 — Cảm ơn và hỏi đáp (3–5')

**Trên slide**

- **PHẦN 4** · Cảm ơn thầy và các bạn
- Nhóm T47 sẵn sàng trả lời câu hỏi
- Mỗi người trả lời các câu thuộc vấn đề con của mình

**Ghi chú:** bị hỏi sâu thì mở slide dự phòng phía sau slide này. Câu trả lời mẫu: [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md), phụ lục A. Bảng thuật ngữ Việt – Anh: phụ lục B.

---

## Phụ lục — Slide dự phòng cho Q&A (không trình bày)

Slide 19 là slide mở đầu phụ lục ("PHỤ LỤC · Slide dự phòng cho Q&A · Chỉ mở khi bị hỏi"). Slide 22–31 (D1–D10) hiện là khung: điền nội dung từ [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md) theo mục ghi trên từng slide.

| Slide | Nội dung | Mở khi bị hỏi | Nguồn |
|---|---|---|---|
| 20 | Chuỗi 5 vấn đề con: VĐ1 → VĐ5, hệ quả trên mỗi mũi tên ("phải chờ server", "chỉ đoán được mình", "đối thủ ở quá khứ", "server có thể quyết khác"); 5 tiêu chí chọn giải pháp (phản hồi, nhất quán — ưu tiên; chịu mạng xấu, độ phức tạp, phù hợp loại game) | Cần giải thích lại mạch 5 vấn đề con hoặc tiêu chí chọn | Ý chính đã gộp vào slide 6 |
| 21 | Tổng kết: bảng vấn đề · giải pháp · đánh đổi (bảng dưới) | Thầy hỏi tóm tắt các đánh đổi | Câu chốt đã chuyển lên slide 16 |
| 22 · D1 | Thành phần của độ trễ: lan truyền, truyền, xếp hàng, xử lý | "Độ trễ đến từ đâu, giảm được không?" | Mục 2.1 |
| 23 · D2 | TCP chi tiết: thời gian chờ gửi lại, thuật toán Nagle | "Sao không dùng TCP cho tất cả, tắt Nagle là xong?" | Mục 2.3 |
| 24 · D3 | Bộ tích luỹ thời gian; tần số mô phỏng và tần số gửi ảnh | "Chọn 30 nhịp/giây thế nào? Client 144 Hz thì sao?" | VĐ1 |
| 25 · D4 | Hàng đợi lệnh ở server: thiếu lệnh, dồn lệnh, chống hack tốc độ | "Lệnh đến không đều thì server làm gì?" | VĐ1 |
| 26 · D5 | Bảng giao thức: `INPUT`, `SNAPSHOT`, `ITEM_*` | "Định dạng gói tin thế nào?" | Mục 5 |
| 27 · D6 | Chọn độ trễ nội suy: bảng theo tần số gửi ảnh, ngoại suy dự phòng | "Độ trễ nội suy chọn bao nhiêu? Mất nhiều gói thì sao?" | VĐ3 |
| 28 · D7 | Phân xử món hàng hiếm: cửa sổ phân xử, kiểm tra số nhịp giả | "Hai người cùng nhặt thì xử thế nào? Gửi nhịp giả thì sao?" | VĐ4 |
| 29 · D8 | Các nguồn phá tính tất định: số thực, thứ tự duyệt, số ngẫu nhiên | "Tính tất định ảnh hưởng thế nào?" | VĐ5 |
| 30 · D9 | Đồng bộ theo bước / GGPO so với mô hình của nhóm | "Sao không làm ngang hàng?" | VĐ1, VĐ5 |
| 31 · D10 | Mạng giả lập: `netsim.js` (độ trễ, dao động độ trễ, mất gói); WebSocket vì trình duyệt không mở được UDP | "Demo có phản ánh mạng thật không?" | Mục 7.2 |

### Bảng tổng kết (slide 21)

| Vấn đề | Giải pháp | Đánh đổi |
|---|---|---|
| Ai quyết định | Server quyết định | Trễ 1 RTT |
| Điều khiển ì | Dự đoán + đối chiếu | Cần mô phỏng chung |
| Đối thủ giật | Nội suy | Thấy đối thủ ở quá khứ |
| Tranh chấp không công bằng | Bù trễ | "Chạy rồi vẫn bị bắt" |
| Dự đoán sai | Quay lui | Thỉnh thoảng thấy hành động bị huỷ |

### Ý chính D10

Đã có trong ghi chú slide 31. demo dùng server Node.js và WebSocket thật. Trình duyệt không mở được socket UDP, nên kênh là TCP; độ trễ, dao động độ trễ và mất gói được giả lập ở server (`netsim.js`) cho từng chiều của từng kết nối, mỗi gói trễ ngẫu nhiên nên có thể đến sai thứ tự giống UDP. Chạy trên cùng một máy thì RTT ≈ 0, nên dù dùng socket nào cũng phải giả lập thì mới thấy được hiệu ứng. Bản cuối thay bằng UDP thật; code kỹ thuật netcode không đổi.

---

## Ghi chú tập luyện

- **Vai:** #1 mục lục, phần 1 và VĐ1 (slide 1–9) · #2 VĐ2, VĐ3 (slide 10–12) · #3 VĐ4, VĐ5, phần 3 và demo (slide 13–17) · cả nhóm Q&A (slide 18).
- **Chuyển người nói** ở cuối slide 9 (#1 → #2) và cuối slide 12 (#2 → #3). Câu chuyển là câu hỏi dẫn sang vấn đề tiếp theo, đã viết sẵn trong lời nói.
- **Bấm giờ theo cột cộng dồn.** Đến slide 12 mà đã quá 8 phút thì nói nhanh slide 13–14.
- Nhãn trên slide chỉ ghi người nói (#1/#2/#3); thời gian từng slide xem ở dòng đầu ghi chú của slide.
- **Mỗi người tự trả lời được** các câu Q&A thuộc vấn đề con của mình (phụ lục A của `NoiDung_ChiTiet.md` có gắn nhãn VĐ).
- Số liệu *(cần đo lại)* ở slide 6 phải thay bằng số thật trước buổi trình bày.
