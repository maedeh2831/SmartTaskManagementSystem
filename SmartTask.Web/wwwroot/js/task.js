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
(function () {
    const aiBtn = document.getElementById("aiSuggestBtn");
    if (!aiBtn) return;

    const overlay = document.getElementById("aiModalOverlay");
    const body = document.getElementById("aiModalBody");
    const closeBtn = document.getElementById("aiModalClose");
    const cancelBtn = document.getElementById("aiModalCancel");
    const form = document.getElementById("aiSubTaskForm");
    const hiddenInputsContainer = document.getElementById("aiHiddenInputs");
    const taskId = aiBtn.dataset.taskId;
    const token = document.querySelector('#aiSubTaskForm input[name="__RequestVerificationToken"]').value;

    function openModal() { overlay.classList.add("active"); }
    function closeModal() { overlay.classList.remove("active"); }

    function renderLoading() {
        body.innerHTML = `
            <div class="ai-loading">
                <div class="ai-spinner"></div>
                <p>در حال تحلیل Task و تولید پیشنهادها...</p>
            </div>`;
    }

    function renderError(message) {
        body.innerHTML = `
            <div class="ai-error">
                <i class="fa-solid fa-triangle-exclamation fa-2x"></i>
                <p>${message}</p>
            </div>`;
    }

    function renderSuggestions(suggestions) {
        body.innerHTML = "";
        suggestions.forEach(title => {
            const row = document.createElement("label");
            row.className = "ai-suggestion-item";
            row.innerHTML = `
                <input type="checkbox" class="ai-suggestion-checkbox" checked value="${title.replace(/"/g, "&quot;")}" />
                <span>${title}</span>
            `;
            body.appendChild(row);
        });
    }

    aiBtn.addEventListener("click", async function () {
        openModal();
        renderLoading();

        try {
            const response = await fetch("/Task/GenerateAiSubTasks", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `taskId=${taskId}&__RequestVerificationToken=${encodeURIComponent(token)}`
            });

            const data = await response.json();

            if (!data.success) {
                renderError(data.message || "خطایی رخ داد.");
                return;
            }

            renderSuggestions(data.suggestions);
        } catch (err) {
            renderError("ارتباط با سرور برقرار نشد.");
        }
    });

    closeBtn.addEventListener("click", closeModal);
    cancelBtn.addEventListener("click", closeModal);
    overlay.addEventListener("click", function (e) {
        if (e.target === overlay) closeModal();
    });

    form.addEventListener("submit", function (e) {

        hiddenInputsContainer.innerHTML = "";

        const selected = document.querySelectorAll(
            ".ai-suggestion-checkbox:checked"
        );

        if (!selected.length) {
            e.preventDefault();

            Swal.fire({
                icon: "warning",
                title: "انتخابی انجام نشده",
                text: "حداقل یک زیروظیفه را انتخاب کنید.",
                confirmButtonText: "باشه"
            });

            return;
        }

        selected.forEach(cb => {
            const input = document.createElement("input");
            input.type = "hidden";
            input.name = "titles";
            input.value = cb.value;
            hiddenInputsContainer.appendChild(input);
        });
    });
})();

// ===== Task Details — Tab Switching =====
(function () {

    const tabs = document.querySelectorAll(".task-tab-btn");
    const panes = document.querySelectorAll(".task-tab-pane");

    if (!tabs.length || !panes.length)
        return;

    function activateTab(tabKey, updateUrl = true) {

        tabs.forEach(tab => {
            const isActive = tab.dataset.tab === tabKey;

            tab.classList.toggle("active", isActive);
            tab.setAttribute("aria-selected", isActive ? "true" : "false");
        });

        panes.forEach(pane => {
            const isActive = pane.dataset.tabPane === tabKey;

            pane.classList.toggle("active", isActive);
            pane.hidden = !isActive;
        });

        if (updateUrl) {
            const url = new URL(window.location.href);

            url.searchParams.set("tab", tabKey);

            window.history.replaceState(
                { tab: tabKey },
                "",
                url
            );
        }
    }

    // ------------------------------------------
    // Tab click
    // ------------------------------------------

    tabs.forEach(tab => {

        tab.addEventListener("click", function () {

            const tabKey = this.dataset.tab;

            if (!tabKey)
                return;

            activateTab(tabKey);

        });

    });

    // ------------------------------------------
    // Browser Back / Forward
    // ------------------------------------------

    window.addEventListener("popstate", function () {

        const url = new URL(window.location.href);
        const tabKey = url.searchParams.get("tab");

        if (tabKey && document.querySelector(
            `.task-tab-btn[data-tab="${tabKey}"]`
        )) {
            activateTab(tabKey, false);
        } else {
            activateTab("overview", false);
        }

    });

    // ------------------------------------------
    // Initial tab
    // ------------------------------------------

    const url = new URL(window.location.href);
    const initialTab = url.searchParams.get("tab");

    const validInitialTab =
        initialTab &&
            document.querySelector(
                `.task-tab-btn[data-tab="${initialTab}"]`
            )
            ? initialTab
            : "overview";

    activateTab(validInitialTab, false);

})();
/* ==========================================================
   Create Task Modal
   ========================================================== */

(function () {

    const modal = document.querySelector("[data-task-modal]");
    const openButton = document.querySelector("[data-task-modal-open]");

    if (!modal || !openButton)
        return;

    const closeButtons =
        modal.querySelectorAll("[data-task-modal-close]");

    function openModal() {

        modal.classList.add("is-open");

        document.body.classList.add("task-modal-open");

        const titleInput =
            modal.querySelector("#taskTitle");

        setTimeout(() => {
            titleInput?.focus();
        }, 150);
    }

    function closeModal() {

        modal.classList.remove("is-open");

        document.body.classList.remove("task-modal-open");
    }

    openButton.addEventListener("click", openModal);

    closeButtons.forEach(button => {

        button.addEventListener("click", closeModal);

    });

    modal.addEventListener("click", function (event) {

        if (event.target === modal) {
            closeModal();
        }

    });

    document.addEventListener("keydown", function (event) {

        if (event.key === "Escape" &&
            modal.classList.contains("is-open")) {

            closeModal();

        }

    });

})();