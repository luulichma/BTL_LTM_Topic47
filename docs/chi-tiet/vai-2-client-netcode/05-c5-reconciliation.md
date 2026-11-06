# C5 - Server reconciliation

| Người làm | Nhánh | Deadline | Phụ thuộc | Mở khoá cho |
| --- | --- | :-: | --- | --- |
| #2 Nguyễn Đức Anh | `feature/c5-reconciliation` | 06/11 | C4, S6 | C7, G8, cảnh demo 4, test 8 |

## Mục tiêu

Khi snapshot của server tới, client sửa dự đoán về đúng trạng thái server mà vẫn giữ được các input chưa xác nhận.

## Thuật toán

```text
khi nhận SNAPSHOT(serverTick, lastProcessedSeq = k, myState = S):
    xoá khỏi bộ đệm mọi input có seq <= k
    localState = S                      # đặt lại theo server
    với mỗi input còn trong bộ đệm (seq > k), theo thứ tự:
        Simulation.Step(ref localState, input, Dt, map)   # chạy lại
    (tuỳ chọn) đo sai số = |localState trước - localState sau| để log/T4
```

## Các bước

- [x] Cài thuật toán trên; bỏ qua snapshot cũ hơn snapshot đã xử lý (UDP có thể tới sai thứ tự).
- [x] Công tắc `Reconciliation` trên Network Panel: tắt → chỉ predict, không bao giờ sửa (cảnh 3 lệch dần).
  Hiện đặt trên overlay `GameController`; chuyển sang Network Panel khi làm G8.
- [x] Log/overlay: sai số mỗi lần reconcile (dùng cho T4).

## Tiến độ

- **06/11:** xong, nhánh `feature/c5-reconciliation` (`26553e1`) đã merge vào `main` (`87b2bf4`).
  Kèm sửa va chạm cạnh/góc vật cản trong `Simulation.Step` (`ContactTolerance` 0.001 m, `3b553db`) và DLL shared
  build lại sau merge. `dotnet test T47.sln` sau merge: 140 shared + 29 server pass.
- Đã thử trên Unity (#2): đi thẳng không va chạm, sai số reconcile ≈ 0.
- **Chưa kiểm chứng:** cảnh 4 (va chạm rồi tự về đúng chỗ) sau khi sửa va chạm; chạy Unity trên `main` sau merge
  (có G3 đồng bộ đơn trong SNAPSHOT).

## Xong khi

- Cảnh 4: lặp lại va chạm ở cảnh 3 → nhân vật tự về đúng chỗ, bóng và nhân vật trùng nhau.
- Đi thẳng không va chạm: sai số ≈ 0 (chứng tỏ simulation tất định).

## Chuẩn bị vấn đáp

- Vì sao phải chạy lại input chưa xác nhận thay vì chỉ nhảy về vị trí server?
- Nếu sai số luôn khác 0 khi đi thẳng thì nghi lỗi ở đâu?
