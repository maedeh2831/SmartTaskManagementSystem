const { Document, Packer, Paragraph, TextRun, AlignmentType, HeadingLevel, BorderStyle, Table, TableRow, TableCell, WidthType, TableBorders, ShadingType, Header, Footer, PageNumber, NumberFormat } = require('docx');
const fs = require('fs');

// ========== Helper Functions ==========
function heading(text, level = HeadingLevel.HEADING_1) {
  return new Paragraph({ text, heading: level, spacing: { before: 300, after: 150 }, alignment: AlignmentType.RIGHT, direction: 'rtl' });
}

function para(text, opts = {}) {
  return new Paragraph({
    children: [new TextRun({ text, font: 'B Nazanin', size: 28, ...opts })],
    spacing: { after: 120, line: 360 },
    alignment: AlignmentType.RIGHT,
    direction: 'rtl',
  });
}

function boldPara(text) {
  return para(text, { bold: true });
}

function emptyLine() {
  return new Paragraph({ children: [new TextRun({ text: '', font: 'B Nazanin', size: 28 })], spacing: { after: 80 } });
}

function refPara(text) {
  return new Paragraph({
    children: [new TextRun({ text, font: 'Times New Roman', size: 22 })],
    spacing: { after: 60, line: 300 },
    alignment: AlignmentType.LEFT,
    direction: 'ltr',
    indent: { left: 400 },
  });
}

function enPara(text, opts = {}) {
  return new Paragraph({
    children: [new TextRun({ text, font: 'Times New Roman', size: 22, ...opts })],
    spacing: { after: 100, line: 320 },
    alignment: AlignmentType.LEFT,
    direction: 'ltr',
  });
}

// ========== Table Helpers ==========
function headerCell(text) {
  return new TableCell({
    children: [new Paragraph({ children: [new TextRun({ text, font: 'B Nazanin', size: 20, bold: true, color: 'FFFFFF' })], alignment: AlignmentType.CENTER, direction: 'rtl' })],
    shading: { type: ShadingType.SOLID, color: '5B5FEF' },
    verticalAlign: 'center',
  });
}

function cell(text) {
  return new TableCell({
    children: [new Paragraph({ children: [new TextRun({ text, font: 'B Nazanin', size: 20 })], alignment: AlignmentType.CENTER, direction: 'rtl' })],
    verticalAlign: 'center',
  });
}

function makeTable(headers, rows) {
  const table = new Table({
    width: { size: 100, type: WidthType.PERCENTAGE },
    rows: [
      new TableRow({ children: headers.map(h => headerCell(h)), tableHeader: true }),
      ...rows.map(row => new TableRow({ children: row.map(c => cell(c)) })),
    ],
    borders: {
      top: { style: BorderStyle.SINGLE, size: 1, color: 'CCCCCC' },
      bottom: { style: BorderStyle.SINGLE, size: 1, color: 'CCCCCC' },
      left: { style: BorderStyle.SINGLE, size: 1, color: 'CCCCCC' },
      right: { style: BorderStyle.SINGLE, size: 1, color: 'CCCCCC' },
      insideHorizontal: { style: BorderStyle.SINGLE, size: 1, color: 'CCCCCC' },
      insideVertical: { style: BorderStyle.SINGLE, size: 1, color: 'CCCCCC' },
    },
  });
  return table;
}

// ========== Build Document ==========
const doc = new Document({
  styles: {
    default: {
      document: { run: { font: 'B Nazanin', size: 28 } },
    },
  },
  sections: [{
    properties: {
      page: {
        margin: { top: 1440, bottom: 1440, left: 1440, right: 1440 },
      },
    },
    children: [
      // ===== TITLE PAGE =====
      emptyLine(), emptyLine(), emptyLine(),
      new Paragraph({ children: [new TextRun({ text: 'دانشگاه فنی و حرفه‌ای', font: 'B Nazanin', size: 32, bold: true })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      new Paragraph({ children: [new TextRun({ text: 'دانشکده فنی و حرفه‌ای دختران دکتر شریعتی', font: 'B Nazanin', size: 28 })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      emptyLine(), emptyLine(),
      new Paragraph({ children: [new TextRun({ text: 'پایان‌نامه کارشناسی ناپیوسته', font: 'B Nazanin', size: 30, bold: true })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      new Paragraph({ children: [new TextRun({ text: 'رشته مهندسی حرفه‌ای کامپیوتر — گرایش نرم‌افزار', font: 'B Nazanin', size: 28 })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      emptyLine(), emptyLine(),
      new Paragraph({ children: [new TextRun({ text: 'عنوان پروژه', font: 'B Nazanin', size: 30, bold: true })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      emptyLine(),
      new Paragraph({ children: [new TextRun({ text: 'طراحی و پیاده‌سازی سامانه مدیریت پروژه چابک با رویکرد تصمیم‌یار مبتنی بر تحلیل داده‌های پروژه', font: 'B Nazanin', size: 32, bold: true, color: '5B5FEF' })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      emptyLine(), emptyLine(),
      new Paragraph({ children: [new TextRun({ text: 'نگارش: مائده اسلامی', font: 'B Nazanin', size: 28 })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      new Paragraph({ children: [new TextRun({ text: 'استاد راهنما: دکتر ستاره امیری', font: 'B Nazanin', size: 28 })], alignment: AlignmentType.CENTER, direction: 'rtl' }),
      emptyLine(), emptyLine(),
      new Paragraph({ children: [new TextRun({ text: 'تابستان ۱۴۰۵', font: 'B Nazanin', size: 28 })], alignment: AlignmentType.CENTER, direction: 'rtl' }),

      // ===== PAGE BREAK =====
      new Paragraph({ children: [new TextRun({ text: '', font: 'B Nazanin', size: 28 })], pageBreakBefore: true }),

      // ===== ABSTRACT =====
      heading('چکیده', HeadingLevel.HEADING_1),
      para('مدیریت پروژه‌های نرم‌افزاری به دلیل تعدد فعالیت‌ها، محدودیت منابع انسانی، وابستگی میان وظایف و ضرورت تصمیم‌گیری مستمر، فرایندی پیچیده و پویاست. با وجود توسعه ابزارهای متنوع مدیریت پروژه، یکپارچه‌سازی اطلاعات مربوط به زمان‌بندی، ظرفیت منابع، وابستگی فعالیت‌ها، ریسک و وضعیت کلی پروژه برای پشتیبانی از تصمیم‌گیری مدیریتی، موضوعی قابل توجه است.'),
      para('در این پژوهش، سامانه SmartTask با هدف طراحی و پیاده‌سازی یک سامانه مدیریت پروژه چابک با رویکرد تصمیم‌یار ارائه می‌شود. ساختار سلسله‌مراتبی سامانه شامل Workspace، Project، Sprint، User Story و Task است و علاوه بر قابلیت‌های متداول مدیریت پروژه، حوزه‌های تحلیل بارکاری اعضای تیم، مدیریت و تحلیل وابستگی فعالیت‌ها، اولویت‌بندی چندعاملی، تحلیل ریسک تأخیر و ارزیابی سلامت پروژه را پوشش می‌دهد.'),
      para('همچنین از مدل‌های زبانی بزرگ برای تفکیک فعالیت‌های پیچیده و تولید گزارش اسپرینت استفاده شده است. رویکرد پروژه در استفاده از هوش مصنوعی، جایگزین‌کردن تصمیم‌گیرنده انسانی نیست.'),
      emptyLine(),
      boldPara('واژگان کلیدی: مدیریت پروژه نرم‌افزاری، مدیریت پروژه چابک، تصمیم‌یار، تحلیل بارکاری، وابستگی وظایف، اولویت‌بندی چندعاملی، تحلیل ریسک تأخیر، سلامت پروژه، مدل‌های زبانی بزرگ.'),

      // ===== PAGE BREAK =====
      new Paragraph({ children: [new TextRun({ text: '', font: 'B Nazanin', size: 28 })], pageBreakBefore: true }),

      // ===== CHAPTER 1 =====
      heading('فصل ۱: مقدمه'),

      // 1-1 Motivation
      heading('۱-۱. انگیزه', HeadingLevel.HEADING_2),
      para('پروژه‌های نرم‌افزاری در محیطی اجرا می‌شوند که مجموعه‌ای از فعالیت‌ها، منابع انسانی، محدودیت‌های زمانی و تغییرات محیطی باید به‌صورت هماهنگ مدیریت شوند. Morcov و همکاران در یک مرور نظام‌مند، پیچیدگی پروژه‌های فناوری اطلاعات را مفهومی چندبعدی معرفی کرده و نشان داده‌اند که شناخت پیچیدگی پروژه برای مدیریت مؤثر آن اهمیت دارد [8]. Butler و همکاران نیز پیچیدگی و پویایی پروژه را از ویژگی‌های مؤثر بر نتایج آن معرفی کرده‌اند [1].'),
      para('یکی از پیامدهای این پیچیدگی، دشواری مدیریت فعالیت‌ها و منابع انسانی است. تصمیم درباره اینکه چه فعالیتی، در چه زمانی، با چه اولویتی و توسط چه فردی انجام شود، مسئله‌ای چندعاملی است. Fatima و همکاران در مرور نظام‌مند خود نشان داده‌اند که مدل‌های زمان‌بندی ایستا و پویا در ادبیات مورد استفاده قرار گرفته‌اند [4]. تخصیص منابع انسانی نیز مسئله‌ای پیچیده است [7, 10, 15].'),
      para('در کنار منابع انسانی، وابستگی میان فعالیت‌ها عامل مؤثری بر زمان‌بندی پروژه است [5]. اولویت‌بندی فعالیت‌ها با افزایش تعداد وظایف و محدودیت منابع اهمیت بیشتری پیدا می‌کند [2]. مدیریت ریسک نیز در پروژه‌های چابک باید به‌صورت مستمر انجام شود [9, 14].'),
      para('در چنین شرایطی، مفهوم سیستم تصمیم‌یار (Decision Support System) مطرح می‌شود. Power DSSها را سیستم‌هایی برای کمک به تصمیم‌گیرنده از طریق فراهم‌کردن داده و قابلیت‌های تحلیلی معرفی کرده است [11]. Cunha و همکاران نیز تصمیم‌گیری در مدیریت پروژه نرم‌افزاری را پدیده‌ای چندوجهی و نیازمند پشتیبانی بهتر دانسته‌اند [3].'),
      para('بخش دیگری از انگیزه انجام این پروژه از تجربه عملی در محیط توسعه نرم‌افزار شکل گرفته است. در چنین محیطی، مسئله صرفاً ثبت Taskها نیست، بلکه لازم است مشخص شود کدام فعالیت‌ها اهمیت بیشتری دارند، ظرفیت هر عضو تا چه حد درگیر است، تأخیر یک فعالیت چه پیامدی خواهد داشت و وضعیت کلی پروژه در چه شرایطی قرار دارد.'),
      para('بنابراین، انگیزه اصلی SmartTask حرکت از مدیریت صرفاً عملیاتی وظایف به سمت مدیریت تحلیلی و تصمیم‌یار پروژه است؛ به‌گونه‌ای که مدیر پروژه بتواند در کنار تجربه حرفه‌ای خود، اطلاعات ساختاریافته‌تری برای تصمیم‌گیری در اختیار داشته باشد.'),

      // 1-2 Literature Review
      heading('۱-۲. مروری بر پیشینه و کارهای مشابه', HeadingLevel.HEADING_2),
      heading('۱-۲-۱. مدیریت پروژه نرم‌افزاری و رویکرد چابک', HeadingLevel.HEADING_3),
      para('مدیریت پروژه نرم‌افزاری با برنامه‌ریزی، سازمان‌دهی و کنترل فعالیت‌ها و منابع سروکار دارد. Butler و همکاران نشان می‌دهند که انتخاب رویکرد مدیریت باید متناسب با شرایط پروژه باشد [1]. در پروژه‌های چابک، سازگاری با تغییر اهمیت ویژه‌ای دارد. راهنمای رسمی Scrum، آن را چارچوبی برای ایجاد ارزش از طریق راهکارهای سازگارشونده معرفی می‌کند [16].'),
      para('در SmartTask، Agile و Scrum به‌عنوان بستر سازمان‌دهی فعالیت‌ها در نظر گرفته شده‌اند. ساختار سلسله‌مراتبی Workspace → Project → Sprint → Story → Task با مفاهیم Scrum سازگار است.'),

      heading('۱-۲-۲. زمان‌بندی و تخصیص وظایف', HeadingLevel.HEADING_3),
      para('Fatima و همکاران نشان داده‌اند که Software Project Scheduling و Task Assignment موضوعات مهمی در مهندسی نرم‌افزار هستند [4]. Rezende و همکاران مدل‌های پویا و سازگارشونده را زمینه‌های قابل توجه برای تحقیقات آتی معرفی کرده‌اند [13].'),

      heading('۱-۲-۳. تخصیص منابع انسانی و مفهوم Workload', HeadingLevel.HEADING_3),
      para('Otero و همکاران تخصیص منابع را وابسته به قابلیت‌ها و نیازمندی‌های تخصصی دانسته‌اند [10]. Acuña و همکاران تخصیص افراد به نقش‌ها را مسئله‌ای تصمیم‌گیری معرفی کرده‌اند [15]. Workload در SmartTask به وضعیت حجم کار تخصیص‌یافته به منابع در ارتباط با ظرفیت آن‌ها اشاره دارد.'),

      heading('۱-۲-۴. وابستگی فعالیت‌ها و تحلیل اثر آن', HeadingLevel.HEADING_3),
      para('Hartmann و Briskorn روابط تقدم و محدودیت منابع را از اجزای اصلی مدل‌سازی پروژه معرفی کرده‌اند [5]. در SmartTask، مدیریت وابستگی در چند لایه شامل تعریف روابط، جلوگیری از چرخه، تحلیل فعالیت‌های تحت تأثیر، نمایش گراف و بررسی زنجیره‌های پرریسک انجام می‌شود.'),

      heading('۱-۲-۵. اولویت‌بندی وظایف', HeadingLevel.HEADING_3),
      para('Bugayenko و همکاران نشان داده‌اند که Task Prioritization موضوع مهمی در مهندسی نرم‌افزار است [2]. در SmartTask، Smart Priority از ترکیب فوریت زمانی، اثر وابستگی و وضعیت بارکاری استفاده می‌کند.'),

      heading('۱-۲-۶. ریسک تأخیر و مدیریت ریسک پروژه', HeadingLevel.HEADING_3),
      para('Masso و همکاران مدیریت ریسک را بخش مهمی از مدیریت پروژه نرم‌افزاری معرفی کرده‌اند [9]. Tavares و همکاران بر ماهیت مستمر مدیریت ریسک در پروژه‌های Scrum تأکید کرده‌اند [14]. Delay Risk در SmartTask شاخصی تحلیلی است که عوامل تأخیر، بارکاری، وابستگی و Cascade را ترکیب می‌کند.'),

      heading('۱-۲-۷. سلامت و وضعیت کلی پروژه', HeadingLevel.HEADING_3),
      para('Rajagopalan و Srivastava شاخص ترکیبی Project Health Index از ۱۷ معیار ارائه کرده‌اند [12]. در SmartTask، شاخص سلامت از ترکیب چهار بعد زمان‌بندی، بارکاری، وابستگی و تحویل طراحی شده است.'),

      heading('۱-۲-۸. سیستم‌های تصمیم‌یار', HeadingLevel.HEADING_3),
      para('Power DSSها را در قالب سیستم‌های داده‌محور، مدل‌محور و دانش‌محور معرفی کرده است [11]. در SmartTask، تصمیم‌یار به معنای تحلیل داده‌ها و ارائه اطلاعات پشتیبان است، در حالی که تصمیم نهایی در اختیار مدیر باقی می‌ماند.'),

      heading('۱-۲-۹. هوش مصنوعی و مدل‌های زبانی', HeadingLevel.HEADING_3),
      para('Hou و همکاران ۳۹۵ پژوهش درباره LLMها در مهندسی نرم‌افزار را بررسی کرده‌اند [6]. در SmartTask، LLM به‌عنوان ابزار پشتیبان برای تفکیک فعالیت و تولید گزارش Sprint از طریق LM Studio مورد استفاده قرار می‌گیرد.'),

      // 1-2-10 Comparison Table
      heading('۱-۲-۱۰. بررسی تطبیقی سامانه‌های مدیریت پروژه', HeadingLevel.HEADING_3),
      para('بررسی ابزارهای موجود، قابلیت‌های رایج و جایگاه تحلیلی SmartTask را مشخص می‌کند. هدف از این بررسی اثبات برتری نیست، بلکه شناسایی جایگاه تصمیم‌یار این سامانه است.'),
      emptyLine(),
      boldPara('جدول ۱-۲. مقایسه تطبیقی قابلیت‌های اصلی سامانه‌های مدیریت پروژه'),
      emptyLine(),
      makeTable(
        ['قابلیت', 'Jira', 'GitHub', 'Asana', 'Planner', 'Trello', 'Monday', 'SmartTask'],
        [
          ['ساختار سلسله‌مراتبی', '✅', '✅', '✅', '✅', '⚠️', '✅', '✅'],
          ['Sprint', '✅', '✅', '✅', '✅', '❌', '✅', '✅'],
          ['Backlog', '✅', '✅', '✅', '⚠️', '⚠️', '✅', '✅'],
          ['Dependency', '✅', '✅', '✅', '✅', '❌', '✅', '✅+تحلیل'],
          ['Workload', '✅', '⚠️', '✅', '✅', '❌', '✅', '✅+چندسطحی'],
          ['Smart Priority', '⚠️', '❌', '⚠️', '❌', '❌', '⚠️', '✅'],
          ['Delay Risk', '❌', '❌', '❌', '❌', '❌', '❌', '✅'],
          ['Health Score', '❌', '❌', '❌', '❌', '❌', '❌', '✅'],
          ['Cascade', '⚠️', '❌', '❌', '✅', '❌', '❌', '✅'],
          ['AI', '⚠️', '⚠️', '⚠️', '⚠️', '❌', '⚠️', '✅+محلی'],
          ['Decision Support', '❌', '❌', '❌', '❌', '❌', '❌', '✅'],
          ['Chat بلادرنگ', '❌', '❌', '❌', '❌', '❌', '❌', '✅'],
          ['RTL + فارسی', '⚠️', '❌', '⚠️', '❌', '❌', '❌', '✅'],
        ]
      ),
      emptyLine(),
      para('✅ = پیاده‌سازی کامل | ⚠️ = محدود/نیاز به پلاگین | ❌ = وجود ندارد'),
      emptyLine(),

      boldPara('جدول ۱-۳. مقایسه سطح تحلیل و پشتیبانی تصمیم‌گیری'),
      emptyLine(),
      makeTable(
        ['سطح تحلیل', 'Jira', 'Asana', 'Monday', 'SmartTask'],
        [
          ['گزارش وضعیت', '✅', '✅', '✅', '✅'],
          ['تحلیل روند', '✅', '⚠️', '✅', '✅'],
          ['تحلیل ظرفیت', '✅', '✅', '✅', '✅'],
          ['تحلیل وابستگی', '⚠️', '⚠️', '⚠️', '✅'],
          ['اولویت‌بندی خودکار', '❌', '❌', '❌', '✅'],
          ['پیش‌بینی ریسک', '❌', '❌', '❌', '✅'],
          ['شاخص سلامت', '❌', '❌', '❌', '✅'],
          ['کاسکاد خودکار', '⚠️', '❌', '❌', '✅'],
          ['پشتیبان تصمیم', '❌', '❌', '❌', '✅'],
        ]
      ),
      emptyLine(),

      heading('۱-۲-۱۱. تحلیل شکاف و جایگاه پروژه پیشنهادی', HeadingLevel.HEADING_3),
      boldPara('جدول ۱-۴. تحلیل شکاف: وضعیت موجود و جهت‌گیری SmartTask'),
      emptyLine(),
      makeTable(
        ['چالش/نیاز', 'وضعیت موجود', 'جهت‌گیری SmartTask', 'نوع نوآوری'],
        [
          ['اولویت‌بندی', 'Manual در ابزارها', 'موتور چندعامله خودکار', 'نوآورانه'],
          ['ریسک تأخیر', 'عدم وجود', 'Delay Risk Score', 'نوآورانه'],
          ['سلامت پروژه', 'معدود در ادبیات', 'Project Health Index', 'نوآورانه'],
          ['کاسکاد خودکار', 'محدود', 'Background Service', 'نوآورانه'],
          ['تحلیل وابستگی', 'محدود', 'گراف + زنجیره تأثیر', 'ارتقا'],
          ['پشتیبان تصمیم', 'عدم وجود', 'ترکیب داده‌ها', 'نوآورانه'],
          ['AI', 'تحلیلی (غیرمحلی)', 'LLM محلی', 'نوآورانه'],
          ['Workload', 'موجود ولی مجزا', 'ارتباط با Priority/Risk', 'ترکیب'],
        ]
      ),
      emptyLine(),

      heading('۱-۲-۱۲. جمع‌بندی پیشینه', HeadingLevel.HEADING_3),
      para('مسئله SmartTask نه ایجاد یک ابزار دیگر برای ثبت Task، بلکه چگونگی ارتباط دادن اطلاعات عملیاتی مختلف پروژه برای پشتیبانی از تصمیم‌گیری مدیریتی است. تمایز اصلی SmartTask در سه محور اولویت‌بندی چندعاملی، تحلیل ریسک تأخیر و شاخص سلامت پروژه است.'),

      // 1-3 Objectives
      heading('۱-۳. اهداف', HeadingLevel.HEADING_2),
      boldPara('هدف اصلی: طراحی و پیاده‌سازی یک سامانه مدیریت پروژه چابک با رویکرد تصمیم‌یار.'),
      emptyLine(),
      para('اهداف فرعی:'),
      para('۱. مدیریت ساختار سلسله‌مراتبی Project، Sprint، Story و Task.'),
      para('۲. پشتیبانی از Agile و Scrum (Backlog، Sprint Planning، Burndown، Velocity).'),
      para('۳. تحلیل Workload بر اساس ظرفیت و فعالیت‌های تخصیص‌یافته.'),
      para('۴. تحلیل وابستگی فعالیت‌ها، تشخیص چرخه و نمایش گراف Dependency.'),
      para('۵. اولویت‌بندی چندعاملی مبتنی بر فوریت زمانی، اثر وابستگی و بارکاری.'),
      para('۶. تحلیل ریسک تأخیر و شاخص سلامت پروژه.'),
      para('۷. کاسکاد خودکار تأخیر از طریق Background Service.'),
      para('۸. استفاده هدفمند از LLM محلی برای تفکیک فعالیت و تولید گزارش.'),
      para('۹. ایجاد ارتباط داده‌های عملیاتی و خروجی‌های تحلیلی.'),
      para('۱۰. محیط یکپارچه همکاری (Chat، Notification، Time Tracking).'),
      emptyLine(),

      heading('۱-۳-۱. رویکرد ارزیابی', HeadingLevel.HEADING_3),
      emptyLine(),
      makeTable(
        ['قابلیت', 'روش ارزیابی', 'معیار اصلی'],
        [
          ['Priority Engine', 'سناریوهای کنترل‌شده', 'سازگاری امتیاز'],
          ['Workload', 'بررسی صحت محاسبه', 'دقت نسبت بارکاری'],
          ['Dependency', 'بررسی زنجیره و چرخه', 'صحت BFS'],
          ['Delay Risk', 'تغییر ورودی‌ها', 'پایداری امتیاز'],
          ['Project Health', 'شرایط مختلف', 'شفافیت مؤلفه‌ها'],
          ['AI', 'سناریوهای آزمایشی', 'کیفیت خروجی'],
        ]
      ),

      // 1-4 Other Chapters
      heading('۱-۴. رئوس مطالب سایر فصل‌ها', HeadingLevel.HEADING_2),
      para('فصل ۲: تحلیل مسئله و نیازمندی‌ها — کاربران، نقش‌ها، نیازمندی‌های عملکردی و غیرعملکردی.'),
      para('فصل ۳: طراحی سامانه — معماری، مدل موجودیت‌ها، طراحی پایگاه داده، الگوریتم‌ها.'),
      para('فصل ۴: پیاده‌سازی — فناوری‌ها، ساختار کد، پیاده‌سازی قابلیت‌ها.'),
      para('فصل ۵: ارزیابی و نتیجه‌گیری — نتایج، محدودیت‌ها، پیشنهادهای آینده.'),

      // ===== PAGE BREAK =====
      new Paragraph({ children: [new TextRun({ text: '', font: 'B Nazanin', size: 28 })], pageBreakBefore: true }),

      // ===== REFERENCES =====
      heading('منابع', HeadingLevel.HEADING_1),
      refPara('[1] C. W. Butler, L. R. Vijayasarathy, and N. Roberts, "Managing Software Development Projects for Success: Aligning Plan- and Agility-Based Approaches to Project Complexity and Project Dynamism," Project Management Journal, Vol. 51, No. 3, pp. 262–277, 2020.'),
      refPara('[2] Y. Bugayenko et al., "Prioritizing Tasks in Software Development: A Systematic Literature Review," PLOS ONE, Vol. 18, No. 4, e0283838, 2023.'),
      refPara('[3] A. C. R. da Cunha, H. P. de Moura, and A. M. L. de Vasconcellos, "Decision-Making in Software Project Management: A Systematic Literature Review," Procedia Computer Science, Vol. 100, pp. 947–954, 2016.'),
      refPara('[4] T. Fatima, F. Azam, M. W. Anwar, and Y. Rasheed, "A Systematic Review on Software Project Scheduling and Task Assignment Approaches," Proceedings of the 6th International Conference on Computing and AI, pp. 369–373, 2020.'),
      refPara('[5] S. Hartmann and D. Briskorn, "An Updated Survey of Variants and Extensions of the Resource-Constrained Project Scheduling Problem," European Journal of Operational Research, Vol. 297, No. 1, pp. 1–14, 2022.'),
      refPara('[6] X. Hou et al., "Large Language Models for Software Engineering: A Systematic Literature Review," ACM TOSEM, Vol. 33, No. 8, Article 220, 2024.'),
      refPara('[7] D. Kang, J. Jung, and D.-H. Bae, "Constraint-Based Human Resource Allocation in Software Projects," Software: Practice and Experience, Vol. 41, No. 5, pp. 551–577, 2011.'),
      refPara('[8] S. Morcov, L. Pintelon, and R. J. Kusters, "Definitions, Characteristics and Measures of IT Project Complexity: A Systematic Literature Review," IJISPM, Vol. 8, No. 2, pp. 5–21, 2020.'),
      refPara('[9] J. E. Masso et al., "Risk Management in the Software Life Cycle: A Systematic Literature Review," Computer Standards & Interfaces, Vol. 71, 103431, 2020.'),
      refPara('[10] L. D. Otero et al., "A Systematic Approach for Resource Allocation in Software Projects," Computers & Industrial Engineering, Vol. 56, No. 4, pp. 1333–1339, 2009.'),
      refPara('[11] D. J. Power, "Decision Support Systems: A Historical Overview," Handbook on Decision Support Systems 1, pp. 121–140, Springer, 2008.'),
      refPara('[12] J. Rajagopalan and P. K. Srivastava, "Introduction of a New Metric \'Project Health Index\' (PHI) to Successfully Manage IT Projects," Journal of Organizational Change Management, Vol. 31, No. 2, pp. 385–409, 2018.'),
      refPara('[13] A. V. Rezende et al., "Software Project Scheduling Problem in the Context of Search-Based Software Engineering: A Systematic Review," Journal of Systems and Software, Vol. 155, pp. 43–56, 2019.'),
      refPara('[14] B. G. Tavares, C. E. S. da Silva, and A. D. de Souza, "Risk Management Analysis in Scrum Software Projects," International Transactions in Operational Research, Vol. 26, pp. 1884–1905, 2019.'),
      refPara('[15] S. T. Acuña, M. A. Ampuero, and G. Baldoquín de la Peña, "Formal Model for Assigning Human Resources to Teams in Software Projects," Information and Software Technology, Vol. 53, No. 3, pp. 259–275, 2011.'),
      refPara('[16] K. Schwaber and J. Sutherland, The Scrum Guide: The Definitive Guide to Scrum: The Rules of the Game, 2020.'),

      // ===== PAGE BREAK =====
      new Paragraph({ children: [new TextRun({ text: '', font: 'B Nazanin', size: 28 })], pageBreakBefore: true }),

      // ===== ENGLISH ABSTRACT =====
      heading('Abstract'),
      enPara('Software project management is a complex and dynamic process due to the multitude of activities, human resource limitations, task dependencies, changing project conditions, and the need for continuous decision-making. Despite the development of various project management tools, integrating information related to scheduling, resource capacity, activity dependencies, risk, and overall project status for managerial decision support remains an important challenge.'),
      enPara('This research presents SmartTask, an agile project management system with a decision-support approach. The system follows a hierarchical structure of Workspace, Project, Sprint, User Story, and Task. Beyond conventional project management capabilities, SmartTask addresses team workload analysis, activity dependency management and analysis, multi-factor prioritization, delay risk analysis, and project health assessment.'),
      enPara('Large Language Models are utilized for decomposing complex activities and generating Sprint reports, not as replacements for human decision-makers but as supporting tools. SmartTask aims to bridge operational project management, data analysis, and managerial decision support.'),
      emptyLine(),
      new Paragraph({ children: [new TextRun({ text: 'Keywords: Software Project Management, Agile Project Management, Decision Support System, Workload Analysis, Task Dependencies, Multi-factor Prioritization, Delay Risk Analysis, Project Health, Large Language Models.', font: 'Times New Roman', size: 22, italics: true })], alignment: AlignmentType.LEFT, direction: 'ltr' }),
    ],
  }],
});

// Generate file
const outPath = 'پروپزال_پایان‌نامه.docx';
Packer.toBuffer(doc).then(buffer => {
  fs.writeFileSync(outPath, buffer);
  console.log(`SUCCESS: ${outPath} created (${(buffer.length / 1024).toFixed(1)} KB)`);
}).catch(err => {
  console.error('ERROR:', err.message);
});
