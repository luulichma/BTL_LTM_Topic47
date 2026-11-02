# R3 - Xử lý lỗi và đầu vào xấu

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến | `feature/r3-error-handling` | 06/11 | S1-S6 | R4 (test 2-5) |

## Mục tiêu

Server không bao giờ crash vì một client: ngắt đột ngột, gửi rác, im lặng, hay gửi gói UDP giả.

## Các bước

- [ ] Frame TCP có `length` > 4096 hoặc = 0 → gửi ERROR (nếu được) rồi đóng kết nối đó.
- [ ] Payload sai định dạng (decode lỗi) → ERROR `BAD_FRAME`, đếm số lần; quá N lần → đóng.
- [ ] Timeout: không nhận gì trong 15 giây (đề xuất) → đóng. Client gửi PING mỗi 5 giây khi rảnh.
- [ ] Ngắt đột ngột (`IOException`, đọc ra 0 byte): `finally` dọn session, rời hàng đợi, báo phòng.
- [ ] Rớt mạng giữa trận (TCP đóng hoặc không có UDP trong 5 giây) → người còn lại thắng, gửi MATCH_END, đóng phòng (Q8).
- [ ] UDP: gói ngắn hơn header, sai magic/version, token không tồn tại → bỏ im lặng, chỉ tăng bộ đếm log.
- [ ] Mọi handler bọc try/catch ở ranh giới session/phòng; log có ID session.

## Xong khi

- Chạy được các test 2, 3, 4, 5 trong `07-r4-test-evidence.md`, server vẫn phục vụ client khác bình thường.

## Lưu ý

- Đừng nuốt exception im lặng ở mọi chỗ; bắt ở ranh giới, log rồi dọn dẹp.
- Gửi rác TCP thử nhanh: `head -c 100000 /dev/urandom | nc localhost 5000`.

## Chuẩn bị vấn đáp

- Server phân biệt "client im lặng" và "client đã chết" thế nào?
- Nếu một phòng ném exception trong vòng tick, các phòng khác có bị ảnh hưởng không? Vì sao?
