# architecture.md - Tài liệu kiến trúc

| Người làm | Nhánh | Deadline | Phụ thuộc |
| --- | --- | :-: | --- |
| #1 Nguyễn Thế Chiến (#2, #3 viết đoạn phần mình) | `docs/architecture` | Bản đầu 04/11 (M2), bản cuối 08/11 | Q1, S1-S6 |

## Mục tiêu

`docs/architecture.md` giải thích **vì sao** chọn kiến trúc này (tiêu chí A1-A3 trong INSTRUCTION.md).

## Các bước

- [ ] Sơ đồ thành phần (mermaid): Unity client, bot, `TcpListener`/`ClientSession`, `UdpGateway`, `Matchmaker`,
      `Room` + tick loop, SQLite, `shared/`.
- [ ] Mô hình đồng thời (Q1): luồng nào làm gì; bảng **shared state** (dữ liệu, ai đọc/ghi, cơ chế bảo vệ).
- [ ] Vòng đời kết nối: kết nối → login → hàng đợi → phòng → kết thúc / rớt mạng.
- [ ] Vòng đời một input: phím bấm → INPUT → inbox → tick → SNAPSHOT → reconcile (đoạn này #2 viết).
- [ ] Lag compensation + bộ mô phỏng mạng (đoạn này #3 viết).
- [ ] Design Decisions: Q1, Q5, Q6, Q8 kèm lý do và đánh đổi.
- [ ] Bản cuối 08/11: cập nhật theo code thật, xoá phần không làm.

## Xong khi

- Người ngoài nhóm đọc xong vẽ lại được luồng dữ liệu và chỉ ra chỗ nào có khoá.
