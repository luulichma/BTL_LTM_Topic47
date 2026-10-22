# Kế hoạch: Thuyết trình T47 + Demo sơ bộ

> Bám theo [`PROJECT_CONTEXT.md`](../PROJECT_CONTEXT.md) và [`T47_YTuongDemo_PhanCong.docx`](T47_YTuongDemo_PhanCong.docx).
> Giả định: còn khoảng **2 tuần** tới buổi trình bày sơ bộ. Nếu hạn khác, co giãn các giai đoạn ở mục 4 theo tỉ lệ.

---

## 1. Mục tiêu buổi sơ bộ

- Thuyết trình đúng khung **15–20 phút**: giải thích kỹ thuật 10–12 phút, demo 5–7 phút, Q&A 3–5 phút.
- Demo **chạy thật qua socket**: 1 server .NET + 2 client Unity, chạy trên localhost.
- Cho khán giả thấy **từng vấn đề mạng và kỹ thuật sửa nó**, mỗi lần bật thêm đúng một kỹ thuật. Đây là cảnh 1–5 trong docx.
- Cảnh 6 (tranh món đồ hiếm, lag compensation, rollback) là **mục tiêu mở rộng**. Nếu chưa kịp làm thì trình bày bằng slide có hình timeline, và nói rõ là sẽ có ở bản cuối.

**Nguyên tắc:** hình đơn giản (ô vuông màu), netcode chuẩn. Điểm nằm ở Technical Depth (30%) và demo chạy được (25%), không nằm ở đồ hoạ.

---

## 2. Phạm vi demo sơ bộ

| Làm | Để sau (bản cuối) |
|---|---|
| Bản đồ phẳng có vài bức tường, 2 nhân vật là ô vuông | Kệ hàng, khách, món đồ, tính điểm |
| WASD di chuyển, va chạm tường và va chạm giữa 2 người | J/K nhặt, thả, giao đồ |
| TCP `JOIN` tối giản: cấp playerId và cổng UDP, không cần mật khẩu | Đăng nhập SQLite, matchmaking, màn kết quả |
| Server authoritative chạy tick 30 Hz, snapshot 20 Hz | Lưu kết quả trận |
| Prediction, reconciliation, interpolation, mỗi kỹ thuật có công tắc riêng | Smoothing khi sửa sai (C7), nếu còn thời gian thì làm |
| Chế độ **naive relay** làm baseline, kèm nút "cheat teleport" | |
| Bộ mô phỏng mạng: latency, jitter, loss (đặt riêng cho từng client) | |
| Overlay: ping, tick, số input chờ, "bóng" server; panel F1 và preset 1–5 | |
| *(Mở rộng)* 1 món đồ hiếm ở giữa bản đồ, lag compensation và rollback | |

Các đầu việc trong docx thuộc phạm vi sơ bộ: **S1, S2, S5, S6, S7, C1, C3–C6, G7, G8** và một bản rút gọn của **C2**. Phần mở rộng gồm **G4–G6**.

---

## 3. Kiến trúc tối thiểu

```
BTL_LTM_Topic47/
├── Shared/      # class library netstandard2.1 (Unity dùng được)
│   ├── Messages.cs     # định nghĩa và serialize nhị phân các gói tin
│   ├── Simulation.cs   # Step(state, input, dt): di chuyển + va chạm
│   └── NetSim.cs       # bọc quanh UdpClient: delay/jitter/loss
├── Server/      # .NET 8 console, tham chiếu Shared
└── Client/      # Unity project; copy Shared.dll vào Assets/Plugins
```

- **Shared simulation** là điều kiện để prediction đúng: client và server chạy **cùng một hàm `Step`**. Không dùng Unity Physics cho phần di chuyển.
- `Shared` phải target **netstandard2.1** thì Unity mới tham chiếu được. Thêm một bước build để copy DLL vào `Client/Assets/Plugins`.
- Đặt bộ mô phỏng mạng ở **phía client** (bọc cả đường gửi lẫn đường nhận). Nhờ vậy mỗi cửa sổ có latency riêng, đúng như cảnh 6 cần (A = 50 ms, B = 250 ms).

### Giao thức tối giản (rút từ mục 4 của docx)

| Kênh | Message | Chiều | Trường |
|---|---|---|---|
| TCP | `JOIN` / `WELCOME` | C→S / S→C | name / playerId, udpPort, serverTick, mode |
| UDP | `INPUT` | C→S | playerId, seq (u32), clientTick (u32), keys (bitmask 1 byte), **kèm 3 input trước đó** để chống mất gói |
| UDP | `SNAPSHOT` | S→C | serverTick; với mỗi player: id, x, y, vx, vy, lastProcessedSeq |
| UDP | `RELAY_POS` | C→S→C | *(chỉ ở chế độ naive)* x, y do client tự báo |
| UDP | `ACTION_RESULT` | S→C | *(mở rộng)* seq, accepted, reason |

Ước lượng băng thông (để đưa vào slide):

- `SNAPSHOT` cho 2 người ≈ 47 B payload + 28 B header UDP/IP ≈ 75 B. Gửi 20 Hz thì mỗi client nhận khoảng **1,5 KB/s**.
- `INPUT` có 4 bản ghi ≈ 37 B + 28 B ≈ 65 B. Gửi 30 Hz thì khoảng **2 KB/s**.

---

## 4. Lộ trình (≈ 2 tuần)

| Giai đoạn | Ngày | Việc | Vai | Kiểm tra bằng |
|---|---|---|---|---|
| **0. Chốt** | 1 | Gán người vào 3 vai; chốt giao thức bảng trên; dựng skeleton repo (Shared/Server/Client), thống nhất phiên bản Unity và .NET | Cả nhóm (#1 làm chủ) | Ai clone về cũng build được |
| **1. Nền** | 2–5 | S1/S2: TCP JOIN và message nhị phân · C1: `Simulation.Step` kèm unit test · S5/S6: tick loop 30 Hz, nhận INPUT, gửi SNAPSHOT · C2/C3: Unity kết nối, gửi input theo tick, vẽ cả 2 người theo snapshot | #1, #2 | 2 cửa sổ di chuyển được. Đây là **cảnh 2** (authoritative, chưa có prediction) |
| | 2–5 | G7: `NetSim` (hàng đợi theo thời điểm giao gói, jitter, loss) · khung panel F1 | #3 | Chỉnh latency thì thấy độ "ì" tăng |
| **2. Netcode** | 6–9 | C4: prediction · C5: reconciliation (giữ buffer input chưa ack, đặt lại state, replay) · C6: interpolation với buffer 100 ms | #2 | **Cảnh 3, 4, 5** |
| | 6–9 | S7: naive relay kèm nút cheat · G8: overlay (ping, tick, pending inputs, bóng server), công tắc, preset 1–5 | #1, #3 | **Cảnh 1**; chuyển cảnh chỉ bằng 1 phím |
| **3. Mở rộng** | 8–10 | G4–G6: món đồ hiếm; server lưu lịch sử vị trí 30 tick; tua lại theo `clientTick − interpDelay`; ACTION_RESULT; rollback phía client; log phân xử | #3 | **Cảnh 6**. Nếu tới ngày 10 vẫn chưa chạy được thì dừng, chuyển cảnh 6 sang slide |
| **4. Đóng gói** | 10–11 | T4: đo số liệu (sai số dự đoán trước và sau reconcile, băng thông thực tế, số lần correction mỗi phút) | #2 (+#1) | Có số liệu và biểu đồ cho slide |
| | 10–12 | T2: slide (mỗi người làm phần mình, trưởng nhóm ghép) · T3: demo script và **quay video dự phòng** | Cả nhóm | Slide hoàn chỉnh; có video đủ các cảnh |
| **5. Tập** | 12–14 | Tổng duyệt bấm giờ ít nhất 3 lần; luyện Q&A (mỗi người 3–4 câu khó) | Cả nhóm | Tổng thời gian 15–20 phút |

**Đường găng:** C1 → S5/S6 → C4/C5. Nếu shared simulation và tick loop chưa xong vào ngày 5 thì mọi việc phía sau đều trễ. Ưu tiên tuyệt đối cho #1 và #2 trong tuần đầu.

---

## 5. Dàn ý slide (~14 slide, 10–12 phút)

| # | Slide | Nội dung chính / hình nên có | Người | Thời gian |
|---|---|---|---|---|
| 1 | Tiêu đề | T47, tên nhóm, thành viên | #1 | — |
| 2 | Bài toán | Game "xếp đơn hàng" 2 người. Ví dụ RTT 200 ms tại 30 Hz: bấm phím xong phải chờ khoảng 6 tick mới thấy nhân vật đi | #1 | 0,5' |
| 3 | Bản gốc làm gì sai | Client-authoritative, không có tick hay seq, chỉ vẽ theo gói mới nhất. Hệ quả: gian lận và giật hình | #1 | 0,5' |
| 4 | Kiến trúc | Sơ đồ 1 server + 2 client; TCP cho JOIN, UDP cho INPUT/SNAPSHOT; shared simulation | #1 | 1' |
| 5 | Tick và giao thức | Fixed timestep 30 Hz; định dạng INPUT và SNAPSHOT (byte layout); seq/ack; gửi kèm input cũ để chống mất gói; băng thông ước tính | #1 | 1' |
| 6 | Client-side prediction | Timeline client/server: client áp input ngay, server xác nhận sau | #2 | 1' |
| 7 | Server reconciliation | Buffer input có seq; nhận `lastProcessedSeq` thì bỏ input đã ack, đặt lại state, replay. Kèm pseudo-code | #2 | 1,5' |
| 8 | Vì sao cần shared simulation | Cùng một hàm `Step` ở hai phía; vấn đề determinism (float, thứ tự xử lý) | #2 | 0,5' |
| 9 | Entity interpolation | Vẽ đối thủ ở thời điểm `now − 100 ms`, nội suy giữa 2 snapshot; cách chọn delay (≥ 2 × chu kỳ snapshot + jitter) | #2 | 1,5' |
| 10 | Lag compensation | Server lưu lịch sử vị trí 1 giây, tua về tick mà client đã nhìn thấy; vì sao có hiện tượng "bị cướp đồ dù đã chạy đi" | #3 | 1,5' |
| 11 | Rollback | So sánh với reconciliation; liên hệ GGPO; cảnh món đồ bật khỏi tay | #3 | 1' |
| 12 | Kết quả đo | Biểu đồ sai số dự đoán; băng thông; so sánh baseline với bản netcode | #3 | 1' |
| 13 | Tiến độ và hướng tiếp | Đã xong gì ở bản sơ bộ, bản cuối sẽ thêm gì | #3 | 0,5' |
| 14 | Chuyển sang demo | | #3 | — |

Mẹo: slide 6, 7, 9, 10 nên dùng **sơ đồ timeline hai trục** (client/server theo thời gian, có mũi tên gói tin). Đây là hình dễ giải thích nhất và cũng hay bị hỏi lại trong Q&A.

---

## 6. Demo script sơ bộ (5–7 phút)

Chuẩn bị trước: chạy server, mở 2 cửa sổ client trái/phải, overlay bật sẵn. **Người dẫn (#3)** điều khiển A, **#2** điều khiển B, **#1** đọc log server.

| Phím | Cảnh | Cấu hình | Thao tác | Câu thuyết minh chính |
|---|---|---|---|---|
| 1 | Baseline | Naive relay, latency 200 ms, jitter 50 ms | Chạy vòng quanh rồi bấm cheat để teleport | "Đối thủ nhảy cóc; server tin client nên cheat thành công" |
| 2 | Authoritative | Prediction tắt | Chạy, thử cheat lần nữa | "Cheat bị chặn, nhưng mỗi lần bấm phải chờ 200 ms" |
| 3 | + Prediction | Reconcile tắt | Chạy, đâm vào người B | "Phản hồi tức thì, nhưng nhân vật lệch khỏi bóng server" |
| 4 | + Reconciliation | | Lặp lại cú va chạm | "Lệch được sửa: nhân vật và bóng trùng nhau. Số input chờ khoảng RTT/tick" |
| 5 | + Interpolation | Jitter 100 ms, loss 10% | Nhìn sang cửa sổ đối thủ | "Đối thủ mượt dù gói đến thất thường; đổi lại thấy đối thủ trễ khoảng 100 ms" |
| 6 | *(mở rộng)* Lag comp | A 50 ms, B 250 ms | Hai người cùng bấm J ở kệ giữa, lần đầu tắt lag comp, lần sau bật | "Tắt thì người mạng nhanh luôn thắng; bật thì phân xử theo thời điểm bấm thật. Xem log và cảnh rollback" |

**Dự phòng:** có video quay sẵn cho từng cảnh; mọi thứ chạy trên localhost, không cần Wi-Fi; một máy dự phòng đã cài sẵn bản build.

---

## 7. Chuẩn bị Q&A

Mỗi người soạn câu trả lời ngắn (2–3 câu) cho các câu ở mục 10 của `PROJECT_CONTEXT.md`, cộng thêm các câu sau:

- Seq bị tràn (u32 ở 30 Hz thì sau khoảng 4,5 năm mới tràn) và so sánh seq khi gói đến sai thứ tự.
- Vì sao interpolation delay chọn 100 ms mà không chọn 50 ms?
- Nếu client gửi `clientTick` giả để được lợi từ lag compensation thì sao? Trả lời: server giới hạn thời gian tua lại, ví dụ tối đa 200 ms.
- Đồng bộ đồng hồ và tick giữa client và server (ước lượng RTT, client chạy trước server khoảng RTT/2 cộng một buffer).
- Vì sao không dùng TCP cho input? (head-of-line blocking)

## 8. Tài liệu tham khảo nên đọc (cho Technical Depth)

- Gabriel Gambetta, *Fast-Paced Multiplayer* (4 phần: prediction, reconciliation, interpolation, lag compensation). Dễ hiểu nhất, nên đọc đầu tiên.
- Valve Developer Wiki, *Source Multiplayer Networking* và *Lag Compensation*.
- Glenn Fiedler, *Gaffer On Games*: Fix Your Timestep, Networked Physics, Reliability over UDP.
- GDC: *Overwatch Gameplay Architecture and Netcode* (Tim Ford); *8 Frames in 16ms: Rollback Networking in Mortal Kombat and Injustice 2*.
- Tài liệu GGPO (rollback netcode).

## 9. Cần quyết ngay

- [ ] Ngày trình bày sơ bộ chính xác, để điền ngày thật cho mục 4
- [ ] Người thứ 3 trong nhóm; gán người vào vai #1, #2, #3
- [ ] Phiên bản Unity (gợi ý Unity 6 LTS) và .NET (gợi ý .NET 8 LTS) dùng chung cho cả nhóm
- [ ] Cảnh 6 làm thật hay chỉ trình bày bằng slide ở buổi sơ bộ (quyết vào ngày 10)
