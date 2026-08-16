document.addEventListener("DOMContentLoaded", function () {

    //  Delete Sprint 
    document.querySelectorAll(".delete-sprint-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف اسپرینت",
                text: "آیا از حذف این اسپرینت مطمئن هستید؟ این عملیات قابل بازگشت نیست.",
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

    //  Activate Sprint 
    document.querySelectorAll(".activate-sprint-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "فعال‌سازی اسپرینت",
                text: "با فعال‌سازی این اسپرینت، اسپرینت فعال قبلی (در صورت وجود) به حالت برنامه‌ریزی برمی‌گردد.",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، فعال کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#5B5FEF",
                cancelButtonColor: "#64748B"
            }).then((result) => {
                if (result.isConfirmed) form.submit();
            });
        });
    });

    //  Complete Sprint 
    document.querySelectorAll(".complete-sprint-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "بستن اسپرینت",
                text: "آیا از پایان‌دادن به این اسپرینت مطمئن هستید؟",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، ببند",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#5B5FEF",
                cancelButtonColor: "#64748B"
            }).then((result) => {
                if (result.isConfirmed) form.submit();
            });
        });
    });

    //  Live Search (Sprint Index) 
    const searchInput = document.getElementById("sprintSearchInput");
    const grid = document.getElementById("sprintGrid");

    if (searchInput && grid) {
        searchInput.addEventListener("input", function () {
            const term = searchInput.value.trim().toLowerCase();
            grid.querySelectorAll(".sprint-card").forEach(card => {
                const name = (card.dataset.name || "").toLowerCase();
                card.style.display = name.includes(term) ? "" : "none";
            });
        });
    }

        //  Date Sync (Create/Edit Sprint) — تاریخ پایان نباید قبل از تاریخ شروع باشد
        const startInput = document.getElementById("sprintStart");
        const endInput = document.getElementById("sprintEnd");
    
        function updateDuration() {
            if (!startInput || !endInput) return;
            
                const startValue = startInput.value;
            if (startValue) {
                    endInput.min = startValue;
                }
        }

    if (startInput) {
        startInput.addEventListener("change", updateDuration);
    }

    if (endInput) {
        endInput.addEventListener("change", updateDuration);
    }

    if (startInput || endInput) {

        let lastStartValue = startInput ? startInput.value : "";
        let lastEndValue = endInput ? endInput.value : "";

        setInterval(() => {

            const currentStartValue = startInput
                ? startInput.value
                : "";

            const currentEndValue = endInput
                ? endInput.value
                : "";

            if (
                currentStartValue !== lastStartValue ||
                currentEndValue !== lastEndValue
            ) {
                lastStartValue = currentStartValue;
                lastEndValue = currentEndValue;

                updateDuration();
            }

        }, 200);
    }

    // ===== Sprint Details — Tabs =====
    const sprintTabs = document.querySelectorAll(".sprint-tab[data-tab]");
    const sprintPanes = document.querySelectorAll(".sprint-tab-pane");

    sprintTabs.forEach(tab => {
        tab.addEventListener("click", function () {
            sprintTabs.forEach(t => t.classList.remove("active"));
            sprintPanes.forEach(p => p.classList.remove("active"));

            this.classList.add("active");
            const pane = document.getElementById("tab-" + this.dataset.tab);
            if (pane) pane.classList.add("active");

            if (this.dataset.tab === "planning" && this.dataset.lazyUrl && !this.dataset.loaded) {
                fetch(this.dataset.lazyUrl)
                    .then(res => res.text())
                    .then(html => {
                        pane.innerHTML = html;
                        this.dataset.loaded = "true";
                        if (window.SmartTask && typeof window.SmartTask.initPlanning === "function") {
                            window.SmartTask.initPlanning(pane);
                        }
                    })
                    .catch(() => {
                        pane.innerHTML = '<div class="team-empty-text">خطا در بارگذاری برنامه‌ریزی.</div>';
                    });
            }
        });
    });

    const urlParams = new URLSearchParams(window.location.search);
    const requestedTab = urlParams.get("tab");
    if (requestedTab) {
        const targetTab = document.querySelector(`.sprint-tab[data-tab="${requestedTab}"]`);
        if (targetTab) targetTab.click();
    }

});


(function () {
    const list = document.getElementById("sprintReportList");
    if (!list) return;

    const sprintId = list.dataset.sprintId;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    function renderReports(reports) {
        if (!reports || !reports.length) {
            list.innerHTML = `<div class="team-empty-text">هنوز گزارشی برای این اسپرینت تولید نشده است.</div>`;
            return;
        }

        list.innerHTML = reports.map(r => `
            <div class="sprint-report-item">
                <p>${r.content}</p>
                <div class="sprint-report-meta">
                    <span>تولیدشده توسط: ${r.generatedByName}</span>
                    <span>${new Date(r.generatedDate).toLocaleDateString("fa-IR")}</span>
                </div>
            </div>
        `).join("");
    }

    async function loadReports() {
        try {
            const response = await fetch(`/SprintReport/GetReports?sprintId=${sprintId}`);
            const data = await response.json();
            renderReports(data.reports);
        } catch (err) {
            list.innerHTML = `<div class="team-empty-text">خطا در بارگذاری گزارش‌ها.</div>`;
        }
    }

    loadReports();

    const generateBtn = document.getElementById("generateSprintReportBtn");
    if (generateBtn) {
        generateBtn.addEventListener("click", async function () {
            generateBtn.disabled = true;
            list.innerHTML = `
                <div class="ai-loading">
                    <div class="ai-spinner"></div>
                    <p>در حال تحلیل عملکرد اسپرینت و نگارش گزارش...</p>
                </div>`;

            try {
                const response = await fetch("/SprintReport/Generate", {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: `sprintId=${sprintId}&__RequestVerificationToken=${encodeURIComponent(token || "")}`
                });

                const data = await response.json();

                if (!data.success) {
                    list.innerHTML = `<div class="ai-error"><i class="fa-solid fa-triangle-exclamation"></i><p>${data.message}</p></div>`;
                } else {
                    await loadReports();
                }
            } catch (err) {
                list.innerHTML = `<div class="ai-error"><i class="fa-solid fa-triangle-exclamation"></i><p>ارتباط با سرور برقرار نشد.</p></div>`;
            } finally {
                generateBtn.disabled = false;
            }
        });
    }
})();