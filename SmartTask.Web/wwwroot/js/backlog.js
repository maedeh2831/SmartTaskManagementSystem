document.addEventListener("DOMContentLoaded", function () {

    // ===== Delete Story =====
    document.querySelectorAll(".delete-story-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف User Story",
                text: "آیا از حذف این آیتم مطمئن هستید؟",
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

    // ===== Live Search =====
    const searchInput = document.getElementById("backlogSearchInput");
    const list = document.getElementById("backlogList");

    if (searchInput && list) {
        searchInput.addEventListener("input", function () {
            const term = searchInput.value.trim().toLowerCase();
            list.querySelectorAll(".backlog-row").forEach(row => {
                const title = (row.dataset.title || "").toLowerCase();
                row.style.display = title.includes(term) ? "" : "none";
            });
        });
    }

    // ===== Drag & Drop Reorder (SortableJS) =====
    if (list && typeof Sortable !== "undefined") {
        Sortable.create(list, {
            handle: ".backlog-drag-handle",
            animation: 150,
            ghostClass: "backlog-ghost",
            chosenClass: "backlog-chosen",
            dragClass: "backlog-drag",
            onEnd: function () {
                saveOrder();
            }
        });

        function saveOrder() {
            const projectId = list.dataset.projectId;
            const orderedIds = Array.from(list.querySelectorAll(".backlog-row"))
                .map(row => row.dataset.id);

            const formData = new FormData();
            formData.append("ProjectId", projectId);
            orderedIds.forEach(id => formData.append("OrderedIds", id));

            const token = document.querySelector('input[name="__RequestVerificationToken"]');
            if (token) {
                formData.append("__RequestVerificationToken", token.value);
            }

            fetch("/Backlog/Reorder", {
                method: "POST",
                body: formData
            }).catch(() => {
                Swal.fire({
                    icon: "error",
                    title: "خطا",
                    text: "ذخیره ترتیب جدید با مشکل مواجه شد.",
                    confirmButtonColor: "#5B5FEF"
                });
            });
        }
    }

});