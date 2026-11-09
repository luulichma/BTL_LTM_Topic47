# 02 — Login và modal Register

Quyết định liên quan: Q3, Q4, Q13. File sửa: `Assets/Scripts/UI/LoginController.cs`.
**Protocol không đổi**: vẫn gửi `LOGIN` / `REGISTER`, nhận `LOGIN_RESULT` / `REGISTER_RESULT` / `ERROR` như cũ.

## 1. Bố cục

```
┌──────────── Login (panel 420 ảo, giữa màn) ────────────┐
│                T47 — Siêu thị đối kháng                │
│  Tên đăng nhập  [______________________]               │
│  Mật khẩu       [______________________]               │
│  [            ĐĂNG NHẬP (primary)            ]         │
│  <status Login>                                        │
│  Chưa có tài khoản?  Đăng ký  (link)                   │
│  ▸ ⚙ Cài đặt máy chủ          (mặc định thu gọn)       │
│     Máy chủ [________]  Cổng TCP [_____]               │
└────────────────────────────────────────────────────────┘

        ░░░░░░ overlay tối khi mở Register ░░░░░░
        ┌──────── Đăng ký tài khoản ────────┐
        │ Tên đăng nhập     [____________]  │
        │ Mật khẩu          [____________]  │
        │ Nhập lại mật khẩu [____________]  │
        │ <status Register>                 │
        │ [ Huỷ ]          [ ĐĂNG KÝ ]      │
        └───────────────────────────────────┘
```

Panel Login rộng 420 đơn vị ảo, chiều cao tự co theo nội dung (`GUILayout`), luôn nằm giữa màn theo `UiScale.Width/Height`. Modal Register rộng 400 đơn vị ảo.

## 2. Trạng thái màn hình

```csharp
private enum Screen { Login, Register }      // chỉ phía client
private Screen _screen = Screen.Login;
private bool _showServerSettings;            // mục Cài đặt máy chủ
private string _loginStatus, _registerStatus;
private string _regUser, _regPass, _regPass2;
```

- Login và Register có **ô nhập và status riêng**. Hai form không còn dùng chung `_username/_password/_status`.
- Khi `_screen == Register`: vẫn vẽ Login phía sau nhưng `GUI.enabled = false`, rồi vẽ overlay và modal lên trên.
- `_pendingAuthentication` giữ như cũ (chống bấm 2 lần). Nút gửi hiển thị "Đang gửi…" và bị khoá khi đang chờ.

## 3. Kiểm tra phía client (trước khi gửi)

| Kiểm tra | Áp dụng | Thông báo |
|----------|---------|-----------|
| Host không trống, port nằm trong 1–65535 | Cả hai form | "Kiểm tra lại Cài đặt máy chủ." (đồng thời tự mở mục đó) |
| Username không trống sau Trim, tối đa 20 | Cả hai form | "Hãy nhập tên đăng nhập." |
| Mật khẩu không trống, tối đa 64 | Cả hai form | "Hãy nhập mật khẩu." |
| `_regPass == _regPass2` | Register | "Mật khẩu nhập lại không khớp." |

Server vẫn là nơi kiểm tra cuối cùng. Kiểm tra phía client chỉ để giảm các vòng gửi thừa và hiện lỗi sớm cho người dùng.

## 4. Luồng sự kiện

```
[Login] --click "Đăng ký"--> [Register modal]
[Register] --Huỷ / Esc--> [Login] (xoá _regPass, _regPass2)
[Register] --Đăng ký hợp lệ--> connect nếu chưa nối --> gửi REGISTER
   REGISTER_RESULT OK   --> đóng modal; _username = _regUser; _loginStatus = "Đăng ký thành công, hãy đăng nhập."
                            focus ô Mật khẩu của Login
   REGISTER_RESULT lỗi  --> ở lại modal; _registerStatus = StatusText.For(code)
   ERROR / mất kết nối  --> status của form đang mở
[Login] --Đăng nhập hợp lệ--> gửi LOGIN
   LOGIN_RESULT OK      --> LoadScene("Lobby") (như cũ)
   lỗi                  --> _loginStatus
```

- Gửi xong thì xoá mật khẩu khỏi bộ nhớ màn hình, giữ hành vi hiện tại của `SendPendingAuthentication`.
- Kết quả trả về đi vào form nào thì tuỳ loại message (`RegisterResult` hay `LoginResult`), không phụ thuộc màn đang mở.
- **Không tự đăng nhập** sau khi đăng ký thành công, để giữ đúng luồng protocol và dễ giải thích khi vấn đáp.

## 5. Phím tắt

- `Tab`: chuyển ô theo thứ tự `GUI.SetNextControlName`. IMGUI đã hỗ trợ sẵn qua keyboard control; chỉ cần đặt tên để focus được bằng code.
- `Enter` / `KeypadEnter`: kiểm tra `Event.current.type == EventType.KeyDown` trong `OnGUI` rồi gọi gửi form đang mở.
- `Esc`: đóng modal Register.
- Khi mở modal thì focus ô Tên đăng nhập (`GUI.FocusControl`).

## 6. Cài đặt máy chủ

- Mặc định thu gọn. Giá trị lấy từ `ServerEndpointConfig.Load()` như hiện tại.
- Một dòng tóm tắt khi thu gọn: "Máy chủ: 127.0.0.1:7777".
- Tự mở ra nếu kiểm tra host/port thất bại.

## 7. Tiêu chí hoàn thành

Các mục Login/Register trong checklist ở [README §5](README.md#5-checklist-kiểm-chứng).
