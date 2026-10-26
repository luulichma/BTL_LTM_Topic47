# Demo sơ bộ T47: dự đoán phía client, đối chiếu, nội suy

Bản chứng minh ý tưởng (proof-of-concept) theo mục 7 của [`docs/NoiDung_ChiTiet.md`](../docs/NoiDung_ChiTiet.md). Gồm 1 server và 2 client chạy trong cùng một tiến trình, nối với nhau qua mạng giả lập có độ trễ, dao động độ trễ và mất gói. Màn hình chia 3 khung: **Client A · Server · Client B**.

## Chạy demo

| Cách | Lệnh |
|---|---|
| Mở trong Unity | Unity Hub → *Add project from disk* → chọn `demo/T47Demo` (Unity **6000.5.9f1**) → mở `Assets/Scenes/Demo.unity` → Play |
| Build bản Windows | Trong Unity: menu **T47 → Build Windows** → chạy `demo/T47Demo/Build/T47Demo.exe` |
| Build từ dòng lệnh | `"F:\Unity Editor\6000.5.9f1\Editor\Unity.exe" -batchmode -nographics -projectPath demo/T47Demo -executeMethod DemoSetup.BuildWindows` |

Khi thuyết trình nên chạy **bản build** cho nhẹ máy và tránh lỗi Editor. Tham số dòng lệnh của bản build:

- `-scene N`: mở thẳng cảnh N (0–5).
- `-autoplay`: A tự chạy vòng tròn khi không bấm phím, tiện khi tập một mình.
- `-screenshot duong_dan.png`: chụp màn hình sau 4 giây rồi thoát, dùng để lấy hình cho slide.

Ví dụ: `T47Demo.exe -screen-width 1600 -screen-height 900 -screen-fullscreen 0 -scene 4 -autoplay`

## Điều khiển

| Phím | Tác dụng |
|---|---|
| WASD | Di chuyển người A |
| Phím mũi tên | Di chuyển người B |
| 0 – 5 | Chuyển sang cảnh demo tương ứng (đặt sẵn mọi thông số) |
| T | Client A gửi **gói vị trí giả mạo** (dịch chuyển tới chỗ ngẫu nhiên) |
| F1 | Đổi chế độ: client tự quyết ↔ server quyết định (đặt lại vị trí) |
| F2 / F3 / F4 | Bật/tắt dự đoán phía client / đối chiếu với server / nội suy thực thể |
| F5 | Đặt lại vị trí |

Thanh trượt bên trái chỉnh **RTT** (0–500 ms), **dao động độ trễ** (±0–100 ms), **mất gói** (0–30 %) và **tần số gửi ảnh trạng thái** (5–30 lần/giây). Độ trễ nội suy tự tính bằng `2 × chu kỳ gửi ảnh + dao động độ trễ`.

Cách đọc màn hình:

- **Ô đặc**: hình mà người chơi đó nhìn thấy.
- **Viền trắng**: "bóng", tức vị trí của chính mình theo ảnh trạng thái mới nhất của server.
- **Chuỗi chấm**: vết đối thủ trong khoảng 0,75 giây gần nhất. Chấm thưa, cách đều từng cụm nghĩa là đối thủ đang nhảy cóc; chấm liền nghĩa là chuyển động mượt.
- Dưới mỗi khung client: RTT đo được, nhịp server mới nhất, số lệnh chờ xác nhận, khoảng bị kéo lệch ở lần đối chiếu gần nhất, độ trễ nội suy.

## Kịch bản demo (~5–6 phút)

| Phím | Cấu hình | Thao tác | Khán giả thấy | Vấn đề con |
|---|---|---|---|---|
| 0 | Client tự quyết, RTT 100 ms | Bấm **T** | Ô A nhảy tới chỗ bất kỳ, khung server và khung B đều chấp nhận | VĐ1 |
| 1 | Server quyết định, RTT 200 ms, tắt mọi kỹ thuật | A chạy qua lại; bấm lại **T** | T hết tác dụng (nhật ký báo server bỏ gói, ô A bị kéo về). Nhưng bấm phím khoảng 200 ms sau mới thấy chạy | VĐ1 → VĐ2 |
| 2 | + dự đoán | Chạy rồi dừng đột ngột | Phản hồi tức thì, nhưng nhân vật rung giật theo từng ảnh trạng thái; "Kéo lệch" khác 0 | VĐ2 |
| 3 | + đối chiếu | Lặp lại thao tác | Mượt; "Kéo lệch" = 0,00; khi dừng, ô khớp với bóng trắng. Lệnh chờ khoảng 8–9 | VĐ2 |
| 4 | Gửi ảnh 10 lần/giây, dao động ±50 ms, chưa nội suy | Nhìn khung B; A chạy vòng tròn | Ô A trong mắt B nhảy từng nấc, vết chấm thưa | VĐ3 |
| 5 | + nội suy (độ trễ tự tính 250 ms) | Lặp lại; rồi kéo tần số ảnh lên 20 | Vết chấm liền, mượt nhưng ô A ở khung B trễ hơn khung server. Lên 20 lần/giây thì độ trễ còn 150 ms | VĐ3 |

Gợi ý khi trình bày:

- Cảnh 2: nên kéo RTT lên 300–400 ms để giật ngược rõ hơn trên máy chiếu.
- Cảnh 3: chỉ vào con số "Lệnh chờ xác nhận". Con số này xấp xỉ (RTT + thời gian chờ nhịp + thời gian chờ ảnh trạng thái) / 33 ms, tức chính là số lệnh mà client đang "đi trước" server.
- Thêm mất gói 10–20 % ở cảnh 3: nhân vật vẫn mượt nhờ gửi dư (mỗi gói lệnh mang thêm 3 lệnh trước đó).
- Luôn có sẵn video quay dự phòng.

## Cấu trúc code

```
demo/
├── T47Demo/                        Project Unity
│   └── Assets/
│       ├── Scripts/Netcode/        Lõi kỹ thuật mạng, C# thuần (asmdef cấm tham chiếu UnityEngine)
│       │   ├── Simulation.cs       Simulation.Step dùng chung cho client và server (30 nhịp/giây)
│       │   ├── Protocol.cs         Gói INPUT / STATE / SNAPSHOT, mã hoá nhị phân
│       │   ├── Transport.cs        ITransport + mạng giả lập (độ trễ, dao động, mất gói, sai thứ tự)
│       │   ├── GameServer.cs       Vòng lặp nhịp, hàng đợi lệnh, chống hack tốc độ, gửi ảnh trạng thái
│       │   └── GameClient.cs       Dự đoán, đối chiếu, nội suy, ngoại suy ngắn, đo RTT
│       ├── Scripts/Demo/DemoApp.cs Giao diện 3 khung + bảng điều khiển (IMGUI)
│       ├── Editor/DemoSetup.cs     Menu T47: tạo scene, build Windows
│       └── Scenes/Demo.unity
└── NetcodeTests/                   Chạy lõi netcode ngoài Unity để kiểm tra: dotnet run
```

Lõi `Scripts/Netcode` không phụ thuộc Unity, nên server .NET ở bản cuối dùng lại nguyên các file này. Khi đó chỉ cần viết thêm `UdpTransport : ITransport`.

## Kiểm tra tự động

```
cd demo/NetcodeTests
dotnet run
```

Bộ kiểm tra dùng đồng hồ giả, chạy 60 khung hình/giây, kiểm tra từng cảnh demo:

| Kiểm tra | Kết quả đo được |
|---|---|
| Client tự quyết chấp nhận gói dịch chuyển | Server và B đều thấy A ở vị trí giả mạo |
| Server quyết định bỏ gói vị trí giả mạo | Server giữ nguyên A; client A bị kéo về |
| Không dự đoán (RTT 200 ms) | Bấm phím 233 ms sau mới thấy chạy |
| Chỉ dự đoán | Mỗi ảnh trạng thái kéo lùi nhân vật 0,40 đơn vị |
| Dự đoán + đối chiếu | Sai lệch 0,000; lệnh chờ 9; khi dừng, client trùng server |
| Đối chiếu khi dao động ±50 ms + mất 10 % gói | Sau khi dừng, client trùng server |
| Client gửi 60 lệnh/giây (hack tốc độ) | Server chỉ cho đi khoảng 6 đơn vị/giây |
| Nội suy (10 ảnh/giây, ±50 ms) | Bước nhảy lớn nhất mỗi khung: 1,00 → 0,17; độ trễ hình ảnh đối thủ 350 ms, khớp lý thuyết 100 + 250 ms |
| Ảnh trạng thái đến sai thứ tự | Bị bỏ; nhịp server ở client luôn tăng |

## Chỗ khác so với tài liệu nội dung

- **Server không dùng lại lệnh cũ khi hàng đợi trống.** Thay vào đó, mỗi nhịp server được cộng 1 lượt xử lý, để dành tối đa 3 lượt. Lệnh đến dồn thì dùng lượt để dành để bắt kịp. Cách này vẫn chống hack tốc độ, vì trung bình không quá 30 lệnh/giây, và tránh được việc server đi thừa bước khiến client bị kéo lệch dù mạng chỉ có độ trễ cố định. Mục "Hàng đợi lệnh ở server" (VĐ1) của `NoiDung_ChiTiet.md` nên sửa theo.
- **Số lệnh chờ đo được là khoảng 8–9, không phải 6** (RTT 200 ms, 20 ảnh/giây). Lý do: ngoài RTT còn có thời gian chờ tới nhịp kế tiếp và tới lần gửi ảnh kế tiếp, đúng với công thức độ trễ điều khiển ở VĐ2.
- **Chưa làm bước làm mượt sai lệch nhỏ** (giảm dần vectơ sai lệch). Demo cố ý để sai lệch hiện rõ.
- Chưa có va chạm giữa hai nhân vật.
