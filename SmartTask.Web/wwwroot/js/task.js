document.addEventListener("DOMContentLoaded", function () {

    // ===== Smart Back Button =====
    var backBtn = document.getElementById("taskBackBtn");
    if (backBtn) {
        var fallback = backBtn.dataset.fallbackUrl || "/";
        var referrer = document.referrer;
        var sameOrigin = referrer && referrer.indexOf(window.location.origin) === 0;

        // Detect where the user came from
        if (sameOrigin) {
            if (referrer.indexOf("/Sprint/Details") !== -1) {
                document.getElementById("taskBackLabel").textContent = "بازگشت به اسپرینت";
            } else if (referrer.indexOf("/Backlog") !== -1) {
                document.getElementById("taskBackLabel").textContent = "بازگشت به بک‌لاگ";
            } else if (referrer.indexOf("/TaskBoard") !== -1) {
                document.getElementById("taskBackLabel").textContent = "بازگشت به TaskBoard";
            } else if (referrer.indexOf("/UserStory/Details") !== -1) {
                document.getElementById("taskBackLabel").textContent = "بازگشت به Story";
            } else {
                document.getElementById("taskBackLabel").textContent = "بازگشت";
            }
        }

        backBtn.addEventListener("click", function () {
            if (sameOrigin && window.history.length > 1) {
                window.history.back();
            } else {
                window.location.href = fallback;
            }
        });
    }

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


// ==========================================================
// AI Sub-Task Suggestions
// ==========================================================
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


// ==========================================================
// Task Details — Tab switching
// ==========================================================
(function () {

    const nav = document.getElementById("taskSectionNav");
    if (!nav) return;

    const buttons = nav.querySelectorAll(".task-section-link[data-tab]");
    if (!buttons.length) return;

    function switchTab(targetId) {
        // hide all sections
        document.querySelectorAll(".task-section").forEach(function (sec) {
            sec.hidden = true;
            sec.classList.remove("task-section--active");
        });

        // deactivate all tab buttons
        buttons.forEach(function (btn) {
            btn.classList.remove("active");
        });

        // show target section
        var target = document.getElementById(targetId);
        if (target) {
            target.hidden = false;
            target.classList.add("task-section--active");
        }

        // activate clicked button
        var activeBtn = nav.querySelector('[data-tab="' + targetId + '"]');
        if (activeBtn) {
            activeBtn.classList.add("active");
        }

        // remember active tab in sessionStorage so refresh keeps position
        try { sessionStorage.setItem("taskActiveTab", targetId); } catch (e) { }
    }

    // attach click handlers
    buttons.forEach(function (btn) {
        btn.addEventListener("click", function () {
            switchTab(this.dataset.tab);
        });
    });

    // restore tab from sessionStorage on page load
    try {
        var saved = sessionStorage.getItem("taskActiveTab");
        if (saved && document.getElementById(saved)) {
            switchTab(saved);
        }
    } catch (e) { }

})();


// ==========================================================
// Task Create / Edit Modal
// پشتیبانی از چند مودال هم‌زمان روی یک صفحه
// (createTaskModal در Index.cshtml + editTaskModal در Details.cshtml)
// ==========================================================
(function () {

    const modals = document.querySelectorAll("[data-task-modal]");

    if (!modals.length)
        return;

    function getModalByKey(key) {
        return document.querySelector(`[data-task-modal="${key}"]`);
    }

    function openModal(modal) {

        modal.classList.add("is-open");

        document.body.classList.add("task-modal-open");

        const firstField = modal.querySelector(
            "input:not([type='hidden']), textarea"
        );

        setTimeout(() => {
            firstField?.focus();
        }, 150);
    }

    function closeModal(modal) {

        modal.classList.remove("is-open");

        // اگر مودال دیگری باز نبود، اسکرول صفحه آزاد بشه
        const anyOpen = document.querySelector(".task-modal-backdrop.is-open");

        if (!anyOpen) {
            document.body.classList.remove("task-modal-open");
        }
    }

    // باز کردن مودال از طریق دکمه‌های data-task-modal-open="key"
    document.querySelectorAll("[data-task-modal-open]").forEach(btn => {

        btn.addEventListener("click", function () {

            const key = this.dataset.taskModalOpen;
            const modal = getModalByKey(key);

            if (modal) {
                openModal(modal);
            }

        });

    });

    // بستن هر مودال با دکمه‌های داخل خودش + کلیک روی بک‌دراپ
    modals.forEach(modal => {

        modal.querySelectorAll("[data-task-modal-close]").forEach(button => {
            button.addEventListener("click", () => closeModal(modal));
        });

        modal.addEventListener("click", function (event) {
            if (event.target === modal) {
                closeModal(modal);
            }
        });

    });

    // بستن با کلید Escape (هر مودالی که باز است)
    document.addEventListener("keydown", function (event) {

        if (event.key !== "Escape")
            return;

        const openModal_ = document.querySelector(".task-modal-backdrop.is-open");

        if (openModal_) {
            closeModal(openModal_);
        }

    });

})();


// ==========================================================
// AJAX Submit — فرم‌های مودال Create/Edit Task
// (کنترلر Json برمی‌گرداند، پس دیگر نباید فرم به‌صورت
//  معمولی Submit شود، وگرنه مرورگر صفحه‌ی خام JSON نشان می‌دهد)
// ==========================================================
(function () {

    const forms = document.querySelectorAll("[data-ajax-task-form]");

    if (!forms.length)
        return;

    forms.forEach(form => {

        form.addEventListener("submit", async function (e) {

            e.preventDefault();

            // ولیدیشن کلاینت (در صورت وجود jQuery Unobtrusive Validation)
            if (window.jQuery && window.jQuery.fn && window.jQuery.fn.valid) {
                const isValid = window.jQuery(form).valid();
                if (isValid === false) {
                    return;
                }
            }

            const submitBtn = form.querySelector(".task-modal-submit");
            const cancelBtn = form.querySelector(".task-modal-cancel");
            const originalHtml = submitBtn ? submitBtn.innerHTML : null;

            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> <span>در حال ذخیره...</span>';
            }
            if (cancelBtn) {
                cancelBtn.disabled = true;
            }

            try {
                const formData = new FormData(form);

                const response = await fetch(form.action, {
                    method: form.method || "POST",
                    body: formData
                });

                let data;

                try {
                    data = await response.json();
                } catch {
                    throw new Error("پاسخ نامعتبر از سرور دریافت شد.");
                }

                if (!data.success) {
                    await Swal.fire({
                        icon: "error",
                        title: "خطا",
                        text: data.message || "عملیات ناموفق بود.",
                        confirmButtonText: "باشه",
                        confirmButtonColor: "#5B5FEF"
                    });
                    return;
                }

                await Swal.fire({
                    icon: "success",
                    title: "انجام شد",
                    text: data.message || "با موفقیت ذخیره شد.",
                    confirmButtonText: "باشه",
                    confirmButtonColor: "#5B5FEF",
                    timer: 1300,
                    timerProgressBar: true
                });

                window.location.reload();

            } catch (err) {
                console.error("Ajax task form error:", err);
                Swal.fire({
                    icon: "error",
                    title: "خطا",
                    text: "ارتباط با سرور برقرار نشد.",
                    confirmButtonText: "باشه",
                    confirmButtonColor: "#5B5FEF"
                });
            } finally {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalHtml;
                }
                if (cancelBtn) {
                    cancelBtn.disabled = false;
                }
            }

        });

    });

})();