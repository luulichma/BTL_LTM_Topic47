# M1 - Proposal

| Người làm | Nhánh | Deadline | Phụ thuộc |
| --- | --- | :-: | --- |
| #1 viết, #2 #3 đọc duyệt | `docs/m1-proposal` | 03/11 | Q1-Q9 |

## Mục tiêu

Hoàn thành `docs/proposal.md` theo yêu cầu M1 của `INSTRUCTION.md`: problem, idea, scope, key technical challenge,
initial protocol sketch, ownership plan.

## Các bước

- [ ] Problem / Idea / Non-trivial / Out of scope: lấy từ README §2 (đã điền) và mở rộng.
- [ ] Key technical challenge: đồng bộ real-time qua UDP, prediction + reconciliation, lag compensation, nhiều phòng song song.
- [ ] Initial protocol sketch: bảng message LOGIN, FIND_MATCH, MATCH_FOUND, MATCH_END, INPUT, SNAPSHOT, ACTION_RESULT.
- [ ] Ownership plan: bảng 3 vai, mã việc chính của từng người (theo `lo_trinh.md` §3).
- [ ] Kế hoạch mốc: M1 03/11, M2 04/11, đóng băng 08/11, M3 nộp 09/11.
- [ ] PR → gộp `main` → `git tag m1` và `git push origin m1`.

## Xong khi

- Không còn placeholder trong `proposal.md`; tag `m1` có trên GitHub.
