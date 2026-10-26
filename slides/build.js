// Build the T47 presentation skeleton from docs/NoiDung_ThuyetTrinh.md
// Usage: npm install && node build.js [output.pptx]   (see README.md in this folder)
const fs = require("fs");
const path = require("path");
const pptxgen = require("pptxgenjs");
const JSZip = require("jszip");
const OUT = process.argv[2] || path.join(__dirname, "T47_ThuyetTrinh_Khung.pptx");

// pptxgenjs writes Office's stock palette into the theme, so scheme colors (C.accent1...) would render
// as Office blue/orange. After writing the deck, replace the color scheme in ppt/theme/theme1.xml.
async function applyTheme(file, theme) {
  const zip = await JSZip.loadAsync(fs.readFileSync(file));
  const part = "ppt/theme/theme1.xml";
  const slots = ["dk1", "lt1", "dk2", "lt2", "accent1", "accent2", "accent3", "accent4", "accent5", "accent6", "hlink", "folHlink"];
  const scheme = `<a:clrScheme name="${theme.name}">` + slots.map((k) => `<a:${k}><a:srgbClr val="${theme.colors[k]}"/></a:${k}>`).join("") + "</a:clrScheme>";
  const xml = (await zip.file(part).async("string")).replace(/<a:clrScheme\b[\s\S]*?<\/a:clrScheme>/, () => scheme);
  zip.file(part, xml);
  fs.writeFileSync(file, await zip.generateAsync({ type: "nodebuffer", compression: "DEFLATE" }));
}

const THEME = {
  name: "T47 Netcode",
  headFontFace: "Cambria",
  bodyFontFace: "Calibri",
  colors: {
    dk1: "14213D", lt1: "FFFFFF", dk2: "0B1626", lt2: "EEF3F8",
    accent1: "00897B", accent2: "6C5CE7", accent3: "F4A259",
    accent4: "D93A3F", accent5: "5B6B82", accent6: "BFE8E3",
    hlink: "00897B", folHlink: "6C5CE7",
  },
};

const pres = new pptxgen();
pres.layout = "LAYOUT_16x9"; // 10 x 5.625 in
pres.theme = { headFontFace: THEME.headFontFace, bodyFontFace: THEME.bodyFontFace };
pres.title = "T47 — Dự đoán phía client";
pres.author = "Nhóm T47";
const C = pres.SchemeColor;

// ---------- Layouts ----------
const FOOT = "T47 · Kỹ thuật mạng cho game thời gian thực · Lập trình mạng";
pres.defineSlideMaster({
  title: "CONTENT",
  background: { color: C.background1 },
  objects: [
    { text: { text: FOOT, options: { x: 0.5, y: 5.2, w: 7, h: 0.3, fontSize: 10, color: C.accent5, margin: 0 } } },
    { placeholder: { options: { name: "eyebrow", type: "body", x: 0.5, y: 0.32, w: 7.6, h: 0.28, fontSize: 11, bold: true, color: C.accent1, charSpacing: 1.5, margin: 0 }, text: "" } },
    { placeholder: { options: { name: "title", type: "title", x: 0.5, y: 0.6, w: 7.7, h: 0.6, fontSize: 26, bold: true, color: C.text1, align: "left", valign: "middle", margin: 0 }, text: "" } },
  ],
  slideNumber: { x: 9.0, y: 5.2, w: 0.5, h: 0.3, fontSize: 10, color: C.accent5, align: "right" },
});
pres.defineSlideMaster({
  title: "DARK_TITLE",
  background: { color: C.text2 },
  objects: [
    { placeholder: { options: { name: "eyebrow", type: "body", x: 0.5, y: 0.32, w: 7.6, h: 0.28, fontSize: 11, bold: true, color: C.accent6, charSpacing: 1.5, margin: 0 }, text: "" } },
    { placeholder: { options: { name: "title", type: "title", x: 0.5, y: 0.6, w: 7.7, h: 0.6, fontSize: 26, bold: true, color: C.background1, align: "left", valign: "middle", margin: 0 }, text: "" } },
  ],
  slideNumber: { x: 9.0, y: 5.2, w: 0.5, h: 0.3, fontSize: 10, color: C.accent6, align: "right" },
});
pres.defineSlideMaster({
  title: "SECTION_DARK",
  background: { color: C.text2 },
  objects: [
    { placeholder: { options: { name: "eyebrow", type: "body", x: 0.6, y: 1.25, w: 5.8, h: 0.3, fontSize: 12, bold: true, color: C.accent6, charSpacing: 2, margin: 0 }, text: "" } },
    { placeholder: { options: { name: "title", type: "title", x: 0.6, y: 1.6, w: 5.8, h: 1.45, fontSize: 28, bold: true, color: C.background1, align: "left", valign: "top", margin: 0 }, text: "" } },
    { placeholder: { options: { name: "subtitle", type: "body", x: 0.6, y: 3.1, w: 5.8, h: 0.5, fontSize: 20, bold: true, color: C.accent6, margin: 0 }, text: "" } },
    { placeholder: { options: { name: "meta", type: "body", x: 0.6, y: 3.7, w: 5.8, h: 1.2, fontSize: 13, color: C.background1, valign: "top", margin: 0 }, text: "" } },
  ],
});

// ---------- Helpers ----------
const WHO = {
  "#1": { fill: C.accent1, text: C.background1 },
  "#2": { fill: C.accent2, text: C.background1 },
  "#3": { fill: C.accent3, text: C.text1 },
  "Q&A": { fill: C.accent5, text: C.background1 },
};

// Inline markup: **bold**, !!to-do!!, `code`
function runs(str, base = {}) {
  const out = [];
  const re = /(\*\*[^*]+\*\*|!![^!]+!!|`[^`]+`)/g;
  let last = 0, m;
  while ((m = re.exec(str))) {
    if (m.index > last) out.push({ text: str.slice(last, m.index), options: { ...base } });
    const tok = m[0];
    if (tok.startsWith("**")) out.push({ text: tok.slice(2, -2), options: { ...base, bold: true } });
    else if (tok.startsWith("!!")) out.push({ text: tok.slice(2, -2), options: { ...base, bold: true, italic: true, color: C.accent4 } });
    else out.push({ text: tok.slice(1, -1), options: { ...base, fontFace: "Courier New" } });
    last = m.index + tok.length;
  }
  if (last < str.length) out.push({ text: str.slice(last), options: { ...base } });
  return out;
}

// items: string | { t, plain, level }
function para(items, o = {}) {
  const arr = [];
  items.forEach((it, i) => {
    const t = typeof it === "string" ? it : it.t;
    const plain = o.plain || (typeof it === "object" && it.plain);
    const rs = runs(t);
    rs.forEach((r, j) => {
      // pptxgenjs writes a pPr per run, so every run carries the paragraph options. An explicit, unchanged
      // align keeps it from starting a new paragraph at each bulleted run; breakLine ends the paragraph.
      r.options.align = "left";
      r.options.bullet = plain ? false : { indent: 14 };
      if (typeof it === "object" && it.level) r.options.indentLevel = it.level;
      r.options.paraSpaceAfter = o.space ?? 6;
      if (j === rs.length - 1 && i < items.length - 1) r.options.breakLine = true;
      arr.push(r);
    });
  });
  return arr;
}

function text(slide, items, box, o = {}) {
  slide.addText(para(items, o), {
    x: box.x, y: box.y, w: box.w, h: box.h, fontSize: o.fontSize || 14, color: o.color || C.text1,
    valign: o.valign || "top", margin: 0, isTextBox: true, objectName: o.name || "Body text",
  });
}

function chip(slide, who, time, label) {
  const c = WHO[who];
  slide.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 8.35, y: 0.38, w: 1.15, h: 0.34, rectRadius: 0.17, fill: { color: c.fill }, line: { color: c.fill }, objectName: "Presenter chip" });
  slide.addText(label || (time ? `${who} · ${time}` : who), { x: 8.35, y: 0.38, w: 1.15, h: 0.34, fontSize: 11, bold: true, color: c.text, align: "center", valign: "middle", margin: 0, isTextBox: true, objectName: "Presenter chip text" });
}

// Dashed box: a figure (or content) the team still has to draw / fill in
function figure(slide, box, desc, label = "HÌNH CẦN VẼ") {
  slide.addShape(pres.shapes.ROUNDED_RECTANGLE, { ...box, rectRadius: 0.08, fill: { color: C.background2 }, line: { color: C.accent1, width: 1.25, dashType: "dash" }, objectName: "Placeholder frame" });
  slide.addText(label, { x: box.x + 0.2, y: box.y + 0.15, w: box.w - 0.4, h: 0.28, fontSize: 10, bold: true, color: C.accent1, charSpacing: 1.5, margin: 0, isTextBox: true, objectName: "Placeholder label" });
  const items = (Array.isArray(desc) ? desc : [desc]).map((d) => (d.startsWith("- ") ? { t: d.slice(2) } : { t: d, plain: true }));
  text(slide, items, { x: box.x + 0.2, y: box.y + 0.5, w: box.w - 0.4, h: box.h - 0.65 }, { fontSize: 12, color: C.accent5, space: 4, name: "Placeholder description" });
}

const GLYPH = { "●": C.accent1, "◐": C.accent3, "○": C.accent5 };
// Option list: [{ t, r, chosen }]
function options(slide, x, y, w, opts) {
  slide.addText("CÁC HƯỚNG", { x, y, w, h: 0.22, fontSize: 10, bold: true, color: C.accent5, charSpacing: 1.5, margin: 0, isTextBox: true, objectName: "Options label" });
  let cy = y + 0.27;
  opts.forEach((o) => {
    if (o.chosen) {
      slide.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: cy, w, h: 0.32, rectRadius: 0.08, fill: { color: C.accent6 }, line: { color: C.accent6 }, objectName: "Chosen option" });
      slide.addText("ĐÃ CHỌN", { x: x + w - 0.95, y: cy, w: 0.85, h: 0.32, fontSize: 10, bold: true, color: C.accent1, align: "right", valign: "middle", margin: 0, isTextBox: true, objectName: "Chosen tag" });
    }
    slide.addText(o.r, { x: x + 0.1, y: cy, w: 0.3, h: 0.32, fontSize: 15, color: GLYPH[o.r], align: "center", valign: "middle", margin: 0, isTextBox: true, objectName: "Rating" });
    slide.addText(o.t, { x: x + 0.45, y: cy, w: w - 1.45, h: 0.32, fontSize: 14, bold: !!o.chosen, color: C.text1, valign: "middle", margin: 0, isTextBox: true, objectName: "Option" });
    cy += 0.36;
  });
  return cy; // bottom
}

// Client A — Server — Client B motif
function network(slide, ox, oy, s = 1) {
  const S = { x: ox + 1.45 * s, y: oy + 0.5 * s, r: 0.5 * s };
  const A = { x: ox + 0.45 * s, y: oy + 2.55 * s, r: 0.45 * s };
  const B = { x: ox + 2.45 * s, y: oy + 2.55 * s, r: 0.45 * s };
  slide.addShape(pres.shapes.LINE, { x: A.x, y: S.y, w: S.x - A.x, h: A.y - S.y, flipV: true, line: { color: C.accent6, width: 2, dashType: "dash" }, objectName: "Link A" });
  slide.addShape(pres.shapes.LINE, { x: S.x, y: S.y, w: B.x - S.x, h: B.y - S.y, line: { color: C.accent6, width: 2, dashType: "dash" }, objectName: "Link B" });
  [[A, 0.35, C.accent1], [A, 0.65, C.accent3], [B, 0.4, C.accent3], [B, 0.7, C.accent1]].forEach(([N, t, col]) => {
    const px = N.x + (S.x - N.x) * t, py = N.y + (S.y - N.y) * t, d = 0.16 * s;
    slide.addShape(pres.shapes.OVAL, { x: px - d / 2, y: py - d / 2, w: d, h: d, fill: { color: col }, line: { color: col }, objectName: "Packet" });
  });
  slide.addShape(pres.shapes.OVAL, { x: S.x - S.r, y: S.y - S.r, w: 2 * S.r, h: 2 * S.r, fill: { color: C.accent1 }, line: { color: C.accent6, width: 2 }, objectName: "Server node" });
  slide.addText("Server", { x: S.x - S.r, y: S.y - S.r, w: 2 * S.r, h: 2 * S.r, fontSize: 13 * s, bold: true, color: C.background1, align: "center", valign: "middle", margin: 0, isTextBox: true });
  [[A, "Client A"], [B, "Client B"]].forEach(([N, label]) => {
    slide.addShape(pres.shapes.OVAL, { x: N.x - N.r, y: N.y - N.r, w: 2 * N.r, h: 2 * N.r, fill: { color: C.text2 }, line: { color: C.accent6, width: 2 }, objectName: label + " node" });
    slide.addText(label.replace(" ", "\n"), { x: N.x - N.r, y: N.y - N.r, w: 2 * N.r, h: 2 * N.r, fontSize: 11 * s, bold: true, color: C.accent6, align: "center", valign: "middle", margin: 0, isTextBox: true });
  });
  slide.addText("RTT 100–200 ms", { x: A.x, y: A.y + A.r + 0.12, w: B.x - A.x, h: 0.3, fontSize: 11, italic: true, color: C.accent6, align: "center", margin: 0, isTextBox: true, objectName: "RTT label" });
}

function table(slide, header, rows, box, colW, o = {}) {
  const border = { type: "solid", pt: 0.75, color: "D3DCE6" };
  const head = header.map((h) => ({ text: h, options: { bold: true, color: C.background1, fill: { color: C.text1 }, fontSize: 13 } }));
  const body = rows.map((r, i) =>
    r.cells.map((c) => {
      const opts = { fill: { color: r.chosen ? C.accent6 : i % 2 ? C.background2 : C.background1 }, bold: !!r.chosen, color: C.text1 };
      if (typeof c === "string") return { text: c, options: opts };
      return { text: c, options: opts }; // already runs
    })
  );
  slide.addTable([head, ...body], { x: box.x, y: box.y, w: box.w, colW, fontSize: o.fontSize || 14, valign: "middle", border, margin: [0.05, 0.1, 0.05, 0.1], rowH: o.rowH, objectName: o.name || "Table" });
}

function rating(sym, extra) {
  const r = [{ text: sym, options: { color: GLYPH[sym], fontSize: 16 } }];
  if (extra) r.push({ text: " " + extra, options: { color: C.accent5, fontSize: 12 } });
  return r;
}

function notesFor(who, time, cum, speech, extra) {
  let n = `Người trình bày: ${who} · Thời gian: ${time} · Cộng dồn: ${cum}\n\n${speech.trim()}`;
  if (extra) n += `\n\n— ${extra.trim()}`;
  return n;
}

function content(section, eyebrow, title, who, time) {
  const s = pres.addSlide({ masterName: "CONTENT", sectionTitle: section });
  s.addText(eyebrow, { placeholder: "eyebrow", bullet: false });
  s.addText(title, { placeholder: "title" });
  if (who) chip(s, who, time);
  return s;
}

// Standard VĐ slide: context bullets, option list, consequence bullets, figure on the right
function vdSlide(section, d) {
  const s = content(section, d.eyebrow, d.title, d.who, d.time);
  const L = { x: 0.5, w: 5.7 };
  let y = 1.35;
  if (d.pre) { text(s, d.pre, { x: L.x, y, w: L.w, h: d.preH }); y += d.preH + 0.05; }
  y = options(s, L.x, y, L.w, d.opts) + 0.12;
  text(s, d.post, { x: L.x, y, w: L.w, h: 5.05 - y });
  figure(s, { x: 6.45, y: 1.35, w: 3.05, h: 3.65 }, d.fig);
  s.addNotes(notesFor(d.who, d.time, d.cum, d.speech, d.extra));
  return s;
}

// ================= SLIDES =================
const SEC1 = "Mở đầu", SEC2 = "Năm vấn đề con", SEC3 = "Tổng kết & áp dụng", SEC4 = "Demo & hỏi đáp", SEC5 = "Dự phòng Q&A";

// ---- 1. Title ----
pres.addSection({ title: SEC1 });
{
  const s = pres.addSlide({ masterName: "SECTION_DARK", sectionTitle: SEC1 });
  s.addText("ĐỀ TÀI T47 · MÔN LẬP TRÌNH MẠNG", { placeholder: "eyebrow", bullet: false });
  s.addText("Kỹ thuật mạng cho game nhiều người chơi thời gian thực", { placeholder: "title" });
  s.addText("Dự đoán phía client", { placeholder: "subtitle", bullet: false });
  s.addText([
    { text: "GV: Đặng Ngọc Hùng", options: { breakLine: true, bullet: false } },
    { text: "Nguyễn Thế Chiến (B23DCCN095) · Phạm Quang Anh (B23DCCN044) · ", options: { bullet: false } },
    { text: "TBD", options: { bullet: false, bold: true, italic: true, color: C.accent3 } },
  ], { placeholder: "meta" });
  network(s, 6.55, 1.15, 1);
  s.addNotes(notesFor("#1", "0,25'", "0,25'", `
Chào thầy và các bạn. Nhóm em trình bày đề tài T47 về kỹ thuật mạng cho game nhiều người chơi thời gian thực. Câu hỏi nhóm em muốn trả lời là: làm sao để chơi game qua mạng mà vẫn có cảm giác như chơi trên máy mình, khi mỗi gói tin mất cả trăm mili giây mới tới server.`, `
Ghi chú tập luyện:
• Vai: #1 server & giao thức (slide 1–6) · #2 client (slide 7–9) · #3 tương tác & demo (slide 10–13, demo).
• Chuyển người nói ở cuối slide 6 (#1 → #2) và cuối slide 9 (#2 → #3).
• Bấm giờ theo cột cộng dồn. Đến slide 9 mà đã quá 9 phút thì nói nhanh slide 10–11, bỏ slide 12.
• Nếu tập thấy quá 20 phút: gộp slide 4 vào slide 3, gộp slide 12 vào slide 13.
• Điền tên thành viên thứ 3 (đang để TBD).`));
}

// ---- 2. Problem ----
{
  const s = content(SEC1, "BÀI TOÁN", "Hai yêu cầu mâu thuẫn", "#1", "0,75'");
  const card = (x, head, body) => {
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 1.4, w: 3.55, h: 1.75, rectRadius: 0.1, fill: { color: C.background2 }, line: { color: C.background2 }, objectName: "Requirement card" });
    s.addText(head, { x: x + 0.3, y: 1.6, w: 2.95, h: 0.45, fontSize: 20, bold: true, color: C.accent1, margin: 0, isTextBox: true });
    s.addText(body, { x: x + 0.3, y: 2.1, w: 2.95, h: 1.2, fontSize: 14, color: C.text1, valign: "top", margin: 0, isTextBox: true });
  };
  card(0.5, "Độ phản hồi", "Bấm phím là nhân vật phản ứng ngay (từ ~100 ms đã thấy ì)");
  card(5.95, "Tính nhất quán", "Mọi người thấy cùng một thế giới, không ai gian lận được");
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 4.2, y: 1.95, w: 1.6, h: 0.65, rectRadius: 0.32, fill: { color: C.text2 }, line: { color: C.text2 }, objectName: "Network pill" });
  s.addText([{ text: "Mạng", options: { bold: true, breakLine: true } }, { text: "100–200 ms" }], { x: 4.2, y: 1.95, w: 1.6, h: 0.65, fontSize: 12, color: C.background1, align: "center", valign: "middle", margin: 0, isTextBox: true });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.5, y: 3.5, w: 9.0, h: 0.95, rectRadius: 0.1, fill: { color: C.text1 }, line: { color: C.text1 }, objectName: "Conflict callout" });
  s.addText(runs("Có mạng ở giữa thì: muốn nhất quán phải **chờ**, muốn phản hồi ngay thì **không được chờ**"), { x: 0.8, y: 3.5, w: 8.4, h: 0.95, fontSize: 16, color: C.background1, valign: "middle", margin: 0, isTextBox: true, objectName: "Conflict text" });
  s.addNotes(notesFor("#1", "0,75'", "1'", `
Game thời gian thực phải thoả hai yêu cầu cùng lúc. Một là độ phản hồi: bấm phím là nhân vật chạy ngay; trễ khoảng 100 mili giây là người chơi đã thấy ì. Hai là tính nhất quán: mọi người phải thấy cùng một thế giới, và không ai được tự ý sửa nó.

Trên một máy thì hai yêu cầu này không mâu thuẫn. Nhưng khi có mạng ở giữa, muốn nhất quán thì phải chờ thông tin đi hết đường, còn muốn phản hồi ngay thì không được chờ. Toàn bộ đề tài là cách dung hoà hai yêu cầu đó.`, `
Hình gợi ý (có thể thay bố cục thẻ hiện tại): cái cân, một đĩa ghi "Phản hồi tức thì", đĩa kia ghi "Nhất quán", ở giữa là đám mây "Mạng: 100–200 ms".`));
}

// ---- 3. Imperfect network ----
{
  const s = content(SEC1, "VẤN ĐỀ GỐC", "Mạng không hoàn hảo", "#1", "1'");
  text(s, [
    "**Độ trễ:** có giới hạn vật lý. Hà Nội – Singapore, RTT tối thiểu ≈ 22 ms; thực tế 30–60 ms",
    "**Dao động độ trễ:** gửi đều 50 ms, nhận cách nhau 30, 80, 45 ms…",
    "**Mất gói, sai thứ tự:** Wi-Fi / 4G mất 0,5–2% !!(cần đo lại)!!",
    "**TCP:** mất 1 gói thì chặn mọi gói sau (**nghẽn đầu hàng**)",
    "**UDP:** không chặn, nhưng không đảm bảo gì",
  ], { x: 0.5, y: 1.35, w: 5.7, h: 2.9 }, { space: 8 });
  s.addText("Không thể loại bỏ, chỉ có thể che giấu", { x: 0.5, y: 4.45, w: 5.7, h: 0.45, fontSize: 18, italic: true, bold: true, color: C.accent1, fontFace: "Cambria", margin: 0, isTextBox: true, objectName: "Takeaway" });
  figure(s, { x: 6.45, y: 1.35, w: 3.05, h: 3.65 }, "Hai dòng thời gian TCP và UDP cùng gửi gói 1–2–3–4, gói 2 bị mất. TCP: gói 3 và 4 phải chờ gói 2 được gửi lại. UDP: gói 3 và 4 vẫn đến đúng hạn.");
  s.addNotes(notesFor("#1", "1'", "2'", `
Vấn đề gốc nằm ở mạng. Thứ nhất là độ trễ, và nó có giới hạn vật lý: tín hiệu trong cáp quang đi khoảng 200 nghìn km mỗi giây, nên từ Hà Nội sang Singapore đi rồi về tối thiểu cũng 22 mili giây; đo thực tế thường 30 đến 60. Thứ hai, độ trễ không đều: gói gửi đều đặn nhưng đến lúc nhanh lúc chậm, gọi là dao động độ trễ. Thứ ba, gói có thể mất hoặc đến sai thứ tự.

Hai giao thức xử lý chuyện này khác nhau. TCP đảm bảo thứ tự, nên khi mất một gói nó giữ lại mọi gói phía sau để chờ gửi lại; hiện tượng này gọi là nghẽn đầu hàng. UDP thì gói nào mất là mất, các gói sau vẫn đến bình thường.

Kết luận là những hiện tượng này không thể loại bỏ, chỉ có thể che giấu.`, `
VIỆC CẦN LÀM: thay số liệu mất gói "(cần đo lại)" bằng số đo thật trước buổi trình bày.`));
}

// ---- 4. Chain of 5 problems ----
{
  const s = content(SEC1, "CÁCH TIẾP CẬN", "Chuỗi 5 vấn đề con", "#1", "0,5'");
  const probs = ["Ai giữ quyền quyết định?", "Điều khiển bị ì", "Đối thủ giật", "Tương tác không công bằng", "Dự đoán hành động bị sai"];
  const caps = ["phải chờ server", "chỉ đoán được mình", "đối thủ ở quá khứ", "server có thể quyết khác"];
  const bw = 1.56, gap = 0.3, by = 1.4, bh = 1.1;
  probs.forEach((p, i) => {
    const x = 0.5 + i * (bw + gap);
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: by, w: bw, h: bh, rectRadius: 0.1, fill: { color: C.background2 }, line: { color: C.accent1, width: 1 }, objectName: `VD${i + 1} box` });
    s.addText([{ text: `VĐ${i + 1}`, options: { fontSize: 11, bold: true, color: C.accent1, breakLine: true } }, { text: p, options: { fontSize: 13, bold: true, color: C.text1 } }],
      { x: x + 0.1, y: by + 0.1, w: bw - 0.2, h: bh - 0.2, valign: "top", margin: 0, isTextBox: true });
    if (i < 4) {
      const gx = x + bw;
      s.addShape(pres.shapes.LINE, { x: gx + 0.03, y: by + bh / 2, w: gap - 0.06, h: 0, line: { color: C.accent1, width: 1.75, endArrowType: "triangle" }, objectName: "Arrow" });
      const cx = gx + gap / 2;
      s.addShape(pres.shapes.LINE, { x: cx, y: by + bh / 2 + 0.12, w: 0, h: bh / 2 + 0.05, line: { color: C.accent5, width: 0.75, dashType: "sysDot" }, objectName: "Caption tick" });
      s.addText(caps[i], { x: cx - 0.8, y: by + bh + 0.2, w: 1.6, h: 0.5, fontSize: 11, italic: true, color: C.accent5, align: "center", valign: "top", margin: 0, isTextBox: true, objectName: "Arrow caption" });
    }
  });
  text(s, ["Giải pháp của vấn đề trước **làm lộ ra** vấn đề sau"], { x: 0.5, y: 3.4, w: 9, h: 0.35 });
  s.addText("TIÊU CHÍ CHỌN GIẢI PHÁP", { x: 0.5, y: 3.95, w: 4, h: 0.25, fontSize: 10, bold: true, color: C.accent5, charSpacing: 1.5, margin: 0, isTextBox: true });
  const crit = [["Phản hồi · ưu tiên", true, 1.75], ["Nhất quán · ưu tiên", true, 1.8], ["Chịu mạng xấu", false, 1.45], ["Độ phức tạp", false, 1.3], ["Phù hợp loại game", false, 1.7]];
  let cx = 0.5;
  crit.forEach(([t, pri, w]) => {
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: cx, y: 4.28, w, h: 0.4, rectRadius: 0.2, fill: { color: pri ? C.accent1 : C.background2 }, line: { color: pri ? C.accent1 : C.accent5, width: 0.75 }, objectName: "Criterion chip" });
    s.addText(t, { x: cx, y: 4.28, w, h: 0.4, fontSize: 12, bold: pri, color: pri ? C.background1 : C.text1, align: "center", valign: "middle", margin: 0, isTextBox: true });
    cx += w + 0.15;
  });
  s.addNotes(notesFor("#1", "0,5'", "2,5'", `
Nhóm em chia vấn đề gốc thành 5 vấn đề con, nối thành chuỗi: giải quyết xong vấn đề trước thì lộ ra vấn đề sau. Với mỗi vấn đề, nhóm em xét vài hướng giải pháp, so sánh, rồi chọn. Hai tiêu chí quan trọng nhất là độ phản hồi và tính nhất quán, đúng hai yêu cầu vừa nói. Ngoài ra còn xét khả năng chịu mạng xấu, độ phức tạp và độ phù hợp với loại game.`));
}

// ---- 5. VĐ1 options ----
pres.addSection({ title: SEC2 });
{
  const s = content(SEC2, "VẤN ĐỀ 1", "Ai giữ quyền quyết định trạng thái?", "#1", "1'");
  table(s, ["Hướng", "Phản hồi", "Chống gian lận", "Ghi chú"], [
    { cells: ["Client tự quyết, server chuyển tiếp", rating("●"), rating("○"), "Sửa gói tin là dịch chuyển, hack tốc độ, tự cộng điểm"] },
    { cells: ["Đồng bộ theo bước (ngang hàng)", rating("○"), rating("◐"), "Chờ người chậm nhất; cần tất định tuyệt đối"] },
    { chosen: true, cells: ["Server quyết định", rating("○", "(giải ở VĐ2)"), rating("●"), "Client chỉ gửi lệnh điều khiển"] },
  ], { x: 0.5, y: 1.35, w: 9.0 }, [2.75, 1.45, 1.4, 3.4], { rowH: [0.38, 0.6, 0.6, 0.6] });
  figure(s, { x: 0.5, y: 3.75, w: 9.0, h: 1.3 }, [
    "- Trái: Client gửi `vị trí = (3,5)`, server chuyển tiếp thẳng; chú thích đỏ “server tin luôn”",
    "- Phải: Client gửi `phím D`, server tự tính rồi gửi về ảnh trạng thái",
  ], "HÌNH CẦN VẼ · HAI SƠ ĐỒ NHỎ ĐẶT CẠNH NHAU");
  s.addNotes(notesFor("#1", "1'", "3,5'", `
Vấn đề đầu tiên: khi mỗi máy giữ một bản sao thế giới, ai được quyền quyết định bản nào đúng?

Cách đơn giản nhất là cho client tự quyết rồi server chuyển tiếp. Phản hồi rất tốt, nhưng client chạy trên máy người chơi, nên chỉ cần sửa gói tin là dịch chuyển được, chạy nhanh gấp đôi được, hoặc báo "tôi giao đúng hàng" để được cộng điểm.

Cách thứ hai là đồng bộ theo bước, kiểu game chiến thuật: các máy chỉ gửi lệnh cho nhau rồi cùng mô phỏng. Nhưng cách này phải chờ người chậm nhất, và mô phỏng phải giống nhau đến từng bit.

Nhóm em chọn cách thứ ba: server quyết định. Client chỉ được nói "tôi bấm phím gì", còn kết quả do server tính.`));
}

// ---- 6. VĐ1 server tick + transport ----
{
  const s = content(SEC2, "VẤN ĐỀ 1", "Server quyết định, nhịp cố định, kênh truyền", "#1", "1'");
  const stat = (x, num, label) => {
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 1.35, w: 2.7, h: 1.05, rectRadius: 0.1, fill: { color: C.background2 }, line: { color: C.background2 }, objectName: "Stat tile" });
    s.addText(num, { x: x + 0.2, y: 1.4, w: 1.0, h: 0.95, fontSize: 40, bold: true, color: C.accent1, fontFace: "Cambria", valign: "middle", margin: 0, isTextBox: true });
    s.addText(label, { x: x + 1.2, y: 1.4, w: 1.4, h: 0.95, fontSize: 13, color: C.text1, valign: "middle", margin: 0, isTextBox: true });
  };
  stat(0.5, "30", "nhịp mô phỏng mỗi giây (cố định)");
  stat(3.5, "20", "ảnh trạng thái gửi mỗi giây");
  text(s, [
    "Mỗi nhịp xử lý tối đa **1 lệnh/người**, nên gửi nhiều lệnh cũng không chạy nhanh hơn",
    "**TCP:** đăng nhập, ghép trận, kết quả quan trọng · **UDP:** lệnh, ảnh trạng thái",
    "Trên UDP tự thêm: **số thứ tự**, **xác nhận** (`lastProcessedSeq`), **gửi dư** 2–3 lệnh trước",
  ], { x: 0.5, y: 2.65, w: 5.7, h: 2.4 }, { space: 8 });
  figure(s, { x: 6.45, y: 1.35, w: 3.05, h: 3.65 }, "Dòng thời gian Client / Server: Client bấm D → mũi tên chéo lên server → server áp lệnh ở nhịp 41 → mũi tên chéo về client. Ghi chú: “chờ ≈ 1 RTT”.");
  s.addNotes(notesFor("#1", "1'", "4,5'", `
Server chạy mô phỏng theo nhịp cố định, 30 nhịp mỗi giây. Nhịp cố định giúp mô phỏng lặp lại được và cho client với server một đồng hồ chung là số nhịp. Mỗi nhịp server chỉ xử lý tối đa một lệnh cho mỗi người, nên dù client gửi dồn dập cũng không chạy nhanh hơn được.

Về giao thức, nhóm em dùng cả hai. Dữ liệu chuyển động thì dùng UDP, vì vị trí cũ 200 mili giây thì gửi lại cũng vô ích. Đăng nhập, ghép trận, kết quả phân xử thì dùng TCP, vì bắt buộc phải tới nơi. Trên UDP, nhóm em tự thêm số thứ tự, xác nhận, và mỗi gói lệnh gửi dư vài lệnh trước để bù mất gói.

Nhưng có một cái giá: nhìn dòng thời gian thì thấy, bấm phím xong phải chờ khoảng một RTT mới thấy nhân vật chạy. Phần này bạn [#2] trình bày tiếp.`, `
CHUYỂN NGƯỜI NÓI: #1 → #2.`));
}

// ---- 7. VĐ2 prediction ----
vdSlide(SEC2, {
  eyebrow: "VẤN ĐỀ 2", title: "Điều khiển bị ì → dự đoán phía client", who: "#2", time: "1,25'", cum: "5,75'",
  pre: ["Độ trễ điều khiển ≈ **RTT + tối đa 83 ms**. Với RTT 200 ms là **200–280 ms**"], preH: 0.6,
  opts: [{ t: "Chờ server", r: "○" }, { t: "Che bằng hoạt ảnh", r: "◐" }, { t: "Dự đoán phía client", r: "●", chosen: true }],
  post: [
    "Dự đoán: client **tự áp lệnh ngay**, server vẫn là người quyết định cuối",
    "Điều kiện: **dùng chung code mô phỏng** `Simulation.Step` (C# cho cả Unity và .NET)",
    "Nhưng chỉ dự đoán thôi thì bị **giật ngược**",
  ],
  fig: "Trục vị trí: ô xanh (dự đoán) ở x = 2,0. Ảnh trạng thái về báo x = 0,8. Mũi tên đỏ kéo ô xanh lùi về 0,8.",
  speech: `
Với RTT 200 mili giây, mỗi lần bấm phím phải chờ 200 đến gần 300 mili giây. Gian lận đã bị chặn, nhưng game không chơi được.

Có thể chấp nhận chờ, hoặc che bằng hoạt ảnh lấy đà, nhưng di chuyển bằng WASD cần phản hồi ngay. Nhóm em chọn dự đoán phía client. Hầu như lúc nào server cũng đồng ý với lệnh của mình, nên client cứ tự áp lệnh luôn, không chờ. Điều kiện là client phải tính ra đúng kết quả như server, nên hai bên dùng chung một hàm mô phỏng. Đây là lý do nhóm em dùng C# cho cả Unity lẫn server .NET.

Nhưng nếu chỉ dự đoán thì có vấn đề. Ảnh trạng thái từ server phản ánh quá khứ. Client đang ở x bằng 2, server báo 0,8; ghi đè vào thì nhân vật bị kéo giật lùi. Hiện tượng này gọi là giật ngược.`,
});

// ---- 8. VĐ2 reconciliation ----
{
  const s = content(SEC2, "VẤN ĐỀ 2", "Đối chiếu với server", "#2", "1,25'");
  const steps = [
    "Client giữ **bộ đệm** các lệnh chưa được xác nhận",
    "Ảnh trạng thái kèm `lastProcessedSeq = N`",
    "Nhận ảnh: **đặt lại** theo server → **bỏ** lệnh ≤ N → **chạy lại** lệnh còn lại",
  ];
  const rowH = [0.5, 0.5, 0.72];
  let y = 1.35;
  steps.forEach((t, i) => {
    s.addShape(pres.shapes.OVAL, { x: 0.5, y, w: 0.4, h: 0.4, fill: { color: C.accent2 }, line: { color: C.accent2 }, objectName: "Step number" });
    s.addText(String(i + 1), { x: 0.5, y, w: 0.4, h: 0.4, fontSize: 14, bold: true, color: C.background1, align: "center", valign: "middle", margin: 0, isTextBox: true });
    s.addText(runs(t), { x: 1.05, y, w: 5.15, h: rowH[i], fontSize: 14, color: C.text1, valign: "top", margin: [6, 0, 0, 0], isTextBox: true, objectName: "Step text" });
    y += rowH[i] + 0.05;
  });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.5, y: 3.65, w: 9.0, h: 1.35, rectRadius: 0.08, fill: { color: C.text2 }, line: { color: C.text2 }, objectName: "Code block" });
  s.addText([
    { text: "state = s.MyState;", options: { breakLine: true } },
    { text: "pending.RemoveAll(i => i.Seq <= s.LastProcessedSeq);", options: { breakLine: true } },
    { text: "foreach (var i in pending) state = Simulation.Step(state, i, Dt);" },
  ], { x: 0.75, y: 3.65, w: 8.5, h: 1.35, fontSize: 13, fontFace: "Courier New", color: C.accent6, valign: "middle", margin: 0, isTextBox: true, objectName: "Code" });
  figure(s, { x: 6.45, y: 1.35, w: 3.05, h: 2.1 }, "Bộ đệm `[1 2 3 4 | 5 6 7 8 9 10]`, gạch bỏ 1–4. Mũi tên “chạy lại 5–10: 0,8 + 6 × 0,2 = 2,0 ✓”.");
  s.addNotes(notesFor("#2", "1,25'", "7'", `
Cách sửa là đối chiếu với server. Client giữ lại mọi lệnh đã gửi mà server chưa xác nhận. Server gửi ảnh trạng thái kèm số thứ tự lệnh cuối cùng nó đã xử lý.

Ví dụ: client đã gửi 10 lệnh, mỗi lệnh đi 0,2 đơn vị, đang dự đoán ở x bằng 2. Server báo "x bằng 0,8, đã xử lý tới lệnh 4". Client đặt lại x bằng 0,8, bỏ lệnh 1 đến 4, chạy lại 6 lệnh còn lại, và được đúng 2,0. Người chơi không thấy gì.

Chỉ khi dự đoán thật sự sai, ví dụ server thấy có va chạm mà client không biết, thì vị trí mới thay đổi; lệch nhỏ thì làm mượt trong vài khung hình. Trong demo, các bạn sẽ thấy số lệnh trong bộ đệm luôn xấp xỉ RTT chia độ dài một nhịp.`));
}

// ---- 9. VĐ3 interpolation ----
vdSlide(SEC2, {
  eyebrow: "VẤN ĐỀ 3", title: "Đối thủ hiển thị giật → nội suy thực thể", who: "#2", time: "1,5'", cum: "8,5'",
  pre: ["Đối thủ thì **không đoán được**. Ảnh trạng thái thưa (10–20 lần/giây), không đều, có thể mất"], preH: 0.6,
  opts: [{ t: "Vẽ ảnh mới nhất: giật", r: "○" }, { t: "Ngoại suy: sai khi đổi hướng", r: "○" }, { t: "Nội suy", r: "●", chosen: true }],
  post: [
    "**Vẽ đối thủ lùi lại** một khoảng, nội suy giữa 2 ảnh trạng thái bao quanh",
    "Độ trễ nội suy ≥ **2 × chu kỳ gửi ảnh + dao động độ trễ** (20 lần/giây thì ~100 ms)",
    "Đánh đổi: **mình ở hiện tại, đối thủ ở quá khứ**",
  ],
  fig: "Trục thời gian: các ảnh S1…S5 là các chấm đặt không đều. Vạch “hiện tại” ở bên phải; vạch “thời điểm vẽ” lùi về giữa S3 và S4. Ô đối thủ nằm trên đoạn nối S3–S4.",
  speech: `
Dự đoán chỉ dùng được cho chính mình, vì mình không biết đối thủ sắp bấm gì. Thông tin về đối thủ chỉ đến qua ảnh trạng thái, mà ảnh thì thưa, không đều và có thể mất. Vẽ theo ảnh mới nhất thì đối thủ đứng yên rồi nhảy cóc. Ngoại suy theo vận tốc thì sai mỗi khi đối thủ đổi hướng.

Nhóm em chọn nội suy: cố ý vẽ đối thủ lùi lại một khoảng, ví dụ 100 mili giây. Nhờ lùi lại, ta luôn có hai ảnh trạng thái kẹp hai bên thời điểm vẽ, và chỉ cần nội suy đường thẳng giữa chúng. Khoảng lùi phải ít nhất bằng hai chu kỳ gửi ảnh cộng dao động độ trễ, để mất một gói vẫn còn dữ liệu.

Cái giá là ta luôn thấy đối thủ ở quá khứ. Như vậy trên màn hình mỗi người có hai mốc thời gian: mình ở hiện tại, đối thủ ở quá khứ. Khi hai người tranh cùng một thứ thì sao? Bạn [#3] trình bày tiếp.`,
  extra: "CHUYỂN NGƯỜI NÓI: #2 → #3.",
});

// ---- 10. VĐ4 lag compensation ----
vdSlide(SEC2, {
  eyebrow: "VẤN ĐỀ 4", title: "Tương tác không công bằng → bù trễ", who: "#3", time: "1,5'", cum: "10'",
  pre: [
    "A (RTT 40 ms) và B (RTT 200 ms) cùng nhặt một vật; trên màn hình, **B chạm trước**",
    "Phân xử theo thứ tự gói đến thì **A luôn thắng**: người trễ thấp luôn được lợi",
  ], preH: 1.1,
  opts: [{ t: "Thứ tự gói đến", r: "○" }, { t: "Client tự quyết (gian lận)", r: "○" }, { t: "Bù trễ", r: "●", chosen: true }],
  post: [
    "**Bù trễ:** server lưu lịch sử 1 giây, **tua lại** về thời điểm người chơi nhìn thấy rồi mới phân xử",
    "Tua tối đa ~200–250 ms · Đánh đổi: **“đã chạy đi rồi vẫn bị bắt”**",
  ],
  fig: [
    "Dòng thời gian hai client, độ dài mũi tên khác nhau:",
    "- Gói của B đến server sau gói của A",
    "- Không bù trễ: A thắng",
    "- Có bù trễ: server tua về thời điểm của từng người, thấy B trước nên B thắng",
  ],
  speech: `
Ví dụ kinh điển là game bắn súng: trên màn hình tôi ngắm trúng đầu, nhưng thứ tôi thấy là đối thủ của 200 mili giây trước. Lệnh bắn tới server thì đối thủ đã đi chỗ khác.

Với vật dùng chung cũng vậy. A ping 40, B ping 200, trên màn hình B chạm vật trước. Nhưng gói của A tới server nhanh hơn, nên nếu xử theo thứ tự gói đến thì A luôn thắng. Để client tự quyết thì quay lại chuyện gian lận.

Giải pháp là bù trễ. Server lưu lịch sử trạng thái khoảng một giây. Mỗi hành động gửi kèm thời điểm người chơi đang nhìn thấy. Server tua lại về đúng lúc đó, kiểm tra, rồi mới phân xử. Để chống gửi thời điểm giả, server giới hạn chỉ tua tối đa khoảng 200 mili giây.

Đánh đổi là công bằng cho người hành động thì người kia chịu thiệt: trên màn hình mình đã chạy đi rồi, vẫn bị tính là bị bắt. Không cách nào làm cả hai cùng thấy công bằng.`,
});

// ---- 11. VĐ5 rollback ----
vdSlide(SEC2, {
  eyebrow: "VẤN ĐỀ 5", title: "Dự đoán hành động bị sai → quay lui", who: "#3", time: "1'", cum: "11'",
  pre: ["Client đã hiển thị “nhặt được”, server báo **từ chối**"], preH: 0.35,
  opts: [{ t: "Không dự đoán: chờ 1 RTT", r: "○" }, { t: "Dự đoán + quay lui cục bộ", r: "●", chosen: true }, { t: "Quay lui toàn phần kiểu GGPO (ngang hàng)", r: "○" }],
  post: [
    "**Quay lui:** về trạng thái đã xác nhận → mô phỏng lại → vật “bật ra khỏi tay”",
    "Đối chiếu (VĐ2) là **trường hợp riêng** của quay lui",
    "Quay lui ngang hàng cần **tính tất định tuyệt đối**; mô hình server quyết định chỉ cần “gần giống”",
  ],
  fig: "Dòng thời gian client: nhịp 50 bấm nhặt (nét đứt: dự đoán); nhịp 56 nhận “TỪ CHỐI”; mũi tên vòng ngược về nhịp 50; nhánh mới (nét liền) không cầm vật.",
  speech: `
Kết quả phân xử về muộn, mà client đã cho người chơi cầm vật từ trước, vì không muốn bắt họ chờ. Nếu thua thì sao? Client quay về trạng thái đã được xác nhận, mô phỏng lại với thông tin đúng, và người chơi thấy vật bật ra khỏi tay. Đó là quay lui.

Đối chiếu ở VĐ2 thực chất là trường hợp riêng của quay lui, chỉ áp cho di chuyển của mình. Game đối kháng như Street Fighter dùng quay lui toàn phần theo mô hình GGPO, chạy ngang hàng. Mô hình đó đòi hỏi mô phỏng giống nhau đến từng bit giữa các máy. Nhóm em có server sửa định kỳ, nên chỉ cần gần giống.`,
});

// ---- 12. Summary ----
pres.addSection({ title: SEC3 });
{
  const s = content(SEC3, "TỔNG KẾT", "Năm vấn đề, năm đánh đổi", "#3", "0,5'");
  table(s, ["Vấn đề", "Giải pháp", "Đánh đổi"], [
    { cells: ["Ai quyết định", "Server quyết định", "Trễ 1 RTT"] },
    { cells: ["Điều khiển ì", "Dự đoán + đối chiếu", "Cần mô phỏng chung"] },
    { cells: ["Đối thủ giật", "Nội suy", "Thấy đối thủ ở quá khứ"] },
    { cells: ["Tranh chấp không công bằng", "Bù trễ", "“Chạy rồi vẫn bị bắt”"] },
    { cells: ["Dự đoán sai", "Quay lui", "Thỉnh thoảng thấy hành động bị huỷ"] },
  ], { x: 0.5, y: 1.35, w: 9.0 }, [2.9, 2.6, 3.5], { rowH: 0.42 });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.5, y: 4.1, w: 9.0, h: 0.85, rectRadius: 0.1, fill: { color: C.text1 }, line: { color: C.text1 }, objectName: "Key message" });
  s.addText("Không kỹ thuật nào xoá được độ trễ, chỉ chuyển nó sang chỗ ít bị để ý hơn", { x: 0.8, y: 4.1, w: 8.4, h: 0.85, fontSize: 18, bold: true, italic: true, fontFace: "Cambria", color: C.background1, valign: "middle", margin: 0, isTextBox: true });
  s.addNotes(notesFor("#3", "0,5'", "11,5'", `
Tóm lại, không có kỹ thuật nào xoá được độ trễ. Mỗi kỹ thuật chuyển độ trễ sang chỗ người chơi ít để ý hơn: mình đi trước nhờ dự đoán, đối thủ đi sau nhờ nội suy, và server bù lại sự lệch thời gian khi hai bên tương tác.`, `
Nếu thiếu giờ: bỏ slide này, nói câu chốt ở slide 13.`));
}

// ---- 13. Application ----
{
  const s = content(SEC3, "ÁP DỤNG", "Game xếp đơn hàng siêu thị → demo", "#3", "0,5'");
  text(s, [
    "Game 2 người, mỗi người là nhân viên siêu thị: **WASD** di chuyển, **J** nhặt / giao, **K** thả",
    "Mỗi khách cần 3 món theo thứ tự, giao đúng **+5 điểm**, trận **2 phút 30 giây**",
    "**Món hàng hiếm** chỉ có 1 cái ở kệ giữa → tranh chấp → bù trễ + quay lui",
    "Unity (C#) + .NET server · TCP + UDP · SQLite",
  ], { x: 0.5, y: 1.35, w: 5.7, h: 2.55 }, { space: 8 });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.5, y: 4.05, w: 5.7, h: 0.95, rectRadius: 0.1, fill: { color: C.accent6 }, line: { color: C.accent6 }, objectName: "Demo callout" });
  s.addText(runs("**Demo hôm nay:** bản chứng minh ý tưởng, 2 ô vuông, 3 khung nhìn, mạng giả lập"), { x: 0.75, y: 4.05, w: 5.25, h: 0.95, fontSize: 14, color: C.text1, valign: "middle", margin: 0, isTextBox: true });
  figure(s, { x: 6.45, y: 1.35, w: 3.05, h: 3.65 }, "Ảnh phác bản đồ siêu thị: hai quầy khách hai bên, kệ hàng, món hàng hiếm phát sáng ở giữa.");
  s.addNotes(notesFor("#3", "0,5'", "12'", `
Nhóm em sẽ áp dụng các kỹ thuật này vào game xếp đơn hàng siêu thị cho 2 người. Game có di chuyển liên tục, nên cần dự đoán và nội suy. Món hàng hiếm chỉ có một cái, hai người phải tranh nhau, nên cần bù trễ và quay lui. Hôm nay nhóm em demo bản chứng minh ý tưởng cho phần di chuyển.`));
}

// ---- 14. Demo ----
pres.addSection({ title: SEC4 });
{
  const s = pres.addSlide({ masterName: "DARK_TITLE", sectionTitle: SEC4 });
  s.addText("DEMO · 5–6 PHÚT", { placeholder: "eyebrow", bullet: false });
  s.addText("Bản chứng minh ý tưởng: phần di chuyển", { placeholder: "title" });
  chip(s, "#3", null, "#3 + #2 · 5–6'");
  const panes = [["Client A", "dự đoán + đối chiếu"], ["Server", "người quyết định"], ["Client B", "nội suy ô của A"]];
  panes.forEach(([name, cap], i) => {
    const x = 0.5 + i * 3.1;
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x, y: 1.35, w: 2.8, h: 1.75, rectRadius: 0.08, fill: { color: C.text1 }, line: { color: C.accent6, width: 1 }, objectName: `${name} pane` });
    s.addText(name, { x: x + 0.2, y: 1.45, w: 2.4, h: 0.3, fontSize: 13, bold: true, color: C.accent6, margin: 0, isTextBox: true });
    s.addShape(pres.shapes.RECTANGLE, { x: x + 0.55 + i * 0.15, y: 2.0, w: 0.42, h: 0.42, fill: { color: C.accent1 }, line: { color: C.accent1 }, objectName: "Square A" });
    s.addShape(pres.shapes.RECTANGLE, { x: x + 1.75, y: 2.15 - i * 0.05, w: 0.42, h: 0.42, fill: { color: C.accent3 }, line: { color: C.accent3 }, objectName: "Square B" });
    s.addText(cap, { x: x + 0.2, y: 2.68, w: 2.4, h: 0.3, fontSize: 11, italic: true, color: C.background1, margin: 0, isTextBox: true });
  });
  s.addText("THANH TRƯỢT", { x: 0.5, y: 3.33, w: 1.3, h: 0.34, fontSize: 10, bold: true, color: C.accent6, charSpacing: 1.5, valign: "middle", margin: 0, isTextBox: true });
  let sx = 1.85;
  [["Độ trễ", 1.0], ["Dao động độ trễ", 1.55], ["Mất gói", 1.0], ["Tần số gửi ảnh", 1.5]].forEach(([t, w]) => {
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: sx, y: 3.33, w, h: 0.34, rectRadius: 0.17, fill: { color: C.text2 }, line: { color: C.accent6, width: 0.75 }, objectName: "Slider chip" });
    s.addText(t, { x: sx, y: 3.33, w, h: 0.34, fontSize: 11, color: C.background1, align: "center", valign: "middle", margin: 0, isTextBox: true });
    sx += w + 0.12;
  });
  const scenes = ["Client tự quyết", "Server quyết định", "Bật dự đoán", "Bật đối chiếu", "Ảnh thưa, chưa nội suy", "Bật nội suy"];
  scenes.forEach((t, i) => {
    const x = 0.5 + i * 1.52;
    s.addShape(pres.shapes.OVAL, { x, y: 3.95, w: 0.36, h: 0.36, fill: { color: C.accent1 }, line: { color: C.accent1 }, objectName: "Scene number" });
    s.addText(String(i), { x, y: 3.95, w: 0.36, h: 0.36, fontSize: 12, bold: true, color: C.background1, align: "center", valign: "middle", margin: 0, isTextBox: true });
    s.addText(t, { x, y: 4.38, w: 1.42, h: 0.55, fontSize: 11, color: C.background1, valign: "top", margin: 0, isTextBox: true, objectName: "Scene label" });
  });
  s.addNotes(`Người trình bày: #3 dẫn, #2 điều khiển client B · Thời gian: 5–6' · Cộng dồn: 17–18'

Màn hình chia 3 khung: Client A · Server · Client B. Thanh trượt chỉnh độ trễ, dao động độ trễ, mất gói, tần số gửi ảnh.

Cảnh 0 — Client tự quyết, trễ 100 ms. Thao tác: bấm nút "dịch chuyển".
"Đây là cách client tự quyết. Em giả lập sửa gói tin, ô của A nhảy sang đây, và server lẫn B đều chấp nhận."

Cảnh 1 — Server quyết định, trễ 200 ms, tắt hết. Thao tác: A chạy qua lại; bấm lại "dịch chuyển".
"Chuyển sang server quyết định: dịch chuyển hết tác dụng. Nhưng thầy để ý, em bấm phím khoảng 0,2 giây sau ô mới chạy."

Cảnh 2 — Bật dự đoán. Thao tác: nhấp nhanh D, rồi giữ D chạy và thả.
"Bật dự đoán: phản hồi ngay. Em nhấp nhanh phím D, thầy thấy ô nhảy lên rồi bị kéo về chỗ cũ, một lúc sau mới tự đi tới. Đó là giật ngược: ảnh trạng thái từ quá khứ vẫn báo vị trí cũ, client ghi đè theo. Chạy liên tục thì ô rung theo từng ảnh, con số kéo lệch ở dưới khác 0. Còn khi thả phím, ô trôi thêm một đoạn, vì các lệnh đang trên đường vẫn tới server."

Cảnh 3 — Bật đối chiếu. Thao tác: lặp lại.
"Bật đối chiếu: mượt và khớp với bóng xám là vị trí theo server. Bộ đệm đang có khoảng 6 lệnh, đúng bằng RTT chia nhịp."

Cảnh 4 — Nhìn khung B; gửi ảnh 10 lần/giây, dao động 50 ms. Thao tác: A chạy vòng tròn.
"Giờ nhìn sang màn hình của B: ô của A giật từng nấc vì ảnh trạng thái đến thưa và không đều."

Cảnh 5 — Bật nội suy (độ trễ nội suy tự tính ≈ 250 ms); sau đó tăng lên 20 lần/giây. Thao tác: lặp lại.
"Bật nội suy: mượt, nhưng so với khung server thì ô của A trễ hơn, đó là đánh đổi. Tăng tần số gửi ảnh lên 20 thì độ trễ giảm còn khoảng 150 mili giây."

PHƯƠNG ÁN DỰ PHÒNG: quay sẵn video toàn bộ 6 cảnh. Demo lỗi thì chuyển sang video ngay, không sửa lỗi trên sân khấu.`);
}

// ---- 15. Q&A ----
{
  const s = pres.addSlide({ masterName: "SECTION_DARK", sectionTitle: SEC4 });
  s.addText("HỎI & ĐÁP · 3–5 PHÚT", { placeholder: "eyebrow", bullet: false });
  s.addText("Cảm ơn thầy và các bạn", { placeholder: "title" });
  s.addText("Nhóm T47 sẵn sàng trả lời câu hỏi", { placeholder: "subtitle", bullet: false });
  s.addText("Mỗi người trả lời các câu thuộc vấn đề con của mình", { placeholder: "meta", bullet: false });
  network(s, 6.55, 1.15, 1);
  s.addNotes(`Người trình bày: cả nhóm · Thời gian: 3–5' · Cộng dồn: 20–23'

Bị hỏi sâu thì mở slide dự phòng D1–D10 (phía sau slide này). Câu trả lời mẫu: NoiDung_ChiTiet.md, phụ lục A. Bảng thuật ngữ Việt – Anh: phụ lục B.`);
}

// ---- 16+. Backup slides ----
pres.addSection({ title: SEC5 });
{
  const s = pres.addSlide({ masterName: "SECTION_DARK", sectionTitle: SEC5 });
  s.addText("PHỤ LỤC · KHÔNG TRÌNH BÀY", { placeholder: "eyebrow", bullet: false });
  s.addText("Slide dự phòng cho Q&A", { placeholder: "title" });
  s.addText("Chỉ mở khi bị hỏi", { placeholder: "subtitle", bullet: false });
  s.addText("Nội dung lấy từ NoiDung_ChiTiet.md. Câu trả lời mẫu ở phụ lục A, thuật ngữ Việt – Anh ở phụ lục B.", { placeholder: "meta", bullet: false });
  network(s, 6.55, 1.15, 1);
  s.addNotes("Các slide D1–D10 là khung: điền nội dung từ NoiDung_ChiTiet.md theo mục ghi trên từng slide.");
}
const BACKUP = [
  ["D1", "Thành phần của độ trễ", "lan truyền, truyền, xếp hàng, xử lý", "Độ trễ đến từ đâu, giảm được không?", "Mục 2.1"],
  ["D2", "TCP chi tiết", "thời gian chờ gửi lại, thuật toán Nagle", "Sao không dùng TCP cho tất cả, tắt Nagle là xong?", "Mục 2.3"],
  ["D3", "Bộ tích luỹ thời gian", "tần số mô phỏng và tần số gửi ảnh", "Chọn 30 nhịp/giây thế nào? Client 144 Hz thì sao?", "VĐ1"],
  ["D4", "Hàng đợi lệnh ở server", "thiếu lệnh, dồn lệnh, chống hack tốc độ", "Lệnh đến không đều thì server làm gì?", "VĐ1"],
  ["D5", "Bảng giao thức", "`INPUT`, `SNAPSHOT`, `ITEM_*`", "Định dạng gói tin thế nào?", "Mục 5"],
  ["D6", "Chọn độ trễ nội suy", "bảng theo tần số gửi ảnh, ngoại suy dự phòng", "Độ trễ nội suy chọn bao nhiêu? Mất nhiều gói thì sao?", "VĐ3"],
  ["D7", "Phân xử món hàng hiếm", "cửa sổ phân xử, kiểm tra số nhịp giả", "Hai người cùng nhặt thì xử thế nào? Gửi nhịp giả thì sao?", "VĐ4"],
  ["D8", "Các nguồn phá tính tất định", "số thực, thứ tự duyệt, số ngẫu nhiên", "Tính tất định ảnh hưởng thế nào?", "VĐ5"],
  ["D9", "Đồng bộ theo bước / GGPO vs mô hình của nhóm", "so sánh hai mô hình", "Sao không làm ngang hàng?", "VĐ1, VĐ5"],
  ["D10", "Mạng giả lập", "`ITransport`, cách tạo độ trễ, mất gói", "Demo có phản ánh mạng thật không?", "Mục 7.2"],
];
BACKUP.forEach(([id, title, scope, q, src]) => {
  const s = content(SEC5, `DỰ PHÒNG ${id} · NGUỒN: ${src.toUpperCase()}`, title, "Q&A");
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.5, y: 1.35, w: 9.0, h: 0.8, rectRadius: 0.1, fill: { color: C.text1 }, line: { color: C.text1 }, objectName: "Trigger question" });
  s.addText("KHI BỊ HỎI", { x: 0.75, y: 1.35, w: 1.3, h: 0.8, fontSize: 10, bold: true, color: C.accent6, charSpacing: 1.5, valign: "middle", margin: 0, isTextBox: true });
  s.addText(`“${q}”`, { x: 2.05, y: 1.35, w: 7.2, h: 0.8, fontSize: 16, italic: true, color: C.background1, valign: "middle", margin: 0, isTextBox: true });
  figure(s, { x: 0.5, y: 2.4, w: 9.0, h: 2.6 }, [
    `Phạm vi: ${scope}`,
    `Lấy từ NoiDung_ChiTiet.md, ${src}. Câu trả lời mẫu: phụ lục A.`,
  ], "NỘI DUNG CẦN ĐIỀN");
  s.addNotes(`Slide dự phòng ${id}: chỉ mở khi bị hỏi “${q}”. Nguồn: NoiDung_ChiTiet.md, ${src}; câu trả lời mẫu ở phụ lục A.`);
});

(async () => {
  await pres.writeFile({ fileName: OUT });
  await applyTheme(OUT, THEME);
  console.log("wrote", OUT);
})();
