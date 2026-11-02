# S8 - Kết thúc trận và MATCH_END

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến | `feature/s8-match-end` | 07/11 | S5, G3 | Màn kết quả (#2/#3), R4 test 2 |

## Các bước

- [ ] Đồng hồ trận 2:30 = 4500 tick ở 30 Hz; hết giờ → so điểm → người thắng (hoặc hoà).
- [ ] Ghi `MatchResult` vào SQLite.
- [ ] Gửi MATCH_END (TCP) cho cả 2: điểm từng người, người thắng, lý do (`TIME_UP`, `OPPONENT_LEFT`).
- [ ] Đóng phòng: dừng vòng tick, gỡ token UDP, đưa người chơi về trạng thái "đã đăng nhập" để tìm trận mới.

## Xong khi

- Hết giờ: cả 2 nhận MATCH_END đúng điểm; có dòng mới trong bảng `MatchResult`.
- Một người rớt mạng: người còn lại nhận MATCH_END với `OPPONENT_LEFT`.
