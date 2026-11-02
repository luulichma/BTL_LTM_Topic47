# G1 - Scene siêu thị

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #3 Phạm Quang Anh | `feature/g1-supermarket-scene` | 04/11 | C1 (`MapData`), project Unity từ C2 | G2, G3, G4 |

## Mục tiêu

Scene gameplay chạy **offline** (chưa cần mạng): bản đồ siêu thị, kệ hàng, 6 món đồ theo seed, quầy của mỗi người,
nhân vật đi bằng WASD qua `Simulation.Step` dùng chung.

## Các bước

- [ ] Dựng bản đồ 2D top-down từ `MapData` (C1): tường, kệ, 2 quầy (A trái, B phải), kệ chung ở giữa (cho G4).
- [ ] 6 loại món đồ; vị trí theo `seed` (dùng `System.Random(seed)` - cùng seed thì client và server ra cùng bố cục).
- [ ] Nhân vật: sprite đơn giản; offline thì gọi thẳng `Simulation.Step` mỗi tick để kiểm tra va chạm.
- [ ] Tách rõ **dữ liệu** (MapData, seed) và **hiển thị** (GameObject) để sau này chỉ cần nhận state từ server.
- [ ] Dùng asset free hoặc hình khối màu; không tốn thời gian vẽ.

## Xong khi

- Chạy scene offline: đi quanh, không xuyên tường/kệ, 6 món hiện đúng vị trí theo seed.

## Lưu ý

- Không dùng Rigidbody/Physics của Unity cho nhân vật (lệch với server → reconcile rung).
- Nếu C1 chưa xong ngày 03/11: dựng scene với `MapData` giả, thay sau.
