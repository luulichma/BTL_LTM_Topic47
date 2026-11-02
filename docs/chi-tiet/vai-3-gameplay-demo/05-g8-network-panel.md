# G8 - Network Panel, preset, overlay debug

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #3 Phạm Quang Anh | `feature/g8-network-panel` | 07/11 (preset 1-5), 08/11 (preset 6) | G7, C4, C5, C6, R5 | Mọi cảnh demo |

## Mục tiêu

Công cụ để người dẫn demo bật từng kỹ thuật và cho khán giả thấy "trước / sau".

## Các bước

- [ ] Panel mở/đóng bằng **F1**:
  - Thanh trượt: latency (0-500 ms), jitter (0-100 ms), loss (0-30%) → gán vào `SimulatedUdp` (G7).
  - Công tắc: Naive relay / Authoritative, Prediction, Reconciliation, Interpolation, Lag compensation.
  - Công tắc phía server (relay, lag comp) gửi DEMO_CONFIG (R5); công tắc phía client đổi cờ trong code #2.
- [ ] Preset phím **1-6** (khớp kịch bản demo):

  | Phím | Cấu hình |
  | :-: | --- |
  | 1 | Relay, latency 200, jitter 50, mọi kỹ thuật tắt |
  | 2 | Authoritative, prediction tắt |
  | 3 | + Prediction |
  | 4 | + Reconciliation |
  | 5 | + Interpolation, jitter 100 |
  | 6 | Như 5; lag comp bật/tắt bằng công tắc; A = 50 ms, B = 250 ms (mỗi cửa sổ tự đặt) |

- [ ] Overlay: ping, tick hiện tại, số input chờ xác nhận, sai số reconcile, chế độ đang bật.
- [ ] "Bóng mờ" vị trí server cạnh nhân vật (dùng dữ liệu C4).
- [ ] Hiện dòng log phân xử từ server (nếu kịp) hoặc #1 chỉ trên cửa sổ server.

## Xong khi

- Bấm 1 → 5 lần lượt, mỗi cảnh thấy đúng hiện tượng như bảng kịch bản.
