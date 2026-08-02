document.addEventListener("DOMContentLoaded", function () {

    // ===== Live Search =====
    const searchInput = document.getElementById("boardSearchInput");
    const kanban = document.getElementById("boardKanban");

    if (searchInput && kanban) {
        searchInput.addEventListener("input", function () {
            const term = searchInput.value.trim().toLowerCase();
            kanban.querySelectorAll(".board-task-card").forEach(card => {
                const title = (card.dataset.title || "").toLowerCase();
                card.style.display = title.includes(term) ? "" : "none";
            });
        });
    }

    // ===== Kanban Drag & Drop (Status Change) =====
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
            group: "board-kanban",
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