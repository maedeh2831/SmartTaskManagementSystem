document.addEventListener("DOMContentLoaded", function () {

    function getToken() {
        const tokenInput = document.querySelector('#notificationAntiForgeryForm input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : "";
    }

    function escapeHtml(str) {
        const div = document.createElement("div");
        div.textContent = str;
        return div.innerHTML;
    }

    function updateAllBadges(unreadCount) {
        document.querySelectorAll(".notification-badge").forEach(badge => {
            if (unreadCount > 0) {
                badge.textContent = unreadCount > 9 ? "9+" : unreadCount;
                badge.classList.remove("d-none");
            } else {
                badge.classList.add("d-none");
            }
        });
    }

    // ===== Mark Single Notification As Read =====
    async function markAsRead(id) {
        const formData = new FormData();
        formData.append("id", id);
        formData.append("__RequestVerificationToken", getToken());

        try {
            const response = await fetch("/Notification/MarkAsRead", { method: "POST", body: formData });
            if (!response.ok) return;

            const data = await response.json();
            updateAllBadges(data.unreadCount);

            document.querySelectorAll(`[data-id="${id}"]`).forEach(el => {
                if (el.classList.contains("notification-dropdown-item") || el.classList.contains("notification-center-item")) {
                    el.classList.remove("unread");
                }
            });

            const dot = document.querySelector(`.notification-dropdown-item[data-id="${id}"] .notification-dot`);
            if (dot) dot.remove();

            const readBtn = document.querySelector(`.notification-read-btn[data-id="${id}"]`);
            if (readBtn) readBtn.remove();

            // Update filter counts
            if (typeof updateFilterCounts === "function") updateFilterCounts();
            if (typeof applyNotifFilters === "function") applyNotifFilters();
        } catch (err) {
            console.error("Network error:", err);
        }
    }

    document.querySelectorAll(".notification-dropdown-item.unread").forEach(item => {
        item.addEventListener("click", function () {
            markAsRead(this.dataset.id);
        });
    });

    document.querySelectorAll(".notification-read-btn").forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.stopPropagation();
            markAsRead(this.dataset.id);
        });
    });

    // ===== Mark All As Read =====
    async function markAllAsRead() {
        const formData = new FormData();
        formData.append("__RequestVerificationToken", getToken());

        try {
            const response = await fetch("/Notification/MarkAllAsRead", { method: "POST", body: formData });
            if (!response.ok) return;

            updateAllBadges(0);
            document.querySelectorAll(".notification-dropdown-item, .notification-center-item").forEach(el => {
                el.classList.remove("unread");
                el.dataset.read = "true";
            });
            document.querySelectorAll(".notification-dot, .notification-read-btn, #markAllReadBtn, #markAllReadPageBtn")
                .forEach(el => el.remove());
            // Update filter counts
            if (typeof updateFilterCounts === "function") updateFilterCounts();
            if (typeof applyNotifFilters === "function") applyNotifFilters();
        } catch (err) {
            console.error("Network error:", err);
        }
    }

    const markAllBtn = document.getElementById("markAllReadBtn");
    if (markAllBtn) markAllBtn.addEventListener("click", markAllAsRead);

    const markAllPageBtn = document.getElementById("markAllReadPageBtn");
    if (markAllPageBtn) {
        markAllPageBtn.addEventListener("click", function () {
            this.classList.add("loading");
            this.disabled = true;
            markAllAsRead().finally(() => {
                this.classList.remove("loading");
                this.disabled = false;
            });
        });
    }

    // ===== Delete Notification (Notification Center only) =====
    document.querySelectorAll(".notification-delete-btn").forEach(btn => {
        btn.addEventListener("click", function (e) {
            e.stopPropagation();
            const id = this.dataset.id;

            Swal.fire({
                title: "حذف اعلان",
                text: "آیا از حذف این اعلان مطمئن هستید؟",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، حذف کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#EF4444",
                cancelButtonColor: "#64748B"
            }).then(async (result) => {
                if (!result.isConfirmed) return;

                const formData = new FormData();
                formData.append("id", id);
                formData.append("__RequestVerificationToken", getToken());

                try {
                    const response = await fetch("/Notification/Delete", { method: "POST", body: formData });
                    if (!response.ok) return;

                    const data = await response.json();
                    updateAllBadges(data.unreadCount);

                    const el = document.querySelector(`.notification-center-item[data-id="${id}"]`);
                    if (el) {
                        el.classList.add("removing");
                        el.addEventListener("animationend", () => {
                            el.remove();
                            // Update filter counts after removal
                            updateFilterCounts();
                            // Show empty state if all removed
                            const list = document.getElementById("notificationCenterList");
                            if (list && !list.querySelector(".notification-center-item")) {
                                const card = list.closest(".notification-center-card");
                                if (card) {
                                    card.innerHTML = '<div class="notification-empty-state"><div class="notification-empty-icon"><i class="fa-regular fa-bell-slash"></i></div><h3>هنوز اعلانی ندارید</h3><p>اعلان‌های شما از فعالیت‌های پروژه، نظرات و تغییرات وضعیت اینجا نمایش داده خواهند شد.</p></div>';
                                }
                            }
                        });
                    }
                } catch (err) {
                    console.error("Network error:", err);
                }
            });
        });
    });

    // ===== Notification Filter Tabs =====
    function toFaDigits(str) {
        var fa = "\u06F0\u06F1\u06F2\u06F3\u06F4\u06F5\u06F6\u06F7\u06F8\u06F9";
        return String(str).replace(/[0-9]/g, function (d) { return fa[+d]; });
    }

    let activeNotifFilter = "all";
    const notifFilterTabs = document.querySelectorAll(".notification-filter-tab[data-filter]");
    const notifFilterSummary = document.getElementById("notificationFilterSummary");
    const notifCards = document.querySelectorAll("#notificationCenterList .notification-center-item");

    function updateFilterCounts() {
        const items = document.querySelectorAll("#notificationCenterList .notification-center-item");
        let total = items.length;
        let unread = 0;
        items.forEach(function (el) {
            if (el.classList.contains("unread")) unread++;
        });
        const read = total - unread;

        // Update tab counts
        notifFilterTabs.forEach(function (tab) {
            const f = tab.dataset.filter;
            const countEl = tab.querySelector(".notification-filter-count");
            if (countEl) {
                let c = 0;
                if (f === "all") c = total;
                else if (f === "unread") c = unread;
                else if (f === "read") c = read;
                countEl.textContent = toFaDigits(c);
            }
        });
    }

    function applyNotifFilters() {
        let visible = 0;
        const total = notifCards.length;

        notifCards.forEach(function (card) {
            const isRead = card.dataset.read === "true";
            let match = true;
            if (activeNotifFilter === "unread") match = !isRead;
            else if (activeNotifFilter === "read") match = isRead;
            card.style.display = match ? "" : "none";
            if (match) visible++;
        });

        if (notifFilterSummary) {
            notifFilterSummary.innerHTML = "نمایش <strong class=\"fa-digits\">" + toFaDigits(visible) + "</strong> از <strong class=\"fa-digits\">" + toFaDigits(total) + "</strong> اعلان";
        }
    }

    notifFilterTabs.forEach(function (tab) {
        tab.addEventListener("click", function () {
            notifFilterTabs.forEach(function (t) {
                t.classList.remove("active");
                t.setAttribute("aria-selected", "false");
            });
            tab.classList.add("active");
            tab.setAttribute("aria-selected", "true");
            activeNotifFilter = tab.dataset.filter;
            applyNotifFilters();
        });
    });

    // ===== SignalR Real-Time Connection =====
    if (typeof signalR === "undefined") return;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/notification")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveNotification", function (notification) {
        updateAllBadges(notification.unreadCount);

        const title = escapeHtml(notification.title);
        const message = escapeHtml(notification.message);
        const type = escapeHtml(notification.type || "reminder");

        const dropdownList = document.getElementById("notificationDropdownList");
        if (dropdownList) {
            const emptyState = dropdownList.querySelector(".notification-empty");
            if (emptyState) emptyState.remove();

            const item = document.createElement("div");
            item.className = "notification-dropdown-item unread";
            item.dataset.id = notification.id;
            item.innerHTML = `
                <div class="notification-item-icon"><i class="fa-solid fa-bell"></i></div>
                <div class="notification-item-body">
                    <span class="notification-item-title">${title}</span>
                    <span class="notification-item-message">${message}</span>
                    <span class="notification-item-time">همین الان</span>
                </div>
                <span class="notification-dot"></span>
            `;
            item.addEventListener("click", function () {
                markAsRead(notification.id);
            });
            dropdownList.prepend(item);
        }

        const centerList = document.getElementById("notificationCenterList");
        if (centerList) {
            // Remove empty state if present
            const emptyState = centerList.closest(".notification-center-card")?.querySelector(".notification-empty-state");
            if (emptyState) emptyState.remove();

            const item = document.createElement("div");
            item.className = "notification-center-item unread";
            item.dataset.id = notification.id;
            item.dataset.read = "false";
            item.dataset.type = type;
            item.setAttribute("role", "listitem");
            item.innerHTML = `
                <div class="notification-item-icon"><i class="fa-solid fa-bell"></i></div>
                <div class="notification-item-body">
                    <div style="display:flex; align-items:center; gap:8px; flex-wrap:wrap;">
                        <span class="notification-item-title">${title}</span>
                        <span class="notification-type-badge type-default"><i class="fa-solid fa-bell" style="font-size:8px;"></i> یادآوری</span>
                    </div>
                    <span class="notification-item-message">${message}</span>
                    <span class="notification-item-time"><i class="fa-regular fa-clock" style="font-size:9px; opacity:.7;"></i> همین الان</span>
                </div>
                <div class="notification-item-actions">
                    <button type="button" class="notification-read-btn" data-id="${notification.id}" title="علامت‌گذاری به‌عنوان خوانده‌شده" aria-label="علامت‌گذاری به‌عنوان خوانده‌شده">
                        <i class="fa-solid fa-check"></i>
                    </button>
                    <button type="button" class="notification-delete-btn" data-id="${notification.id}" title="حذف" aria-label="حذف اعلان">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
            `;
            centerList.prepend(item);

            // Rebind events for new item
            item.querySelector(".notification-read-btn")?.addEventListener("click", function (e) {
                e.stopPropagation();
                markAsRead(this.dataset.id);
            });
            item.querySelector(".notification-delete-btn")?.addEventListener("click", function (e) {
                e.stopPropagation();
                this.closest(".notification-center-item")?.remove();
                if (typeof updateFilterCounts === "function") updateFilterCounts();
            });

            // Update filter counts
            if (typeof updateFilterCounts === "function") updateFilterCounts();
        }
    });

    connection.start().catch(err => console.error("SignalR connection error:", err));
});