document.addEventListener("DOMContentLoaded", function () {

    // ===== صفحه Index: تب‌ها =====
    const tabButtons = document.querySelectorAll(".workload-tab-btn");
    if (tabButtons.length) {
        tabButtons.forEach(btn => {
            btn.addEventListener("click", function () {
                tabButtons.forEach(b => b.classList.remove("active"));
                btn.classList.add("active");

                document.querySelectorAll(".trade-tab-panel").forEach(panel => {
                    panel.style.display = "none";
                });

                document.getElementById(`tab-${btn.dataset.tab}`).style.display = "block";
            });
        });
    }

    // ===== مودال شروع درخواست (توی Task Details) =====
    const openBtn = document.getElementById("openTradeModalBtn");
    if (!openBtn) return;

    const overlay = document.getElementById("tradeModalOverlay");
    const closeBtn = document.getElementById("tradeModalClose");
    const cancelBtn = document.getElementById("tradeModalCancel");
    const targetUserSelect = document.getElementById("tradeTargetUserSelect");
    const targetTaskSelect = document.getElementById("tradeTargetTaskSelect");
    const projectId = overlay.querySelector('input[name="projectId"]').value;
    const taskId = overlay.querySelector('input[name="taskId"]').value;

    openBtn.addEventListener("click", () => overlay.classList.add("active"));
    closeBtn.addEventListener("click", () => overlay.classList.remove("active"));
    cancelBtn.addEventListener("click", () => overlay.classList.remove("active"));
    overlay.addEventListener("click", (e) => { if (e.target === overlay) overlay.classList.remove("active"); });

    targetUserSelect.addEventListener("change", async function () {
        const userId = targetUserSelect.value;

        targetTaskSelect.innerHTML = `<option value="">در حال بارگذاری...</option>`;
        targetTaskSelect.disabled = true;

        if (!userId) {
            targetTaskSelect.innerHTML = `<option value="">— ابتدا عضو را انتخاب کنید —</option>`;
            return;
        }

        try {
            const response = await fetch(`/TaskTrade/GetUserTasks?projectId=${projectId}&userId=${userId}&excludeTaskId=${taskId}`);
            const data = await response.json();

            if (data.tasks && data.tasks.length) {
                targetTaskSelect.innerHTML = `<option value="">— بدون تسک متقابل (واگذاری) —</option>` +
                    data.tasks.map(t => `<option value="${t.value}">${t.text}</option>`).join("");
            } else {
                targetTaskSelect.innerHTML = `<option value="">این عضو تسک بازی برای مبادله ندارد</option>`;
            }

            targetTaskSelect.disabled = false;
        } catch (err) {
            targetTaskSelect.innerHTML = `<option value="">خطا در بارگذاری</option>`;
        }
    });

});