"""
ساخت PDF فارسی (RTL) از جزوه‌های مارک‌داون با reportlab.

چرا reportlab و نه مرورگر؟
حالت headless مرورگر Edge روی این سیستم غیرقابل‌اعتماد بود: با کد خروج ۰
تمام می‌شد ولی فایلی نمی‌ساخت. این مسیر قطعی و تکرارپذیر است.

متن فارسی پیش از رسم، reshape و سپس با الگوریتم bidi به ترتیب بصری
تبدیل می‌شود، چون reportlab خودش چیدمان پیچیده متن را انجام نمی‌دهد.
"""
import re
import sys
from pathlib import Path

import arabic_reshaper
from bidi.algorithm import get_display
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_JUSTIFY, TA_LEFT, TA_RIGHT
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import cm
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    HRFlowable,
    KeepTogether,
    PageBreak,
    Paragraph,
    Preformatted,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")

BASE_DIR = Path(r"E:\taskManager\SmartTaskManagementSystem\SmartTask Learn")
FONT_DIR = BASE_DIR / "fonts"

NAVY = colors.HexColor("#1E3A8A")
BLUE = colors.HexColor("#2563EB")
SLATE = colors.HexColor("#334155")
GREY = colors.HexColor("#64748B")
BORDER = colors.HexColor("#CBD5E1")
ZEBRA = colors.HexColor("#F1F5F9")
CODE_BG = colors.HexColor("#0F172A")
CODE_FG = colors.HexColor("#E2E8F0")

PERSIAN_RE = re.compile(r"[\u0600-\u06FF]")


def register_fonts() -> tuple[str, str]:
    """ثبت فونت وزیرمتن؛ در صورت نبودن، بازگشت به Tahoma."""
    regular = FONT_DIR / "Vazirmatn-Regular.ttf"
    bold = FONT_DIR / "Vazirmatn-Bold.ttf"

    if regular.exists() and bold.exists():
        pdfmetrics.registerFont(TTFont("Vazir", str(regular)))
        pdfmetrics.registerFont(TTFont("Vazir-Bold", str(bold)))
        body, strong = "Vazir", "Vazir-Bold"
    else:
        pdfmetrics.registerFont(TTFont("Vazir", r"C:\Windows\Fonts\tahoma.ttf"))
        pdfmetrics.registerFont(TTFont("Vazir-Bold", r"C:\Windows\Fonts\tahomabd.ttf"))
        body, strong = "Vazir", "Vazir-Bold"

    pdfmetrics.registerFont(TTFont("Mono", r"C:\Windows\Fonts\consola.ttf"))
    return body, strong


def shape(text: str) -> str:
    """آماده‌سازی متن فارسی برای رسم: reshape + ترتیب بصری."""
    if not text:
        return ""
    if not PERSIAN_RE.search(text):
        return text
    return get_display(arabic_reshaper.reshape(text))


def esc(text: str) -> str:
    return text.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")


def inline(text: str, strong_font: str) -> str:
    """
    تبدیل نشانه‌گذاری درون‌خطی به مارک‌آپ reportlab.

    نکته مهم: نمی‌توان از placeholder داخل متن استفاده کرد، چون الگوریتم
    bidi کاراکترهای کنترلی را حذف/جابه‌جا می‌کند و شماره‌ها لو می‌روند.
    پس متن به قطعه‌های مستقل شکسته می‌شود، هر قطعه جدا reshape می‌شود و
    برای RTL ترتیب قطعه‌ها معکوس می‌گردد.
    """
    token_re = re.compile(
        r"`([^`]+)`"                 # کد درون‌خطی
        r"|\*\*(.+?)\*\*"            # بولد
        r"|\[(.+?)\]\((?:.+?)\)"     # لینک
    )

    pieces: list[str] = []
    pos = 0
    for m in token_re.finditer(text):
        if m.start() > pos:
            pieces.append(("plain", text[pos:m.start()]))
        if m.group(1) is not None:
            pieces.append(("code", m.group(1)))
        elif m.group(2) is not None:
            pieces.append(("bold", m.group(2)))
        else:
            pieces.append(("link", m.group(3)))
        pos = m.end()
    if pos < len(text):
        pieces.append(("plain", text[pos:]))

    if not pieces:
        pieces = [("plain", text)]

    rendered: list[str] = []
    for kind, value in pieces:
        if kind == "code":
            rendered.append(f"<font name='Mono' size='8.5'>{esc(value)}</font>")
        elif kind == "bold":
            rendered.append(f"<font name='{strong_font}'>{shape(esc(value))}</font>")
        elif kind == "link":
            rendered.append(f"<font color='#2563EB'>{shape(esc(value))}</font>")
        else:
            rendered.append(shape(esc(value)))

    # در متن راست‌به‌چپ ترتیب بصری قطعه‌ها برعکس ترتیب منطقی است
    if PERSIAN_RE.search(text):
        rendered.reverse()

    return "".join(rendered)


def build_pdf(md_path: Path, pdf_path: Path, title: str) -> bool:
    body_font, strong_font = register_fonts()

    styles = {
        "h1": ParagraphStyle("h1", fontName=strong_font, fontSize=19, leading=26,
                             alignment=TA_RIGHT, textColor=NAVY, spaceBefore=16, spaceAfter=10),
        "h2": ParagraphStyle("h2", fontName=strong_font, fontSize=15, leading=22,
                             alignment=TA_RIGHT, textColor=NAVY, spaceBefore=14, spaceAfter=8),
        "h3": ParagraphStyle("h3", fontName=strong_font, fontSize=12.5, leading=19,
                             alignment=TA_RIGHT, textColor=SLATE, spaceBefore=11, spaceAfter=6),
        "h4": ParagraphStyle("h4", fontName=strong_font, fontSize=11.5, leading=18,
                             alignment=TA_RIGHT, textColor=SLATE, spaceBefore=9, spaceAfter=5),
        "body": ParagraphStyle("body", fontName=body_font, fontSize=10.5, leading=19,
                               alignment=TA_JUSTIFY, textColor=colors.HexColor("#1F2937"),
                               spaceAfter=6, wordWrap="RTL"),
        "li": ParagraphStyle("li", fontName=body_font, fontSize=10.5, leading=18,
                             alignment=TA_RIGHT, rightIndent=18, spaceAfter=3, wordWrap="RTL"),
        "quote": ParagraphStyle("quote", fontName=body_font, fontSize=10, leading=18,
                                alignment=TA_RIGHT, textColor=GREY, rightIndent=16,
                                borderColor=BLUE, borderWidth=0, spaceAfter=6, wordWrap="RTL"),
        "cell": ParagraphStyle("cell", fontName=body_font, fontSize=9, leading=14,
                               alignment=TA_RIGHT, wordWrap="RTL"),
        "cellh": ParagraphStyle("cellh", fontName=strong_font, fontSize=9, leading=14,
                                alignment=TA_RIGHT, textColor=colors.white, wordWrap="RTL"),
        "code": ParagraphStyle("code", fontName="Mono", fontSize=8, leading=11.5,
                               alignment=TA_LEFT, textColor=CODE_FG),
        "cover": ParagraphStyle("cover", fontName=strong_font, fontSize=26, leading=38,
                                alignment=TA_CENTER, textColor=NAVY),
        "coversub": ParagraphStyle("coversub", fontName=body_font, fontSize=12, leading=22,
                                   alignment=TA_CENTER, textColor=GREY),
    }

    doc = SimpleDocTemplate(
        str(pdf_path), pagesize=A4,
        rightMargin=1.7 * cm, leftMargin=1.7 * cm,
        topMargin=1.8 * cm, bottomMargin=1.8 * cm,
        title=title, author="SmartTask",
    )

    story: list = [
        Spacer(1, 6 * cm),
        Paragraph(shape(title), styles["cover"]),
        Spacer(1, 1 * cm),
        Paragraph(shape("سامانه مدیریت هوشمند وظایف چابک"), styles["coversub"]),
        Paragraph(shape("پروژه کارشناسی — نسخه ۱.۰"), styles["coversub"]),
        PageBreak(),
    ]

    lines = md_path.read_text(encoding="utf-8").split("\n")
    i = 0
    in_code = False
    code_buf: list[str] = []

    while i < len(lines):
        raw = lines[i]
        s = raw.strip()

        # کدبلاک — همیشه چپ‌چین و بدون reshape
        if s.startswith("```"):
            if in_code:
                if code_buf:
                    text = "\n".join(code_buf)
                    block = Preformatted(text, styles["code"])
                    tbl = Table([[block]], colWidths=[doc.width])
                    tbl.setStyle(TableStyle([
                        ("BACKGROUND", (0, 0), (-1, -1), CODE_BG),
                        ("LEFTPADDING", (0, 0), (-1, -1), 9),
                        ("RIGHTPADDING", (0, 0), (-1, -1), 9),
                        ("TOPPADDING", (0, 0), (-1, -1), 8),
                        ("BOTTOMPADDING", (0, 0), (-1, -1), 8),
                        ("ROUNDEDCORNERS", [4, 4, 4, 4]),
                    ]))
                    story.extend([tbl, Spacer(1, 8)])
                code_buf = []
                in_code = False
            else:
                in_code = True
            i += 1
            continue

        if in_code:
            code_buf.append(raw.replace("\t", "    "))
            i += 1
            continue

        # جدول
        if s.startswith("|") and i + 1 < len(lines) and re.match(r"^\|[\s\-:|]+\|$", lines[i + 1].strip()):
            headers = [c.strip() for c in s.strip("|").split("|")]
            rows = []
            j = i + 2
            while j < len(lines) and lines[j].strip().startswith("|"):
                rows.append([c.strip() for c in lines[j].strip().strip("|").split("|")])
                j += 1

            ncols = len(headers)
            # ستون‌ها برای RTL معکوس می‌شوند تا ستون اول سمت راست بیفتد
            head_cells = [Paragraph(inline(h, strong_font), styles["cellh"]) for h in reversed(headers)]
            data = [head_cells]
            for row in rows:
                padded = (row + [""] * ncols)[:ncols]
                data.append([Paragraph(inline(c, strong_font), styles["cell"]) for c in reversed(padded)])

            col_w = doc.width / ncols
            tbl = Table(data, colWidths=[col_w] * ncols, repeatRows=1)
            style = [
                ("BACKGROUND", (0, 0), (-1, 0), BLUE),
                ("GRID", (0, 0), (-1, -1), 0.5, BORDER),
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 6),
                ("RIGHTPADDING", (0, 0), (-1, -1), 6),
                ("TOPPADDING", (0, 0), (-1, -1), 5),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 5),
            ]
            for r in range(1, len(data)):
                if r % 2 == 0:
                    style.append(("BACKGROUND", (0, r), (-1, r), ZEBRA))
            tbl.setStyle(TableStyle(style))
            story.extend([tbl, Spacer(1, 10)])
            i = j
            continue

        if not s:
            i += 1
            continue

        # خط جداکننده
        if re.match(r"^(-{3,}|\*{3,}|_{3,})$", s):
            story.extend([Spacer(1, 4), HRFlowable(width="100%", color=BORDER, thickness=0.6), Spacer(1, 8)])
            i += 1
            continue

        # سرتیتر
        h = re.match(r"^(#{1,6})\s+(.*)$", s)
        if h:
            level = min(len(h.group(1)), 4)
            para = Paragraph(inline(h.group(2), strong_font), styles[f"h{level}"])
            story.append(para if level > 2 else KeepTogether([para]))
            i += 1
            continue

        if s.startswith(">"):
            story.append(Paragraph(inline(s.lstrip("> "), strong_font), styles["quote"]))
            i += 1
            continue

        # فهرست‌ها — گلوله در سمت راست قرار می‌گیرد
        bullet = re.match(r"^[-*+]\s+(.*)$", s)
        if bullet:
            story.append(Paragraph(inline(bullet.group(1), strong_font) + " •", styles["li"]))
            i += 1
            continue

        num = re.match(r"^(\d+)[.)]\s+(.*)$", s)
        if num:
            story.append(Paragraph(inline(num.group(2), strong_font) + f" .{num.group(1)}", styles["li"]))
            i += 1
            continue

        story.append(Paragraph(inline(s, strong_font), styles["body"]))
        i += 1

    def footer(canvas, _doc):
        canvas.saveState()
        canvas.setFont(body_font, 8.5)
        canvas.setFillColor(GREY)
        canvas.drawCentredString(A4[0] / 2, 1.1 * cm, str(canvas.getPageNumber()))
        canvas.restoreState()

    doc.build(story, onFirstPage=footer, onLaterPages=footer)
    return pdf_path.exists() and pdf_path.stat().st_size > 5000


DOCUMENTS = [
    ("جزوه 3 - سیستم گیمیفیکیشن", "جزوه ۳ — سیستم گیمیفیکیشن"),
    ("راهنمای بهره‌وری و شبیه‌سازی", "راهنمای بهره‌وری و شبیه‌سازی پروژه"),
]


def main() -> int:
    failures = 0
    for stem, title in DOCUMENTS:
        md_path = BASE_DIR / f"{stem}.md"
        if not md_path.exists():
            print(f"[!] پیدا نشد: {md_path.name}")
            failures += 1
            continue

        pdf_path = BASE_DIR / f"{stem}.pdf"
        try:
            if build_pdf(md_path, pdf_path, title):
                print(f"[ok] PDF -> {pdf_path.name} ({pdf_path.stat().st_size // 1024} KB)")
            else:
                print(f"[fail] PDF ساخته نشد: {stem}")
                failures += 1
        except Exception as exc:  # noqa: BLE001
            print(f"[error] {stem}: {type(exc).__name__}: {exc}")
            failures += 1

    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())
