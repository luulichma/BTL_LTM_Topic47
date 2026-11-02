# C4 - Client-side prediction

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #2 Nguyễn Đức Anh | `feature/c4-prediction` | 06/11 | C3 | C5, G6, cảnh demo 3 |

## Mục tiêu

Nhân vật của mình đi **ngay khi bấm phím**, không chờ server trả lời.

## Các bước

- [ ] Mỗi tick client: ngoài việc gửi input, gọi luôn `Simulation.Step(ref localState, input, Dt, map)` và vẽ theo `localState`.
- [ ] Công tắc `Prediction` (bật/tắt từ Network Panel): tắt → vẽ theo vị trí trong snapshot mới nhất (cảnh 2 "ì").
- [ ] "Bóng mờ" server: vẽ thêm một hình mờ ở vị trí server báo trong snapshot gần nhất (G8 dùng chung).
- [ ] Va chạm với đối thủ: prediction dùng vị trí đối thủ đang vẽ → sẽ lệch với server, đây là điều cảnh 3 muốn cho thấy.

## Xong khi

- Cảnh 3: latency 200 ms, điều khiển vẫn mượt; khi va chạm thấy nhân vật lệch so với bóng server.

## Chuẩn bị vấn đáp

- Prediction đúng trong trường hợp nào và sai trong trường hợp nào?
- Vì sao không predict đối thủ? (không biết input của họ → dùng interpolation)
