// ==========================================================
//                  SmartTask Sprint UI
// ==========================================================

document.addEventListener("DOMContentLoaded", function () {

    // ===== Tab System =====
    const tabButtons = document.querySelectorAll(".sprint-tab[data-tab]");
    const tabPanes = document.querySelectorAll(".sprint-tab-pane");

    tabButtons.forEach(btn => {
        btn.addEventListener("click", function () {
            const targetTab = btn.dataset.tab;

            // Switch active button
            tabButtons.forEach(b => b.classList.remove("active"));
            btn.classList.add("active");

            // Switch active pane
            tabPanes.forEach(pane => pane.classList.remove("active"));
            const targetPane = document.getElementById("tab-" + targetTab);
            if (targetPane) {
                targetPane.classList.add("active");

                // Lazy load if needed
                if (btn.dataset.lazyUrl && !targetPane.dataset.loaded) {
                    loadTabContent(targetPane, btn.dataset.lazyUrl);
                }
            }
        });
    });

    // ===== Lazy Load Tab Content (for Planning tab) =====
    function loadTabContent(pane, url) {
        pane.dataset.loaded = "true";

        fetch(url)
            .then(r => {
                if (!r.ok) throw new Error("Failed to load tab");
                return r.text();
            })
            .then(html => {
                pane.innerHTML = html;
                // Initialize planning.js for this pane
                if (window.SmartTask && window.SmartTask.initPlanning) {
                    window.SmartTask.initPlanning(pane);
                }
            })
            .catch(err => {
                console.error(err);
                pane.innerHTML = '<div class="workspace-empty"><i class="fa-solid fa-exclamation-triangle"></i><h3>خطا در بارگذاری محتوا</h3></div>';
            });
    }

    // ===== Handle ?tab= Query String (for old Planning URL redirect) =====
    const urlParams = new URLSearchParams(window.location.search);
    const tabParam = urlParams.get("tab");
    if (tabParam) {
        const targetBtn = document.querySelector(`.sprint-tab[data-tab="${tabParam}"]`);
        if (targetBtn) {
            targetBtn.click();
        }
    }

    // ===== More Actions Dropdown =====
    const moreBtn = document.getElementById("sprintMoreBtn");
    const moreMenu = document.getElementById("sprintMoreMenu");

    if (moreBtn && moreMenu) {
        moreBtn.addEventListener("click", function (e) {
            e.stopPropagation();
            moreMenu.classList.toggle("active");
        });

        document.addEventListener("click", function () {
            moreMenu.classList.remove("active");
        });
    }

    // ===== Delete Sprint Confirmation =====
    const deleteForm = document.querySelector(".delete-sprint-form");
    if (deleteForm) {
        deleteForm.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف اسپرینت",
                text: "آیا از حذف این اسپرینت مطمئن هستید؟ این عمل غیرقابل بازگشت است.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، حذف کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#EF4444",
                cancelButtonColor: "#64748B"
            }).then(result => {
                if (result.isConfirmed) {
                    deleteForm.submit();
                }
            });
        });
    }

    // ===== Activate Sprint Confirmation =====
    const activateForm = document.querySelector(".activate-sprint-form");
    if (activateForm) {
        activateForm.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "فعال‌سازی اسپرینت",
                text: "آیا از فعال‌سازی این اسپرینت مطمئن هستید؟",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، فعال کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#4F46E5",
                cancelButtonColor: "#64748B"
            }).then(result => {
                if (result.isConfirmed) {
                    activateForm.submit();
                }
            });
        });
    }

    // ===== Complete Sprint Confirmation =====
    const completeForm = document.querySelector(".complete-sprint-form");
    if (completeForm) {
        completeForm.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "بستن اسپرینت",
                text: "آیا از بستن این اسپرینت و اتمام آن مطمئن هستید؟",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، ببند",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#4F46E5",
                cancelButtonColor: "#64748B"
            }).then(result => {
                if (result.isConfirmed) {
                    completeForm.submit();
                }
            });
        });
    }

    // ===== Sprint Report Loading (if tab-overview exists and sprint is completed) =====
    const reportList = document.getElementById("sprintReportList");
    if (reportList) {
        loadSprintReports(reportList.dataset.sprintId);
    }

    const generateBtn = document.getElementById("generateSprintReportBtn");
    if (generateBtn) {
        generateBtn.addEventListener("click", function () {
            generateSprintReport(generateBtn.dataset.sprintId);
        });
    }

});

// ===== Sprint Report Functions =====
function loadSprintReports(sprintId) {
    const container = document.getElementById("sprintReportList");
    if (!container) return;

    fetch(`/SprintReport/List?sprintId=${sprintId}`)
        .then(r => r.json())
        .then(data => {
            if (data.length === 0) {
                container.innerHTML = '<div class="team-empty-text">هنوز گزارشی تولید نشده است.</div>';
            } else {
                container.innerHTML = data.map(report => `
                    <div class="sprint-report-card">
                        <div class="sprint-report-header">
                            <i class="fa-solid fa-file-lines"></i>
                            <span>${report.title}</span>
                            <small>${report.createDate}</small>
                        </div>
                        <div class="sprint-report-body">
                            ${report.summary || "—"}
                        </div>
                    </div>
                `).join("");
            }
        })
        .catch(() => {
            container.innerHTML = '<div class="team-empty-text">خطا در بارگذاری گزارش‌ها</div>';
        });
}

function generateSprintReport(sprintId) {
    const btn = document.getElementById("generateSprintReportBtn");
    if (!btn) return;

    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> در حال تولید...';

    fetch(`/SprintReport/Generate?sprintId=${sprintId}`, { method: "POST" })
        .then(r => r.json())
        .then(data => {
            if (data.success) {
                showSuccess("گزارش با موفقیت تولید شد.");
                loadSprintReports(sprintId);
            } else {
                showError(data.message || "خطا در تولید گزارش");
            }
        })
        .catch(() => {
            showError("خطا در تولید گزارش");
        })
        .finally(() => {
            btn.disabled = false;
            btn.innerHTML = '<i class="fa-solid fa-wand-magic-sparkles"></i> تولید گزارش جدید';
        });
}
