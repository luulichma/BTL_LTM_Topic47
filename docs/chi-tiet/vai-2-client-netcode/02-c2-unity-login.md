# C2 - Unity: đăng nhập, ghép trận, kết nối

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #2 Nguyễn Đức Anh | `feature/c2-unity-login` | Login 04/11 (M2), vào trận 05/11 | T1, S2, S3 (S4 cho vào trận) | C3, C8, G1-G3 tích hợp |

## Mục tiêu

Unity client đăng nhập, tìm trận và chuyển vào scene gameplay, dùng chung thư viện Protocol.

## Các bước

- [ ] Tạo project Unity trong `client/unity/` đúng phiên bản nhóm đã chốt; kiểm tra `.gitignore` trước commit đầu.
- [ ] Đưa `T47.Shared.dll` (build netstandard2.1) vào `Assets/Plugins/` - viết script copy sau mỗi lần build, hoặc
      dùng asmdef trỏ vào source. Ghi cách làm vào README repo.
- [ ] `NetClient` (MonoBehaviour, DontDestroyOnLoad): TCP `async` riêng; message nhận được đẩy vào `ConcurrentQueue`,
      xử lý trong `Update()` (**API Unity chỉ gọi được trên main thread**).
- [ ] Scene `Login`: ô host/port (mặc định `localhost:5000`, không hard-code trong code), username, password,
      nút Đăng nhập / Đăng ký, hiện lỗi theo status code.
- [ ] Scene `Lobby`: nút Tìm trận / Huỷ; nhận MATCH_FOUND → lưu roomId, seed, udpPort, token → load scene `Game`.
- [ ] Mở `UdpClient`, gửi UDP_HELLO (token) để server học endpoint.
- [ ] Ngắt kết nối → về màn Login với thông báo.

## Xong khi

- 04/11: Unity đăng nhập được tới server chạy trong Docker.
- 05/11: 2 cửa sổ Unity (hoặc Unity + bot) vào chung phòng.

## Lưu ý

- Chạy 2 cửa sổ Unity: build một bản standalone + chạy Editor, hoặc dùng ParrelSync.
- Không ghi tài khoản/mật khẩu mặc định thật vào code.

## Chuẩn bị vấn đáp

- Vì sao message mạng phải đưa về main thread mới xử lý?
