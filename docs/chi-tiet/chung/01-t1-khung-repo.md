# T1 - Khung repo và thư viện dùng chung

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến (cả nhóm duyệt) | `chore/t1-repo-skeleton` | 02/11 (tối) | Q5 đã chốt | **Mọi nhánh khác** |

## Mục tiêu

Có cấu trúc thư mục và solution .NET build được, để 3 người tạo nhánh và làm song song ngay từ 03/11.

## Các bước

- [ ] Cài .NET 8 SDK; `dotnet --version` ra 8.x trên cả 3 máy.
- [ ] Tạo cấu trúc:

  ```text
  shared/T47.Shared/           # classlib netstandard2.1: Protocol/, Simulation/
  shared/T47.Shared.Tests/     # xunit, net8.0
  server/src/T47.Server/       # console net8.0, tham chiếu T47.Shared
  client/bot/T47.Bot/          # console net8.0, tham chiếu T47.Shared
  client/unity/                # project Unity (tạo ở C2)
  tests/                       # script test tải / dữ liệu rác
  T47.sln
  ```

- [ ] `dotnet new sln`, `dotnet new classlib -f netstandard2.1`, `dotnet new console -f net8.0`, `dotnet new xunit`,
      `dotnet sln add ...`, `dotnet add reference ...`.
- [ ] Server `Program.cs` tối thiểu: đọc `SERVER_HOST`, `SERVER_PORT`, `UDP_PORT` từ env, in ra rồi thoát.
- [ ] Kiểm tra `.gitignore` có `bin/`, `obj/`, các mục Unity `client/unity/[Ll]ibrary/`... (đã có sẵn trong starter).
- [ ] `dotnet build T47.sln` và `dotnet test` chạy xanh.
- [ ] Chốt **phiên bản Unity** cả nhóm dùng, ghi vào README repo (§5 Quick Start).
- [ ] PR → 1 người review → gộp `main`. Báo nhóm để tạo nhánh của mình.

## Xong khi

- Clone sạch, `dotnet build T47.sln` thành công trên ít nhất 2 máy.
- 3 người đều tạo được nhánh việc đầu tiên từ `main`.

## Lưu ý

- `netstandard2.1` để Unity dùng được; Unity không nạp được thư viện `net8.0`.
- Docker build context sau này phải là **thư mục gốc** (để thấy `shared/`): trong compose dùng `context: .` và
  `dockerfile: server/Dockerfile`.
- Không commit `Library/` của Unity (hàng trăm MB).

## Chuẩn bị vấn đáp

- Vì sao tách `shared/` thay vì copy code? (một nguồn sự thật cho simulation → reconcile không rung)
- Vì sao `netstandard2.1`?
