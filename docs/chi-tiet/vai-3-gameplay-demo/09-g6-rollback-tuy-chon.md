# G6 - Rollback hành động dự đoán sai (tùy chọn)

| Người làm | Nhánh | Deadline | Phụ thuộc |
| --- | --- | :-: | --- |
| #3 Phạm Quang Anh | `feature/g6-rollback` | **Đã cắt**; chỉ làm nếu cảnh 6 xong trước 08/11 trưa | G5, C4 |

## Mục tiêu

Người thua trong cuộc tranh chấp đã "nhặt được" nhờ dự đoán; khi server phán thua, món đồ bật khỏi tay.

## Các bước

- [ ] Khi bấm J gần món: client dự đoán nhặt ngay (hiện món trong tay), ghi lại `seq` của hành động.
- [ ] Nhận ACTION_RESULT `accepted = 0` với `seq` đó → huỷ dự đoán: món bay về vị trí server báo (hiệu ứng "bật khỏi tay").
- [ ] Đếm số lần rollback để T4 đưa vào slide.

## Bản tối thiểu nếu không làm

- Không dự đoán nhặt; món chỉ vào tay khi snapshot xác nhận. Trình bày rollback bằng slide + giải thích.
