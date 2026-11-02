# C3 - Gửi INPUT theo tick qua UDP

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #2 Nguyễn Đức Anh | `feature/c3-input-udp` | 05/11 | C1, C2, S5 | C4, test 6 |

## Mục tiêu

Client gửi input đều 30 lần/giây, có số thứ tự, chịu được mất gói.

## Các bước

- [ ] Client tick cố định 30 Hz (tích luỹ thời gian trong `Update`, chạy 0..n tick mỗi frame), không phụ thuộc FPS.
- [ ] Mỗi tick: đọc phím → `InputCmd { Seq = ++seq, ClientTick, ViewTick, Keys }`.
- [ ] Lưu vào **bộ đệm input chưa xác nhận** (`List`/ring buffer).
- [ ] Gửi gói INPUT chứa input hiện tại + tối đa 3 input gần nhất chưa được xác nhận (dư thừa chống mất gói).
- [ ] Khi SNAPSHOT tới với `lastProcessedSeq = k` → xoá mọi input có `seq ≤ k` khỏi bộ đệm.
- [ ] Overlay debug: số input đang chờ xác nhận (G8 dùng lại).

## Xong khi

- Server log nhận ≈ 30 input/giây/người; bật loss 30% (G7) vẫn không mất bước di chuyển nào (test §5 số 6).

## Chuẩn bị vấn đáp

- Vì sao gửi kèm input cũ thay vì dùng TCP cho input?
- `ViewTick` là gì, lấy từ đâu? (tick snapshot đối thủ đang được vẽ, sau khi trừ interp delay - dùng cho G5)
