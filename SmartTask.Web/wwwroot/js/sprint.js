document.addEventListener("DOMContentLoaded", function () {

    // ===== Delete Sprint =====
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

    // ===== Activate Sprint =====
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

    // ===== Complete Sprint =====
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

    // ===== Live Search (Sprint Index) =====
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

    // ===== Live Preview (Create Sprint) =====
    const nameInput = document.getElementById("sprintName");
    const goalInput = document.getElementById("sprintGoal");
    const startInput = document.getElementById("sprintStart");
    const endInput = document.getElementById("sprintEnd");
    const capacityInput = document.getElementById("sprintCapacity");

    const previewName = document.getElementById("previewName");
    const previewGoal = document.getElementById("previewGoal");
    const previewDuration = document.getElementById("previewDuration");
    const previewCapacity = document.getElementById("previewCapacity");

    if (nameInput && previewName) {
        nameInput.addEventListener("input", () => {
            previewName.innerText = nameInput.value || "اسپرینت جدید";
        });
    }

    if (goalInput && previewGoal) {
        goalInput.addEventListener("input", () => {
            previewGoal.innerText = goalInput.value || "هدف اسپرینت اینجا نمایش داده خواهد شد...";
        });
    }

    if (capacityInput && previewCapacity) {
        capacityInput.addEventListener("input", () => {
            previewCapacity.innerText = `${capacityInput.value || 0} ظرفیت`;
        });
    }

    function updateDuration() {
        if (!startInput || !endInput || !previewDuration) return;
        const start = new Date(startInput.value);
        const end = new Date(endInput.value);
        if (!isNaN(start) && !isNaN(end) && end > start) {
            const days = Math.round((end - start) / (1000 * 60 * 60 * 24));
            previewDuration.innerText = `${days} روز`;
        }
        // اجازه نمی‌دهیم تاریخ پایان قبل از تاریخ شروع انتخاب شود
        if (startInput.value) {
            endInput.min = startInput.value;
        }
    }

    if (startInput) startInput.addEventListener("change", updateDuration);
    if (endInput) endInput.addEventListener("change", updateDuration);

});