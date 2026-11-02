# Hỗ trợ #3 - Phần server của G3 và G5

| Người làm | Nhánh | Deadline | Phụ thuộc | Làm cùng |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến | `feature/g3-server-validation` (06/11), `feature/g5-server-lag-comp` (08/11 trưa) | 06/11, 08/11 (trưa) | S5, S6, G2, G4 | #3 Phạm Quang Anh (phần Unity) |

Chỉ nhận khi việc chính của #1 ở bước tương ứng đã xong. Phần Unity (scene, UI, panel) vẫn do #3 làm.

## G3 phía server - xác thực nhặt/thả/giao

- [ ] Phím J/K là một phần của INPUT; server xử lý trong tick: J gần kệ (khoảng cách ≤ R) và tay rỗng → nhặt;
      J ở quầy của mình và đang cầm món đúng đơn → giao, +5 điểm; K → thả xuống đất tại vị trí hiện tại.
- [ ] Trạng thái đồ (trên kệ / trong tay ai / dưới đất) chỉ server sửa; đưa vào SNAPSHOT.
- [ ] Gửi ACTION_RESULT cho hành động bị từ chối (để client sửa dự đoán).

## G5 phía server - lag compensation

- [ ] Ring buffer vị trí 30 tick (đã có từ S6).
- [ ] Khi 2 người cùng J vào một món hiếm trong cùng khoảng tick: với mỗi người, tua về `viewTick` của input đó
      (giới hạn tối đa 30 tick trước), kiểm tra người đó có chạm món lúc ấy không; ai chạm sớm hơn (theo tick) thắng.
- [ ] Bật/tắt theo `lagComp` của R5. Khi tắt: ai có input tới server trước thắng.
- [ ] Log phân xử: `Tick 812: A chạm món #3 lúc tick 805, B lúc tick 807, A thắng (lagComp=on)`.

## Xong khi

- Cảnh 6: tắt lag comp → người mạng nhanh luôn thắng; bật → phân xử theo thời điểm bấm thật, có dòng log.

## Chuẩn bị vấn đáp

- Vì sao phải giới hạn tua lại tối đa 1 giây?
- Lag compensation có bất lợi gì cho người mạng tốt? ("bị nhặt mất dù đã tới trước trên màn hình mình")
