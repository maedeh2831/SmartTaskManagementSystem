const fs = require('fs');
const path = require('path');
const { Document, Packer, Paragraph, TextRun, HeadingLevel, Table, TableCell, TableRow, WidthType, BorderStyle, VerticalAlign, AlignmentType, PageBreak, UnorderedList, ListLevel } = require('docx');

// خواندن فایل markdown
const markdownPath = path.join(__dirname, 'SmartTask-Documentation.md');
const markdownContent = fs.readFileSync(markdownPath, 'utf-8');

// جداسازی خطوط
const lines = markdownContent.split('\n');

// تبدیل markdown به docx elements
const elements = [];

let currentHeadingLevel = 0;
let inList = false;
let listItems = [];

for (let i = 0; i < lines.length; i++) {
  const line = lines[i];

  // تیتل اصلی
  if (line.startsWith('# ') && !line.startsWith('## ')) {
    // اگر در لیست بودیم، آن را بپایان ببرید
    if (inList && listItems.length > 0) {
      elements.push(...createListParagraphs(listItems));
      listItems = [];
      inList = false;
    }

    const title = line.replace(/^# /, '').trim();
    elements.push(
      new Paragraph({
        text: title,
        heading: HeadingLevel.HEADING_1,
        themeColor: 'accent1',
        bold: true,
        size: 28 * 2,
        spacing: { after: 200, before: 400 }
      })
    );
  }

  // تیتل دوم
  else if (line.startsWith('## ')) {
    // اگر در لیست بودیم، آن را بپایان ببرید
    if (inList && listItems.length > 0) {
      elements.push(...createListParagraphs(listItems));
      listItems = [];
      inList = false;
    }

    const title = line.replace(/^## /, '').trim();
    elements.push(
      new Paragraph({
        text: title,
        heading: HeadingLevel.HEADING_2,
        bold: true,
        size: 24 * 2,
        spacing: { after: 120, before: 240 },
        border: {
          bottom: {
            color: '4472C4',
            space: 1,
            style: BorderStyle.SINGLE,
            size: 6
          }
        }
      })
    );
  }

  // تیتل سوم
  else if (line.startsWith('### ')) {
    // اگر در لیست بودیم، آن را بپایان ببرید
    if (inList && listItems.length > 0) {
      elements.push(...createListParagraphs(listItems));
      listItems = [];
      inList = false;
    }

    const title = line.replace(/^### /, '').trim();
    elements.push(
      new Paragraph({
        text: title,
        heading: HeadingLevel.HEADING_3,
        bold: true,
        size: 20 * 2,
        spacing: { after: 100, before: 200 },
        color: '4472C4'
      })
    );
  }

  // لیست‌های نقطه‌ای
  else if (line.trim().startsWith('- ') || line.trim().startsWith('* ')) {
    inList = true;
    const text = line.trim().replace(/^[-*] /, '').trim();
    if (text) {
      listItems.push(text);
    }
  }

  // تاریخ جداکننده
  else if (line.includes('========')) {
    elements.push(new Paragraph(''));
  }

  // متن خالی
  else if (line.trim() === '') {
    if (inList && listItems.length > 0) {
      elements.push(...createListParagraphs(listItems));
      listItems = [];
      inList = false;
    }
    if (elements.length > 0 && elements[elements.length - 1].text !== '') {
      elements.push(new Paragraph(''));
    }
  }

  // متن عادی
  else if (line.trim()) {
    if (inList && listItems.length > 0) {
      // متن عادی یعنی لیست تمام شد
      elements.push(...createListParagraphs(listItems));
      listItems = [];
      inList = false;
    }

    // بررسی کد‌های فارسی و فرمت خاص
    if (line.includes('```')) {
      elements.push(
        new Paragraph({
          text: line,
          style: 'Code',
          border: {
            top: { color: 'D9D9D9', space: 1, style: BorderStyle.SINGLE, size: 6 },
            bottom: { color: 'D9D9D9', space: 1, style: BorderStyle.SINGLE, size: 6 },
            left: { color: 'D9D9D9', space: 1, style: BorderStyle.SINGLE, size: 12 },
            right: { color: 'D9D9D9', space: 1, style: BorderStyle.SINGLE, size: 12 }
          },
          shading: { type: 'clear', fill: 'F2F2F2' },
          spacing: { before: 100, after: 100 }
        })
      );
    } else {
      elements.push(
        new Paragraph({
          text: line,
          size: 22,
          spacing: { line: 360, lineRule: 'auto', after: 100 },
          alignment: AlignmentType.RIGHT
        })
      );
    }
  }
}

// اگر در انتهای فایل در لیست بودیم
if (inList && listItems.length > 0) {
  elements.push(...createListParagraphs(listItems));
}

// ایجاد document
const doc = new Document({
  sections: [{
    properties: {
      page: {
        margins: {
          top: 1440,
          right: 1440,
          bottom: 1440,
          left: 1440
        }
      }
    },
    children: elements
  }]
});

// ذخیره فایل
const outputPath = path.join(__dirname, 'SmartTask-جزوه-کامل.docx');
Packer.toBuffer(doc).then(buffer => {
  fs.writeFileSync(outputPath, buffer);
  console.log(`✅ فایل Word با موفقیت ایجاد شد: ${outputPath}`);
  console.log(`📊 تعداد عناصر: ${elements.length}`);
});

// تابع کمکی برای ایجاد لیست
function createListParagraphs(items) {
  return items.map((item, index) =>
    new Paragraph({
      text: item,
      bullet: {
        level: 0
      },
      size: 22,
      spacing: { line: 360, lineRule: 'auto', after: 60 },
      alignment: AlignmentType.RIGHT
    })
  );
}
