// Sidebar submenu expand/collapse — used by Sidebar.cshtml's parent links.
window.SmartTask = window.SmartTask || {};

SmartTask.toggleSidebarGroup = function (link) {
    const li = link.closest("li.has-children");
    if (li) li.classList.toggle("open");
    return false;
};

/* Prevent double-initialization if site.js is loaded more than once */
if (!window.__siteJsInit) {
    window.__siteJsInit = true;

    document.addEventListener("DOMContentLoaded", function () {

        const sidebarToggle = document.getElementById("sidebarToggle");
        const sidebar = document.getElementById("sidebar");
        const overlay = document.getElementById("sidebarOverlay");
        const isMobile = () => window.innerWidth <= 992;

        function closeMobileSidebar() {
            if (sidebar) sidebar.classList.remove("mobile-open");
            if (overlay) overlay.classList.remove("active");
            document.body.style.overflow = "";
        }

        if (sidebarToggle && sidebar) {
            sidebarToggle.addEventListener("click", function () {
                if (isMobile()) {
                    sidebar.classList.toggle("mobile-open");
                    overlay.classList.toggle("active");
                    document.body.style.overflow =
                        sidebar.classList.contains("mobile-open") ? "hidden" : "";
                } else {
                    sidebar.classList.toggle("collapsed");
                }
            });
        }

        if (overlay) {
            overlay.addEventListener("click", closeMobileSidebar);
        }

        /* Close mobile sidebar on Escape key */
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && sidebar && sidebar.classList.contains("mobile-open")) {
                closeMobileSidebar();
            }
        });

        /* Close mobile sidebar on window resize to desktop */
        window.addEventListener("resize", function () {
            if (!isMobile()) closeMobileSidebar();
        });

        /* ============================
           Logout Confirmation
           Uses event delegation to survive Bootstrap dropdown auto-close.
        ============================ */
        document.addEventListener("click", function (e) {
            var btn = e.target.closest("#logoutButton");
            if (!btn) return;
            e.preventDefault();
            e.stopPropagation();

            Swal.fire({
                title: "خروج از حساب",
                text: "آیا مطمئن هستید؟",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "بله، خارج شو",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#4f46e5",
                cancelButtonColor: "#64748b"
            }).then((result) => {
                if (result.isConfirmed) {
                    var form = document.getElementById("logoutForm");
                    if (form) form.submit();
                }
            });
        });

        /* ============================
           Delete Form Confirmations
        ============================ */
        document.querySelectorAll(".delete-form").forEach(form => {
            form.addEventListener("submit", function (e) {
                e.preventDefault();
                Swal.fire({
                    title: "حذف Workspace",
                    text: "آیا از حذف این فضای کاری مطمئن هستید؟",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonText: "بله، حذف کن",
                    cancelButtonText: "انصراف",
                    confirmButtonColor: "#EF4444",
                    cancelButtonColor: "#64748B"
                }).then((result) => {
                    if (result.isConfirmed) {
                        form.submit();
                    }
                });
            });
        });

    });
}

function showSuccess(message) {

    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'success',
        title: message,
        showConfirmButton: false,
        timer: 3500,
        timerProgressBar: true,
        didOpen: function (toast) {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

}

function showError(message) {

    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'error',
        title: message,
        showConfirmButton: false,
        timer: 5000,
        timerProgressBar: true,
        didOpen: function (toast) {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

}

function showWarning(message) {

    Swal.fire({
        toast: true,
        position: 'top-end',
        icon: 'warning',
        title: message,
        showConfirmButton: false,
        timer: 4000,
        timerProgressBar: true,
        didOpen: function (toast) {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });
}

async function enablePushNotifications() {
    if (!("Notification" in window)) {
        console.error("Browser does not support notifications.");
        return false;
    }

    if (Notification.permission === "granted") {
        console.log("Notifications already enabled.");
        return true;
    }

    if (Notification.permission === "denied") {
        console.warn("Notifications are blocked by the browser.");
        return false;
    }

    const permission = await Notification.requestPermission();

    console.log("Notification permission:", permission);

    return permission === "granted";
}
document
    .getElementById("notificationBellBtn")
    ?.addEventListener("click", async function () {

        const enabled = await enablePushNotifications();

        if (enabled) {
            console.log("Push notifications enabled.");
            if (window.ensureWebpushrSubscription) {
                window.ensureWebpushrSubscription();
            }
        }
    });