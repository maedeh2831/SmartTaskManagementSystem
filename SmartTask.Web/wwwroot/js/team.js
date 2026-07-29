document.addEventListener("DOMContentLoaded", function () {

    // ===== Delete Team =====
    document.querySelectorAll(".delete-team-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف تیم",
                text: "آیا از حذف این تیم مطمئن هستید؟ این عملیات قابل بازگشت نیست.",
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

    // ===== Remove Member =====
    document.querySelectorAll(".remove-member-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف عضو",
                text: "آیا از حذف این عضو از تیم مطمئن هستید؟",
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

    // ===== Change Role =====
    document.querySelectorAll(".change-role-form .role-select").forEach(select => {
        const originalValue = select.value;
        select.addEventListener("change", function () {
            const form = select.closest("form");
            Swal.fire({
                title: "تغییر نقش",
                text: "آیا از تغییر نقش این عضو مطمئن هستید؟",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، تغییر بده",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#5B5FEF",
                cancelButtonColor: "#64748B"
            }).then((result) => {
                if (result.isConfirmed) form.submit();
                else select.value = originalValue;
            });
        });
    });

    // ===== Live Search (Team Index) =====
    const searchInput = document.getElementById("teamSearchInput");
    const grid = document.getElementById("teamGrid");

    if (searchInput && grid) {
        searchInput.addEventListener("input", function () {
            const term = searchInput.value.trim().toLowerCase();
            grid.querySelectorAll(".team-card").forEach(card => {
                const name = (card.dataset.name || "").toLowerCase();
                card.style.display = name.includes(term) ? "" : "none";
            });
        });
    }

    // ===== Live Preview (Create/Edit Team) =====
    const nameInput = document.getElementById("teamName");
    const descInput = document.getElementById("teamDescription");
    const previewName = document.getElementById("previewName");
    const previewDesc = document.getElementById("previewDescription");
    const previewLogo = document.getElementById("previewLogo");
    const previewVisibility = document.getElementById("previewVisibility");

    if (nameInput && previewName) {
        nameInput.addEventListener("input", () => {
            previewName.innerText = nameInput.value || "تیم جدید";
        });
    }

    if (descInput && previewDesc) {
        descInput.addEventListener("input", () => {
            previewDesc.innerText = descInput.value || "توضیح تیم اینجا نمایش داده خواهد شد...";
        });
    }

    if (previewLogo) {
        document.querySelectorAll(".color-radio").forEach(x => {
            x.addEventListener("change", () => {
                previewLogo.style.background = x.value;
            });
        });
    }

    if (previewVisibility) {
        document.querySelectorAll('input[name="IsPrivate"]').forEach(x => {
            x.addEventListener("change", () => {
                previewVisibility.innerHTML = x.value === "true"
                    ? '<i class="fa-solid fa-lock"></i> خصوصی'
                    : '<i class="fa-solid fa-globe"></i> عمومی';
            });
        });
    }

});