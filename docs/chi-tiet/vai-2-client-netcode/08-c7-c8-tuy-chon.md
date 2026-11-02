# C7 + C8 - Smoothing, camera, HUD (tùy chọn)

| Người làm | Nhánh | Deadline | Phụ thuộc |
| --- | --- | :-: | --- |
| #2 Nguyễn Đức Anh | `feature/c7-smoothing`, `feature/c8-hud-camera` | **Đã cắt**; chỉ làm nếu C6 xong trước 07/11 trưa | C5, C2 |

## C7 - Smoothing khi reconcile

- [ ] Tách "vị trí logic" (sau reconcile) và "vị trí hiển thị"; mỗi frame kéo vị trí hiển thị về vị trí logic một phần
      (vd. 20%/frame). Sai số quá lớn (> 1 ô) thì nhảy thẳng.

## C8 - Camera, HUD

- [ ] Camera theo nhân vật; HUD: đồng hồ 2:30 (lấy từ server tick), điểm 2 người, đơn hàng hiện tại.
- [ ] Màn kết quả khi nhận MATCH_END (nếu #3 chưa làm).

## Bản tối thiểu nếu không làm

- HUD chỉ cần đồng hồ + điểm dạng chữ; camera cố định nhìn toàn bản đồ.
