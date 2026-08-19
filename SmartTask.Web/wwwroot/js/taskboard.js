// TaskBoard AJAX Filtering
var TaskBoardFilter = {
    filter: function () {
        var form = document.getElementById("taskboardFilterForm");
        if (!form) return;
        var projectId = form.dataset.projectId;
        var params = new URLSearchParams();
        params.set("projectId", projectId);

        form.querySelectorAll("select").forEach(function (sel) {
            if (sel.name && sel.value) params.set(sel.name, sel.value);
        });

        var tabPane = form.closest(".project-tab-pane");
        if (tabPane) tabPane.style.opacity = "0.5";

        fetch("/Project/TaskBoardFiltered?" + params.toString(), {
            headers: { "X-Requested-With": "XMLHttpRequest" }
        })
        .then(function (r) { return r.text(); })
        .then(function (html) {
            if (tabPane) tabPane.style.opacity = "1";
            // Replace the board content but keep the filter form
            var temp = document.createElement("div");
            temp.innerHTML = html;
            var newForm = temp.querySelector("#taskboardFilterForm");
            var newKanban = temp.querySelector("#boardKanban") || temp.querySelector(".workspace-empty");
            var newAntiForgery = temp.querySelector("#antiForgeryForm");

            if (newForm) form.outerHTML = newForm.outerHTML;
            var oldKanban = document.getElementById("boardKanban") || document.querySelector(".workspace-empty");
            if (oldKanban && newKanban) oldKanban.outerHTML = newKanban.outerHTML;
            if (newAntiForgery) {
                var oldAF = document.getElementById("antiForgeryForm");
                if (oldAF) oldAF.outerHTML = newAntiForgery.outerHTML;
            }
            // Re-init drag & drop
            if (typeof SmartTask !== "undefined" && SmartTask.initPageScripts) {
                SmartTask.initPageScripts();
            }
        })
        .catch(function () {
            if (tabPane) tabPane.style.opacity = "1";
        });
    },
    clear: function () {
        var form = document.getElementById("taskboardFilterForm");
        if (!form) return;
        form.querySelectorAll("select").forEach(function (sel) { sel.value = ""; });
        this.filter();
    }
};

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