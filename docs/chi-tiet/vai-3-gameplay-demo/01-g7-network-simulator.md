# G7 - Bộ mô phỏng mạng (latency, jitter, packet loss)

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #3 Phạm Quang Anh | `feature/g7-network-simulator` | 03/11 | T1 (làm được ngay, không chờ ai) | G8, test 6, mọi cảnh demo |

## Mục tiêu

Tạo mạng xấu "nhân tạo" trên localhost để demo không phụ thuộc Wi-Fi: mỗi cửa sổ client chỉnh riêng độ trễ, jitter,
tỉ lệ mất gói.

## Các bước

- [ ] Lớp `SimulatedUdp` trong `shared/T47.Shared/Net/` (để Unity và bot cùng dùng), bọc `UdpClient`:
  - `Send(bytes)`: với xác suất `loss` thì bỏ; ngược lại xếp vào hàng đợi với thời điểm gửi
    `now + latency/2 + random(-jitter, +jitter)`.
  - Nhận: gói tới cũng đi qua hàng đợi trễ tương tự trước khi trả cho code game.
  - Một vòng/timer (hoặc gọi `Pump()` mỗi frame) đẩy các gói đã tới hạn đi.
- [ ] Tham số thay đổi lúc chạy: `LatencyMs` (0-500), `JitterMs` (0-100), `LossPercent` (0-30).
- [ ] Jitter có thể làm gói **đến sai thứ tự** - giữ nguyên hành vi này (thật như mạng thật) và báo #1/#2 biết.
- [ ] Test độc lập bằng 2 socket loopback: gửi 1000 gói đánh số, đo độ trễ trung bình/độ lệch, tỉ lệ mất.

## Xong khi

- Test loopback: latency 200 ms → đo được ≈ 200 ms; loss 30% → mất ≈ 30%.
- #2 thay `UdpClient` bằng `SimulatedUdp` trong `NetClient` mà không phải sửa logic khác.

## Lưu ý

- Chỉ áp dụng cho UDP; TCP (login) không cần mô phỏng.
- Latency là **RTT** hay **một chiều**? Chốt một cách hiểu, ghi vào doc và hiển thị đúng trên Network Panel.

## Chuẩn bị vấn đáp

- Vì sao đặt bộ mô phỏng ở client mà không ở server (Q6)?
- Jitter làm gì với thứ tự gói? Code phía nhận xử lý thế nào?
