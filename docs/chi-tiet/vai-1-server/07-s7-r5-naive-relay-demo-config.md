# S7 + R5 - Naive relay và DEMO_CONFIG

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến (phối hợp #3) | `feature/s7-r5-naive-relay-demo-config` | 06/11 | S5, S6 | Cảnh demo 1-2, G8 |

## Mục tiêu

Server có thêm chế độ "giống bản gốc" để làm baseline, và Network Panel trên Unity bật/tắt được chế độ phía server.

## Các bước

### S7 - Naive relay

- [ ] Message UDP `POS_REPORT` (client tự báo vị trí) - chỉ dùng ở chế độ relay.
- [ ] Chế độ relay: server **tin** vị trí client gửi lên, chỉ chuyển tiếp cho đối thủ (không chạy `Step`).
- [ ] Cheat demo: client có phím bật "dịch chuyển" (gửi vị trí sai) - ở relay thì thành công, ở authoritative thì vô tác dụng
      (server bỏ qua POS_REPORT).

### R5 - DEMO_CONFIG

- [ ] Message TCP `DEMO_CONFIG(roomMode: relay/authoritative, lagComp: on/off)`; chỉ chấp nhận khi env `DEMO_MODE=true`,
      ngược lại trả ERROR.
- [ ] Đổi chế độ áp dụng ở đầu tick kế tiếp (không đổi giữa tick).
- [ ] Log khi đổi chế độ: `Room 3: mode=relay lagComp=off (bởi player A)`.

## Xong khi

- Cảnh 1 (relay, cheat thành công) và cảnh 2 (authoritative, cheat bị chặn) chạy được bằng preset trên Network Panel.

## Chuẩn bị vấn đáp

- Vì sao DEMO_CONFIG phải khoá bằng `DEMO_MODE`? (không cho người chơi thật đổi luật)
