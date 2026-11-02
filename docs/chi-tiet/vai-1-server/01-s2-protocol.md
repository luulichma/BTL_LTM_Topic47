# S2 - Giao thức và thư viện message

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #1 Nguyễn Thế Chiến | `feature/s2-protocol` | 03/11 | T1, Q2-Q4 | S1, S3, C2, C3, R2 |

## Mục tiêu

Một thư viện `T47.Shared/Protocol` encode/decode được mọi message, và `docs/protocol-design.md` mô tả đủ để người khác
viết client mà không cần đọc code server.

## Các bước

- [ ] Enum `MessageType : byte` cho TCP (LOGIN, LOGIN_RESULT, REGISTER, FIND_MATCH, CANCEL_FIND, MATCH_FOUND, MATCH_END,
      PING/PONG, DEMO_CONFIG, ERROR) và UDP (INPUT, SNAPSHOT, ACTION_RESULT, UDP_HELLO).
- [ ] **Framing TCP** (đề xuất): `[u32 length][u8 type][payload]`, little-endian, `length` = số byte của type + payload,
      tối đa 4096. Viết `FrameReader` đọc đủ 4 byte rồi đủ `length` byte (TCP là luồng byte, một lần `Read` có thể
      ra nửa frame hoặc 2 frame).
- [ ] **Header UDP** (đề xuất): `[u16 magic 0x4737][u8 version][u8 type][u32 token][payload]`. Gói sai magic,
      version, hoặc ngắn hơn header → bỏ.
- [ ] Payload từng message (chuỗi = `[u16 length][UTF-8]`):
  - INPUT: `u32 seq, u32 clientTick, u32 viewTick, u8 keys (bitmask W A S D J K)`, kèm tối đa 3 input cũ chưa được xác nhận.
  - SNAPSHOT: `u32 serverTick`, với mỗi người: `u32 lastProcessedSeq, f32 x, f32 y, f32 vx, f32 vy, i16 heldItem`;
    trạng thái kệ hàng; điểm.
  - ACTION_RESULT: `u32 seq, u8 accepted, u8 reasonCode, i16 itemId`.
- [ ] Bảng **status / error code** (vd. 0 OK, 1 WRONG_PASSWORD, 2 USERNAME_TAKEN, 3 NOT_LOGGED_IN, 4 BAD_FRAME...).
- [ ] Unit test: encode → decode ra đúng giá trị cho từng message; decode gói cụt trả lỗi chứ không ném exception lung tung.
- [ ] Viết `docs/protocol-design.md`: bảng message, layout từng byte, sequence diagram login → ghép trận → trong trận →
      kết thúc, status code, lý do chọn (Q2-Q4).

## Xong khi

- `dotnet test` xanh, có test cho mọi message.
- #2 đọc `protocol-design.md` và tự viết được phần gửi LOGIN mà không phải hỏi.

## Lưu ý

- Dùng `BinaryPrimitives` / `BinaryWriter` với thứ tự byte cố định; không dùng `BinaryFormatter`.
- `float` của Unity và .NET cùng IEEE 754, nhưng cẩn thận khi so sánh bằng `==` trong test.
- Sửa layout sau khi đã gộp → phải báo cả nhóm và sửa doc trong cùng PR.

## Chuẩn bị vấn đáp

- Vì sao length-prefix mà không phải `\n`? Nếu một lần `Read` chỉ nhận 3 byte thì code xử lý thế nào?
- Vì sao UDP cần token? Điều gì xảy ra nếu chỉ tin `IPEndPoint`?
- Live change hay gặp: thêm 1 message mới (vd. CHAT) - cần sửa những chỗ nào?
