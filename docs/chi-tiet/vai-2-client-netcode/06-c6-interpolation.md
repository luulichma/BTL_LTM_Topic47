# C6 - Entity interpolation cho đối thủ

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #2 Nguyễn Đức Anh | `feature/c6-interpolation` | 07/11 | S6 | Cảnh demo 5, `ViewTick` cho G5 |

## Mục tiêu

Đối thủ di chuyển mượt dù snapshot tới không đều (jitter), bằng cách vẽ chậm 100 ms và nội suy giữa 2 snapshot.

## Các bước

- [ ] Buffer snapshot của đối thủ theo `serverTick` (giữ ~1 giây).
- [ ] `renderTime = (thời gian server ước lượng) - 100 ms`; tìm 2 snapshot kẹp `renderTime`, nội suy tuyến tính vị trí.
- [ ] Ước lượng thời gian server: từ `serverTick` snapshot mới nhất + thời gian trôi kể từ khi nhận (làm mượt bằng trung bình).
- [ ] Hết snapshot (mất gói dài): giữ vị trí cuối, hoặc ngoại suy tối đa 1 snapshot rồi dừng.
- [ ] Công tắc `Interpolation`: tắt → vẽ đối thủ ở snapshot mới nhất (giật như cảnh 1).
- [ ] Xuất `ViewTick` (tick tương ứng `renderTime`) cho C3 gửi lên server (G5 dùng).

## Xong khi

- Cảnh 5: jitter 100 ms, đối thủ vẫn mượt; tắt interpolation thấy giật rõ.

## Chuẩn bị vấn đáp

- Vì sao delay 100 ms? (snapshot 20 Hz = 50 ms/gói → luôn có ít nhất 2 gói để nội suy, chịu được 1 gói mất)
- Đánh đổi: đối thủ hiển thị ở quá khứ → vì vậy mới cần lag compensation.
