# S5 + S6 - Vòng tick 30 Hz và SNAPSHOT

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến | `feature/s5-s6-tick-snapshot` | 05/11 | S4, C1 | C3, C5, C6, G3, G5, R2 |

## Mục tiêu

Mỗi phòng có một vòng lặp cố định 30 tick/giây: lấy input, chạy `Simulation.Step` dùng chung, gửi SNAPSHOT 20 Hz
kèm `lastProcessedSeq` để client reconcile.

## Các bước

### S5 - Tick loop và nhận INPUT

- [ ] `UdpGateway`: một `UdpClient` bind `UDP_PORT`, một vòng `ReceiveAsync` duy nhất. Kiểm tra header + token →
      tìm phòng → đẩy vào `room.Inbox` (`ConcurrentQueue`). Gói đầu tiên (UDP_HELLO) dùng để **học endpoint** của người chơi.
- [ ] `Room.Run()`: `Stopwatch` + bước cố định `dt = 1/30 s`; mỗi tick: rút hết inbox → với mỗi người, áp dụng input
      theo thứ tự `seq` (bỏ seq ≤ seq đã xử lý) → `Simulation.Step` → `tick++`.
- [ ] Input dư thừa (client gửi kèm 3 input cũ) giúp chống mất gói: server chỉ xử lý input có `seq` mới.
- [ ] Người không gửi input ở tick này → dùng input "không bấm gì" (hoặc lặp input cuối, ghi lựa chọn vào doc).
- [ ] Mỗi phòng một `Task` riêng; phòng lỗi không làm sập phòng khác (try/catch quanh vòng tick, log rồi đóng phòng).

### S6 - SNAPSHOT

- [ ] Mỗi 1,5 tick (≈ 20 Hz; đơn giản nhất: gửi ở tick chẵn và mỗi tick thứ 3, hoặc dùng bộ đếm thời gian) gửi
      SNAPSHOT tới cả 2 người: `serverTick`, `lastProcessedSeq` từng người, vị trí/vận tốc, đồ cầm, kệ, điểm.
- [ ] Lưu lịch sử vị trí 30 tick gần nhất (ring buffer) - chuẩn bị cho G5.
- [ ] Gói INPUT trùng/sai thứ tự: bỏ nếu `seq` ≤ `lastProcessedSeq`.

## Xong khi

- 2 client vào trận và di chuyển, mỗi bên thấy bên kia (chưa prediction, chưa mượt cũng được).
- Log mỗi 5 giây: tick thực tế/giây của mỗi phòng ≈ 30.
- 10 phòng chạy song song không phòng nào tụt tick rõ rệt.

## Lưu ý

- Không dùng `Thread.Sleep(33)` đơn thuần - sai số tích luỹ. Tính thời điểm tick kế tiếp từ `Stopwatch`.
- Vòng tick là **luồng duy nhất** sửa state phòng, nên không cần `lock` trên state game; chỉ inbox là thread-safe.
- `Simulation.Step` phải là bản trong `shared/`, không viết lại logic di chuyển ở server.

## Chuẩn bị vấn đáp

- Vì sao tick cố định thay vì xử lý input ngay khi tới?
- `lastProcessedSeq` dùng để làm gì ở phía client?
- Live change: đổi tick rate 30 → 20 thì những gì bị ảnh hưởng?
