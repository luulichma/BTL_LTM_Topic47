# T47 — Nội dung chi tiết: Kỹ thuật mạng cho game nhiều người chơi thời gian thực

> Tài liệu nội dung gốc của nhóm. Slide, lời thuyết trình và câu trả lời Q&A đều rút ra từ đây.
>
> Cấu trúc:
>
> - **Phần I:** đặt vấn đề.
> - **Phần II:** 5 vấn đề con. Mỗi vấn đề con đi theo thứ tự: giải thích → các hướng giải pháp → so sánh → chọn.
> - **Phần III:** giải pháp đã chọn và demo sơ bộ.
> - **Phụ lục**, trong đó có **bảng đối chiếu thuật ngữ Việt – Anh** (phụ lục B) để tra khi giảng viên hỏi bằng thuật ngữ tiếng Anh.
>
> Quy ước: thuật ngữ tiếng Anh chỉ ghi trong ngoặc ở lần đầu xuất hiện. Tên viết tắt, tên giao thức và tên trong code giữ nguyên.
> Số liệu đánh dấu *(cần đo lại)* là giá trị tham khảo. Nhóm nên tự đo trước khi đưa lên slide.

---

## Mục lục

- **Phần I. Đặt vấn đề**
  - [1. Game nhiều người chơi thời gian thực cần gì](#1-game-nhiều-người-chơi-thời-gian-thực-cần-gì)
  - [2. Vấn đề gốc: mạng không hoàn hảo](#2-vấn-đề-gốc-mạng-không-hoàn-hảo)
  - [3. Phân rã thành 5 vấn đề con](#3-phân-rã-thành-5-vấn-đề-con)
  - [4. Tiêu chí đánh giá giải pháp](#4-tiêu-chí-đánh-giá-giải-pháp)
- **Phần II. Các vấn đề con và giải pháp**
  - [VĐ1. Ai giữ quyền quyết định trạng thái?](#vđ1-ai-giữ-quyền-quyết-định-trạng-thái)
  - [VĐ2. Điều khiển bị ì](#vđ2-điều-khiển-bị-ì)
  - [VĐ3. Đối thủ hiển thị giật](#vđ3-đối-thủ-hiển-thị-giật)
  - [VĐ4. Tương tác không công bằng](#vđ4-tương-tác-không-công-bằng)
  - [VĐ5. Dự đoán hành động bị sai](#vđ5-dự-đoán-hành-động-bị-sai)
- **Phần III. Giải pháp lựa chọn và demo sơ bộ**
  - [5. Kiến trúc tổng hợp](#5-kiến-trúc-tổng-hợp)
  - [6. Bối cảnh áp dụng: game xếp đơn hàng siêu thị](#6-bối-cảnh-áp-dụng-game-xếp-đơn-hàng-siêu-thị)
  - [7. Demo sơ bộ](#7-demo-sơ-bộ)
  - [8. Giới hạn và hướng làm tiếp](#8-giới-hạn-và-hướng-làm-tiếp)
- **Phụ lục**
  - [A. Trả lời Q&A](#a-trả-lời-qa)
  - [B. Bảng đối chiếu thuật ngữ Việt – Anh](#b-bảng-đối-chiếu-thuật-ngữ-việt--anh)
  - [C. Tài liệu tham khảo](#c-tài-liệu-tham-khảo)

---

# Phần I. Đặt vấn đề

## 1. Game nhiều người chơi thời gian thực cần gì

Game nhiều người chơi thời gian thực (real-time multiplayer) là game có nhiều người cùng chơi trong một thế giới chung, và trạng thái thế giới thay đổi liên tục theo thao tác của họ. Ví dụ: game bắn súng, game đối kháng, game hành động, game đua xe. Khác với game theo lượt (cờ vua trực tuyến), ở đây từng phần trăm giây đều có ý nghĩa.

Loại game này phải thoả cùng lúc hai yêu cầu:

- **Độ phản hồi** (responsiveness): bấm phím là nhân vật phải phản ứng ngay ở khung hình tiếp theo. Với game hành động, độ trễ điều khiển từ khoảng 100 ms đã thấy rõ là "ì". Từ 200 ms trở lên thì rất khó chơi.
- **Tính nhất quán** (consistency): mọi người chơi phải thấy cùng một trạng thái, tức cùng vị trí, cùng điểm số, cùng biết ai đang cầm vật gì. Không người chơi nào được tự ý thay đổi trạng thái đó (chống gian lận).

Nếu mọi người chơi trên cùng một máy thì hai yêu cầu không mâu thuẫn. Khi có mạng ở giữa, thao tác của người A cần một khoảng thời gian mới tới được máy khác. Muốn **nhất quán** thì phải chờ thông tin đi hết đường; muốn **phản hồi tức thì** thì không được chờ. Các kỹ thuật mạng cho game (netcode) là những cách dung hoà hai yêu cầu mâu thuẫn này.

## 2. Vấn đề gốc: mạng không hoàn hảo

### 2.1 Độ trễ đến từ đâu

**Độ trễ** (latency) một chiều của gói tin gồm bốn thành phần:

| Thành phần | Ý nghĩa | Có giảm được không |
|---|---|---|
| Trễ lan truyền | Tín hiệu đi trong cáp quang với tốc độ khoảng 200.000 km/s (≈ 2/3 tốc độ ánh sáng) | Không, đây là giới hạn vật lý |
| Trễ truyền | Kích thước gói chia cho băng thông | Gói tin game nhỏ nên gần như bằng 0 |
| Trễ xếp hàng | Gói tin chờ ở hàng đợi của bộ định tuyến, phụ thuộc mức tải | Không kiểm soát được; **đây là nguồn chính gây dao động độ trễ** |
| Trễ xử lý | Bộ định tuyến, hệ điều hành, ứng dụng xử lý gói | Nhỏ |

Ví dụ về giới hạn vật lý: Hà Nội cách Singapore khoảng 2.200 km theo đường chim bay. Dù cáp đi thẳng, **thời gian khứ hồi** (round-trip time, RTT: thời gian gói tin đi và quay về) tối thiểu cũng là 2 × 2.200 / 200.000 ≈ 22 ms. Thực tế cáp đi vòng và qua nhiều bộ định tuyến, nên RTT đo được thường gấp 1,5–3 lần con số này.

RTT tham khảo *(cần đo lại bằng `ping`)*:

| Kết nối | RTT điển hình |
|---|---|
| Cùng máy / mạng LAN | < 1 ms |
| Wi-Fi gia đình → server trong nước | 10–40 ms |
| 4G | 30–80 ms, dao động lớn |
| Việt Nam → Singapore | 30–60 ms |
| Việt Nam → Mỹ | 180–250 ms |

### 2.2 Ba hiện tượng khác ngoài độ trễ

- **Dao động độ trễ** (jitter): mỗi gói tin có độ trễ khác nhau. Bên gửi gửi đều đặn mỗi 50 ms, nhưng bên nhận nhận các gói cách nhau 30 ms, 80 ms, 45 ms… Độ trễ trung bình có thể không đổi, nhưng nó không đều giữa các gói.
- **Mất gói** (packet loss): gói tin bị bỏ giữa đường do bộ định tuyến quá tải hoặc sóng không dây yếu. Wi-Fi và 4G thường mất 0,5–2% *(cần đo lại)*, có lúc mất cả một loạt gói liên tiếp.
- **Sai thứ tự và trùng gói** (out-of-order, duplicate): gói gửi sau đến trước vì đi đường khác, hoặc một gói đến hai lần.

### 2.3 TCP và UDP xử lý các hiện tượng này thế nào

**TCP** đảm bảo dữ liệu đến đủ, đúng thứ tự và không trùng. Để làm được vậy, khi một đoạn dữ liệu bị mất, TCP **giữ lại mọi dữ liệu đến sau** cho tới khi đoạn bị mất được gửi lại thành công. Hiện tượng này gọi là **nghẽn đầu hàng** (head-of-line blocking).

- Thời gian chờ ít nhất khoảng 1 RTT (cơ chế gửi lại nhanh). Nếu phải chờ hết **thời gian chờ gửi lại** (RTO) thì lâu hơn nhiều: tối thiểu khoảng 200 ms trên Linux, RFC 6298 khuyến nghị 1 s.
- Trong lúc chờ, ứng dụng không nhận được gì, sau đó nhận dồn một loạt dữ liệu cũ cùng lúc.
- Ngoài ra, TCP mặc định bật thuật toán Nagle (gom các gói nhỏ để gửi một lần). Thuật toán này làm gói nhỏ trễ thêm nếu không tắt bằng tuỳ chọn `TCP_NODELAY`.

**UDP** không đảm bảo gì: không giữ thứ tự, không gửi lại, không phát hiện trùng. Bù lại, mất gói nào thì chỉ mất gói đó, các gói sau vẫn đến đúng hạn.

Như vậy, dù chọn giao thức nào thì ứng dụng game vẫn phải đối mặt với độ trễ. TCP biến mất gói thành **độ trễ đột biến**. UDP để nguyên mất gói và sai thứ tự cho ứng dụng tự xử lý. Chọn giao thức nào là một quyết định thiết kế, trình bày ở VĐ1.

**Kết luận của phần vấn đề gốc:** độ trễ, dao động độ trễ và mất gói **không thể loại bỏ**. Mọi giải pháp phía sau đều là cách **che giấu** hoặc **phân bổ lại** ảnh hưởng của chúng.

## 3. Phân rã thành 5 vấn đề con

Vấn đề gốc sinh ra 5 vấn đề cụ thể. Chúng nối thành chuỗi: **giải pháp của vấn đề trước làm lộ ra vấn đề sau.**

```
Vấn đề gốc: mạng có độ trễ, dao động độ trễ, mất gói
   │
   ▼
VĐ1. Ai giữ quyền quyết định trạng thái?  → chọn: server quyết định
   │   (hệ quả: mọi thao tác phải chờ server)
   ▼
VĐ2. Điều khiển bị ì                       → chọn: dự đoán phía client + đối chiếu với server
   │   (hệ quả: chỉ dự đoán được chính mình, không dự đoán được đối thủ)
   ▼
VĐ3. Đối thủ hiển thị giật                 → chọn: nội suy thực thể
   │   (hệ quả: mình ở hiện tại, đối thủ ở quá khứ)
   ▼
VĐ4. Tương tác không công bằng             → chọn: bù trễ
   │   (hệ quả: client đã hiển thị kết quả, nhưng server có thể quyết khác)
   ▼
VĐ5. Dự đoán hành động bị sai              → chọn: quay lui cục bộ
```

Chuỗi này là mạch chính của bài thuyết trình. Mỗi khi chuyển sang vấn đề con mới, người nói chỉ cần trả lời một câu: "giải pháp vừa rồi tốt, nhưng nó để lại vấn đề gì?".

## 4. Tiêu chí đánh giá giải pháp

Mỗi vấn đề con có nhiều hướng giải pháp. Nhóm so sánh chúng theo 5 tiêu chí:

| # | Tiêu chí | Câu hỏi | Ưu tiên |
|---|---|---|---|
| T1 | Độ phản hồi | Người chơi có thấy kết quả thao tác ngay không? | **Cao** |
| T2 | Nhất quán / chống gian lận | Mọi người có thấy cùng một thế giới không? Client có tự sửa được trạng thái không? | **Cao** |
| T3 | Chịu được mạng xấu | Còn giữ được chất lượng khi có dao động độ trễ, mất gói không? | Trung bình |
| T4 | Độ phức tạp | Nhóm 3 người có làm kịp và làm đúng không? | Trung bình |
| T5 | Phù hợp loại game | Có hợp với game 2 người, có server, di chuyển và nhặt đồ không? | Trung bình |

T1 và T2 được ưu tiên cao nhất vì đó chính là hai yêu cầu mâu thuẫn ở mục 1. T4 dùng để loại những hướng quá nặng so với quy mô nhóm.

Ký hiệu trong các bảng so sánh: **●** tốt · **◐** trung bình · **○** kém.

---

# Phần II. Các vấn đề con và giải pháp

## VĐ1. Ai giữ quyền quyết định trạng thái?

### Vấn đề

Trong game nhiều người chơi, mỗi máy giữ một bản sao trạng thái của thế giới. Cần có quy ước **bản nào là đúng** khi các bản sao khác nhau, và **ai được phép thay đổi** trạng thái.

Cách đơn giản nhất là để mỗi client tự quyết trạng thái nhân vật của mình rồi báo cho mọi người. Cách này có hai điểm yếu:

- **Gian lận được.** Client chạy trên máy người chơi, nên mọi thứ client gửi đi đều sửa được:
  - gửi vị trí tuỳ ý là **dịch chuyển tức thời** tới bất kỳ đâu;
  - gửi các vị trí cách nhau xa hơn bình thường là **hack tốc độ**;
  - tự báo "đã nhặt được", "giao đúng hàng" là được cộng điểm mà không cần làm gì.
- **Không có trọng tài.** Hai người cùng nhặt một vật thì mỗi client đều cho là mình nhặt được. Hai màn hình mâu thuẫn nhau và không ai phân xử.

### Các hướng giải pháp

**A. Client tự quyết, server chỉ chuyển tiếp** (client-authoritative, relay). Client gửi **trạng thái** (vị trí, hành động) lên server, server chuyển tiếp cho người khác.
- Ưu điểm: đơn giản; nhân vật của mình phản hồi tức thì.
- Nhược điểm: mắc cả hai điểm yếu ở trên.
- Thường gặp ở game thử nghiệm và game hợp tác không có cạnh tranh.

**B. Đồng bộ theo bước, ngang hàng** (lockstep, peer-to-peer). Các máy chỉ gửi cho nhau **lệnh điều khiển** (input), tức phím nào đang được bấm. Mỗi máy chạy cùng một mô phỏng, chờ đủ lệnh của mọi người cho bước hiện tại rồi mới bước tiếp.
- Ưu điểm: không ai sửa được trạng thái, vì trạng thái do mô phỏng tính ra.
- Nhược điểm:
  - mỗi bước phải chờ lệnh của người chậm nhất;
  - mô phỏng phải **tất định tuyệt đối**, tức giống nhau đến từng bit (xem VĐ5);
  - mỗi máy có toàn bộ trạng thái, nên dễ bị hack lộ bản đồ.
- Phổ biến ở game chiến thuật thời gian thực (RTS) có hàng nghìn đơn vị, nơi gửi lệnh rẻ hơn nhiều so với gửi trạng thái.

**C. Server quyết định** (server-authoritative). Client chỉ gửi **lệnh điều khiển** lên server.
- Server chạy mô phỏng, là **nguồn sự thật duy nhất**, và gửi **ảnh trạng thái** (snapshot) cho mọi client.
- Server kiểm tra được mọi luật chơi: tốc độ tối đa, va chạm, vật có trong tầm với không.
- Được dùng trong hầu hết game bắn súng và game hành động trực tuyến.

### So sánh

| | T1 Phản hồi | T2 Nhất quán / chống gian lận | T3 Mạng xấu | T4 Độ phức tạp | T5 Phù hợp |
|---|---|---|---|---|---|
| A. Client tự quyết | ● | ○ | ○ | ● đơn giản | ○ không phân xử được |
| B. Đồng bộ theo bước | ○ chờ người chậm nhất | ◐ khó sửa trạng thái, dễ hack lộ bản đồ | ○ một người mạng chậm làm cả trận dừng | ○ cần tất định tuyệt đối | ◐ hợp game chiến thuật hơn |
| C. Server quyết định | ○ nếu đứng riêng (xem VĐ2) | ● | ◐ | ◐ | ● đã có server |

### Lựa chọn: server quyết định, mô phỏng theo nhịp cố định

Nhóm chọn **C**: đây là hướng thoả T2 tốt nhất, còn điểm yếu ở T1 có lời giải riêng ở VĐ2. Hướng B bị loại vì T4 (đòi hỏi tất định tuyệt đối) và không hợp với game ít người chơi.

**Nhịp mô phỏng cố định** (fixed tick). Server không mô phỏng "nhanh nhất có thể" mà theo **bước thời gian cố định**, ví dụ 30 nhịp mỗi giây (mỗi nhịp ≈ 33,3 ms). Ở mỗi nhịp, server:

1. Lấy lệnh điều khiển của từng người chơi trong hàng đợi.
2. Gọi `Simulation.Step(state, inputs, dt)` với `dt` cố định.
3. Lưu trạng thái vào lịch sử (dùng cho VĐ4).
4. Cứ vài nhịp một lần, gửi ảnh trạng thái.

Dùng `dt` cố định có ba lợi ích:

- **Lặp lại được:** cùng trạng thái và cùng lệnh thì cho cùng kết quả. Đây là điều kiện để làm VĐ2.
- **Không phụ thuộc tốc độ máy:** máy mạnh hay yếu đều chạy cùng số nhịp mỗi giây.
- **Có đồng hồ chung:** số thứ tự nhịp là đơn vị thời gian mà cả client và server cùng hiểu.

Phía client, vòng lặp vẽ hình chạy theo tần số màn hình, còn mô phỏng vẫn chạy theo nhịp cố định nhờ một **bộ tích luỹ thời gian** (Glenn Fiedler, *Fix Your Timestep*):

```csharp
accumulator += frameDeltaTime;       // cộng dồn thời gian thực đã trôi qua
while (accumulator >= Dt) {          // Dt = 1/30 s
    SimulateOneTick();
    accumulator -= Dt;
}
Render(alpha: accumulator / Dt);     // vẽ nội suy giữa nhịp trước và nhịp hiện tại
```

**Tần số mô phỏng và tần số gửi ảnh trạng thái** là hai thông số khác nhau:

| | Tăng lên thì | Giảm xuống thì |
|---|---|---|
| Tần số mô phỏng | Mô phỏng chính xác hơn, lệnh được xử lý sớm hơn; tốn CPU | Trễ thêm tối đa 1 nhịp; va chạm kém chính xác |
| Tần số gửi ảnh trạng thái | Client có thông tin mới hơn, độ trễ nội suy nhỏ hơn (VĐ3); tốn băng thông | Phải tăng độ trễ nội suy |

Để tham khảo: Source Engine (Counter-Strike) mặc định chạy 64–66 nhịp/giây, nhiều game khác chạy 20–60 nhịp/giây. Với game có di chuyển không quá nhanh, **30 nhịp/giây và 20 ảnh trạng thái/giây** là đủ.

Băng thông không phải là giới hạn. Một gói lệnh có khoảng 10–20 byte dữ liệu cộng 28 byte phần đầu UDP/IPv4. Gửi 30 gói mỗi giây chỉ tốn khoảng 1,5 KB/s.

**Hàng đợi lệnh ở server.** Lệnh đến không đều vì dao động độ trễ: có nhịp không có lệnh nào, có nhịp dồn hai lệnh. Chính sách xử lý:

- Mỗi nhịp xử lý **tối đa 1 lệnh** cho mỗi người chơi, cho phép bắt kịp tối đa 2 lệnh khi bị dồn. Giới hạn này đồng thời **chống hack tốc độ**: client có gửi 60 lệnh/giây cũng không chạy nhanh gấp đôi.
- Hàng đợi trống thì **dùng lại lệnh trước**, tức đoán người chơi vẫn đang giữ phím. Nếu đoán sai, client sẽ tự sửa (VĐ2).
- Lệnh có số thứ tự không lớn hơn số của lệnh đã xử lý thì bỏ, vì đó là gói trùng hoặc đến muộn.

**Giao thức truyền: TCP hay UDP.** Dữ liệu chuyển động có **thời hạn sử dụng rất ngắn**: vị trí của 200 ms trước đã vô giá trị khi có vị trí mới hơn. TCP vẫn gửi lại nó và chặn mọi dữ liệu mới phía sau (mục 2.3), nên không phù hợp. Nhóm dùng **cả hai giao thức** tuỳ loại dữ liệu:

| Loại dữ liệu | Đặc điểm | Kênh |
|---|---|---|
| Đăng nhập, ghép trận, kết thúc trận | Ít, bắt buộc tới nơi, không gấp từng ms | TCP |
| Lệnh điều khiển | 30 gói/giây, cần nhanh, mất một ít thì bù được | UDP, có gửi dư |
| Ảnh trạng thái | 20 gói/giây, chỉ cần bản mới nhất | UDP, kèm số nhịp |
| Sự kiện quan trọng (kết quả phân xử, điểm số) | Bắt buộc tới nơi | TCP |

Vì UDP không đảm bảo gì, nhóm tự bổ sung các cơ chế sau:

- **Số thứ tự** (sequence number, `seq`) và **số nhịp** ở mỗi gói: để phát hiện gói mất, bỏ gói đến muộn và gói trùng.
- **Xác nhận** (ack): ảnh trạng thái mang trường `lastProcessedSeq`, nghĩa là "tôi đã xử lý tới lệnh số N của bạn".
- **Gửi dư** (redundancy): mỗi gói lệnh kèm thêm 2–3 lệnh trước đó. Mất một gói thì gói sau mang lại lệnh đó, không cần gửi lại.
- **Kích thước gói** dưới khoảng 1.200 byte để tránh bị phân mảnh IP. Gói của nhóm chỉ vài chục byte.

**Hệ quả của lựa chọn này.** Nếu client chỉ vẽ theo ảnh trạng thái, mỗi thao tác phải đi một vòng tới server rồi mới quay về. Đó là VĐ2.

---

## VĐ2. Điều khiển bị ì

### Vấn đề

Khi server quyết định, độ trễ từ lúc bấm phím đến lúc thấy nhân vật chạy là:

```
độ trễ điều khiển ≈ RTT/2 (lệnh lên server)
                  + chờ tới nhịp kế tiếp (0–33 ms ở 30 nhịp/giây)
                  + chờ tới lần gửi ảnh trạng thái kế tiếp (0–50 ms ở 20 lần/giây)
                  + RTT/2 (ảnh trạng thái về client)
                ≈ RTT + (0–83 ms)
```

Với RTT 200 ms, mỗi lần bấm phím người chơi phải chờ khoảng 200–280 ms. Gian lận đã bị chặn, nhưng game không chơi được.

```
Client ──●──────────────────────────────────●──────▶ thời gian
         │ bấm D (lệnh số 1)                ▲ thấy nhân vật chạy
          ╲  ~RTT/2                        ╱  ~RTT/2
           ▼                              ╱
Server ────●── chờ nhịp ── nhịp 41: áp lệnh 1 ── gửi ảnh trạng thái ──▶ thời gian
```

### Các hướng giải pháp

**A. Chấp nhận chờ server.** Không làm gì thêm. Chỉ dùng được khi RTT rất nhỏ (mạng LAN) hoặc khi game chậm (game theo lượt).

**B. Che độ trễ bằng thiết kế game.** Mỗi thao tác có một hoạt ảnh "chuẩn bị" (vung tay, lấy đà) kéo dài khoảng một RTT, để người chơi không nhận ra mình đang phải chờ. Một số game chiến thuật dùng cách này cho thao tác bấm chuột để di chuyển. Cách này không xoá được độ trễ, và chỉ hợp với thao tác không cần phản xạ tức thì.

**C. Chỉ dự đoán phía client** (client-side prediction). Trong đa số trường hợp, server **sẽ đồng ý** với lệnh của người chơi: bấm D thì nhân vật đi sang phải, trừ khi có tường. Vì vậy client **tự áp lệnh ngay lên trạng thái của mình**, đồng thời vẫn gửi lệnh lên server như bình thường. Độ trễ điều khiển về lại gần 0.

Nhưng khi ảnh trạng thái tới, client **ghi đè** vị trí theo ảnh đó, mà ảnh trạng thái phản ánh quá khứ: thời điểm server xử lý xong lệnh của khoảng RTT/2 trước. Kết quả là nhân vật bị kéo lùi về vị trí cũ rồi lại chạy tiếp. Hiện tượng này gọi là **giật ngược** (rubber-banding).

**D. Dự đoán phía client + đối chiếu với server** (server reconciliation). Giống C, nhưng khi nhận ảnh trạng thái thì client **không ghi đè máy móc**. Client đặt lại trạng thái theo server, rồi **chạy lại** (replay) những lệnh mà server chưa xử lý.

### So sánh

| | T1 | T2 | T3 | T4 | T5 |
|---|---|---|---|---|---|
| A. Chờ server | ○ | ● | ◐ | ● | ○ game có di chuyển liên tục |
| B. Che bằng hoạt ảnh | ◐ | ● | ◐ | ● | ○ di chuyển bằng WASD cần phản hồi ngay |
| C. Chỉ dự đoán | ◐ bị giật ngược | ● server vẫn quyết định | ○ mạng càng trễ càng giật | ◐ | ◐ |
| D. Dự đoán + đối chiếu | ● | ● | ● | ◐ cần mô phỏng dùng chung | ● |

### Lựa chọn: dự đoán phía client + đối chiếu với server

**Điều kiện: client và server dùng chung code mô phỏng.** Dự đoán chỉ đúng nếu client tính ra **cùng kết quả** với server. Cách chắc chắn nhất là cả hai chạy **cùng một hàm** `Simulation.Step(state, input, dt)`. Đây là lý do nhóm chọn Unity (C#) cho client và .NET (C#) cho server: hàm mô phỏng nằm trong một file C# thuần, dùng chung cho cả hai phía. Điều đó kéo theo ba ràng buộc:

- `Simulation.Step` **không dùng API của Unity** (`Transform`, `Rigidbody`, `Physics`, `Time.deltaTime`). Hàm chỉ nhận vào trạng thái, lệnh và `dt`, rồi trả về trạng thái mới.
- Va chạm tự viết đơn giản bằng hình chữ nhật, không dùng bộ vật lý của Unity. Bộ vật lý đó không chạy được trên server .NET, và cũng không đảm bảo ra kết quả giống nhau giữa các lần chạy.
- Trạng thái phải **sao chép được** (kiểu `struct`) để lưu lịch sử và chạy lại.

**Thuật toán đối chiếu.**

1. Client lưu mọi lệnh đã gửi nhưng chưa được xác nhận vào **bộ đệm chờ** (`pending`).
2. Mỗi ảnh trạng thái gửi kèm `lastProcessedSeq` cho từng client.
3. Khi nhận ảnh trạng thái, client:
   1. **Đặt lại** trạng thái nhân vật của mình theo server.
   2. **Bỏ** khỏi bộ đệm chờ mọi lệnh có số thứ tự ≤ `lastProcessedSeq`.
   3. **Chạy lại** các lệnh còn trong bộ đệm.

```csharp
// Mỗi nhịp ở client
var input = ReadInput();
input.Seq = ++seq;
pending.Add(input);
transport.Send(input);                       // gửi kèm vài lệnh trước đó (gửi dư)
state = Simulation.Step(state, input, Dt);   // dự đoán

// Khi nhận ảnh trạng thái
void OnSnapshot(Snapshot s) {
    if (s.ServerTick <= lastSnapshotTick) return;   // gói cũ hoặc sai thứ tự: bỏ
    lastSnapshotTick = s.ServerTick;

    state = s.MyState;                                       // (1) tin theo server
    pending.RemoveAll(i => i.Seq <= s.LastProcessedSeq);     // (2) bỏ lệnh đã được xác nhận
    foreach (var i in pending)                               // (3) chạy lại lệnh còn chờ
        state = Simulation.Step(state, i, Dt);
}
```

**Ví dụ bằng số.** Mô phỏng 30 nhịp/giây, nhân vật chạy 6 đơn vị/giây, tức **0,2 đơn vị mỗi nhịp**. RTT 200 ms ≈ 6 nhịp. Người chơi giữ phím D, xuất phát từ x = 0.

- Client đã gửi các lệnh số 1 đến 10 và dự đoán x = 10 × 0,2 = **2,0**.
- Ảnh trạng thái về: "x = 0,8; lastProcessedSeq = 4" (server mới xử lý 4 lệnh).
- Nếu chỉ dự đoán (C): client đặt x = 0,8, nhân vật **giật lùi 1,2 đơn vị**.
- Nếu có đối chiếu (D): client đặt x = 0,8, bỏ lệnh 1–4, chạy lại lệnh 5–10 (6 × 0,2 = 1,2), được x = **2,0**. Kết quả trùng với vị trí đang dự đoán, nên người chơi **không thấy gì**.

Số lệnh trong bộ đệm chờ luôn xấp xỉ RTT chia độ dài một nhịp (ở đây khoảng 6). Demo hiển thị con số này trên màn hình.

**Khi dự đoán thật sự sai.** Chạy lại chỉ cho đúng vị trí dự đoán khi server và client mô phỏng giống nhau. Có ba trường hợp lệch:

- Lệnh mới đến trễ nên server dùng lại lệnh cũ.
- Hai nhân vật va chạm nhau, trong khi client không biết chính xác vị trí hiện tại của đối thủ.
- Server từ chối hành động (VĐ5).

Lệch nhỏ thì **làm mượt**: giữ lại một vectơ sai lệch và giảm dần nó về 0 trong vài khung hình. Lệch lớn hơn một ngưỡng thì cho nhân vật nhảy thẳng tới vị trí đúng.

**Mất gói và sai thứ tự.**

- Mất gói lệnh: gói sau mang lại lệnh đó (gửi dư). Mất cả loạt gói thì server dùng lại lệnh cũ, và client được sửa qua bước đối chiếu.
- Mất gói ảnh trạng thái: ảnh sau thay thế được, vì mỗi ảnh chứa trạng thái đầy đủ.
- Ảnh trạng thái đến sai thứ tự: bỏ ảnh có số nhịp cũ hơn ảnh đã xử lý.

**Hệ quả của lựa chọn này.** Client chỉ dự đoán được **chính mình**, vì chỉ biết lệnh của mình mà không biết đối thủ sắp bấm gì. Đó là VĐ3.

---

## VĐ3. Đối thủ hiển thị giật

### Vấn đề

Client chỉ biết vị trí đối thủ qua ảnh trạng thái, mà ảnh trạng thái thì:

- **Thưa:** 10–20 lần mỗi giây, trong khi màn hình vẽ 60 khung hình mỗi giây hoặc hơn.
- **Không đều:** dao động độ trễ làm khoảng cách giữa các ảnh thay đổi.
- **Có thể mất.**

Nếu vẽ đối thủ theo ảnh trạng thái mới nhất, nhân vật sẽ đứng yên vài khung hình rồi nhảy cóc. Khi có dao động độ trễ, nhịp nhảy không đều; khi mất gói, nhân vật đứng khựng.

### Các hướng giải pháp

**A. Vẽ theo ảnh trạng thái mới nhất.** Đơn giản và có độ trễ thấp nhất, nhưng bị giật như mô tả ở trên.

**B. Ngoại suy** (extrapolation, dead reckoning). Vẽ đối thủ ở **hiện tại** bằng cách đoán tiếp từ vị trí và vận tốc cuối cùng.
- Ưu điểm: không bị trễ.
- Nhược điểm: **sai mỗi khi đối thủ đổi hướng hoặc dừng lại**, nhân vật chạy quá đà rồi giật về.
- Hợp với vật thể có quán tính lớn, đổi hướng chậm như xe, máy bay, tàu.

**C. Nội suy thực thể** (entity interpolation). Vẽ đối thủ **lùi lại một khoảng thời gian cố định**, gọi là **độ trễ nội suy**, và **nội suy** vị trí giữa hai ảnh trạng thái đã nhận. Hình ảnh luôn mượt và luôn đúng đường đi thật của đối thủ, đổi lại ta thấy đối thủ ở quá khứ.

### So sánh

| | T1 (độ trễ hình ảnh đối thủ) | T2 (đúng với server) | T3 | T4 | T5 |
|---|---|---|---|---|---|
| A. Ảnh mới nhất | ● | ● | ○ giật theo dao động trễ, khựng khi mất gói | ● | ○ |
| B. Ngoại suy | ● | ○ sai khi đối thủ đổi hướng | ◐ | ◐ | ○ nhân vật đổi hướng tức thì |
| C. Nội suy | ◐ trễ thêm một khoảng nội suy | ● | ● | ◐ | ● |

### Lựa chọn: nội suy thực thể, ngoại suy ngắn khi thiếu gói

**Thuật toán.**

```
thời điểm vẽ = thời gian server ước tính hiện tại − độ trễ nội suy
```

Vì lùi lại một khoảng, client gần như luôn có **hai ảnh trạng thái bao quanh** thời điểm vẽ: một ảnh cũ hơn (`s0`) và một ảnh mới hơn (`s1`). Vị trí đối thủ được **nội suy tuyến tính** giữa hai ảnh đó:

```csharp
float renderTime = serverClock.Now - InterpDelay;
// trong bộ đệm ảnh trạng thái (sắp theo thời gian), tìm s0, s1 sao cho s0.Time <= renderTime < s1.Time
float t = (renderTime - s0.Time) / (s1.Time - s0.Time);
opponent.Position = Vector2.Lerp(s0.Pos, s1.Pos, t);
```

```
ảnh trạng thái: S1     S2   S3       S4    S5       (đến không đều)
  ─────────────●──────●────●────────●─────●──────────▶ thời gian
                             ▲              ▲
                         thời điểm vẽ     hiện tại
                      (vẽ đối thủ ở đây, ← độ trễ nội suy →
                       giữa S3 và S4)
```

**Chọn độ trễ nội suy.** Độ trễ phải đủ lớn để phía sau thời điểm vẽ luôn có ảnh `s1`, kể cả khi mất một gói. Quy tắc thực nghiệm:

```
độ trễ nội suy ≥ 2 × chu kỳ gửi ảnh trạng thái + dao động độ trễ dự kiến
```

| Tần số gửi ảnh | Chu kỳ | Dao động độ trễ | Độ trễ nội suy |
|---|---|---|---|
| 20 lần/giây | 50 ms | ~0 | ~100 ms (mặc định của Source Engine: `cl_interp 0.1`, 20 lần cập nhật/giây) |
| 20 lần/giây | 50 ms | 50 ms | ~150 ms |
| 10 lần/giây | 100 ms | 50 ms | ~250 ms |

Nếu vẫn hết dữ liệu (mất nhiều gói liên tiếp), client **ngoại suy ngắn** theo vận tốc cuối, tối đa khoảng 100 ms, rồi dừng lại để không đoán sai quá xa. Hướng B được dùng như phương án dự phòng, không phải phương án chính.

**Hệ quả của lựa chọn này.** Ta thấy đối thủ trễ khoảng `độ trễ nội suy + RTT/2` so với vị trí thật trên server. Quan trọng hơn: trên màn hình của mỗi người, **hai nhân vật đang ở hai mốc thời gian khác nhau**.

- Nhân vật của mình ở **tương lai gần**: được dự đoán, nên đi trước server.
- Đối thủ ở **quá khứ**: được nội suy, nên đi sau server.

Khi hai người tương tác với nhau hoặc tranh cùng một vật, sự lệch thời gian này gây ra VĐ4.

---

## VĐ4. Tương tác không công bằng

### Vấn đề

Ví dụ kinh điển là game bắn súng. Tôi ngắm trúng đầu đối thủ **trên màn hình của tôi**, nhưng hình ảnh đó là vị trí của đối thủ cách đây khoảng 200 ms. Khi lệnh bắn tới server thì đối thủ đã đi chỗ khác, và server tính là trượt. Người chơi buộc phải "bắn đón đầu" theo độ trễ mạng của mình, điều không ai ước lượng được.

Với vật thể dùng chung cũng vậy. Giả sử:

- A có RTT 40 ms, B có RTT 200 ms.
- Có đúng một vật nằm giữa bản đồ.
- Trên màn hình của mỗi người, B chạm tới vật và bấm nhặt **trước** A khoảng 50 ms.

Nhưng gói của B mất khoảng 100 ms mới lên tới server, còn gói của A chỉ mất khoảng 20 ms. Nếu server phân xử theo **thứ tự gói đến**, A gần như luôn thắng. **Người có độ trễ thấp luôn được lợi.**

### Các hướng giải pháp

**A. Phân xử theo thứ tự gói đến server.** Đơn giản, nhưng không công bằng như ví dụ trên.

**B. Client tự quyết kết quả.** Client báo "tôi bắn trúng" hoặc "tôi nhặt được", server chấp nhận luôn. Công bằng theo góc nhìn người hành động, nhưng quay lại đúng điểm yếu gian lận của VĐ1.

**C. Bù trễ** (lag compensation). Server **tua lại thời gian** về đúng lúc người chơi hành động, nhìn thế giới **như người chơi đã nhìn thấy**, kiểm tra hành động trong trạng thái đó, rồi mới phân xử.

### So sánh

| | T1 | T2 | T3 | T4 | T5 |
|---|---|---|---|---|---|
| A. Thứ tự gói đến | ● | ◐ nhất quán nhưng thiên vị người trễ thấp | ○ người trễ cao luôn thua | ● | ◐ |
| B. Client tự quyết | ● | ○ gian lận được | ● | ● | ○ trái với lựa chọn ở VĐ1 |
| C. Bù trễ | ● | ● server vẫn kiểm tra | ● | ◐ cần lưu lịch sử trạng thái | ● |

### Lựa chọn: bù trễ có giới hạn thời gian tua lại

**Thuật toán.**

1. **Server lưu lịch sử trạng thái theo nhịp:** vị trí mọi người chơi và trạng thái mọi vật, trong một **bộ đệm vòng** khoảng 1 giây (30 nhịp).
2. **Mỗi hành động gửi kèm thời điểm mà client đang nhìn thấy:** số nhịp ứng với thời điểm vẽ (VĐ3) lúc bấm phím.
3. **Server tua lại:** lấy trạng thái ở nhịp đó trong lịch sử (nội suy nếu cần), rồi kiểm tra hành động có hợp lệ không: người chơi có trong tầm với không, vật có còn đó không.
4. **Server trở về hiện tại** và áp dụng kết quả.

Valve (Source Engine) mô tả thời điểm cần tua về như sau:

```
thời điểm hành động = thời gian server hiện tại − độ trễ gói tin của client − độ trễ nội suy của client
```

**Mở rộng cho vật thể bị tranh chấp: cửa sổ phân xử.** Trong game bắn súng, server chỉ cần trả lời "lúc đó có trúng không". Với một vật chỉ có một cái, câu hỏi trở thành "**ai chạm tới trước**". Thiết kế của nhóm:

- Khi nhận yêu cầu nhặt vật đầu tiên, server **chưa trả lời ngay** mà đợi một khoảng ngắn (100–150 ms) để gom mọi yêu cầu cho vật đó.
- Hết khoảng chờ, server tua về thời điểm hành động của từng yêu cầu và kiểm tra yêu cầu đó có hợp lệ không.
- Trong các yêu cầu hợp lệ, server chọn người có **thời điểm hành động sớm nhất**, rồi gửi kết quả cho cả hai người.

Client đã **dự đoán** việc nhặt (VĐ5), nên khoảng chờ này không làm game bị ì.

**Giới hạn và đánh đổi.**

- **Giới hạn thời gian tua lại tối đa**, ví dụ 200–250 ms. Nếu không giới hạn, client gian lận có thể gửi số nhịp giả rất cũ để được lợi, hoặc người có độ trễ quá cao sẽ "hành động" ở một quá khứ quá xa.
- **Kiểm tra số nhịp có hợp lý không.** Số nhịp client gửi phải gần với giá trị ước tính `nhịp hiện tại của server − RTT đo được − độ trễ nội suy`. Sai lệch lớn thì server kẹp về giới hạn hoặc từ chối.
- **Chi phí.** Server phải lưu lịch sử và tính lại. Với 2 người chơi và vài vật thì không đáng kể.
- **"Đã chạy đi rồi vẫn bị bắt"** (còn gọi là ưu tiên người bắn, "favor the shooter"). Trên màn hình của A, A đã đứng ở chỗ vật và cầm được nó. Nhưng ở quá khứ mà B nhìn thấy, B chạm tới trước, nên A mất vật. Không có cách nào làm cả hai cùng thấy công bằng. Bù trễ chọn **tin theo góc nhìn của người hành động**. Một số game giới hạn mức ưu tiên này khi độ trễ vượt một ngưỡng, để người có độ trễ cao không được lợi quá nhiều.

**Hệ quả của lựa chọn này.** Kết quả phân xử về muộn (sau một RTT và khoảng chờ phân xử), trong khi client đã hiển thị người chơi cầm vật. Nếu người chơi thua, client phải sửa một kết quả đã hiển thị. Đó là VĐ5.

---

## VĐ5. Dự đoán hành động bị sai

### Vấn đề

Ở VĐ2, dự đoán áp cho việc di chuyển, và mọi sai lệch được sửa liên tục đến mức người chơi gần như không nhận ra. Với **hành động rời rạc** (nhặt vật, giao hàng, bắn) thì khác. Nếu client dự đoán "nhặt được" rồi server báo "không", client đã hiển thị một trạng thái **không bao giờ xảy ra** trên server. Các bước mô phỏng sau đó (người chơi đi tiếp trong khi đang cầm vật) cũng đều dựa trên trạng thái sai này.

### Các hướng giải pháp

**A. Không dự đoán hành động.** Chờ server xác nhận rồi mới hiển thị. Luôn đúng, nhưng mỗi lần nhặt phải chờ khoảng một RTT, quay lại đúng vấn đề của VĐ2.

**B. Dự đoán + quay lui cục bộ** (rollback). Client hiển thị kết quả ngay. Nếu server từ chối, client **quay về trạng thái đã được xác nhận gần nhất**, rồi **mô phỏng lại** tới hiện tại với thông tin đúng.

**C. Quay lui toàn phần kiểu GGPO.** Đây là mô hình của game đối kháng (Street Fighter, Mortal Kombat), chạy **ngang hàng**, không có server mô phỏng:
- Mỗi máy mô phỏng toàn bộ trận đấu, hai máy chỉ gửi lệnh điều khiển cho nhau.
- Khi chưa có lệnh của đối thủ, mỗi máy **đoán** đối thủ giữ nguyên lệnh của khung hình trước.
- Khi lệnh thật đến mà khác với dự đoán, máy **quay lui** về khung hình đó và mô phỏng lại mọi khung hình tới hiện tại, **gói gọn trong thời gian vẽ một khung hình**.
- Thường kết hợp thêm 1–3 khung hình **trễ lệnh cố định** để giảm số khung hình phải quay lui.

### So sánh

| | T1 | T2 | T3 | T4 | T5 |
|---|---|---|---|---|---|
| A. Không dự đoán hành động | ○ chờ 1 RTT mỗi lần nhặt | ● | ◐ | ● | ◐ |
| B. Dự đoán + quay lui cục bộ | ● | ● server vẫn quyết định | ● | ◐ cần lưu trạng thái đã xác nhận | ● |
| C. Quay lui toàn phần (GGPO) | ● | ◐ ngang hàng, không có trọng tài | ● | ○ cần tất định tuyệt đối | ○ thiết kế cho mô hình ngang hàng, nhóm đã có server |

### Lựa chọn: dự đoán + quay lui cục bộ

**Cách hoạt động.**

```
Client A:
  nhịp 50   bấm nhặt → (dự đoán) vật vào tay ┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄
  nhịp 56   nhận ITEM_REJECTED (bị từ chối)
            ↺ quay về trạng thái đã xác nhận ở nhịp 50
            mô phỏng lại từ nhịp 50 tới 56 với "không cầm vật" ━━━━━━
            → vật "bật ra khỏi tay", xuất hiện trong tay người B
```

Về hiển thị, nên có hoạt ảnh (vật bay từ tay A sang tay B, hiệu ứng "hụt") để người chơi hiểu là mình bị tranh mất, chứ không phải game bị lỗi.

Không phải hành động nào cũng xử lý giống nhau:

- **Vật thường** (không ai tranh): gần như chắc chắn thành công, nên luôn dự đoán.
- **Vật bị tranh chấp:** vẫn dự đoán, chấp nhận thỉnh thoảng phải quay lui.
- **Điểm số chính thức và kết thúc trận:** **không dự đoán**, chỉ hiển thị khi server đã xác nhận.

**Quay lui khác đối chiếu ở điểm nào.**

| | Đối chiếu với server (VĐ2) | Quay lui (tổng quát) |
|---|---|---|
| Áp dụng cho | Trạng thái di chuyển của chính mình | Toàn bộ trạng thái game, kể cả hành động và lệnh của người khác |
| Nguồn sự thật | Ảnh trạng thái từ server | Server, hoặc lệnh thật của đối thủ (mô hình ngang hàng) |
| Tần suất | Mỗi ảnh trạng thái; người chơi thường không thấy | Khi dự đoán sai; người chơi thường thấy được |
| Quan hệ | **Là một trường hợp riêng của quay lui**: quay về trạng thái server rồi chạy lại lệnh của mình | |

**Tính tất định** (determinism). Quay lui kiểu ngang hàng và đồng bộ theo bước (VĐ1, hướng B) đòi hỏi **tất định tuyệt đối**: cùng trạng thái và cùng lệnh phải ra cùng kết quả, đến từng bit, trên mọi máy. Nếu không, các máy sẽ dần **lệch đồng bộ** (desync) mà không ai sửa được. Những nguồn phá vỡ tính tất định:

- **Số thực dấu phẩy động:** kết quả có thể khác nhau giữa các CPU, trình biên dịch và mức tối ưu hoá. Thứ tự cộng cũng ảnh hưởng (`(a+b)+c ≠ a+(b+c)`), và các hàm như `sin`, `sqrt` có thể cho kết quả khác nhau giữa các thư viện. Cách khắc phục: dùng số dấu phẩy cố định.
- **Thứ tự xử lý:** duyệt `Dictionary` hoặc `HashSet` không đảm bảo thứ tự, nên phải xử lý các thực thể theo thứ tự mã định danh.
- **Số ngẫu nhiên:** dùng bộ sinh số ngẫu nhiên có hạt giống chung, và gọi đúng số lần như nhau ở mọi phía.
- **Thời gian thực:** không dùng đồng hồ hệ thống trong mô phỏng, chỉ dùng số nhịp.

Trong mô hình **server quyết định** của nhóm, yêu cầu này nhẹ hơn nhiều. Client chỉ cần mô phỏng **gần giống** server, vì server định kỳ gửi trạng thái đầy đủ và client tự sửa theo (VĐ2). Sai số nhỏ của số thực chỉ gây những lần chỉnh nhỏ, không tích luỹ thành lệch đồng bộ. Đây thêm một lý do để chọn mô hình server quyết định ở VĐ1.

---

# Phần III. Giải pháp lựa chọn và demo sơ bộ

## 5. Kiến trúc tổng hợp

| Vấn đề con | Giải pháp chọn | Đánh đổi chấp nhận |
|---|---|---|
| VĐ1. Ai giữ quyền quyết định | Server quyết định, mô phỏng 30 nhịp/giây, dùng TCP + UDP | Điều khiển trễ 1 RTT (giải ở VĐ2) |
| VĐ2. Điều khiển bị ì | Dự đoán phía client + đối chiếu với server | Phải giữ bộ đệm lệnh và chạy lại; cần mô phỏng dùng chung |
| VĐ3. Đối thủ giật | Nội suy thực thể, ngoại suy ngắn khi thiếu gói | Thấy đối thủ ở quá khứ |
| VĐ4. Tương tác không công bằng | Bù trễ, cửa sổ phân xử, giới hạn thời gian tua lại | Người bị ảnh hưởng chịu thiệt |
| VĐ5. Dự đoán hành động sai | Dự đoán + quay lui cục bộ | Thỉnh thoảng thấy hành động bị huỷ |

```
┌──────────────────── Client ─────────────────────┐            ┌──────────────── Server ──────────────────┐
│ Bàn phím → Lệnh (seq, nhịp) ──┬─ bộ đệm chờ     │  UDP lệnh  │ Hàng đợi lệnh (mỗi người chơi)            │
│                               │                 │ ─────────▶ │        │                                  │
│                               ▼                 │            │        ▼                                  │
│                 Simulation.Step (dự đoán)       │            │ Vòng lặp 30 nhịp/giây: Simulation.Step    │
│                               ▲                 │            │        │  (cùng code với client)          │
│    đối chiếu: đặt lại + chạy lại lệnh chờ       │            │        ├─▶ Lịch sử trạng thái (1 giây)    │
│                               ▲                 │  UDP ảnh   │        │     └─▶ Bù trễ                    │
│ Bộ đệm ảnh trạng thái ────────┤ ◀────────────── │ ◀───────── │        ▼                                  │
│        └─▶ Nội suy (đối thủ)                    │            │ Ảnh trạng thái 20 lần/giây               │
│                                                 │            │   (+ lastProcessedSeq)                    │
│ Quay lui hành động ◀── ITEM_GRANTED/REJECTED ── │ ◀─ TCP ─── │ Phân xử vật bị tranh chấp                 │
└─────────────────────────────────────────────────┘            └───────────────────────────────────────────┘
```

Giao thức (nháp):

| Gói | Kênh | Nội dung |
|---|---|---|
| `LOGIN`, `FIND_MATCH`, `MATCH_FOUND` | TCP | Đăng nhập, ghép trận, mã phòng, danh sách vật của trận |
| `INPUT` | UDP, client → server | `seq:u32`, `clientTick:u32`, `renderTick:u32`, `keys:u8` (mỗi bit ứng với một phím), kèm 2–3 lệnh trước |
| `SNAPSHOT` | UDP, server → client | `serverTick:u32`, `lastProcessedSeq:u32`, danh sách thực thể `{id, x, y, holdingItem}`, trạng thái vật |
| `ITEM_GRANTED` / `ITEM_REJECTED` | TCP | Kết quả phân xử vật bị tranh chấp, kèm số nhịp |
| `SCORE`, `MATCH_END` | TCP | Điểm chính thức, kết quả trận |

Công nghệ: client dùng Unity (C#); server dùng .NET (C#) với `System.Net.Sockets`; cơ sở dữ liệu SQLite. Code mô phỏng dùng chung là một file C# thuần.

## 6. Bối cảnh áp dụng: game xếp đơn hàng siêu thị

### 6.1 Luật chơi

Nhóm áp dụng kiến trúc trên vào một game đối kháng 2 người, chủ đề siêu thị:

- Mỗi người chơi là một **nhân viên siêu thị** và có **quầy khách hàng riêng**.
- Mỗi trận, server chọn ngẫu nhiên **6 loại hàng** (trong số 10 loại) để bày lên kệ.
- Điều khiển: **WASD** để di chuyển, **J** để nhặt đồ hoặc đưa đồ cho khách, **K** để thả đồ đang cầm.
- Mỗi khách yêu cầu **3 món** theo thứ tự. Bong bóng yêu cầu chỉ hiện khi người chơi **đứng gần khách**, nên người chơi phải nhớ yêu cầu và chạy qua lại.
- Giao đủ và đúng thứ tự thì được **+5 điểm**.
- Trận kéo dài **2 phút 30 giây**, ai nhiều điểm hơn thì thắng. Người nào thoát giữa trận thì đối thủ thắng.
- Bổ sung để tạo tranh chấp: **món hàng hiếm**. Một số món chỉ có **1 cái** trên kệ chung ở giữa bản đồ, hai người phải tranh nhau.

### 6.2 Vì sao game này hợp để minh hoạ

Luật chơi đơn giản, nhưng game có đủ cả 5 vấn đề con:

| Yếu tố trong game | Vấn đề con | Kỹ thuật |
|---|---|---|
| Điểm số, giao hàng đúng hay sai | VĐ1: phải do server quyết định | Server quyết định |
| Chạy bằng WASD liên tục | VĐ2: cần phản hồi ngay | Dự đoán + đối chiếu |
| Thấy đối thủ chạy trong siêu thị | VĐ3: đối thủ phải chuyển động mượt | Nội suy |
| Hai người cùng bấm J vào món hàng hiếm | VĐ4: ai chạm tới trước | Bù trễ |
| Món hàng hiếm "bật ra khỏi tay" người thua | VĐ5: dự đoán bị sai | Quay lui |

Tình huống tranh món hàng hiếm đặc biệt dễ thấy trên sân khấu: khán giả cùng lúc nhìn thấy cả bù trễ (ai thắng) và quay lui (người thua bị lấy lại món hàng).

## 7. Demo sơ bộ

### 7.1 Phạm vi

Demo ở buổi này là **bản chứng minh ý tưởng** (proof-of-concept) để minh hoạ kỹ thuật, chưa phải game hoàn chỉnh.

- **Đối tượng:** 2 ô vuông màu trên một mặt phẳng, chỉ có di chuyển. Người A dùng WASD, người B dùng phím mũi tên.
- **Màn hình chia 3 khung:** Client A · Server · Client B. Khung server là "sự thật"; hai khung client là những gì mỗi người chơi nhìn thấy.
- **Mạng giả lập** chạy trong cùng tiến trình, chỉnh được bằng thanh trượt: độ trễ, dao động độ trễ, tỉ lệ mất gói, tần số gửi ảnh trạng thái.
- **Công tắc:**
  - chuyển giữa chế độ client tự quyết và server quyết định;
  - bật/tắt dự đoán, đối chiếu, nội suy.
- **Thông tin hiển thị trên màn hình:**
  - RTT;
  - nhịp hiện tại của server;
  - số lệnh đang nằm trong bộ đệm chờ;
  - "bóng" vị trí theo server, đặt cạnh vị trí dự đoán;
  - độ trễ nội suy hiện tại.
- **Thông số:** mô phỏng 30 nhịp/giây. Độ trễ nội suy **tự tính** theo công thức `2 × chu kỳ gửi ảnh + dao động độ trễ` (VĐ3), nên khi đổi tần số gửi ảnh trong lúc demo, độ trễ hiển thị luôn khớp với lý thuyết.

### 7.2 Mạng giả lập

Lớp truyền tin được viết dưới dạng một interface:

```csharp
interface ITransport {
    void Send(byte[] packet);
    bool TryReceive(out byte[] packet);
}
```

Lớp `SimulatedTransport` hoạt động như sau:

- **Khi gửi:** bỏ gói theo xác suất mất gói. Nếu không bỏ, tính thời điểm giao `deliverAt = now + độ trễ/2 + ngẫu nhiên(−dao động, +dao động)` rồi đưa gói vào hàng đợi ưu tiên sắp theo `deliverAt`.
- **Khi nhận:** chỉ trả ra các gói có `deliverAt ≤ now`.
- Mỗi gói có độ trễ ngẫu nhiên riêng nên có thể **đến sai thứ tự**, giống UDP thật.

Lý do dùng mạng giả lập ở buổi này:

- Chạy trên cùng một máy thì RTT gần bằng 0, nên các kỹ thuật trông như không cần thiết. Dù có dùng socket thật thì vẫn phải giả lập độ trễ.
- Không phụ thuộc mạng của phòng học, lặp lại được, và đặt được 3 khung nhìn cạnh nhau.
- Bản cuối chỉ cần thay bằng `UdpTransport` (vẫn có thể bọc lớp giả lập bên ngoài), **không phải sửa code kỹ thuật mạng**.

Bộ giả lập tạo ra đúng các hiện tượng mà kỹ thuật mạng phải xử lý: độ trễ, dao động độ trễ, mất gói, sai thứ tự. Mạng thật còn có thêm vài hiện tượng: mất gói hàng loạt, nghẽn làm độ trễ tăng dần, thay đổi đường đi. Những hiện tượng này có thể bổ sung sau và không làm thay đổi thiết kế.

### 7.3 Kịch bản (~5–6 phút)

| # | Cấu hình | Thao tác | Khán giả thấy | Vấn đề con |
|---|---|---|---|---|
| 0 | Client tự quyết, độ trễ 100 ms | Bấm nút "dịch chuyển" (giả lập gói tin bị sửa) | Ô của A nhảy tới bất kỳ đâu; server và B đều chấp nhận | VĐ1 |
| 1 | Server quyết định, độ trễ 200 ms, tắt mọi kỹ thuật | A chạy qua lại; bấm lại nút "dịch chuyển" | Nút dịch chuyển hết tác dụng, nhưng bấm phím khoảng 200 ms sau mới thấy chạy | VĐ1 → VĐ2 |
| 2 | Bật dự đoán | Chạy rồi dừng đột ngột | Phản hồi tức thì, nhưng nhân vật bị giật ngược | VĐ2 |
| 3 | Bật thêm đối chiếu | Lặp lại thao tác | Mượt, khớp với "bóng" server; số lệnh chờ ≈ RTT / nhịp ≈ 6 | VĐ2 |
| 4 | Nhìn khung B; gửi ảnh 10 lần/giây, dao động độ trễ 50 ms | A chạy vòng tròn | Ô của A trong mắt B giật từng nấc | VĐ3 |
| 5 | Bật nội suy (độ trễ nội suy tự tính ≈ 250 ms) | Lặp lại; sau đó tăng tần số gửi ảnh lên 20 lần/giây | Mượt nhưng trễ so với khung server; ở 20 lần/giây, độ trễ giảm còn ≈ 150 ms | VĐ3 |

VĐ4 và VĐ5 **không demo ở buổi này**. Hai vấn đề này giải thích bằng hình dòng thời gian trên slide, và sẽ được demo ở bản cuối qua tình huống tranh món hàng hiếm.

## 8. Giới hạn và hướng làm tiếp

**Giới hạn của demo sơ bộ:**

- Mạng giả lập chạy trong cùng tiến trình, chưa đi qua socket thật.
- Chỉ có di chuyển, chưa có vật thể, nên chưa minh hoạ được VĐ4 và VĐ5.
- Chưa có đo đạc định lượng (sai lệch vị trí, số lần quay lui theo độ trễ).

**Bản cuối:**

- Server .NET chạy độc lập, dùng UDP và TCP thật, có đăng nhập và ghép trận đơn giản (SQLite).
- Game xếp đơn hàng đầy đủ: kệ hàng, khách hàng, điểm số, món hàng hiếm.
- Bù trễ với cửa sổ phân xử; quay lui cho hành động nhặt.
- Công tắc bật/tắt bù trễ để so sánh kết quả phân xử.
- Đo đạc và vẽ biểu đồ:
  - sai lệch giữa vị trí dự đoán và vị trí server theo độ trễ;
  - tỉ lệ thắng khi tranh vật theo chênh lệch độ trễ, khi bật và khi tắt bù trễ.

---

# Phụ lục

## A. Trả lời Q&A

Mỗi câu trả lời dài 2–3 câu. Ai phụ trách phần nào thì nắm các câu hỏi của phần đó.

**Vì sao không tin trạng thái do client gửi lên?** (VĐ1)
Client chạy trên máy người chơi nên mọi thứ client gửi đều sửa được: dịch chuyển tức thời, hack tốc độ, tự cộng điểm. Khi server quyết định, client chỉ gửi lệnh điều khiển, nên tệ nhất cũng chỉ gửi được những phím mà người thật bấm được; server còn giới hạn số lệnh xử lý mỗi nhịp.

**Sao không dùng đồng bộ theo bước, ngang hàng?** (VĐ1)
Cách này phải chờ lệnh của người chậm nhất ở mỗi bước, và cần tính tất định tuyệt đối giữa các máy. Nó hợp với game chiến thuật có nhiều đơn vị, nơi gửi lệnh rẻ hơn gửi trạng thái. Game của nhóm chỉ có 2 người và đã có server, nên để server quyết định thì đơn giản và an toàn hơn.

**UDP hay TCP/WebSocket: đánh đổi gì? Vì sao dùng cả hai?** (VĐ1)
TCP đảm bảo thứ tự, nên một gói mất sẽ chặn mọi gói sau (nghẽn đầu hàng). Điều đó không hợp với dữ liệu chuyển động, vốn chỉ cần bản mới nhất. UDP không chặn, nhưng ứng dụng phải tự thêm số thứ tự, xác nhận và gửi dư. Dữ liệu cần chắc chắn tới nơi (đăng nhập, kết quả phân xử, điểm) đi bằng TCP; dữ liệu tần suất cao và nhanh cũ (lệnh, ảnh trạng thái) đi bằng UDP. WebSocket chạy trên TCP nên cũng gặp vấn đề nghẽn đầu hàng.

**Vì sao không gửi lệnh điều khiển bằng TCP?** (VĐ1)
Mất một gói lệnh thì TCP giữ lại mọi lệnh phía sau cho tới khi gửi lại xong, gây khựng ít nhất một RTT. Với UDP, mỗi gói kèm vài lệnh trước đó, nên mất một gói thì gói sau đã mang lại lệnh bị mất.

**Chọn tần số mô phỏng thế nào?** (VĐ1)
Đó là cân bằng giữa độ chính xác, độ trễ và chi phí CPU. Game di chuyển không quá nhanh như xếp đơn hàng thì 30 nhịp/giây là đủ; game bắn súng cạnh tranh thường dùng từ 60 nhịp/giây trở lên.

**Xử lý mất gói và gói đến sai thứ tự ra sao?** (VĐ1, VĐ2)
Mọi gói đều có số thứ tự hoặc số nhịp, gói cũ hơn gói đã nhận thì bỏ. Lệnh được gửi dư vài lệnh trước để bù mất gói. Ảnh trạng thái luôn chứa trạng thái đầy đủ, nên mất một ảnh thì ảnh sau thay thế. Server thiếu lệnh thì dùng lại lệnh trước, và client tự sửa qua bước đối chiếu.

**Chỉ dự đoán mà không đối chiếu thì sao?** (VĐ2)
Ảnh trạng thái phản ánh quá khứ, nên ghi đè vị trí theo nó sẽ kéo nhân vật lùi về chỗ cũ (giật ngược). Bước đối chiếu giữ lại những lệnh server chưa xử lý và chạy lại chúng sau khi đặt lại trạng thái.

**Có dự đoán được cho đối thủ không?** (VĐ3)
Có thể, đó là ngoại suy: đoán vị trí theo vận tốc cuối cùng. Ngoại suy không bị trễ nhưng sai mỗi khi đối thủ đổi hướng, nên hợp với vật thể có quán tính lớn. Nhân vật người đổi hướng tức thì nên dùng nội suy; nhóm chỉ ngoại suy ngắn khi thiếu gói.

**Độ trễ nội suy nên chọn bao nhiêu?** (VĐ3)
Ít nhất bằng 2 lần chu kỳ gửi ảnh cộng dao động độ trễ, để mất một gói vẫn còn dữ liệu nội suy. Ví dụ 20 lần/giây thì khoảng 100 ms; 10 lần/giây với dao động 50 ms thì khoảng 250 ms. Demo tự tính theo đúng công thức này.

**Vì sao bù trễ gây ra "đã chạy đi rồi vẫn bị bắt"? Đánh đổi gì?** (VĐ4)
Server tua về thời điểm mà người hành động nhìn thấy, tức một quá khứ trong đó người kia vẫn đứng ở chỗ cũ. Công bằng cho người hành động phải trả giá bằng việc người bị ảnh hưởng thấy bất công. Nhóm giới hạn thời gian tua lại để người có độ trễ quá cao không được lợi quá nhiều.

**Client gửi số nhịp giả để được lợi từ bù trễ thì sao?** (VĐ4)
Server chỉ chấp nhận số nhịp gần với `nhịp hiện tại − RTT đo được − độ trễ nội suy`, và giới hạn thời gian tua lại tối đa (200–250 ms). Số nhịp ngoài khoảng đó thì bị kẹp về giới hạn hoặc bị từ chối.

**Quay lui khác đối chiếu ở điểm nào?** (VĐ5)
Đối chiếu là trường hợp riêng của quay lui: chỉ áp cho việc di chuyển của mình, diễn ra liên tục và thường không thấy được. Quay lui tổng quát áp cho toàn bộ trạng thái (hành động, lệnh của người khác), chỉ xảy ra khi dự đoán sai và thường thấy được.

**Tính tất định ảnh hưởng thế nào tới dự đoán và quay lui?** (VĐ5)
Dự đoán chỉ đúng khi client mô phỏng giống server. Khi server quyết định, sai số nhỏ được ảnh trạng thái sửa lại, nên chỉ cần "gần giống". Quay lui kiểu ngang hàng (GGPO) và đồng bộ theo bước không có ai sửa, nên phải giống đến từng bit: dùng số dấu phẩy cố định thay số thực, cố định thứ tự xử lý thực thể, dùng bộ sinh số ngẫu nhiên có hạt giống chung.

**Demo dùng mạng giả lập thì có phản ánh đúng mạng thật không?** (Demo)
UDP trên cùng một máy có độ trễ gần 0, nên dù dùng socket thật vẫn phải giả lập độ trễ. Bộ giả lập tạo ra đúng các hiện tượng mà kỹ thuật mạng phải xử lý. Lớp truyền tin là interface `ITransport`, nên bản cuối thay bằng UDP thật mà không phải sửa phần còn lại.

## B. Bảng đối chiếu thuật ngữ Việt – Anh

Dùng để tra khi đọc tài liệu tiếng Anh hoặc khi giảng viên hỏi bằng thuật ngữ tiếng Anh.

| Tiếng Việt (dùng trong tài liệu) | Tiếng Anh | Nghĩa |
|---|---|---|
| Game nhiều người chơi thời gian thực | Real-time multiplayer game | Nhiều người chơi cùng lúc, trạng thái thay đổi liên tục |
| Kỹ thuật mạng cho game | Netcode | Các kỹ thuật đồng bộ game qua mạng |
| Độ phản hồi | Responsiveness | Mức độ thao tác có kết quả ngay |
| Tính nhất quán | Consistency | Mọi người thấy cùng một trạng thái |
| Độ trễ | Latency | Thời gian gói tin đi một chiều |
| Thời gian khứ hồi | Round-trip time (RTT) | Thời gian gói đi và quay về |
| Dao động độ trễ | Jitter | Độ trễ thay đổi giữa các gói |
| Mất gói | Packet loss | Gói tin bị bỏ giữa đường |
| Sai thứ tự / trùng gói | Out-of-order / duplicate | Gói đến không đúng thứ tự gửi / đến hai lần |
| Nghẽn đầu hàng | Head-of-line blocking | Trong TCP, một gói mất chặn mọi dữ liệu phía sau |
| Thời gian chờ gửi lại | Retransmission timeout (RTO) | Thời gian TCP chờ trước khi gửi lại gói |
| Client tự quyết | Client-authoritative | Client quyết định trạng thái của mình |
| Chuyển tiếp | Relay | Server chỉ chuyển gói cho người khác |
| Server quyết định | Server-authoritative | Server là nguồn sự thật duy nhất |
| Đồng bộ theo bước | Lockstep | Mọi máy chờ đủ lệnh của mọi người rồi mới bước tiếp |
| Ngang hàng | Peer-to-peer (P2P) | Các máy kết nối trực tiếp, không qua server |
| Nhịp mô phỏng | Tick | Một bước mô phỏng có độ dài cố định |
| Bước thời gian cố định | Fixed timestep | Mỗi bước mô phỏng dùng cùng một `dt` |
| Tần số mô phỏng | Tick rate | Số nhịp mỗi giây |
| Lệnh điều khiển | Input | Phím người chơi bấm ở một nhịp |
| Số thứ tự | Sequence number (seq) | Số tăng dần đánh cho mỗi gói / lệnh |
| Xác nhận | Acknowledgement (ack) | Báo cho bên gửi biết đã nhận |
| Gửi dư | Redundancy | Gửi kèm dữ liệu cũ để bù mất gói |
| Ảnh trạng thái | Snapshot | Trạng thái game server gửi cho client |
| Bộ đệm / bộ đệm vòng | Buffer / ring buffer | Vùng nhớ tạm / vùng nhớ ghi đè vòng tròn |
| Dự đoán phía client | Client-side prediction | Client tự áp lệnh lên trạng thái của mình, không chờ server |
| Đối chiếu với server | Server reconciliation | Đặt lại trạng thái theo server rồi chạy lại các lệnh chưa được xác nhận |
| Chạy lại | Replay | Áp lại các lệnh lên trạng thái |
| Giật ngược | Rubber-banding | Nhân vật bị kéo lùi rồi chạy tiếp |
| Thực thể | Entity | Một đối tượng trong game (nhân vật, vật) |
| Nội suy thực thể | Entity interpolation | Vẽ thực thể khác ở quá khứ, nội suy giữa hai ảnh trạng thái |
| Độ trễ nội suy | Interpolation delay | Khoảng thời gian vẽ lùi lại |
| Ngoại suy | Extrapolation / dead reckoning | Đoán vị trí tương lai theo vận tốc |
| Bù trễ | Lag compensation | Server tua lại trạng thái về thời điểm client nhìn thấy để phân xử |
| Ưu tiên người bắn | Favor the shooter | Tin theo góc nhìn của người hành động |
| Quay lui | Rollback | Quay về trạng thái đã xác nhận rồi mô phỏng lại |
| Trễ lệnh cố định | Input delay | Cố ý trễ lệnh của mình vài khung hình |
| Tính tất định | Determinism | Cùng trạng thái và cùng lệnh luôn cho cùng kết quả |
| Lệch đồng bộ | Desync | Các máy mô phỏng dần khác nhau |
| Khung hình | Frame | Một lần vẽ màn hình |
| Bản chứng minh ý tưởng | Proof-of-concept | Bản làm nhanh để chứng minh kỹ thuật chạy được |

## C. Tài liệu tham khảo

1. Gabriel Gambetta, *Fast-Paced Multiplayer* (4 phần: Client-Server Game Architecture; Client-Side Prediction and Server Reconciliation; Entity Interpolation; Lag Compensation). gabrielgambetta.com
2. Valve Developer Community, *Source Multiplayer Networking*; Yahn Bernier, *Latency Compensating Methods in Client/Server In-game Protocol Design and Optimization*, 2001.
3. Glenn Fiedler, *Gaffer On Games*: *Fix Your Timestep!*; *What Every Programmer Needs to Know About Game Networking*; *UDP vs. TCP*.
4. Timothy Ford, *Overwatch Gameplay Architecture and Netcode*, GDC 2017.
5. Michael Stallone, *8 Frames in 16ms: Rollback Networking in Mortal Kombat and Injustice 2*, GDC 2018.
6. GGPO — Rollback networking SDK. ggpo.net
7. RFC 768 (UDP), RFC 9293 (TCP), RFC 6298 (Computing TCP's Retransmission Timer).
