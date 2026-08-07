document.addEventListener("DOMContentLoaded", function () {

    // ===== Tabs =====
    const tabButtons = document.querySelectorAll(".workload-tab-btn");
    tabButtons.forEach(btn => {
        btn.addEventListener("click", function () {
            tabButtons.forEach(b => b.classList.remove("active"));
            btn.classList.add("active");

            document.querySelectorAll(".workload-tab-panel").forEach(panel => {
                panel.style.display = "none";
            });

            document.getElementById(`tab-${btn.dataset.tab}`).style.display = "block";
        });
    });

    // ===== Animate Bars =====
    document.querySelectorAll(".workload-bar-fill").forEach(bar => {
        const targetWidth = bar.style.width;
        bar.style.width = "0";
        requestAnimationFrame(() => {
            setTimeout(() => { bar.style.width = targetWidth; }, 100);
        });
    });

    // ===== Capacity Edit (فقط برای مدیران) =====
    const container = document.querySelector(".workspace-container[data-can-manage]");
    const canManage = container?.dataset.canManage === "true";

    if (canManage) {
        document.querySelectorAll(".workload-capacity-form").forEach(form => {
            form.style.display = "flex";
        });
    }

    document.querySelectorAll(".workload-capacity-save").forEach(btn => {
        btn.addEventListener("click", async function () {
            const form = btn.closest(".workload-capacity-form");
            const memberId = form.dataset.memberId;
            const value = form.querySelector(".workload-capacity-input").value;
            const projectId = new URLSearchParams(window.location.search).get("projectId");

            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

            const response = await fetch("/Workload/UpdateCapacity", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `projectMemberId=${memberId}&projectId=${projectId}&weeklyCapacityHours=${value}&__RequestVerificationToken=${encodeURIComponent(token || "")}`
            });

            const data = await response.json();
            if (data.success) {
                Swal.fire({ icon: "success", title: "ظرفیت به‌روزرسانی شد", timer: 1200, showConfirmButton: false });
            }
        });
    });

});