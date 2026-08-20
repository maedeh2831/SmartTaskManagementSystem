document.addEventListener("DOMContentLoaded", function () {

    // ===== Delete Project =====
    document.querySelectorAll(".delete-project-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف پروژه",
                text: "آیا از حذف این پروژه مطمئن هستید؟ این عملیات قابل بازگشت نیست.",
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

    // ===== Live Search (Project Index) =====
    const searchInput = document.getElementById("projectSearchInput");
    const grid = document.getElementById("projectGrid");

    if (searchInput && grid) {
        searchInput.addEventListener("input", function () {
            const term = searchInput.value.trim().toLowerCase();
            grid.querySelectorAll(".project-card").forEach(card => {
                const name = (card.dataset.name || "").toLowerCase();
                card.style.display = name.includes(term) ? "" : "none";
            });
        });
    }

    // ===== Auto-uppercase Key =====
    const keyInput = document.getElementById("projectKey");
    if (keyInput) {
        keyInput.addEventListener("input", () => {
            keyInput.value = keyInput.value.toUpperCase();
        });
    }

    // ===== Live Preview (Create Project) =====
    const nameInput = document.getElementById("projectName");
    const descInput = document.getElementById("projectDescription");
    const previewName = document.getElementById("previewName");
    const previewDesc = document.getElementById("previewDescription");
    const previewKey = document.getElementById("previewKey");
    const previewLogo = document.getElementById("previewLogo");

    if (nameInput && previewName) {
        nameInput.addEventListener("input", () => {
            previewName.innerText = nameInput.value || "پروژه جدید";
        });
    }

    if (keyInput && previewKey) {
        keyInput.addEventListener("input", () => {
            previewKey.innerText = keyInput.value || "KEY";
        });
    }

    if (descInput && previewDesc) {
        descInput.addEventListener("input", () => {
            previewDesc.innerText = descInput.value || "توضیح پروژه اینجا نمایش داده خواهد شد...";
        });
    }

    if (previewLogo) {
        document.querySelectorAll(".color-radio").forEach(x => {
            x.addEventListener("change", () => {
                previewLogo.style.background = x.value;
            });
        });

        document.querySelectorAll(".icon-radio").forEach(x => {
            x.addEventListener("change", () => {
                previewLogo.innerHTML = `<i class="${x.value}"></i>`;
            });
        });
    }

    // ===== Project Tab System =====
    var tabBtns = document.querySelectorAll(".project-tab-btn[data-tab]");
    var tabPanes = document.querySelectorAll(".project-tab-pane");

    tabBtns.forEach(function (btn) {
        btn.addEventListener("click", function () {
            tabBtns.forEach(function (b) { b.classList.remove("active"); });
            btn.classList.add("active");
            tabPanes.forEach(function (p) { p.classList.remove("active"); });
            var targetPane = document.getElementById("tab-" + btn.dataset.tab);
            if (targetPane) targetPane.classList.add("active");
        });
    });

});