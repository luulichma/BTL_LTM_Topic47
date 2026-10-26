# Kế hoạch: Thuyết trình T47 (vấn đề & kỹ thuật) + Demo sơ bộ

> Buổi này trình bày **vấn đề nhóm giải quyết** và **các kỹ thuật netcode**. Đây **không** phải buổi giới thiệu sản phẩm hoàn thiện.
> Game "xếp đơn hàng siêu thị" chỉ xuất hiện như **bối cảnh áp dụng**. Demo là một **proof-of-concept nhỏ** để minh hoạ kỹ thuật, không phải bản game.
> Thông tin nền: [`PROJECT_CONTEXT.md`](../PROJECT_CONTEXT.md).

---

## 1. Mục tiêu

- Người nghe hiểu: **vì sao độ trễ mạng là vấn đề** trong game real-time, và **mỗi kỹ thuật giải quyết phần nào, đánh đổi gì**.
- Bám tiêu chí chấm. Technical Depth chiếm 30% và Q&A chiếm 20%, nên phần lớn thời lượng dành cho cơ chế bên trong, timeline gói tin và các đánh đổi.
- Demo sơ bộ cho thấy **trước/sau** của từng kỹ thuật trên 2 ô vuông di chuyển.

## 2. Mạch câu chuyện

Chi tiết ở [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md). Cấu trúc: **đặt vấn đề → giải thích → các hướng giải pháp → so sánh → chọn**, áp cho từng vấn đề con. Giải pháp của vấn đề trước làm lộ ra vấn đề sau.

1. **Vấn đề gốc:** game real-time cần vừa phản hồi tức thì vừa nhất quán, nhưng mạng luôn có độ trễ, jitter và mất gói.
2. **VĐ1, ai nắm trạng thái thật:** chọn server authoritative (so với client-authoritative, lockstep). Hệ quả: điều khiển bị "ì".
3. **VĐ2, điều khiển bị ì:** chọn prediction + reconciliation. Hệ quả: chỉ dự đoán được chính mình.
4. **VĐ3, đối thủ giật:** chọn interpolation. Hệ quả: mình ở hiện tại, đối thủ ở quá khứ.
5. **VĐ4, tương tác không công bằng:** chọn lag compensation. Hệ quả: kết quả đã hiển thị có thể bị server bác.
6. **VĐ5, dự đoán hành động sai:** chọn rollback cục bộ.
7. **Giải pháp lựa chọn và demo:** kiến trúc tổng hợp, bối cảnh game xếp đơn hàng siêu thị, rồi chuyển sang demo.

---

## 3. Dàn ý slide (~13 slide, 10–12 phút)

Nội dung chi tiết (nguồn gốc của slide): [`NoiDung_ChiTiet.md`](NoiDung_ChiTiet.md). Nội dung sơ bộ từng slide: [`NoiDung_Slide_SoBo.md`](NoiDung_Slide_SoBo.md).

| # | Slide | Nội dung / hình nên có | Người | Thời gian |
|---|---|---|---|---|
| 1 | Tiêu đề | T47, nhóm, thành viên | #1 | — |
| 2 | Bài toán | Game real-time cần gì: phản hồi dưới khoảng 100 ms, mọi người thấy cùng một thế giới. Số liệu RTT thực tế (LAN, 4G, quốc tế) | #1 | 1' |
| 3 | Mạng không hoàn hảo | Latency, jitter, packet loss, gói đến sai thứ tự; UDP hay TCP (head-of-line blocking) | #1 | 1' |
| 4 | Cách ngây thơ: client-authoritative | Client gửi vị trí, server chuyển tiếp. Hậu quả: teleport hack, giật hình. Ví dụ từ dự án tham khảo | #1 | 1' |
| 5 | Server authoritative + tick | Client chỉ gửi input (kèm seq, tick); server chạy fixed timestep. Timeline cho thấy bấm phím xong phải chờ đúng 1 RTT | #1 | 1' |
| 6 | Client-side prediction | Timeline: client áp input ngay; điều kiện là client và server dùng chung code mô phỏng | #2 | 1' |
| 7 | Prediction một mình chưa đủ | Snapshot đến trễ ghi đè vị trí làm nhân vật bị kéo giật lùi (rubber-banding) | #2 | 0,5' |
| 8 | Server reconciliation | Buffer input có seq; nhận `lastProcessedSeq` thì bỏ input đã ack, đặt lại state, replay. Kèm pseudo-code | #2 | 1,5' |
| 9 | Entity interpolation | Vẽ đối thủ ở `now − delay`, nội suy giữa 2 snapshot. Đánh đổi: thấy đối thủ trễ hơn thực tế. Cách chọn delay | #2 | 1,5' |
| 10 | Lag compensation | Vấn đề: mỗi người nhìn thấy đối thủ ở một thời điểm quá khứ khác nhau. Server lưu lịch sử vị trí rồi tua lại. Hệ quả "đã chạy đi vẫn bị bắt" | #3 | 1,5' |
| 11 | Rollback | Dự đoán sai thì quay lui rồi mô phỏng lại. So sánh với reconciliation; GGPO trong game đối kháng; vấn đề determinism | #3 | 1' |
| 12 | Tổng kết kỹ thuật | Bảng: kỹ thuật, giải quyết vấn đề gì, đánh đổi gì | #3 | 0,5' |
| 13 | Hướng áp dụng của nhóm | Game xếp đơn hàng 2 người. Món đồ hiếm là nơi xảy ra tranh chấp, cần lag compensation và rollback. Stack Unity + .NET. Chuyển sang demo | #3 | 0,5' |

**Hình là trọng tâm.** Slide 5–11 nên dùng **sơ đồ timeline hai trục** (client và server theo thời gian, có mũi tên gói tin). Mỗi slide một sơ đồ, nên vẽ cùng một kiểu cho thống nhất. Đây là phần dễ ghi điểm Technical Depth nhất và cũng là phần hay bị hỏi lại.

**Không đưa vào:** tính năng game (đăng nhập, ghép trận, điểm số), giao diện, tiến độ code, kết quả đo của sản phẩm.

---

## 4. Demo sơ bộ (proof-of-concept)

### Phạm vi

- 2 ô vuông màu trên một mặt phẳng, chỉ có di chuyển bằng WASD (người A) và phím mũi tên (người B).
- Màn hình chia **3 khung**: Client A, Server, Client B. Khung server hiển thị "sự thật"; hai khung client hiển thị những gì mỗi người nhìn thấy.
- **Mạng giả lập trong cùng tiến trình:** gói tin được đưa vào hàng đợi và trả ra sau khoảng latency đã chỉnh, có jitter và loss. Chỉnh được bằng thanh trượt.
- Công tắc: chế độ client-authoritative / server authoritative (kèm nút "teleport" giả lập gói tin bị sửa), Prediction, Reconciliation, Interpolation. Overlay: số input chờ ack, tick server, interpDelay.
- `interpDelay` tự tính theo `2 × chu kỳ snapshot + jitter`.
- Server tick 30 Hz, gửi snapshot ở tần số chỉnh được (đặt thấp, ví dụ 10 Hz, để thấy rõ giật khi chưa bật interpolation).

**Vì sao chọn mạng giả lập thay cho socket thật ở buổi này:** làm nhanh (khoảng 1 tuần), không phụ thuộc mạng phòng học, và đặt được 3 góc nhìn cạnh nhau nên dễ giải thích. Nên viết lớp truyền tin dưới dạng interface `ITransport` để bản cuối chỉ cần thay bằng UDP thật. Code mô phỏng và logic prediction/reconciliation dùng lại nguyên vẹn.

Công nghệ: một scene Unity (đúng stack đã chốt). Code `Simulation.Step` để ở file thuần C#, không dùng Unity Physics, để sau này server .NET dùng chung được.

### Kịch bản demo (~5 phút)

| # | Cấu hình | Thao tác | Khán giả thấy | Ứng với slide |
|---|---|---|---|---|
| 0 | Client-authoritative, latency 100 ms | Bấm "teleport" | Ô của A nhảy tới bất kỳ đâu, server và B chấp nhận | 4 |
| 1 | Server authoritative, latency 200 ms, tắt hết | A chạy qua lại; bấm lại "teleport" | Teleport hết tác dụng, nhưng bấm phím xong khoảng 200 ms mới thấy chạy | 5 |
| 2 | + Prediction | Nhấp nhanh D; rồi giữ D chạy và thả | Phản hồi tức thì. Nhấp phím thì ô bị kéo về chỗ cũ (giật ngược); khi chạy thì rung; khi thả thì trôi thêm một đoạn | 6, 7 |
| 3 | + Reconciliation | Lặp lại | Mượt và khớp vị trí server. Số input chờ ≈ RTT/tick | 8 |
| 4 | Nhìn khung client B; snapshot 10 Hz, jitter 50 ms | A chạy vòng tròn | Ô của A trong mắt B giật từng nấc | 9 |
| 5 | + Interpolation (interpDelay ≈ 250 ms) | Lặp lại; rồi tăng snapshot lên 20 Hz | Mượt, nhưng trễ so với khung server; lên 20 Hz thì delay còn ≈ 150 ms (đánh đổi) | 9 |

Lag compensation và rollback **không demo ở buổi này**. Hai kỹ thuật này giải thích bằng slide 10–11, kèm hình timeline hoặc animation.

---

## 5. Lộ trình (~1,5–2 tuần)

| Ngày | Slide & nội dung | Demo |
|---|---|---|
| 1 | Gán vai; chốt mạch câu chuyện (mục 2); chia slide theo người | Chốt phạm vi demo; tạo project Unity |
| 2–4 | Mỗi người đọc tài liệu (mục 7) phần mình, viết nội dung slide | #2: `Simulation.Step`, transport giả lập có latency/jitter/loss, server tick |
| 5–7 | Vẽ sơ đồ timeline (#1: slide 5; #2: slide 6–9; #3: slide 10–11); trưởng nhóm ghép slide | #2: prediction, reconciliation, interpolation · #3: thanh trượt, công tắc, overlay, phím preset 1–5 |
| 8–9 | Tập lần 1, sửa slide | Chạy đủ 5 cảnh; quay **video dự phòng** |
| 10–12 | Tập bấm giờ ít nhất 2 lần nữa; luyện Q&A | Sửa lỗi; chụp ảnh màn hình demo đưa vào slide nếu cần |

## 6. Chuẩn bị Q&A

Mỗi người soạn trả lời ngắn (2–3 câu) cho các câu ở mục 10 của `PROJECT_CONTEXT.md`, cộng thêm các câu sau:

- Vì sao không dùng TCP cho input?
- Prediction có làm được với đối thủ không? (dead reckoning khác interpolation thế nào)
- Interpolation delay nên chọn bao nhiêu? (lớn hơn hoặc bằng 2 chu kỳ snapshot cộng jitter)
- Client gửi tick giả để được lợi từ lag compensation thì sao? (server giới hạn thời gian tua lại)
- Demo dùng mạng giả lập thì có phản ánh đúng mạng thật không? (độ trễ của UDP trên localhost gần như 0, và bộ giả lập tạo đúng độ trễ cần mô phỏng; bản cuối dùng socket thật)

## 7. Tài liệu nên đọc

- Gabriel Gambetta, *Fast-Paced Multiplayer* (4 phần). Đọc đầu tiên; demo của nhóm theo đúng mạch bài này.
- Valve Developer Wiki, *Source Multiplayer Networking* và *Lag Compensation*.
- Glenn Fiedler, *Gaffer On Games*: Fix Your Timestep, What Every Programmer Needs to Know About Game Networking.
- GDC: *Overwatch Gameplay Architecture and Netcode*; *8 Frames in 16ms* (rollback trong Mortal Kombat).
- Tài liệu GGPO.

## 8. Cần quyết ngay

- [ ] Ngày trình bày, để điền ngày thật vào mục 5
- [ ] Thành viên thứ 3; gán vai #1, #2, #3
- [ ] Phiên bản Unity dùng chung
