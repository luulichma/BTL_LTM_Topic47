# Slides

`T47_ThuyetTrinh_Khung.pptx` là khung slide thuyết trình, được sinh từ [`../docs/NoiDung_ThuyetTrinh.md`](../docs/NoiDung_ThuyetTrinh.md)
bằng script `build.js` (dùng thư viện [PptxGenJS](https://gitbrent.github.io/PptxGenJS/)).

## Chạy lại script

Cần Node.js 18 trở lên. Mở terminal trong thư mục `slides/`:

```bash
npm install
```

```bash
node build.js
```

Lệnh trên ghi đè `T47_ThuyetTrinh_Khung.pptx` trong thư mục này. Muốn ghi ra file khác thì truyền tên file:
`node build.js ban_thu.pptx`.

> **Lưu ý:** mọi chỉnh sửa làm tay trong PowerPoint sẽ **mất** khi chạy lại script với cùng tên file.
> Chỉ dùng script khi khung còn thay đổi nhiều; khi đã vẽ hình và chỉnh tay thì sửa thẳng trong PowerPoint.

## Sửa gì ở đâu trong `build.js`

| Muốn đổi | Chỗ sửa |
|---|---|
| Màu, font | `THEME` ở đầu file |
| Footer, vị trí tiêu đề | các `defineSlideMaster` (layout `CONTENT`, `DARK_TITLE`, `SECTION_DARK`) |
| Chữ trên slide, lời nói (speaker notes) | từng khối `// ---- N. ... ----` |
| Slide dự phòng D1–D10 | mảng `BACKUP` |

Trong chuỗi chữ: `**đậm**`, `` `code` ``, `!!cần làm!!` (chữ đỏ nghiêng, đánh dấu chỗ còn phải điền).
