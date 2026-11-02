# R4 - Kịch bản test và bằng chứng

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #3 Phạm Quang Anh làm chủ; #1 chạy test 1-5, 10; #2 chạy test 6, 8 | `test/r4-test-evidence` | Viết kịch bản 06/11, chạy xong 08/11 | R2, R3, các cảnh demo | `testing-guide.md`, README §6 |

## Mục tiêu

`docs/testing-guide.md` Part 2 có kết quả **chạy thật** cho bảng test dưới (tiêu chí B1, B2).

## Bảng test

| # | Test | Cách làm | Kỳ vọng | Ai | Xong |
| :-: | --- | --- | --- | --- | :-: |
| 1 | Nhiều phòng đồng thời | `docker compose run` bot với N = 20, 50 | 10/25 phòng, tick ≈ 30, không lỗi | #1 | [ ] |
| 2 | Client bị kill giữa trận | Tắt Unity / `docker kill` bot | Server dọn session, người còn lại nhận MATCH_END | #1 | [ ] |
| 3 | Client im lặng | Mở TCP rồi không gửi gì | Bị đóng sau timeout | #1 | [ ] |
| 4 | Dữ liệu rác TCP | `head -c 100000 /dev/urandom \| nc localhost 5000` | Đóng kết nối đó, server vẫn chạy | #1 | [ ] |
| 5 | UDP rác / giả token | Gửi gói sai độ dài, sai token | Bị bỏ, không ảnh hưởng phòng | #1 | [ ] |
| 6 | Mất gói / sai thứ tự | Loss 30% bằng G7 | Không mất bước di chuyển | #2 | [ ] |
| 7 | Cheat bị chặn | Cảnh 1 vs cảnh 2 | Relay: cheat được; authoritative: bị chặn | #3 | [ ] |
| 8 | Prediction / reconciliation | Sai số trước/sau reconcile (T4) | Sai số về ≈ 0 sau reconcile | #2 | [ ] |
| 9 | Lag compensation | Cảnh 6, A = 50 ms, B = 250 ms | Có/không lag comp cho kết quả khác nhau, khớp log | #3 | [ ] |
| 10 | Login sai, đăng ký trùng | Gửi sai mật khẩu, trùng username | Đúng status code trong `protocol-design.md` | #1 | [ ] |

## Các bước

- [ ] Viết kịch bản từng test (lệnh chạy, cấu hình, cách đọc kết quả) vào `testing-guide.md` Part 1 trước 06/11.
- [ ] Mỗi test: chụp ảnh/terminal output, ghi ngày giờ + commit hash → `docs/asset/`.
- [ ] Tổng hợp vào Part 2.

## Lưu ý

- **Không** bịa số liệu hay ảnh; test nào chưa chạy được thì ghi rõ "chưa đạt" và lý do.
