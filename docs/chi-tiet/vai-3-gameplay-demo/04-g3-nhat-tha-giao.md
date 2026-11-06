# G3 - Nhặt, thả, giao đồ (phần Unity)

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #3 Phạm Quang Anh (phần server: #1, xem `vai-1-server/09-ho-tro-g3-g5.md`) | `feature/g3-pickup-deliver` | 06/11 | G2, S5, C3 | G4, S8 |

## Mục tiêu

Gameplay hoàn chỉnh trên mạng: J nhặt/giao, K thả; **server quyết định**, client chỉ hiển thị.

## Các bước

- [x] Phím J/K nằm trong bitmask INPUT (C3 đã gửi) - không gửi message riêng.
  `NetClient` đặt `SimInputKeys.J/K` trong INPUT; server xử lý theo cạnh nhấn phím.
- [ ] Hiển thị trạng thái đồ theo SNAPSHOT: trên kệ / trong tay ai / dưới đất.
- [ ] Hiệu ứng nhặt/giao/thả; điểm cập nhật theo snapshot.
- [ ] (Nếu kịp) dự đoán nhặt ngay khi bấm J để cảm giác nhanh; nếu ACTION_RESULT từ chối thì trả món về (đây chính là G6).
- [x] Phối hợp #1: khoảng cách nhặt `PickupRange` lấy từ hằng số chung trong `shared/`.
  `SimConstants.PickupRange = 1.2f`, server `MatchItems` dùng chung.

## Tiến độ

- **06/11:** phần server xong và đã vào `main` (`8490442`). Phần Unity chưa có nhánh `feature/g3-pickup-deliver`
  trên origin: client chưa vẽ `Items`/`Orders` từ SNAPSHOT, chưa xử lý `ACTION_RESULT`.

## Xong khi

- 2 người chơi qua mạng: nhặt, giao đủ đơn, được +5, cả 2 màn hình thấy giống nhau.
