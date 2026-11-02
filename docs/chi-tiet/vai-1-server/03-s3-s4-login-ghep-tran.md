# S3 + S4 - Đăng nhập SQLite và ghép trận

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến | `feature/s3-s4-login-matchmaking` | S3: 04/11 (M2), S4: 05/11 | S1, S2 | C2, S5, R2 |

## Mục tiêu

Người chơi đăng ký / đăng nhập, bấm tìm trận, hai người đầu hàng đợi được ghép vào một phòng.

## Các bước

### S3 - Đăng nhập

- [ ] Package `Microsoft.Data.Sqlite`. Bảng `User(id INTEGER PK, username TEXT UNIQUE, pass_hash BLOB, salt BLOB,
      created_at)` và `MatchResult(id, p1, p2, score1, score2, winner, ended_at)`.
- [ ] Tạo bảng khi khởi động nếu chưa có; đường dẫn từ `DB_PATH`.
- [ ] REGISTER: kiểm tra độ dài username/password, hash PBKDF2 (`Rfc2898DeriveBytes`, salt 16 byte ngẫu nhiên).
- [ ] LOGIN: so hash; trả `LOGIN_RESULT(status, playerId, sessionToken)`. Token ngẫu nhiên 4-8 byte, lưu trong session.
- [ ] Một tài khoản đăng nhập 2 nơi → từ chối lần sau (hoặc đá phiên cũ; chọn một và ghi vào doc).
- [ ] SQLite không thích ghi đồng thời: dùng một kết nối + `lock`, hoặc mở kết nối mới mỗi lệnh.

### S4 - Ghép trận

- [ ] `Matchmaker`: hàng đợi `ConcurrentQueue` hoặc `List` + `lock`. FIND_MATCH → vào hàng; CANCEL_FIND hoặc ngắt → rời hàng.
- [ ] Đủ 2 người → tạo `Room(roomId, seed, itemIds[6])`, gửi MATCH_FOUND cho cả 2 (đối thủ, `udpPort`, seed, item).
- [ ] Người đã trong hàng / trong phòng gửi FIND_MATCH lần nữa → trả lỗi, không ghép trùng.

## Xong khi

- Bot/Unity: đăng ký → đăng nhập → sai mật khẩu bị từ chối đúng status code.
- 2 client FIND_MATCH → cả 2 nhận MATCH_FOUND cùng roomId; 4 client → 2 phòng khác nhau.

## Lưu ý

- Không ghi mật khẩu thật/tài khoản test có thật vào repo.
- Race condition kinh điển: 2 luồng cùng lấy người thứ 2 trong hàng → ghép 3 người. Phải lấy 2 người trong cùng một `lock`.

## Chuẩn bị vấn đáp

- Vì sao hash có salt? PBKDF2 khác SHA256 thường ở đâu?
- Nếu 100 người bấm FIND_MATCH cùng lúc, chỗ nào có thể lỗi và code đã chặn thế nào?
