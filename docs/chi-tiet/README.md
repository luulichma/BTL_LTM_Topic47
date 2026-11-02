# Chi tiết đầu việc T47

Mỗi file là một đầu việc (thường = một nhánh git). Lịch tổng và deadline: [`../lo_trinh.md`](../lo_trinh.md).

Mỗi file có cùng khung: **thông tin** (người làm, nhánh, deadline, phụ thuộc) → **mục tiêu** → **các bước** (checklist)
→ **xong khi** → **lưu ý** → **chuẩn bị vấn đáp**. Đánh dấu `[x]` ngay trong file khi làm xong từng bước.

> Các con số, định dạng gói tin, tên lớp trong các file là **đề xuất**. Nhóm được đổi, nhưng phải ghi quyết định và
> lý do vào `docs/protocol-design.md` / `docs/architecture.md` của repo code.

## Chung (cả nhóm)

| File | Việc | Làm chủ | Deadline |
| --- | --- | --- | :-: |
| [01-t1-khung-repo.md](chung/01-t1-khung-repo.md) | T1 khung repo, `shared/`, solution | #1 | 02/11 |
| [02-quyet-dinh-thiet-ke.md](chung/02-quyet-dinh-thiet-ke.md) | Chốt Q1-Q9 | Cả nhóm | 02/11 |
| [03-m1-proposal.md](chung/03-m1-proposal.md) | M1 `proposal.md` | #1 | 03/11 |
| [04-t2-t5-t6-thuyet-trinh.md](chung/04-t2-t5-t6-thuyet-trinh.md) | Slide, Q&A, tập thuyết trình | #1 ghép | 08/11-09/11 |
| [05-m3-nop-bai.md](chung/05-m3-nop-bai.md) | M3 nộp bài, AI Disclosure, Contribution | Cả nhóm | 09/11 |

## Vai #1 - Server & giao thức (Nguyễn Thế Chiến)

| File | Việc | Deadline |
| --- | --- | :-: |
| [01-s2-protocol.md](vai-1-server/01-s2-protocol.md) | S2 protocol | 03/11 |
| [02-s1-r1-tcp-server-docker.md](vai-1-server/02-s1-r1-tcp-server-docker.md) | S1 TCP server, R1 Docker | 04/11 |
| [03-s3-s4-login-ghep-tran.md](vai-1-server/03-s3-s4-login-ghep-tran.md) | S3 login, S4 ghép trận | 04/11, 05/11 |
| [04-s5-s6-tick-snapshot.md](vai-1-server/04-s5-s6-tick-snapshot.md) | S5 tick loop, S6 snapshot | 05/11 |
| [05-r3-xu-ly-loi.md](vai-1-server/05-r3-xu-ly-loi.md) | R3 xử lý lỗi | 06/11 |
| [06-architecture.md](vai-1-server/06-architecture.md) | `architecture.md` | 04/11 → 08/11 |
| [07-s7-r5-naive-relay-demo-config.md](vai-1-server/07-s7-r5-naive-relay-demo-config.md) | S7 naive relay, R5 DEMO_CONFIG | 06/11 |
| [08-r2-bot-client.md](vai-1-server/08-r2-bot-client.md) | R2 bot client (cùng #2) | 07/11 |
| [09-ho-tro-g3-g5.md](vai-1-server/09-ho-tro-g3-g5.md) | Server phần G3, G5 | 06/11, 08/11 (trưa) |
| [10-s8-ket-thuc-tran.md](vai-1-server/10-s8-ket-thuc-tran.md) | S8 kết thúc trận | 07/11 |

## Vai #2 - Client netcode (Nguyễn Đức Anh)

| File | Việc | Deadline |
| --- | --- | :-: |
| [01-c1-shared-simulation.md](vai-2-client-netcode/01-c1-shared-simulation.md) | C1 shared simulation | 03/11 |
| [02-c2-unity-login.md](vai-2-client-netcode/02-c2-unity-login.md) | C2 Unity login, ghép trận | 04/11, 05/11 |
| [03-c3-input-udp.md](vai-2-client-netcode/03-c3-input-udp.md) | C3 INPUT qua UDP | 05/11 |
| [04-c4-prediction.md](vai-2-client-netcode/04-c4-prediction.md) | C4 prediction | 06/11 |
| [05-c5-reconciliation.md](vai-2-client-netcode/05-c5-reconciliation.md) | C5 reconciliation | 06/11 |
| [06-c6-interpolation.md](vai-2-client-netcode/06-c6-interpolation.md) | C6 interpolation | 07/11 |
| [07-t4-do-dac.md](vai-2-client-netcode/07-t4-do-dac.md) | T4 đo đạc, test 6 và 8 | 08/11 |
| [08-c7-c8-tuy-chon.md](vai-2-client-netcode/08-c7-c8-tuy-chon.md) | C7, C8 (tùy chọn) | nếu dư |

## Vai #3 - Gameplay, lag compensation, demo & test (Phạm Quang Anh)

| File | Việc | Deadline |
| --- | --- | :-: |
| [01-g7-network-simulator.md](vai-3-gameplay-demo/01-g7-network-simulator.md) | G7 bộ mô phỏng mạng | 03/11 |
| [02-g1-supermarket-scene.md](vai-3-gameplay-demo/02-g1-supermarket-scene.md) | G1 scene siêu thị | 04/11 |
| [03-g2-khach-hang-don-hang.md](vai-3-gameplay-demo/03-g2-khach-hang-don-hang.md) | G2 khách hàng, đơn hàng | 05/11 |
| [04-g3-nhat-tha-giao.md](vai-3-gameplay-demo/04-g3-nhat-tha-giao.md) | G3 nhặt/thả/giao | 06/11 |
| [05-g8-network-panel.md](vai-3-gameplay-demo/05-g8-network-panel.md) | G8 Network Panel | 07/11 |
| [06-g4-g5-do-hiem-lag-comp.md](vai-3-gameplay-demo/06-g4-g5-do-hiem-lag-comp.md) | G4 đồ hiếm, G5 lag compensation | 08/11 (trưa) |
| [07-r4-test-evidence.md](vai-3-gameplay-demo/07-r4-test-evidence.md) | R4 test + bằng chứng | 08/11 |
| [08-t3-demo-script-readme.md](vai-3-gameplay-demo/08-t3-demo-script-readme.md) | T3 demo script, README §3-§6 | 08/11 |
| [09-g6-rollback-tuy-chon.md](vai-3-gameplay-demo/09-g6-rollback-tuy-chon.md) | G6 rollback (tùy chọn) | nếu dư |
