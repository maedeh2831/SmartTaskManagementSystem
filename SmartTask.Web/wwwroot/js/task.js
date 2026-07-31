document.addEventListener("DOMContentLoaded", function () {

    // ===== Delete Task =====
    document.querySelectorAll(".delete-task-form").forEach(form => {
        form.addEventListener("submit", function (e) {
            e.preventDefault();
            Swal.fire({
                title: "حذف Task",
                text: "آیا از حذف این Task مطمئن هستید؟",
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

    // ===== Kanban Drag & Drop =====
    const kanban = document.getElementById("taskKanban");
    if (!kanban || typeof Sortable === "undefined") return;

    const token = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');

    function getToken() {
        return token ? token.value : "";
    }

    async function changeStatus(taskId, status) {
        const formData = new FormData();
        formData.append("taskId", taskId);
        formData.append("status", status);
        formData.append("__RequestVerificationToken", getToken());

        try {
            const response = await fetch("/Task/ChangeStatus", { method: "POST", body: formData });

            if (!response.ok) {
                const text = await response.text();
                console.error(`Server error [${response.status}]:`, text);
                Swal.fire({
                    icon: "error",
                    title: `خطا (${response.status})`,
                    text: "تغییر وضعیت ذخیره نشد.",
                    confirmButtonColor: "#5B5FEF"
                });
            }
        } catch (err) {
            console.error("Network error:", err);
        }
    }

    kanban.querySelectorAll(".task-list").forEach(list => {
        const canManage = list.dataset.canManage === "true";
        if (!canManage) return;

        Sortable.create(list, {
            group: "task-kanban",
            animation: 150,
            handle: ".task-drag-handle",
            ghostClass: "task-ghost",
            chosenClass: "task-chosen",
            onAdd: function (evt) {
                const taskId = evt.item.dataset.id;
                const newStatus = evt.to.dataset.status;
                changeStatus(taskId, newStatus);
                updateColumnCounts();
            },
            onSort: updateColumnCounts
        });
    });

    function updateColumnCounts() {
        kanban.querySelectorAll(".task-column").forEach(col => {
            const list = col.querySelector(".task-list");
            const countBadge = col.querySelector(".planning-count");
            if (list && countBadge) {
                countBadge.innerText = list.querySelectorAll(".task-card").length;
            }
        });
    }

});