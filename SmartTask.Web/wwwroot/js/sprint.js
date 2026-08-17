// ==========================================================
//                  SmartTask Sprint UI
// ==========================================================

// ---- Helpers -------------------------------------------------

// Convert Latin digits (0-9) to Persian digits (۰-۹)
function toFaDigits(str) {
    var fa = "۰۱۲۳۴۵۶۷۸۹";
    return String(str).replace(/[0-9]/g, function (d) { return fa[+d]; });
}

// Fill a cycle ring from its data-percent attribute
function setRingFill(circle) {
    var c = parseFloat(circle.getAttribute("stroke-dasharray")) || 0;
    var pct = Math.min(100, Math.max(0, parseFloat(circle.dataset.percent) || 0));
    circle.style.strokeDashoffset = String(c * (1 - pct / 100));
}

document.addEventListener("DOMContentLoaded", function () {

    // ===== Persian digits for .fa-digits elements =====
    document.querySelectorAll(".fa-digits").forEach(function (el) {
        el.textContent = toFaDigits(el.textContent);
    });

    // ===== Cycle rings =====
    document.querySelectorAll(".sprint-ring-fill[data-percent]").forEach(setRingFill);

    // ===== Sliding tab indicator =====
    function positionTabIndicator() {
        var bar = document.getElementById("sprintTabs");
        var ind = document.getElementById("sprintTabIndicator");
        if (!bar || !ind) return;
        var btn = bar.querySelector(".sprint-tab.active");
        if (!btn) {
            ind.style.display = "none";
            return;
        }
        ind.style.display = "block";
        ind.style.left = btn.offsetLeft + "px";
        ind.style.width = btn.offsetWidth + "px";
    }

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

            positionTabIndicator();
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
                // Convert any Persian digits inside the loaded content
                pane.querySelectorAll(".fa-digits").forEach(function (el) {
                    el.textContent = toFaDigits(el.textContent);
                });
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

    // Re-position the indicator once layout/fonts settle
    setTimeout(positionTabIndicator, 0);
    setTimeout(positionTabIndicator, 200);
    window.addEventListener("load", positionTabIndicator);
    window.addEventListener("resize", positionTabIndicator);

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
                confirmButtonColor: "#5B5FEF",
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
                confirmButtonColor: "#5B5FEF",
                cancelButtonColor: "#64748B"
            }).then(result => {
                if (result.isConfirmed) {
                    completeForm.submit();
                }
            });
        });
    }

    // ===== Go to Planning tab (from Stories empty state) =====
    const goToPlanningBtn = document.getElementById("goToPlanningBtn");
    if (goToPlanningBtn) {
        goToPlanningBtn.addEventListener("click", function () {
            const planningTab = document.querySelector('.sprint-tab[data-tab="planning"]');
            if (planningTab) planningTab.click();
        });
    }

    // ===== Live Index search =====
    const searchInput = document.getElementById("sprintSearchInput");
    if (searchInput) {
        const sprintCards = document.querySelectorAll("#sprintGrid .sprint-card");
        searchInput.addEventListener("input", function () {
            const q = this.value.trim().toLowerCase();
            sprintCards.forEach(function (card) {
                const name = (card.dataset.name || "").toLowerCase();
                card.style.display = (!q || name.includes(q)) ? "" : "none";
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

    // ===== Live Create preview =====
    initSprintPreview(document);

});

// ===== Live preview on Create page =====
function initSprintPreview(scope) {
    const previewName = document.getElementById("previewName");
    if (!previewName) return;

    const nameInput = scope.querySelector('input[name="Name"]');
    const goalInput = scope.querySelector('textarea[name="Goal"]');
    const capacityInput = scope.querySelector('input[name="Capacity"]');
    const startInput = scope.querySelector('input[name="StartDate"]');
    const endInput = scope.querySelector('input[name="EndDate"]');

    const previewGoal = document.getElementById("previewGoal");
    const previewCapacity = document.getElementById("previewCapacity");
    const previewDuration = document.getElementById("previewDuration");
    const previewRingNum = document.getElementById("previewRingNum");
    const previewRingFill = document.getElementById("previewRingFill");

    function update() {
        if (nameInput && previewName) {
            previewName.textContent = nameInput.value.trim() || "اسپرینت جدید";
        }

        if (goalInput && previewGoal) {
            const val = goalInput.value.trim();
            previewGoal.textContent = val || "هدف اسپرینت اینجا نمایش داده خواهد شد...";
            previewGoal.classList.toggle("is-empty", !val);
        }

        let days = 14;
        if (startInput && endInput && startInput.value && endInput.value) {
            const s = new Date(startInput.value);
            const e = new Date(endInput.value);
            if (!isNaN(s) && !isNaN(e)) {
                days = Math.max(0, Math.round((e - s) / 86400000));
            }
        }

        if (previewRingNum) previewRingNum.textContent = toFaDigits(days);
        if (previewDuration) {
            previewDuration.textContent = days > 0 ? "دورهٔ " + toFaDigits(days) + " روزه" : "تاریخ‌ها را مشخص کنید";
        }
        if (previewRingFill) {
            // Closed cycle = the sprint's full duration
            const c = parseFloat(previewRingFill.getAttribute("stroke-dasharray")) || 0;
            previewRingFill.style.strokeDashoffset = "0";
            if (c) previewRingFill.style.strokeDasharray = String(c);
        }
        if (capacityInput && previewCapacity) {
            const cap = capacityInput.value.trim();
            previewCapacity.textContent = cap ? toFaDigits(cap) + " ظرفیت" : "—";
        }
    }

    [nameInput, goalInput, capacityInput, startInput, endInput].forEach(function (el) {
        if (el) {
            el.addEventListener("input", update);
            el.addEventListener("change", update);
        }
    });

    update();
}

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
