# Lộ trình T47 - Real-time Multiplayer (Client Prediction)

> Lộ trình làm sản phẩm từ **02/11/2026**. Hạn nộp chính thức **10/11/2026**; nhóm tự đặt **09/11** là xong hết và nộp,
> giữ 10/11 làm ngày dự phòng (chỉ sửa lỗi gấp, không thêm gì).
> Chi tiết từng đầu việc nằm trong [`chi-tiet/`](chi-tiet/README.md). Mã việc (S/C/G/R/T) theo file phân công
> `T47_YTuongDemo_PhanCong.docx`. Repo code: `int1433-05-20261-grp-int1433-05-0005`.
> Cập nhật file này sau mỗi buổi họp: đánh dấu `[x]` khi xong, ghi lý do nếu trễ.

---

## 1. Nhóm và vai

| # | Họ và tên | MSSV | Vai | Trọng tâm |
| :-: | --- | --- | --- | --- |
| 1 | Nguyễn Thế Chiến (trưởng nhóm) | B23DCCN095 | #1 - Server & giao thức; hỗ trợ #3 | Server authoritative, tick loop, TCP/UDP, login, ghép trận, SQLite, Docker, bot, xử lý lỗi |
| 2 | Nguyễn Đức Anh | B23DCCN031 | #2 - Client netcode | Shared simulation, Unity kết nối, prediction, reconciliation, interpolation, đo đạc |
| 3 | Phạm Quang Anh | B23DCCN044 | #3 - Gameplay, lag compensation, demo & test | Scene siêu thị, đơn hàng, nhặt/giao, đồ hiếm, lag compensation, bộ mô phỏng mạng, Network Panel, test, demo |

**Đã có:** Unity và Docker đã cài (02/11); 9 quyết định thiết kế Q1-Q9 đã chốt theo đề xuất
([chung/02](chi-tiet/chung/02-quyet-dinh-thiet-ke.md)).
**Còn thiếu:** .NET 8 SDK trên máy từng người, GitHub username #2 và #3 trong README repo, cột Lớp.

---

## 1b. Hai sản phẩm khác nhau, ưu tiên theo điểm

- **Repo classroom** (`int1433-05-...`) là thứ **được chấm** theo rubric INSTRUCTION.md. Phát triển gần như riêng với
  bản demo; cái gì từ demo áp sang được thì áp.
- **Demo thuyết trình (simulation) đã xong**, không nằm trong lộ trình này.

Điểm mỗi người = A (3, nhóm) + B (3, nhóm) + C (4, **cá nhân**). Thứ tự ưu tiên khi thiếu thời gian:

| Ưu tiên | Tiêu chí | Điểm | Rẻ hay đắt | Việc tương ứng |
| :-: | --- | :-: | --- | --- |
| 1 | C - Vấn đáp (mỗi người) | 4.0 | Rẻ nếu tự viết/tự hiểu code | Mỗi người tự commit phần mình, đọc lại từng dòng, luyện "sửa nhanh tại chỗ" (đổi max frame size, thêm 1 lệnh) |
| 2 | A2 + A3 - Protocol & Architecture | 2.0 | **Rẻ nhất** (chỉ là tài liệu) | `protocol-design.md`: framing, bảng lệnh, status code, sequence diagram, **vì sao** TCP+UDP; `architecture.md`: mô hình đồng thời, mọi shared state + cách khoá, vòng đời kết nối (lấy từ Q1-Q9) |
| 3 | B1 - Đồng thời & chịu lỗi | 1.5 | Trung bình | R3 + bot R2: nhiều client, kill -9, dữ liệu rác, im lặng → server không sập, giải phóng socket |
| 4 | A1 - Problem & Idea | 1.0 | Rẻ | `proposal.md` có so sánh phương án (relay vs authoritative) |
| 5 | B2 - Features | 1.0 | Đắt | Chỉ tính tính năng **có test thật** trong `testing-guide.md`; ít mà chắc hơn nhiều mà dở |
| 6 | B3 - Hygiene | 0.5 | Rẻ | Clone sạch → `docker compose up --build` chạy; `.env`; lịch sử commit đều, mỗi người một tài khoản |

Hệ quả: tài liệu thiết kế, chịu lỗi và việc **mỗi người hiểu phần mình** quan trọng hơn cảnh demo đẹp. Không ai nhận
phần mình không giải thích được (không giải thích được thì mất điểm cả B lẫn C).

---

## 2. Các mốc bắt buộc

Chỉ có **7 ngày**, nên mỗi ngày là một mốc. Deadline là **cuối ngày (23:00)**, đã gộp vào `main`.

| Ngày | Mốc | Điều kiện đạt | Chịu trách nhiệm |
| --- | --- | --- | --- |
| **02/11 (tối)** | Khung repo | Nhánh `chore/t1-repo-skeleton` đã vào `main`; cả nhóm đã đọc Q1-Q9 | #1 |
| **03/11** | **M1 - Proposal** + 3 việc nền | `proposal.md` xong, `git tag m1`; S2 protocol, C1 simulation, G7 mô phỏng mạng đã gộp | Cả nhóm |
| **04/11** | **M2 - Walking skeleton** | `docker compose up --build` chạy server, nhận nhiều kết nối cùng lúc, **LOGIN chạy end-to-end**; `protocol-design.md` + `architecture.md` bản đầu; `git tag m2` | #1 (chính), #2 |
| **05/11** | Vào trận | 2 client vào chung phòng, di chuyển được qua server (chưa prediction) | #1 + #2 |
| **06/11** | Cảnh demo 1-4 | Relay vs authoritative, prediction, reconciliation chạy được; server chịu lỗi (R3) | Cả nhóm |
| **07/11** | Cảnh demo 1-5 + tải | Interpolation, Network Panel preset 1-5; 20 bot chạy song song không lỗi; MATCH_END | Cả nhóm |
| **08/11** | **Đóng băng tính năng** | Cảnh 6 (nếu kịp); chạy đủ bảng test, có log/ảnh thật; slide xong; README §3-§6 | Cả nhóm |
| **09/11** | **M3 - Nộp** | Clone sạch → `docker compose up --build` → demo; AI Disclosure, Contribution, `ai-log.md`; tổng duyệt; **nộp** | Cả nhóm |

> Sau 08/11 chỉ sửa lỗi, không thêm tính năng. Ngày 09/11 dành cho kiểm tra cuối, tổng duyệt và nộp.
> 10/11 là hạn chính thức (commit cuối trên `main` của repo classroom): chỉ dùng nếu 09/11 phát hiện lỗi chặn.

---

## 3. Đầu việc theo vai và deadline

### 3.1 Vai #1 - Server & giao thức (Nguyễn Thế Chiến)

| # | Việc | Nhánh | Deadline | Chi tiết | Xong |
| :-: | --- | --- | :-: | --- | :-: |
| 0 | T1 khung repo; chép Q1-Q9 vào docs | `chore/t1-repo-skeleton` | 02/11 | [chung/01](chi-tiet/chung/01-t1-khung-repo.md), [chung/02](chi-tiet/chung/02-quyet-dinh-thiet-ke.md) | [ ] |
| 1 | M1 proposal | `docs/m1-proposal` | 03/11 | [chung/03](chi-tiet/chung/03-m1-proposal.md) | [ ] |
| 2 | S2 protocol + `protocol-design.md` | `feature/s2-protocol` | 03/11 | [vai-1/01](chi-tiet/vai-1-server/01-s2-protocol.md) | [ ] |
| 3 | S1 TCP server + R1 Docker | `feature/s1-tcp-server` | 04/11 | [vai-1/02](chi-tiet/vai-1-server/02-s1-r1-tcp-server-docker.md) | [ ] |
| 4 | S3 login SQLite (+ S4 ghép trận) | `feature/s3-s4-login-matchmaking` | 04/11 (S3), 05/11 (S4) | [vai-1/03](chi-tiet/vai-1-server/03-s3-s4-login-ghep-tran.md) | [ ] |
| 5 | S5 tick 30 Hz + S6 SNAPSHOT | `feature/s5-s6-tick-snapshot` | 05/11 | [vai-1/04](chi-tiet/vai-1-server/04-s5-s6-tick-snapshot.md) | [ ] |
| 6 | R3 xử lý lỗi | `feature/r3-error-handling` | 06/11 | [vai-1/05](chi-tiet/vai-1-server/05-r3-xu-ly-loi.md) | [ ] |
| 7 | S7 naive relay + R5 `DEMO_CONFIG` | `feature/s7-r5-naive-relay-demo-config` | 06/11 | [vai-1/07](chi-tiet/vai-1-server/07-s7-r5-naive-relay-demo-config.md) | [ ] |
| 8 | Hỗ trợ #3: server phần G3 | `feature/g3-server-validation` | 06/11 | [vai-1/09](chi-tiet/vai-1-server/09-ho-tro-g3-g5.md) | [ ] |
| 9 | R2 bot client (cùng #2) | `feature/r2-bot-client` | 07/11 | [vai-1/08](chi-tiet/vai-1-server/08-r2-bot-client.md) | [ ] |
| 10 | S8 kết thúc trận, MATCH_END | `feature/s8-match-end` | 07/11 | [vai-1/10](chi-tiet/vai-1-server/10-s8-ket-thuc-tran.md) | [ ] |
| 11 | Hỗ trợ #3: server phần G5 (cảnh 6) | `feature/g5-server-lag-comp` | 08/11 (trưa) | [vai-1/09](chi-tiet/vai-1-server/09-ho-tro-g3-g5.md) | [ ] |
| 12 | `architecture.md` (bản đầu 04/11, bản cuối 08/11) | `docs/architecture` | 04/11 → 08/11 | [vai-1/06](chi-tiet/vai-1-server/06-architecture.md) | [ ] |
| 13 | Chạy test số 1-5, 10 | (cùng `test/r4-test-evidence`) | 08/11 | [vai-3/07](chi-tiet/vai-3-gameplay-demo/07-r4-test-evidence.md) | [ ] |

### 3.2 Vai #2 - Client netcode (Nguyễn Đức Anh)

| # | Việc | Nhánh | Deadline | Chi tiết | Xong |
| :-: | --- | --- | :-: | --- | :-: |
| 1 | C1 shared simulation + unit test tất định | `feature/c1-shared-simulation` | 03/11 | [vai-2/01](chi-tiet/vai-2-client-netcode/01-c1-shared-simulation.md) | [ ] |
| 2 | C2 Unity: login, kết nối TCP, ghép trận | `feature/c2-unity-login` | 04/11 (login), 05/11 (vào trận) | [vai-2/02](chi-tiet/vai-2-client-netcode/02-c2-unity-login.md) | [ ] |
| 3 | C3 gửi INPUT qua UDP | `feature/c3-input-udp` | 05/11 | [vai-2/03](chi-tiet/vai-2-client-netcode/03-c3-input-udp.md) | [ ] |
| 4 | C4 client-side prediction | `feature/c4-prediction` | 06/11 | [vai-2/04](chi-tiet/vai-2-client-netcode/04-c4-prediction.md) | [ ] |
| 5 | C5 server reconciliation | `feature/c5-reconciliation` | 06/11 | [vai-2/05](chi-tiet/vai-2-client-netcode/05-c5-reconciliation.md) | [ ] |
| 6 | C6 entity interpolation | `feature/c6-interpolation` | 07/11 | [vai-2/06](chi-tiet/vai-2-client-netcode/06-c6-interpolation.md) | [ ] |
| 7 | R2 bot client (cùng #1) | `feature/r2-bot-client` | 07/11 | [vai-1/08](chi-tiet/vai-1-server/08-r2-bot-client.md) | [ ] |
| 8 | T4 đo đạc + test số 6, 8 | `feature/t4-metrics` | 08/11 | [vai-2/07](chi-tiet/vai-2-client-netcode/07-t4-do-dac.md) | [ ] |
| - | C7 smoothing, C8 HUD/camera | `feature/c7-smoothing`, `feature/c8-hud-camera` | **Cắt**, chỉ làm nếu C6 xong trước 07/11 trưa | [vai-2/08](chi-tiet/vai-2-client-netcode/08-c7-c8-tuy-chon.md) | [ ] |

### 3.3 Vai #3 - Gameplay, lag compensation, demo & test (Phạm Quang Anh)

| # | Việc | Nhánh | Deadline | Chi tiết | Xong |
| :-: | --- | --- | :-: | --- | :-: |
| 1 | G7 bộ mô phỏng mạng | `feature/g7-network-simulator` | 03/11 | [vai-3/01](chi-tiet/vai-3-gameplay-demo/01-g7-network-simulator.md) | [ ] |
| 2 | G1 scene siêu thị offline | `feature/g1-supermarket-scene` | 04/11 | [vai-3/02](chi-tiet/vai-3-gameplay-demo/02-g1-supermarket-scene.md) | [ ] |
| 3 | G2 khách hàng, đơn 3 món | `feature/g2-customers-orders` | 05/11 | [vai-3/03](chi-tiet/vai-3-gameplay-demo/03-g2-khach-hang-don-hang.md) | [ ] |
| 4 | G3 nhặt/thả/giao (phần Unity) | `feature/g3-pickup-deliver` | 06/11 | [vai-3/04](chi-tiet/vai-3-gameplay-demo/04-g3-nhat-tha-giao.md) | [ ] |
| 5 | R4 viết kịch bản test | `test/r4-test-evidence` | 06/11 | [vai-3/07](chi-tiet/vai-3-gameplay-demo/07-r4-test-evidence.md) | [ ] |
| 6 | G8 Network Panel, preset 1-5, overlay | `feature/g8-network-panel` | 07/11 | [vai-3/05](chi-tiet/vai-3-gameplay-demo/05-g8-network-panel.md) | [ ] |
| 7 | G4 đồ hiếm + G5 lag compensation (phần Unity), preset 6 | `feature/g4-g5-rare-items-lag-comp` | 08/11 (trưa) | [vai-3/06](chi-tiet/vai-3-gameplay-demo/06-g4-g5-do-hiem-lag-comp.md) | [ ] |
| 8 | R4 chạy test + bằng chứng → `testing-guide.md` | `test/r4-test-evidence` | 08/11 | [vai-3/07](chi-tiet/vai-3-gameplay-demo/07-r4-test-evidence.md) | [ ] |
| 9 | T3 demo script + README §3-§6 | `docs/readme-final` | 08/11 | [vai-3/08](chi-tiet/vai-3-gameplay-demo/08-t3-demo-script-readme.md) | [ ] |
| - | G6 rollback | `feature/g6-rollback` | **Cắt**, chỉ làm nếu cảnh 6 xong trước 08/11 trưa | [vai-3/09](chi-tiet/vai-3-gameplay-demo/09-g6-rollback-tuy-chon.md) | [ ] |

### 3.4 Việc chung

| Việc | Làm chủ | Deadline | Chi tiết | Xong |
| --- | --- | :-: | --- | :-: |
| T2 slide, T5 Q&A | #1 ghép, mỗi người phần mình | 08/11 | [chung/04](chi-tiet/chung/04-t2-t5-t6-thuyet-trinh.md) | [ ] |
| T6 tổng duyệt thuyết trình, nộp db.ptit.edu.vn | #1 | 09/11 | [chung/04](chi-tiet/chung/04-t2-t5-t6-thuyet-trinh.md) | [ ] |
| AI Disclosure, Contribution, `ai-log.md`, kiểm tra clone sạch | Mỗi người tự điền, #1 kiểm tra | 09/11 | [chung/05](chi-tiet/chung/05-m3-nop-bai.md) | [ ] |

---

## 4. Lịch theo ngày (nhìn nhanh)

| Ngày | #1 Chiến | #2 Đức Anh | #3 Quang Anh |
| --- | --- | --- | --- |
| 02/11 | T1 khung repo; gửi nhóm Q1-Q9 | Cài .NET 8; đọc C1 + Q5 | Cài .NET 8; đọc G7 + Q6, Q7 |
| 03/11 | S2 protocol; M1 proposal | C1 simulation + test | G7 network simulator |
| 04/11 | S1 + R1 Docker, S3 login → **M2** | C2 Unity login | G1 scene offline |
| 05/11 | S4 ghép trận, S5 + S6 | C2 vào trận, C3 INPUT | G2 khách hàng |
| 06/11 | R3 xử lý lỗi, S7 + R5, server G3 | C4 prediction, C5 reconciliation | G3 nhặt/thả/giao; viết kịch bản test |
| 07/11 | R2 bot, S8 | C6 interpolation, R2 bot | G8 Network Panel → **cảnh 1-5** |
| 08/11 | Server G5; test 1-5, 10; `architecture.md`; slide | T4 đo đạc, test 6, 8; slide | G4 + G5 → cảnh 6; R4 tổng hợp; README §3-§6; demo script; slide |
| 09/11 | Clone sạch, tổng duyệt, **nộp** | Tổng duyệt, AI log, Contribution | Tổng duyệt, AI log, Contribution |

---

## 5. Thứ tự phụ thuộc (cần biết để không ngồi chờ)

```text
T1 khung repo ──┬─> S2 protocol ──> S1 TCP+Docker ──> S3 login ──> S4 ghép trận ──┐
                ├─> C1 simulation ─────────────────────────────────────────────────┼─> S5/S6 tick+snapshot
                └─> G7 network simulator (độc lập)                                  │
C1 ──> G1 scene ──> G2 đơn hàng ──> G3 nhặt/giao (cần S5) ──> G4/G5 đồ hiếm (cần S6)
S2 + S3 ──> C2 Unity login ──> C3 INPUT (cần S5) ──> C4 prediction ──> C5 reconcile (cần S6)
S6 ──> C6 interpolation;  G7 + C5 ──> G8 Network Panel;  S4 + S5 + C1 ──> R2 bot ──> R4 test
```

**Việc làm được ngay mà không cần chờ ai:** S2 (#1), C1 (#2), G7 (#3).

---

## 6. Nếu trễ thì cắt gì

1. **Đã cắt sẵn** (chỉ làm nếu dư): G6 rollback, C7 smoothing, C8 HUD/camera cầu kỳ, video dự phòng, reconnect.
2. Nếu tối 07/11 chưa có cảnh 1-5: **bỏ cảnh 6** (G4/G5), trình bày lag compensation bằng slide; dồn 08/11 vào test chịu lỗi
   và tài liệu (nhiều điểm hơn).
3. Nếu tối 05/11 chưa vào được trận: #3 tạm dừng G2, sang hỗ trợ tích hợp C2/C3 với #2.
4. Không bao giờ cắt: Docker chạy được, nhiều client đồng thời, xử lý ngắt kết nối/dữ liệu rác, tài liệu thiết kế,
   AI log, Contribution.

---

## 7. Quy ước

- Mỗi việc một nhánh từ `main` mới nhất, gộp qua PR, có người khác review **trong ngày**. Tên nhánh như bảng §3.
- Commit bằng tài khoản GitHub của chính mình; làm cặp thì thêm `Co-authored-by:`.
- `shared/` (Protocol + Simulation) là hợp đồng chung: sửa phải báo cả nhóm, cập nhật `protocol-design.md` trong cùng PR.
- Dùng AI cho code/tài liệu → tự ghi một dòng vào `docs/ai-log.md` của repo code.
- Mỗi tối 22:00 nhắn nhóm 3 dòng: xong gì / vướng gì / mai làm gì. Vướng quá 2 tiếng thì báo ngay, không chờ tối.
