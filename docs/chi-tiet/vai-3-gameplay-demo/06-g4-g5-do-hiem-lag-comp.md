# G4 + G5 - Kệ đồ hiếm và lag compensation (cảnh 6)

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #3 Phạm Quang Anh (server: #1, xem `vai-1-server/09-ho-tro-g3-g5.md`) | `feature/g4-g5-rare-items-lag-comp` | 08/11 (trưa); bỏ nếu tối 07/11 chưa có cảnh 1-5 | G3, S6, C6 (`ViewTick`) | Cảnh demo 6, test 9 |

## Mục tiêu

Tạo tình huống 2 người tranh một món đồ hiếm và cho thấy lag compensation phân xử công bằng.

## Các bước

### G4 - Kệ đồ hiếm

- [ ] Kệ chung ở giữa bản đồ, mỗi món hiếm chỉ có 1 cái; đơn hàng thỉnh thoảng yêu cầu món hiếm (để 2 người cùng cần).
- [ ] Bản gọn cho demo: luôn có 1 món hiếm mà cả 2 đơn đều cần.

### G5 - Lag compensation (hiểu và trình bày, phối hợp #1 phía server)

- [ ] Hiểu vì sao cần: đối thủ trên màn hình mình là **quá khứ** (interp delay 100 ms + latency), nên người mạng chậm
      luôn "tới trễ" nếu server xét theo thời điểm nhận gói.
- [ ] Client gửi `ViewTick` trong INPUT (C3/C6 đã có) - kiểm tra giá trị đúng.
- [ ] Cùng #1 kiểm thử: A = 50 ms, B = 250 ms, cùng lao vào kệ giữa và bấm J; tắt lag comp → A luôn thắng;
      bật → ai chạm trước theo màn hình của mình thì thắng.
- [ ] Hiển thị kết quả rõ: món đồ về tay người thắng; người thua thấy thông báo (rollback nếu làm G6).

## Xong khi

- Chạy cảnh 6 được 5 lần liên tiếp, kết quả khớp với dòng log phân xử của server.

## Chuẩn bị vấn đáp

- Lag compensation khác prediction ở chỗ nào? (một cái ở server, sửa quá khứ; một cái ở client, đoán tương lai)
- Vì sao giới hạn tua 1 giây; người gian lận gửi `ViewTick` giả thì sao?
