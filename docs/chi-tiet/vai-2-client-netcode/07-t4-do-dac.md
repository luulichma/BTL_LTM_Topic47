# T4 - Đo đạc và test 6, 8

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #2 Nguyễn Đức Anh (#1 hỗ trợ phía server) | `feature/t4-metrics` | 08/11 | C5, C6, G7 | Slide phần 6, `testing-guide.md` |

## Mục tiêu

Có **số đo thật** để đưa vào slide và `testing-guide.md`.

## Các bước

- [ ] Ghi log CSV phía client: tick, sai số reconcile, số input chờ, RTT.
- [ ] Đo ở 3 mức mạng (0 / 100 / 200 ms, jitter 50 ms):
  - Sai số dự đoán trung bình / lớn nhất khi đi thẳng và khi va chạm.
  - Độ trễ cảm nhận: có prediction vs không prediction (ước lượng = RTT).
  - Băng thông: byte/giây gửi và nhận mỗi client (đếm byte trong NetClient), so với bản naive relay.
- [ ] Test §5 số 6: loss 30% → đếm bước di chuyển bị mất (kỳ vọng 0 nhờ input dư thừa).
- [ ] Test §5 số 8: bảng sai số trước/sau reconcile.
- [ ] Vẽ 1-2 biểu đồ từ CSV, chụp ảnh overlay → `docs/asset/`.

## Xong khi

- Có bảng số và ảnh trong `testing-guide.md` Part 2, đều lấy từ lần chạy thật (ghi ngày giờ, cấu hình).
