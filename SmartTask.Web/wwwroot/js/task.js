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

    if (kanban && typeof Sortable !== "undefined") {
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

        function updateColumnCounts() {
            kanban.querySelectorAll(".task-column").forEach(col => {
                const list = col.querySelector(".task-list");
                const countBadge = col.querySelector(".planning-count");
                if (list && countBadge) {
                    countBadge.innerText = list.querySelectorAll(".task-card").length;
                }
            });
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
    }

    // ===== Delete Comment / Attachment / Checklist / ChecklistItem / TimeLog =====
    function confirmDelete(selector, title, text) {
        document.querySelectorAll(selector).forEach(form => {
            form.addEventListener("submit", function (e) {
                e.preventDefault();
                Swal.fire({
                    title: title,
                    text: text,
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
    }

    confirmDelete(".delete-comment-form", "حذف نظر", "آیا از حذف این نظر مطمئن هستید؟");
    confirmDelete(".delete-attachment-form", "حذف فایل", "آیا از حذف این فایل مطمئن هستید؟");
    confirmDelete(".delete-checklist-form", "حذف Checklist", "آیا از حذف این Checklist و همه آیتم‌هایش مطمئن هستید؟");
    confirmDelete(".delete-checklist-item-form", "حذف آیتم", "آیا از حذف این آیتم مطمئن هستید؟");
    confirmDelete(".delete-timelog-form", "حذف رکورد زمانی", "آیا از حذف این رکورد مطمئن هستید؟");

    // ===== Toggle Checklist Item =====
    document.querySelectorAll(".checklist-item-toggle").forEach(checkbox => {
        checkbox.addEventListener("change", async function () {
            const itemId = this.dataset.id;
            const item = this.closest(".subtask-item");

            const formData = new FormData();
            formData.append("id", itemId);

            const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
            if (tokenInput) formData.append("__RequestVerificationToken", tokenInput.value);

            try {
                const response = await fetch("/Checklist/ToggleItem", { method: "POST", body: formData });

                if (!response.ok) {
                    this.checked = !this.checked;
                    Swal.fire({
                        icon: "error",
                        title: "خطا",
                        text: "تغییر وضعیت آیتم ذخیره نشد.",
                        confirmButtonColor: "#5B5FEF"
                    });
                    return;
                }

                item.classList.toggle("completed", this.checked);

            } catch (err) {
                this.checked = !this.checked;
                console.error("Network error:", err);
            }
        });
    });

    // ===== Live Timer Display =====
    const liveTimer = document.getElementById("liveTimer");
    if (liveTimer) {
        const startTime = new Date(liveTimer.dataset.start);

        function updateTimerDisplay() {
            const now = new Date();
            let diffSeconds = Math.floor((now - startTime) / 1000);
            if (diffSeconds < 0) diffSeconds = 0;

            const hours = Math.floor(diffSeconds / 3600);
            const minutes = Math.floor((diffSeconds % 3600) / 60);
            const seconds = diffSeconds % 60;

            liveTimer.innerText =
                String(hours).padStart(2, "0") + ":" +
                String(minutes).padStart(2, "0") + ":" +
                String(seconds).padStart(2, "0");
        }

        updateTimerDisplay();
        setInterval(updateTimerDisplay, 1000);
    }

});