# پروپوزال پایان‌نامه کارشناسی

## عنوان پروژه

**طراحی و پیاده‌سازی سامانه مدیریت پروژه چابک با رویکرد تصمیم‌یار مبتنی بر تحلیل داده‌های پروژه**

---

نگارش: مائده اسلامی

استاد راهنما: دکتر ستاره امیری

دانشگاه فنی و حرفه‌ای — دانشکده فنی و حرفه‌ای دختران دکتر شریعتی

رشته مهندسی حرفه‌ای کامپیوتر — گرایش نرم‌افزار

تابستان ۱۴۰۵

---

## چکیده

مدیریت پروژه‌های نرم‌افزاری به دلیل تعدد فعالیت‌ها، محدودیت منابع انسانی، وابستگی میان وظایف و ضرورت تصمیم‌گیری مستمر، فرایندی پیچیده و پویاست. با وجود توسعه ابزارهای متنوع مدیریت پروژه، یکپارچه‌سازی اطلاعات مربوط به زمان‌بندی، ظرفیت منابع، وابستگی فعالیت‌ها، ریسک و وضعیت کلی پروژه برای پشتیبانی از تصمیم‌گیری مدیریتی، موضوعی قابل توجه است.

در این پژوهش، سامانه SmartTask با هدف طراحی و پیاده‌سازی یک سامانه مدیریت پروژه چابک با رویکرد تصمیم‌یار ارائه می‌شود. ساختار سلسله‌مراتبی سامانه شامل Workspace، Project، Sprint، User Story و Task است و علاوه بر قابلیت‌های متداول مدیریت پروژه، حوزه‌های تحلیل بارکاری اعضای تیم، مدیریت و تحلیل وابستگی فعالیت‌ها، اولویت‌بندی چندعاملی، تحلیل ریسک تأخیر و ارزیابی سلامت پروژه را پوشش می‌دهد. همچنین از مدل‌های زبانی بزرگ برای تفکیک فعالیت‌های پیچیده و تولید گزارش اسپرینت استفاده شده است. رویکرد پروژه در استفاده از هوش مصنوعی، جایگزین‌کردن تصمیم‌گیرنده انسانی نیست، بلکه مدل‌های زبانی به‌عنوان ابزار پشتیبان برای فرایندهای تحلیلی مورد توجه قرار گرفته‌اند.

واژگان کلیدی: مدیریت پروژه نرم‌افزاری، مدیریت پروژه چابک، تصمیم‌یار، تحلیل بارکاری، وابستگی وظایف، اولویت‌بندی چندعاملی، تحلیل ریسک تأخیر، سلامت پروژه، مدل‌های زبانی بزرگ.

---

## فهرست نوشتار

فصل ۱: مقدمه
- ۱-۱. انگیزه
- ۱-۲. مروری بر پیشینه و کارهای مشابه
  - ۱-۲-۱. مدیریت پروژه نرم‌افزاری و رویکرد چابک
  - ۱-۲-۲. زمان‌بندی و تخصیص وظایف
  - ۱-۲-۳. تخصیص منابع انسانی و مفهوم Workload
  - ۱-۲-۴. وابستگی فعالیت‌ها و تحلیل اثر آن
  - ۱-۲-۵. اولویت‌بندی وظایف
  - ۱-۲-۶. ریسک تأخیر و مدیریت ریسک پروژه
  - ۱-۲-۷. سلامت و وضعیت کلی پروژه
  - ۱-۲-۸. سیستم‌های تصمیم‌یار
  - ۱-۲-۹. هوش مصنوعی و مدل‌های زبانی در مدیریت فعالیت‌ها
  - ۱-۲-۱۰. بررسی تطبیقی سامانه‌های مدیریت پروژه
  - ۱-۲-۱۱. تحلیل شکاف و جایگاه پروژه پیشنهادی
  - ۱-۲-۱۲. جمع‌بندی پیشینه و جایگاه SmartTask
- ۱-۳. اهداف
- ۱-۴. رئوس مطالب سایر فصل‌ها
- منابع

---

فصل ۱:
# مقدمه

## ۱-۱. انگیزه

پروژه‌های نرم‌افزاری در محیطی اجرا می‌شوند که مجموعه‌ای از فعالیت‌ها، منابع انسانی، محدودیت‌های زمانی و تغییرات محیطی باید به‌صورت هماهنگ مدیریت شوند. Morcov و همکاران در یک مرور نظام‌مند، پیچیدگی پروژه‌های فناوری اطلاعات را مفهومی چندبعدی معرفی کرده و نشان داده‌اند که شناخت و اندازه‌گیری پیچیدگی پروژه برای مدیریت مؤثر آن اهمیت دارد (Morcov et al., 2020). Butler و همکاران نیز پیچیدگی و پویایی پروژه را از ویژگی‌های مؤثر بر نتایج پروژه معرفی کرده‌اند (Butler et al., 2020).

یکی از پیامدهای این پیچیدگی، دشواری مدیریت فعالیت‌ها و منابع انسانی است. تصمیم درباره اینکه چه فعالیتی، در چه زمانی، با چه اولویتی و توسط چه فردی انجام شود، مسئله‌ای چندعاملی است. Fatima و همکاران در مرور نظام‌مند خود نشان داده‌اند که مدل‌های زمان‌بندی ایستا و پویا در ادبیات مورد استفاده قرار گرفته‌اند (Fatima et al., 2020). تخصیص منابع انسانی نیز مسئله‌ای پیچیده است؛ Otero و همکاران تخصیص منابع را وابسته به قابلیت‌ها و نیازمندی‌های تخصصی فعالیت‌ها دانسته‌اند (Otero et al., 2009) و Kang و همکاران بر تأثیر ویژگی‌های فردی و تیمی بر تصمیم‌های تخصیص منابع تأکید کرده‌اند (Kang et al., 2011).

در کنار منابع انسانی، وابستگی میان فعالیت‌ها نیز عامل مؤثری بر زمان‌بندی پروژه است. در مسائل Resource-Constrained Project Scheduling، روابط تقدم میان فعالیت‌ها و محدودیت منابع از اجزای اصلی مدل‌سازی هستند (Hartmann & Briskorn, 2022). اولویت‌بندی فعالیت‌ها نیز با افزایش تعداد وظایف و محدودیت منابع اهمیت پیدا می‌کند (Bugayenko et al., 2023). مدیریت ریسک نیز در پروژه‌های چابک باید به‌صورت مستمر و متناسب با ویژگی‌های این محیط انجام شود (Tavares et al., 2019).

در چنین شرایطی، مفهوم سیستم تصمیم‌یار (Decision Support System) مطرح می‌شود. Power در مرور تاریخی خود، DSSها را سیستم‌هایی برای کمک به تصمیم‌گیرنده از طریق فراهم‌کردن داده، اطلاعات و قابلیت‌های تحلیلی معرفی کرده است (Power, 2008). Cunha و همکاران نیز تصمیم‌گیری در مدیریت پروژه نرم‌افزاری را پدیده‌ای چندوجهی و نیازمند پشتیبانی بهتر دانسته‌اند (Cunha et al., 2016).

بخش دیگری از انگیزه انجام این پروژه از تجربه عملی در محیط توسعه نرم‌افزار شکل گرفته است. در چنین محیطی، مسئله صرفاً ثبت Taskها نیست، بلکه لازم است مشخص شود کدام فعالیت‌ها اهمیت بیشتری دارند، ظرفیت هر عضو تا چه حد درگیر است، تأخیر یک فعالیت چه پیامدی خواهد داشت و وضعیت کلی پروژه در چه شرایطی قرار دارد.

بنابراین، انگیزه اصلی SmartTask حرکت از مدیریت صرفاً عملیاتی وظایف به سمت مدیریت تحلیلی و تصمیم‌یار پروژه است؛ به‌گونه‌ای که مدیر پروژه بتواند در کنار تجربه حرفه‌ای خود، اطلاعات ساختاریافته‌تری برای تصمیم‌گیری درباره اولویت‌ها، ظرفیت تیم، ریسک‌ها و وضعیت پروژه در اختیار داشته باشد.

---

## ۱-۲. مروری بر پیشینه و کارهای مشابه

### ۱-۲-۱. مدیریت پروژه نرم‌افزاری و رویکرد چابک

مدیریت پروژه نرم‌افزاری با برنامه‌ریزی، سازمان‌دهی و کنترل فعالیت‌ها و منابع سروکار دارد. Butler و همکاران نشان می‌دهند که انتخاب رویکرد مدیریت باید متناسب با شرایط پروژه باشد (Butler et al., 2020). در پروژه‌های چابک، سازگاری با تغییر و استفاده از بازخورد اهمیت ویژه‌ای دارد. راهنمای رسمی Scrum، Scrum را چارچوبی برای کمک به ایجاد ارزش از طریق راهکارهای سازگارشونده معرفی می‌کند (Schwaber & Sutherland, 2020).

در SmartTask، Agile و Scrum به‌عنوان بستر سازمان‌دهی فعالیت‌ها در نظر گرفته شده‌اند و قابلیت‌های تحلیلی بر روی همین بستر قرار می‌گیرند. ساختار سلسله‌مراتبی Workspace → Project → Sprint → Story → Task با مفاهیم Scrum از جمله Product Backlog، Sprint Planning و Sprint Board سازگار است.

### ۱-۲-۲. زمان‌بندی و تخصیص وظایف

Fatima و همکاران در مرور نظام‌مند خود نشان داده‌اند که Software Project Scheduling و Task Assignment موضوعات مهمی در حوزه مهندسی نرم‌افزار هستند (Fatima et al., 2020). Rezende و همکاران در مرور ۳۷ مطالعه مرتبط، مدل‌های پویا و سازگارشونده را از زمینه‌های قابل توجه برای تحقیقات آتی معرفی کرده‌اند (Rezende et al., 2019). این مطالعات نشان می‌دهند که زمان‌بندی فعالیت‌ها به ویژگی‌های پروژه، منابع و شرایط اجرای آن وابسته است.

در SmartTask، امکان نگهداری اطلاعات زمانی فعالیت‌ها، تخمین زمان و ثبت Time Log برای هر Task فراهم شده است. همچنین مفاهیم Sprint و Velocity برای پایش پیشرفت مورد استفاده قرار می‌گیرند.

### ۱-۲-۳. تخصیص منابع انسانی و مفهوم Workload

منابع انسانی در پروژه‌های نرم‌افزاری نقشی اساسی دارند. Otero و همکاران نشان داده‌اند که تخصیص منابع می‌تواند بر اساس قابلیت‌ها و نیازمندی‌های تخصصی فعالیت‌ها انجام شود (Otero et al., 2009). Acuña و همکاران نیز تخصیص افراد به نقش‌ها را مسئله‌ای تصمیم‌گیری معرفی کرده‌اند (Acuña et al., 2011).

Workload در این پروژه به وضعیت حجم کار تخصیص‌یافته به منابع انسانی در ارتباط با ظرفیت قابل استفاده آن‌ها اشاره دارد. در SmartTask، ساعت‌های تخمینی فعالیت‌های تخصیص‌یافته با ظرفیت اعضای تیم مقایسه شده و وضعیت بارکاری (زیرظرفیت، متعادل، اضافه‌بار) به‌عنوان یکی از داده‌های مورد استفاده در تصمیم‌گیری مدیریتی ارائه می‌شود.

### ۱-۲-۴. وابستگی فعالیت‌ها و تحلیل اثر آن

وابستگی فعالیت‌ها یکی از عناصر مهم در زمان‌بندی پروژه است. Hartmann و Briskorn در بررسی مسائل Resource-Constrained Project Scheduling، روابط تقدم و محدودیت منابع را از اجزای اصلی مدل‌سازی معرفی کرده‌اند (Hartmann & Briskorn, 2022). در چنین محیطی، یک Task ممکن است نه‌تنها به دلیل ویژگی‌های خود، بلکه به دلیل اثرش بر فعالیت‌های دیگر اهمیت پیدا کند.

در SmartTask، مدیریت وابستگی در چند لایه در نظر گرفته شده است: تعریف و مدیریت روابط وابستگی، جلوگیری از ایجاد چرخه (Cycle Detection)، تحلیل فعالیت‌های تحت تأثیر، نمایش گراف وابستگی و بررسی زنجیره‌های پرریسک. Dependency Impact در این پروژه به میزان اثرگذاری بالقوه وضعیت یک فعالیت بر فعالیت‌های وابسته اشاره دارد.

### ۱-۲-۵. اولویت‌بندی وظایف

Bugayenko و همکاران در یک مرور نظام‌مند نشان داده‌اند که Task Prioritization یکی از موضوعات مهم مهندسی نرم‌افزار است (Bugayenko et al., 2023). در SmartTask، Smart Priority از ترکیب چند عامل شامل فوریت زمانی، اثر وابستگی و وضعیت بارکاری استفاده می‌کند. این وزن‌ها و منطق امتیازدهی، تصمیم‌های طراحی پروژه هستند و در فصل طراحی تشریح خواهند شد.

### ۱-۲-۶. ریسک تأخیر و مدیریت ریسک پروژه

Masso و همکاران در مرور نظام‌مند ۴۵ مطالعه مرتبط، مدیریت ریسک را بخش مهمی از مدیریت پروژه نرم‌افزاری معرفی کرده‌اند (Masso et al., 2020). Tavares و همکاران بر ماهیت مستمر مدیریت ریسک در پروژه‌های Scrum تأکید کرده‌اند (Tavares et al., 2019). در SmartTask، Delay Risk شاخصی تحلیلی است که عواملی مانند وضعیت تأخیر، بارکاری، زنجیره‌های وابستگی و رخدادهای Cascade را در یک امتیاز ترکیبی در نظر می‌گیرد.

### ۱-۲-۷. سلامت و وضعیت کلی پروژه

Rajagopalan و Srivastava یک شاخص ترکیبی Project Health Index از ۱۷ معیار ارائه کرده‌اند که می‌تواند برای پیش‌بینی وضعیت پروژه مورد استفاده قرار گیرد (Rajagopalan & Srivastava, 2018). در SmartTask، شاخص سلامت پروژه از ترکیب چهار بعد زمان‌بندی، بارکاری، وابستگی و تحویل با وزن‌های مشخص طراحی شده است.

### ۱-۲-۸. سیستم‌های تصمیم‌یار

Power در بررسی تاریخی خود، DSSها را در قالب سیستم‌های داده‌محور، مدل‌محور و دانش‌محور معرفی کرده است (Power, 2008). Cunha و همکاران نیز تصمیم‌گیری در مدیریت پروژه را پدیده‌ای چندوجهی دانسته‌اند (Cunha et al., 2016). در SmartTask، مفهوم تصمیم‌یار به این معناست که سیستم داده‌ها را تحلیل کرده و اطلاعات یا پیشنهادهای پشتیبان ارائه می‌دهد، در حالی که تصمیم نهایی در اختیار مدیر پروژه باقی می‌ماند.

### ۱-۲-۹. هوش مصنوعی و مدل‌های زبانی در مدیریت فعالیت‌ها

Hou و همکاران در یک مرور نظام‌مند ۳۹۵ پژوهش درباره LLMها در مهندسی نرم‌افزار را بررسی کرده‌اند و نشان داده‌اند که این حوزه همچنان در حال توسعه است (Hou et al., 2024). در SmartTask، استفاده از هوش مصنوعی به‌عنوان جایگزین تصمیم‌گیرنده در نظر گرفته نشده، بلکه مدل‌های زبانی برای وظایف مشخص مانند تفکیک فعالیت و تولید گزارش Sprint مورد توجه قرار گرفته‌اند. اتصال به یک مدل زبانی محلی از طریق LM Studio امکان‌پذیر شده تا از ارسال اطلاعات پروژه به سرویس‌های خارجی جلوگیری شود.

### ۱-۲-۱۰. بررسی تطبیقی سامانه‌های مدیریت پروژه

بررسی ابزارهای موجود در حوزه مدیریت پروژه، قابلیت‌های رایج و نیازهای پوشش‌داده‌نشده را مشخص می‌کند. هدف از این بررسی، اثبات برتری SmartTask نیست، بلکه شناسایی جایگاه تحلیلی و تصمیم‌یار این سامانه در مقایسه با ابزارهای موجود است.

**جدول ۱-۲. مقایسه تطبیقی قابلیت‌های اصلی سامانه‌های مدیریت پروژه**

| قابلیت | Jira | GitHub Projects | Asana | Microsoft Planner | Trello | Monday.com | SmartTask |
|---|---|---|---|---|---|---|---|
| **ساختار سلسله‌مراتبی** | ✅ Epic→Story→Task | ✅ Issue→Sub-issue | ✅ Project→Section→Task | ✅ Bucket→Task | ⚠️ Board→Card | ✅ Group→Item | ✅ Workspace→Project→Sprint→Story→Task |
| **Sprint/Iteration** | ✅ (Jira Agile) | ✅ (Iterations) | ✅ (Rules) | ✅ (Sprints) | ❌ | ✅ | ✅ |
| **Backlog Management** | ✅ | ✅ | ✅ | ⚠️ محدود | ⚠️ محدود | ✅ | ✅ |
| **Dependency** | ✅ (Advanced) | ✅ (Blocked by/Blocking) | ✅ (4 نوع رابطه) | ✅ (Critical Path) | ❌ | ✅ | ✅ + تحلیل زنجیره تأثیر |
| **Workload/ Capacity** | ✅ (Advanced Roadmaps) | ⚠️ محدود | ✅ | ✅ | ❌ | ✅ | ✅ + تحلیل بارکاری چندسطحی |
| **Timeline/ Gantt** | ✅ (Advanced) | ✅ (Roadmap) | ✅ | ✅ | ❌ | ✅ | ⚠️ محدود |
| **Reports/ Charts** | ✅ (Dashboards) | ✅ (Insights) | ✅ (Dashboards) | ✅ (Charts) | ⚠️ | ✅ (Dashboards) | ✅ + داشبورد تحلیلی |
| **Chat/ Collaboration** | ⚠️ (Comments) | ⚠️ (Comments) | ⚠️ (Comments) | ⚠️ (Comments) | ❌ | ⚠️ (Updates) | ✅ Chat بلادرنگ + Comment |
| **Smart Priority** | ⚠️ (Manual) | ❌ | ⚠️ (Custom Fields) | ❌ | ❌ | ⚠️ (Formula) | ✅ موتور اولویت‌بندی چندعاملی |
| **Delay Risk Analysis** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| **Project Health Score** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ شاخص ترکیبی |
| **Dependency Cascade** | ⚠️ (Manual) | ❌ | ❌ | ✅ (Auto-update dates) | ❌ | ❌ | ✅ کاسکاد خودکار با Background Service |
| **AI Integration** | ⚠️ (Atlassian Intelligence) | ⚠️ (Copilot) | ⚠️ (AI Studio) | ⚠️ (Copilot) | ❌ | ⚠️ (AI Assistant) | ✅ Task Breakdown + Sprint Report (LLM محلی) |
| **Decision Support** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ ترکیب داده‌ها برای تصمیم‌یار |
| **Real-time Chat** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ SignalR |
| **RTL Support** | ⚠️ | ❌ | ⚠️ | ❌ | ❌ | ❌ | ✅ RTL-native |
| **محلی‌سازی فارسی** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

**جدول ۱-۳. مقایسه سطح تحلیل و پشتیبانی تصمیم‌گیری**

| سطح تحلیل | Jira | Asana | Monday | SmartTask |
|---|---|---|---|---|
| **گزارش وضعیت** (Activity Reporting) | ✅ | ✅ | ✅ | ✅ |
| **تحلیل روند** (Trend Analysis) | ✅ | ⚠️ | ✅ | ✅ Burndown + Velocity |
| **تحلیل ظرفیت** (Capacity Analysis) | ✅ | ✅ | ✅ | ✅ Workload Analysis |
| **تحلیل وابستگی** (Dependency Impact) | ⚠️ | ⚠️ | ⚠️ | ✅ تحلیل زنجیره تأثیر + گراف |
| **اولویت‌بندی خودکار** (Smart Prioritization) | ❌ | ❌ | ❌ | ✅ چندعاملی |
| **پیش‌بینی ریسک** (Risk Prediction) | ❌ | ❌ | ❌ | ✅ Delay Risk Score |
| **شاخص سلامت** (Health Score) | ❌ | ❌ | ❌ | ✅ Project Health Index |
| **کاسکاد خودکار** (Cascade Automation) | ⚠️ | ❌ | ❌ | ✅ Background Service |
| **پشتیبان تصمیم** (Decision Support) | ❌ | ❌ | ❌ | ✅ ترکیب داده‌ها + پیشنهاد |

**توضیح:** ✅ = پیاده‌سازی کامل، ⚠️ = پیاده‌سازی محدود یا نیاز به پلاگین، ❌ = وجود ندارد

**نتیجه بررسی:** ابزارهای موجود بخش زیادی از نیازهای پایه مدیریت پروژه را پوشش می‌دهند. تمایز SmartTask در سطح ترکیب و مدل‌سازی اطلاعات است؛ یعنی استفاده هم‌زمان از اطلاعات زمانی، بارکاری، وابستگی، تأخیر و پیشرفت برای تولید تحلیل‌هایی مانند اولویت پیشنهادی، امتیاز ریسک و شاخص سلامت پروژه. این سطح از تحلیل یکپارچه در هیچ‌کدام از ابزارهای بررسی‌شده وجود ندارد.

### ۱-۲-۱۱. تحلیل شکاف و جایگاه پروژه پیشنهادی

بر اساس مطالعات علمی و بررسی سامانه‌های مشابه، تحلیل شکاف زیر استخراج می‌شود:

**جدول ۱-۴. تحلیل شکاف: وضعیت موجود و جهت‌گیری SmartTask**

| چالش/نیاز | وضعیت در مطالعات و سامانه‌ها | جهت‌گیری SmartTask | نوع نوآوری |
|---|---|---|---|
| مدیریت تعداد زیاد فعالیت‌ها | وجود در ابزارها | ساختار سلسله‌مراتبی Project→Sprint→Story→Task | بهبود |
| زمان‌بندی فعالیت‌ها | پشتیبانی در ابزارها | استفاده از اطلاعات زمان برای تحلیل اولویت و ریسک | ترکیب |
| تخصیص منابع انسانی | Workload در Asana, Jira | تحلیل بارکاری چندسطحی + ارتباط با Priority و Risk | ترکیب |
| وابستگی فعالیت‌ها | Dependency در Jira, Asana, Planner | تحلیل زنجیره تأثیر + گراف + کاسکاد خودکار | ارتقا |
| اولویت‌بندی | Manual Priority در ابزارها | موتور اولویت‌بندی چندعامله خودکار | **نوآورانه** |
| مدیریت ریسک | مدیریت ریسک عمومی | Delay Risk Score مبتنی بر داده‌های واقعی پروژه | **نوآورانه** |
| سلامت پروژه | معدود در ادبیات | Project Health Index ترکیبی چندبعدی | **نوآورانه** |
| کاسکاد خودکار تأخیر | Auto-update dates در Planner | Background Service بررسی زنجیره وابستگی و به‌روزرسانی خودکار | **نوآورانه** |
| پشتیبانی از تصمیم | وجود ندارد | ترکیب داده‌ها + ارائه شاخص و پیشنهاد | **نوآورانه** |
| هوش مصنوعی | Copilot, AI Assistant (تحلیلی) | LLM محلی برای Task Breakdown + Sprint Report | **نوآورانه** |
| حفظ نقش مدیر | خودکارسازی کامل در برخی ابزارها | پیشنهاد، نه تصمیم خودکار | طراحی |

### ۱-۲-۱۲. جمع‌بندی پیشینه و جایگاه SmartTask

بررسی مطالعات نشان می‌دهد که مدیریت پروژه‌های نرم‌افزاری با مجموعه‌ای از مسائل شامل پیچیدگی، زمان‌بندی، تخصیص منابع، وابستگی، اولویت‌بندی و ریسک مواجه است. بررسی ابزارهای موجود نیز نشان می‌دهد که بسیاری از قابلیت‌های پایه در سامانه‌های دیگر وجود دارند.

مسئله SmartTask نه ایجاد یک ابزار دیگر برای ثبت Task، بلکه چگونگی ارتباط دادن اطلاعات عملیاتی مختلف پروژه برای پشتیبانی از تصمیم‌گیری مدیریتی است. تمایز اصلی SmartTask در سه محور است:

1. **اولویت‌بندی چندعاملی خودکار:** موتوری که از ترکیب فوریت زمانی، اثر وابستگی و وضعیت بارکاری، اولویت پیشنهادی تولید می‌کند.
2. **تحلیل ریسک تأخیر:** مدلی کمّی برای ارزیابی ریسک تأخیر پروژه بر اساس داده‌های واقعی.
3. **شاخص سلامت پروژه:** نمایش ترکیبی وضعیت پروژه از چند بعد مختلف.

علاوه بر این، قابلیت‌هایی مانند کاسکاد خودکار تأخیر، تحلیل بارکاری و استفاده هدفمند از LLM محلی، SmartTask را از ابزارهای موجود متمایز می‌کنند.

---

## ۱-۳. اهداف

**هدف اصلی:** طراحی و پیاده‌سازی یک سامانه مدیریت پروژه چابک با رویکرد تصمیم‌یار برای تیم‌های توسعه نرم‌افزار.

**اهداف فرعی:**

۱. **مدیریت ساختار پروژه و فعالیت‌ها:** طراحی ساختار سلسله‌مراتبی شامل Workspace، Project، Sprint، User Story و Task و ایجاد ارتباط مناسب میان این اجزا.

۲. **پشتیبانی از Agile و Scrum:** فراهم‌کردن امکانات Product Backlog، Sprint، Sprint Planning، Sprint Board و ارائه Burndown و Velocity.

۳. **مدیریت منابع انسانی و بارکاری:** تعریف ظرفیت کاری اعضای تیم و تحلیل Workload بر اساس فعالیت‌های تخصیص‌یافته.

۴. **تحلیل وابستگی فعالیت‌ها:** تعریف روابط وابستگی، تشخیص چرخه، نمایش گراف Dependency و تحلیل فعالیت‌های تحت تأثیر.

۵. **اولویت‌بندی چندعاملی:** طراحی موتور اولویت‌بندی مبتنی بر فوریت زمانی، اثر وابستگی و وضعیت منابع انسانی.

۶. **تحلیل ریسک تأخیر و سلامت پروژه:** طراحی Delay Risk Score و Project Health Index برای ارزیابی وضعیت پروژه.

۷. **مدیریت اثر آبشاری تأخیرها:** پیاده‌سازی Background Service برای بررسی و به‌روزرسانی خودکار زنجیره وابستگی.

۸. **استفاده هدفمند از هوش مصنوعی:** استفاده از مدل‌های زبانی محلی برای تفکیک فعالیت و تولید گزارش Sprint.

۹. **پشتیبانی از تصمیم‌گیری مدیریتی:** ایجاد ارتباط میان داده‌های عملیاتی و خروجی‌های تحلیلی.

۱۰. **محیط یکپارچه همکاری تیمی:** فراهم‌کردن Comment، Attachment، Checklist، Notification، Chat و Time Tracking.

### ۱-۳-۱. رویکرد ارزیابی

قابلیت‌های تحلیلی و هوش مصنوعی با معیارهای مشخص مورد ارزیابی قرار خواهند گرفت:

| قابلیت | روش ارزیابی | معیار اصلی |
|---|---|---|
| Priority Engine | بررسی منطقی تغییر امتیاز در سناریوهای کنترل‌شده | سازگاری امتیاز با تغییرات ورودی |
| Workload Analysis | بررسی صحت محاسبه ظرفیت | دقت نسبت بارکاری ظرفیت |
| Dependency Analysis | بررسی شناسایی زنجیره وابستگی و تشخیص چرخه | صحت نتایج BFS |
| Delay Risk | بررسی سازگاری امتیاز ریسک با تغییر ورودی‌ها | پایداری و تفسیرپذیری |
| Project Health | بررسی رفتار شاخص در شرایط مختلف | شفافیت مؤلفه‌ها |
| AI | بررسی کیفیت خروجی، زمان پاسخ، نرخ خطا | کیفیت ساختارمند خروجی |

---

## ۱-۴. رئوس مطالب سایر فصل‌ها

**فصل ۲: تحلیل مسئله و نیازمندی‌ها**
کاربران و نقش‌های سیستم، نیازمندی‌های عملکردی و غیرعملکردی، سناریوهای اصلی و چالش‌های شناسایی‌شده.

**فصل ۳: طراحی سامانه**
معماری سامانه، مدل موجودیت‌ها و روابط، طراحی پایگاه داده، جریان اطلاعات، طراحی الگوریتم‌ها و مدل‌های تحلیلی (Priority Engine، Delay Risk، Project Health، Workload Analysis).

**فصل ۴: پیاده‌سازی**
فناوری‌ها و ابزارها، ساختار Controllers، Services و Data Access، پیاده‌سازی قابلیت‌های تحلیلی، Background Services، ارتباطات بلادرنگ و هوش مصنوعی.

**فصل ۵: ارزیابی و نتیجه‌گیری**
ارزیابی نتایج، میزان تحقق اهداف، محدودیت‌ها و پیشنهادهای آینده.

---

## منابع

[1] Butler, C.W., Vijayasarathy, L.R., and Roberts, N. "Managing Software Development Projects for Success: Aligning Plan- and Agility-Based Approaches to Project Complexity and Project Dynamism," Project Management Journal, Vol. 51, No. 3, pp. 262–277, 2020.

[2] Bugayenko, Y., Bakare, A., Cheverda, A., Farina, M., Kruglov, A., Plaksin, Y., Pedrycz, W., and Succi, G. "Prioritizing Tasks in Software Development: A Systematic Literature Review," PLOS ONE, Vol. 18, No. 4, e0283838, 2023.

[3] Cunha, A.C.R., de Moura, H.P., and de Vasconcellos, A.M.L. "Decision-Making in Software Project Management: A Systematic Literature Review," Procedia Computer Science, Vol. 100, pp. 947–954, 2016.

[4] Fatima, T., Azam, F., Anwar, M.W., and Rasheed, Y. "A Systematic Review on Software Project Scheduling and Task Assignment Approaches," Proceedings of the 6th International Conference on Computing and AI, pp. 369–373, 2020.

[5] Hartmann, S. and Briskorn, D. "An Updated Survey of Variants and Extensions of the Resource-Constrained Project Scheduling Problem," European Journal of Operational Research, Vol. 297, No. 1, pp. 1–14, 2022.

[6] Hou, X., Zhao, Y., Liu, Y., Yang, Z., Wang, K., Li, L., Luo, X., Lo, D., Grundy, J., and Wang, H. "Large Language Models for Software Engineering: A Systematic Literature Review," ACM TOSEM, Vol. 33, No. 8, Article 220, 2024.

[7] Kang, D., Jung, J., and Bae, D.-H. "Constraint-Based Human Resource Allocation in Software Projects," Software: Practice and Experience, Vol. 41, No. 5, pp. 551–577, 2011.

[8] Morcov, S., Pintelon, L., and Kusters, R.J. "Definitions, Characteristics and Measures of IT Project Complexity: A Systematic Literature Review," IJISPM, Vol. 8, No. 2, pp. 5–21, 2020.

[9] Masso, J.E., Pino, F.J., Pardo, C., García, F., and Piattini, M. "Risk Management in the Software Life Cycle: A Systematic Literature Review," Computer Standards & Interfaces, Vol. 71, 103431, 2020.

[10] Otero, L.D., Centeno, G., Ruiz-Torres, A.J., and Otero, C.E. "A Systematic Approach for Resource Allocation in Software Projects," Computers & Industrial Engineering, Vol. 56, No. 4, pp. 1333–1339, 2009.

[11] Power, D.J. "Decision Support Systems: A Historical Overview," Handbook on Decision Support Systems 1, pp. 121–140, Springer, 2008.

[12] Rajagopalan, J. and Srivastava, P.K. "Introduction of a New Metric 'Project Health Index' (PHI) to Successfully Manage IT Projects," Journal of Organizational Change Management, Vol. 31, No. 2, pp. 385–409, 2018.

[13] Rezende, A.V., Silva, L.M.A., Britto, A., and Amaral, R. "Software Project Scheduling Problem in the Context of Search-Based Software Engineering: A Systematic Review," Journal of Systems and Software, Vol. 155, pp. 43–56, 2019.

[14] Tavares, B.G., da Silva, C.E.S., and de Souza, A.D. "Risk Management Analysis in Scrum Software Projects," International Transactions in Operational Research, Vol. 26, pp. 1884–1905, 2019.

[15] Acuña, S.T., Ampuero, M.A., and Baldoquín de la Peña, G. "Formal Model for Assigning Human Resources to Teams in Software Projects," Information and Software Technology, Vol. 53, No. 3, pp. 259–275, 2011.

[16] Schwaber, K. and Sutherland, J. The Scrum Guide: The Definitive Guide to Scrum: The Rules of the Game, 2020.

---

## Abstract

Software project management is a complex and dynamic process due to the multitude of activities, human resource limitations, task dependencies, changing project conditions, and the need for continuous decision-making. Despite the development of various project management tools, integrating information related to scheduling, resource capacity, activity dependencies, risk, and overall project status for managerial decision support remains an important challenge.

This research presents SmartTask, an agile project management system with a decision-support approach. The system follows a hierarchical structure of Workspace, Project, Sprint, User Story, and Task. Beyond conventional project management capabilities, SmartTask addresses team workload analysis, activity dependency management and analysis, multi-factor prioritization, delay risk analysis, and project health assessment. Large Language Models are utilized for decomposing complex activities and generating Sprint reports, not as replacements for human decision-makers but as supporting tools.

SmartTask aims to bridge operational project management, data analysis, and managerial decision support, enabling project managers to utilize structured information and analytical insights alongside their professional judgment.

Keywords: Software Project Management, Agile Project Management, Decision Support System, Workload Analysis, Task Dependencies, Multi-factor Prioritization, Delay Risk Analysis, Project Health, Large Language Models.
