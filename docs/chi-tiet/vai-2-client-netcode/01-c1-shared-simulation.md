# C1 - Shared simulation

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #2 Nguyễn Đức Anh | `feature/c1-shared-simulation` | 03/11 | T1 | S5, C4, C5, G1, R2 |

## Mục tiêu

Một hàm mô phỏng **tất định** dùng chung cho server, bot và Unity: cùng state + cùng input → cùng kết quả đến từng bit.
Đây là nền móng của prediction và reconciliation.

## Các bước

- [ ] `shared/T47.Shared/Simulation/`:
  - `PlayerState { float X, Y, VX, VY; short HeldItem; }`
  - `InputCmd { uint Seq; uint ClientTick; uint ViewTick; byte Keys; }` (bitmask W A S D J K - thống nhất với S2)
  - `MapData`: kích thước bản đồ, danh sách hình chữ nhật tường/kệ (dữ liệu cố định, #3 dùng để dựng scene G1).
  - `Simulation.Step(ref PlayerState s, InputCmd input, float dt, MapData map)`: tính vận tốc từ WASD (tốc độ hằng số,
    chéo thì chuẩn hoá), cộng vị trí, va chạm AABB với tường/kệ (trượt theo trục), giới hạn biên bản đồ.
  - Va chạm người với người (đẩy nhau ra) - làm bản đơn giản.
- [ ] Hằng số chung: `TickRate = 30`, `Dt = 1f/30`, `MoveSpeed`, `PlayerRadius`, `PickupRange`.
- [ ] Unit test (`T47.Shared.Tests`):
  - Đứng yên khi không bấm phím.
  - Đi 30 tick phím D → X tăng đúng `MoveSpeed * 1s`.
  - Đâm vào tường → dừng ở mép tường, không xuyên.
  - **Tất định:** chạy cùng chuỗi 1000 input 2 lần → state giống hệt.
- [ ] Không dùng `UnityEngine.*`, `Random` không seed, `DateTime.Now` trong `Simulation`.

## Xong khi

- `dotnet test` xanh; #1 gọi được `Simulation.Step` trong tick loop; #3 dựng được scene từ `MapData`.

## Lưu ý

- Luôn dùng `dt` cố định, không dùng `Time.deltaTime` của Unity trong simulation.
- `float` giữa .NET và Unity (Mono/IL2CPP) thường ra giống nhau với phép tính đơn giản; tránh `Math.Sin/Cos` phức tạp.
  Nếu lệch, reconcile sẽ sửa - nhưng phải biết để giải thích.

## Chuẩn bị vấn đáp

- "Tất định" nghĩa là gì và vì sao prediction cần nó?
- Nếu client và server dùng 2 bản code di chuyển khác nhau thì người chơi thấy gì?
