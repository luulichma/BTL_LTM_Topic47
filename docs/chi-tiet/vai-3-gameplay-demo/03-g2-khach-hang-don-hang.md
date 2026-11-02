# G2 - Khách hàng và đơn hàng 3 món

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #3 Phạm Quang Anh | `feature/g2-customers-orders` | 05/11 | G1 | G3 |

## Mục tiêu

Mỗi quầy có khách yêu cầu 3 món; đứng gần quầy thì bong bóng hiện yêu cầu.

## Các bước

- [ ] Logic đơn hàng đặt trong `shared/` (vd. `Simulation/Orders.cs`) để server cũng dùng: sinh đơn 3 món từ seed + số
      thứ tự đơn; giao đúng 1 món → gạch món đó; đủ 3 → +5 điểm, sinh đơn mới.
- [ ] Unity: bong bóng hiện danh sách món khi khoảng cách tới quầy ≤ R.
- [ ] Hiển thị món đã giao/còn thiếu.
- [ ] Unit test cho logic đơn hàng (cùng seed → cùng chuỗi đơn).

## Xong khi

- Offline: đứng gần quầy thấy yêu cầu; logic đơn hàng có test.

## Lưu ý

- Chốt với #1: +5 điểm cho **mỗi đơn đủ 3 món** hay **mỗi món đúng**? (tài liệu ghi "đưa đúng đơn được +5") - ghi rõ vào doc.
