document.addEventListener("DOMContentLoaded", function () {

    function escapeHtml(str) {
        const div = document.createElement("div");
        div.textContent = str;
        return div.innerHTML;
    }

    // ===== Delete Reminder =====
    document.querySelectorAll(".delete-reminder-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف یادآوری",
                text: "آیا از حذف این یادآوری مطمئن هستید؟",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، حذف کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#EF4444",
                cancelButtonColor: "#64748B"
            }).then((result) => {
                if (result.isConfirmed) form.submit();
            });
        });
    });

    // ===== Calendar =====
    const calendarGrid = document.getElementById("calendarGrid");
    if (!calendarGrid || typeof window.reminderCalendarData === "undefined") return;

    const reminders = window.reminderCalendarData;
    const isJalali = calendarGrid.dataset.dateFormat === "jalali" && typeof persianDate !== "undefined";

    const gregorianMonthNames = [
        "ژانویه", "فوریه", "مارس", "آوریل", "مه", "ژوئن",
        "ژوئیه", "اوت", "سپتامبر", "اکتبر", "نوامبر", "دسامبر"
    ];
    const jalaliMonthNames = [
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    ];

    function formatDateKey(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, "0");
        const d = String(date.getDate()).padStart(2, "0");
        return `${y}-${m}-${d}`;
    }

    function getRemindersForDate(dateKey) {
        return reminders.filter(r => r.date === dateKey);
    }

    // حالت میلادی
    let currentDate = new Date();
    currentDate.setDate(1);

    // حالت شمسی
    let currentJalali = isJalali ? new persianDate() : null;
    if (currentJalali) currentJalali.date(1);

    function renderCalendar() {
        calendarGrid.innerHTML = "";
        isJalali ? renderJalaliCalendar() : renderGregorianCalendar();
    }

    function renderGregorianCalendar() {
        const year = currentDate.getFullYear();
        const month = currentDate.getMonth();

        document.getElementById("calMonthLabel").textContent = `${gregorianMonthNames[month]} ${year}`;

        const firstDay = new Date(year, month, 1);
        const startOffset = (firstDay.getDay() + 1) % 7; // شنبه = اول هفته
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        appendEmptyCells(startOffset);

        const todayKey = formatDateKey(new Date());

        for (let day = 1; day <= daysInMonth; day++) {
            const cellDate = new Date(year, month, day);
            const dateKey = formatDateKey(cellDate);
            appendDayCell(day, dateKey, dateKey === todayKey);
        }
    }

    function renderJalaliCalendar() {
        const jYear = currentJalali.year();
        const jMonth = currentJalali.month(); // 1..12

        document.getElementById("calMonthLabel").textContent = `${jalaliMonthNames[jMonth - 1]} ${jYear}`;

        const firstOfMonth = new persianDate([jYear, jMonth, 1]);
        const firstGregorian = firstOfMonth.toCalendar("gregorian").toDate();
        const startOffset = (firstGregorian.getDay() + 1) % 7; // شنبه = اول هفته
        const daysInMonth = firstOfMonth.daysInMonth();

        appendEmptyCells(startOffset);

        const todayKey = formatDateKey(new Date());

        for (let day = 1; day <= daysInMonth; day++) {
            const jDate = new persianDate([jYear, jMonth, day]);
            const gDate = jDate.toCalendar("gregorian").toDate();
            const dateKey = formatDateKey(gDate);
            appendDayCell(day, dateKey, dateKey === todayKey);
        }
    }

    function appendEmptyCells(count) {
        for (let i = 0; i < count; i++) {
            const empty = document.createElement("div");
            empty.className = "reminder-calendar-day empty";
            calendarGrid.appendChild(empty);
        }
    }

    function appendDayCell(dayNumberText, dateKey, isToday) {
        const dayReminders = getRemindersForDate(dateKey);

        const cell = document.createElement("div");
        cell.className = "reminder-calendar-day" + (isToday ? " today" : "");
        cell.dataset.date = dateKey;

        const dayNumber = document.createElement("div");
        dayNumber.className = "reminder-calendar-day-number";
        dayNumber.textContent = dayNumberText;
        cell.appendChild(dayNumber);

        if (dayReminders.length > 0) {
            const dots = document.createElement("div");
            dots.className = "reminder-calendar-day-dots";
            dayReminders.forEach(r => {
                const dot = document.createElement("span");
                dot.className = "reminder-calendar-dot" + (r.isSent ? " sent" : "");
                dots.appendChild(dot);
            });
            cell.appendChild(dots);
        }

        cell.addEventListener("click", function () {
            showDayDetails(dateKey, dayReminders);
        });

        calendarGrid.appendChild(cell);
    }

    function showDayDetails(dateKey, dayReminders) {
        const panel = document.getElementById("calendarDayDetails");
        const list = document.getElementById("calendarDayList");
        const label = document.getElementById("calendarSelectedDate");

        label.textContent = isJalali
            ? new persianDate(new Date(dateKey)).format("YYYY/MM/DD")
            : dateKey;

        list.innerHTML = "";

        if (dayReminders.length === 0) {
            panel.style.display = "none";
            return;
        }

        dayReminders.forEach(r => {
            const item = document.createElement("div");
            item.className = "reminder-item" + (r.isSent ? " past sent" : "");
            item.innerHTML = `
                <div class="reminder-item-icon"><i class="fa-regular fa-clock"></i></div>
                <div class="reminder-item-body">
                    <a href="/Task/Details/${r.taskItemId}" class="reminder-item-title">${escapeHtml(r.title)}</a>
                    <span class="reminder-item-meta">Task: ${escapeHtml(r.taskTitle)} — ${r.time}</span>
                </div>
            `;
            list.appendChild(item);
        });

        panel.style.display = "block";
    }

    document.getElementById("calPrevBtn").addEventListener("click", function () {
        if (isJalali) {
            currentJalali.subtract("month", 1);
        } else {
            currentDate.setMonth(currentDate.getMonth() - 1);
        }
        renderCalendar();
    });

    document.getElementById("calNextBtn").addEventListener("click", function () {
        if (isJalali) {
            currentJalali.add("month", 1);
        } else {
            currentDate.setMonth(currentDate.getMonth() + 1);
        }
        renderCalendar();
    });

    renderCalendar();
});