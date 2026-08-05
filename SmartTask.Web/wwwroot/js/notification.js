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
            });
            document.querySelectorAll(".notification-dot, .notification-read-btn, #markAllReadBtn, #markAllReadPageBtn")
                .forEach(el => el.remove());
        } catch (err) {
            console.error("Network error:", err);
        }
    }

    const markAllBtn = document.getElementById("markAllReadBtn");
    if (markAllBtn) markAllBtn.addEventListener("click", markAllAsRead);

    const markAllPageBtn = document.getElementById("markAllReadPageBtn");
    if (markAllPageBtn) markAllPageBtn.addEventListener("click", markAllAsRead);

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
                    if (el) el.remove();
                } catch (err) {
                    console.error("Network error:", err);
                }
            });
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
            const item = document.createElement("div");
            item.className = "notification-center-item unread";
            item.dataset.id = notification.id;
            item.innerHTML = `
                <div class="notification-item-icon"><i class="fa-solid fa-bell"></i></div>
                <div class="notification-item-body">
                    <span class="notification-item-title">${title}</span>
                    <span class="notification-item-message">${message}</span>
                    <span class="notification-item-time">همین الان</span>
                </div>
                <div class="notification-item-actions">
                    <button type="button" class="notification-read-btn" data-id="${notification.id}" title="علامت‌گذاری به‌عنوان خوانده‌شده">
                        <i class="fa-solid fa-check"></i>
                    </button>
                    <button type="button" class="notification-delete-btn" data-id="${notification.id}" title="حذف">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
            `;
            centerList.prepend(item);
        }
    });

    connection.start().catch(err => console.error("SignalR connection error:", err));
});