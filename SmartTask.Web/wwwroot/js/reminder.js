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
    const monthNames = [
        "ژانویه", "فوریه", "مارس", "آوریل", "مه", "ژوئن",
        "ژوئیه", "اوت", "سپتامبر", "اکتبر", "نوامبر", "دسامبر"
    ];

    let currentDate = new Date();
    currentDate.setDate(1);

    function formatDateKey(date) {
        const y = date.getFullYear();
        const m = String(date.getMonth() + 1).padStart(2, "0");
        const d = String(date.getDate()).padStart(2, "0");
        return `${y}-${m}-${d}`;
    }

    function getRemindersForDate(dateKey) {
        return reminders.filter(r => r.date === dateKey);
    }

    function renderCalendar() {
        const year = currentDate.getFullYear();
        const month = currentDate.getMonth();

        document.getElementById("calMonthLabel").textContent = `${monthNames[month]} ${year}`;

        // شنبه = روز اول هفته
        const firstDay = new Date(year, month, 1);
        const startOffset = (firstDay.getDay() + 1) % 7;
        const daysInMonth = new Date(year, month + 1, 0).getDate();

        calendarGrid.innerHTML = "";

        for (let i = 0; i < startOffset; i++) {
            const empty = document.createElement("div");
            empty.className = "reminder-calendar-day empty";
            calendarGrid.appendChild(empty);
        }

        const todayKey = formatDateKey(new Date());

        for (let day = 1; day <= daysInMonth; day++) {
            const cellDate = new Date(year, month, day);
            const dateKey = formatDateKey(cellDate);
            const dayReminders = getRemindersForDate(dateKey);

            const cell = document.createElement("div");
            cell.className = "reminder-calendar-day" + (dateKey === todayKey ? " today" : "");
            cell.dataset.date = dateKey;

            const dayNumber = document.createElement("div");
            dayNumber.className = "reminder-calendar-day-number";
            dayNumber.textContent = day;
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
    }

    function showDayDetails(dateKey, dayReminders) {
        const panel = document.getElementById("calendarDayDetails");
        const list = document.getElementById("calendarDayList");
        const label = document.getElementById("calendarSelectedDate");

        label.textContent = dateKey;
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
        currentDate.setMonth(currentDate.getMonth() - 1);
        renderCalendar();
    });

    document.getElementById("calNextBtn").addEventListener("click", function () {
        currentDate.setMonth(currentDate.getMonth() + 1);
        renderCalendar();
    });

    renderCalendar();
});