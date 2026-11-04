# Lịch đưa tài liệu của #1 lên repo classroom

> **Đã đổi (02/11):** nhóm quyết định đưa hết bản nháp lên một lần, commit ghi rõ "đăng bù" và có dòng `ai-log.md`.
> Lịch dưới đây không dùng nữa, chỉ giữ làm thứ tự **cập nhật tài liệu cho khớp code** khi từng phần code xong.

| Người làm | Bản nháp đầy đủ | Deadline cuối |
| --- | --- | :-: |
| #1 Nguyễn Thế Chiến | [`../../ban-nhap-repo/`](../../ban-nhap-repo/) (protocol-design, architecture, testing-guide, README) | 08/11 |

## Nguyên tắc

- **Tài liệu lên cùng lúc với code mà nó mô tả.** Nếu viết sẵn chi tiết về phần chưa có code thì tài liệu sai lệch với
  repo, và thầy hỏi "chỗ này code ở đâu?" thì không trả lời được.
- Mỗi lần đưa lên: **đọc lại, sửa bằng lời của mình**, chỉnh cho khớp với code thật (tên lớp, hằng số, timeout).
  Chỗ nào chưa tự giải thích được thì chưa đưa lên.
- Commit bằng tài khoản của chính mình, **đúng ngày làm thật**. Không sửa ngày commit.
- Mỗi lần commit có dùng bản nháp AI thì ghi **một dòng `docs/ai-log.md` cùng ngày**. Khai báo AI không làm giảm điểm,
  giấu mới bị trừ (INSTRUCTION.md §7).

## Lịch

| Ngày | Nhánh / PR | Đưa lên | Đi cùng code |
| :-: | --- | --- | --- |
| 03/11 | `feature/s2-protocol` | `protocol-design.md` §1-§5 (tổng quan, framing, format, bảng lệnh, status code), §7 (giới hạn), §8.1-§8.4 (TCP+UDP, framing, nhị phân, token) | S2: `MessageType`, encode/decode, `FrameReader`, unit test |
| 04/11 (M2) | `feature/s1-tcp-server`, `feature/s3-s4-login-matchmaking` | `protocol-design.md` §6.1 (đăng nhập), §6.6 (dữ liệu rác), §8.5 (stateful), §8.6 (mật khẩu); `architecture.md` §1 (mô hình đồng thời, chưa có §1.2), §2, §3 (bảng shared state với phần đã có code); README §4 | S1 TCP server + Docker, S3 login; `git tag m2` |
| 05/11 | `feature/s5-s6-tick-snapshot` | `protocol-design.md` §4.3 (quy tắc INPUT), §6.2 (ghép trận + UDP), §6.3 (INPUT/SNAPSHOT); `architecture.md` §1.1 (vòng tick), bổ sung §3 (Inbox, Channel UDP) | S4, S5, S6 |
| 06/11 | `feature/r3-error-handling` | `architecture.md` §4 (vòng đời kết nối, phát hiện ngắt, dọn dẹp, rớt mạng = thua); `protocol-design.md` §6.5 | R3 |
| 07/11 | `feature/s8-match-end` | `protocol-design.md` §6.4 (kết thúc trận), §8.7 (`viewTick`); `architecture.md` §1.2 (1.000 client, kèm số đo thật từ bot) | S8, R2 bot |
| 08/11 | `docs/architecture` | `architecture.md` §5 (Docker, 2 máy LAN), §6 (bảng quyết định); README §5.2; `testing-guide.md` §1.5; rà cả hai file khớp code; §9 mở rộng giao thức | Chạy thử 2 máy LAN, ghi log thật vào Part 2 |

Nếu code của một ngày trễ thì phần tài liệu tương ứng **trễ theo**, không đưa trước.

## Xong khi

- [ ] Hết bảng trên, mỗi dòng là một commit/PR riêng đúng ngày.
- [ ] Mỗi lần có dòng `ai-log.md` tương ứng.
- [ ] Tự giải thích được mọi đoạn đã đưa lên, kể cả từng sequence diagram.
