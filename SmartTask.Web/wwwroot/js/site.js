// Sidebar submenu expand/collapse
window.SmartTask = window.SmartTask || {};

SmartTask.toggleSidebarGroup = function (link) {
    var li = link.closest("li.has-children");
    if (li) li.classList.toggle("open");
    return false;
};

// SPA-like navigation
SmartTask.spaNavigate = async function (url) {
    var contentArea = document.querySelector(".content-area");
    if (!contentArea) return false;

    contentArea.classList.add("spa-loading");
    contentArea.classList.remove("spa-loaded");

    try {
        var resp = await fetch(url, {
            headers: { "X-Requested-With": "XMLHttpRequest" },
            credentials: "same-origin"
        });
        if (!resp.ok) throw new Error(resp.status);
        var ct = resp.headers.get("content-type") || "";
        if (!ct.includes("text/html")) throw new Error("Not HTML");
        var html = await resp.text();

        var parser = new DOMParser();
        var doc = parser.parseFromString(html, "text/html");
        var newContent = doc.querySelector(".content-area");
        var newTitle = doc.querySelector("title");

        if (!newContent) throw new Error("No content");

        // All CSS is preloaded in _Layout.cshtml
        contentArea.innerHTML = newContent.innerHTML;
        if (newTitle) document.title = newTitle.textContent;

        // Execute inline scripts from content area
        contentArea.querySelectorAll("script").forEach(function (old) {
            var s = document.createElement("script");
            s.textContent = old.textContent;
            document.body.appendChild(s);
            old.remove();
        });

        // Execute scripts from @section Scripts (marked with data-page-scripts)
        var pageScriptsEl = doc.querySelector("[data-page-scripts]");
        if (pageScriptsEl) {
            // Execute inline scripts (like chatBootstrap JSON)
            pageScriptsEl.querySelectorAll("script:not([src])").forEach(function (old) {
                var text = (old.textContent || "").trim();
                if (!text) return;
                var s = document.createElement("script");
                var type = old.getAttribute("type");
                if (type) s.setAttribute("type", type);
                s.textContent = text;
                document.body.appendChild(s);
            });
            // Execute src scripts (like chat.js, taskboard.js)
            pageScriptsEl.querySelectorAll("script[src]").forEach(function (old) {
                var src = old.getAttribute("src") || "";
                if (!src) return;
                var s2 = document.createElement("script");
                s2.src = src;
                document.body.appendChild(s2);
            });
        }

        contentArea.classList.remove("spa-loading");
        contentArea.classList.add("spa-loaded");
        history.pushState({}, "", url);
        SmartTask.initPageScripts();
        window.scrollTo(0, 0);
        return true;
    } catch (e) {
        contentArea.classList.remove("spa-loading");
        return false;
    }
};

SmartTask.initPageScripts = function () {
    // Persian digits
    document.querySelectorAll(".fa-digits").forEach(function (el) {
        var fa = "\u06F0\u06F1\u06F2\u06F3\u06F4\u06F5\u06F6\u06F7\u06F8\u06F9";
        el.textContent = el.textContent.replace(/[0-9]/g, function (d) { return fa[+d]; });
    });
    // Donut charts
    document.querySelectorAll(".dashboard-donut[data-progress]").forEach(function (donut) {
        var progress = parseFloat(donut.dataset.progress) || 0;
        var circle = donut.querySelector(".donut-value");
        if (!circle) return;
        var c = 2 * Math.PI * 52;
        circle.style.strokeDasharray = c;
        circle.style.strokeDashoffset = c;
        requestAnimationFrame(function () {
            circle.style.strokeDashoffset = c - (progress / 100) * c;
        });
    });
    // Sprint rings
    document.querySelectorAll(".sprint-ring-fill[data-percent]").forEach(function (circle) {
        var c = parseFloat(circle.getAttribute("stroke-dasharray")) || 0;
        var pct = Math.min(100, Math.max(0, parseFloat(circle.dataset.percent) || 0));
        circle.style.strokeDashoffset = String(c * (1 - pct / 100));
    });
    // Re-execute page-specific scripts (like chat.js)
    document.querySelectorAll(".content-area script[src]").forEach(function (old) {
        var s = document.createElement("script");
        s.src = old.src;
        document.body.appendChild(s);
    });
    // Init sweet-scroll if available
    if (typeof SweetScroll !== "undefined") {
        if (!window._sweetScrollInstance) {
            window._sweetScrollInstance = new SweetScroll({
                top: 0,
                easing: "easeInOutQuart",
                duration: 600,
                vertical: true
            });
        }
    }
};

if (!window.__siteJsInit) {
    window.__siteJsInit = true;
    document.addEventListener("DOMContentLoaded", function () {

        var sidebarToggle = document.getElementById("sidebarToggle");
        var sidebar = document.getElementById("sidebar");
        var overlay = document.getElementById("sidebarOverlay");
        var isMobile = function () { return window.innerWidth <= 992; };

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

        // Flyout submenu hover handling for collapsed sidebar
        var _flyoutTimer = null;
        document.querySelectorAll(".sidebar-menu li.has-children").forEach(function (li) {
            li.addEventListener("mouseenter", function () {
                if (!sidebar || !sidebar.classList.contains("collapsed")) return;
                clearTimeout(_flyoutTimer);
                li.classList.add("flyout-open");
            });
            li.addEventListener("mouseleave", function () {
                if (!sidebar || !sidebar.classList.contains("collapsed")) return;
                _flyoutTimer = setTimeout(function () {
                    li.classList.remove("flyout-open");
                }, 150);
            });
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && sidebar && sidebar.classList.contains("mobile-open")) {
                closeMobileSidebar();
            }
        });

        window.addEventListener("resize", function () {
            if (!isMobile()) closeMobileSidebar();
        });

        // Logout
        document.addEventListener("click", function (e) {
            var btn = e.target.closest("#logoutButton");
            if (!btn) return;
            e.preventDefault();
            e.stopPropagation();
            Swal.fire({
                title: "\u062E\u0631\u0648\u062C \u0627\u0632 \u062D\u0633\u0627\u0628",
                text: "\u0622\u06CC\u0627 \u0645\u0637\u0645\u0626\u0646 \u0647\u0633\u062A\u06CC\u062F\u061F",
                icon: "question",
                showCancelButton: true,
                confirmButtonText: "\u0628\u0644\u0647\u060C \u062E\u0627\u0631\u062C \u0634\u0648",
                cancelButtonText: "\u0627\u0646\u0635\u0631\u0627\u0641",
                confirmButtonColor: "#4f46e5",
                cancelButtonColor: "#64748b"
            }).then(function (result) {
                if (result.isConfirmed) {
                    var form = document.getElementById("logoutForm");
                    if (form) form.submit();
                }
            });
        });

        // Delete forms
        document.querySelectorAll(".delete-form").forEach(function (form) {
            form.addEventListener("submit", function (e) {
                e.preventDefault();
                Swal.fire({
                    title: "\u062D\u0630\u0641",
                    text: "\u0622\u06CC\u0627 \u0627\u0632 \u062D\u0630\u0641 \u0645\u0637\u0645\u0626\u0646 \u0647\u0633\u062A\u06CC\u062F\u061F",
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonText: "\u0628\u0644\u0647\u060C \u062D\u0630\u0641 \u06A9\u0646",
                    cancelButtonText: "\u0627\u0646\u0635\u0631\u0627\u0641",
                    confirmButtonColor: "#EF4444",
                    cancelButtonColor: "#64748B"
                }).then(function (result) {
                    if (result.isConfirmed) {
                        form.submit();
                    }
                });
            });
        });

        // SPA navigation
        var _spaNavigating = false;

        document.addEventListener("click", function (e) {
            if (e.ctrlKey || e.metaKey || e.shiftKey || e.button !== 0) return;
            if (_spaNavigating) return;

            var link = e.target.closest(".sidebar-menu a[href]");
            if (!link) return;
            if (link.classList.contains("sidebar-parent-link")) return;
            if (link.classList.contains("sidebar-locked-link")) return;
            if (link.getAttribute("href") === "#") return;

            var url = link.getAttribute("href");
            if (!url || url.indexOf("/") !== 0) return;
            if (url.indexOf("//") !== -1) return;

            var cleanUrl = url.split("?")[0];
            if (cleanUrl === window.location.pathname) return;
            if (link.closest("form")) return;
            // Skip SPA for Chat page — it needs full page load for SignalR
            if (cleanUrl.indexOf("/Chat") === 0) return;

            e.preventDefault();
            e.stopPropagation();
            _spaNavigating = true;

            document.querySelectorAll(".sidebar-menu li.active").forEach(function (item) {
                item.classList.remove("active");
            });
            var li = link.closest("li");
            if (li) li.classList.add("active");

            SmartTask.spaNavigate(url).then(function (ok) {
                _spaNavigating = false;
                if (!ok) window.location.href = url;
            }).catch(function () {
                _spaNavigating = false;
                window.location.href = url;
            });
        });

        window.addEventListener("popstate", function () {
            if (!_spaNavigating) {
                SmartTask.spaNavigate(window.location.href);
            }
        });

    });
}

function showSuccess(message) {
    Swal.fire({ toast: true, position: "top-end", icon: "success", title: message, showConfirmButton: false, timer: 3500, timerProgressBar: true });
}

function showError(message) {
    Swal.fire({ toast: true, position: "top-end", icon: "error", title: message, showConfirmButton: false, timer: 5000, timerProgressBar: true });
}

function showWarning(message) {
    Swal.fire({ toast: true, position: "top-end", icon: "warning", title: message, showConfirmButton: false, timer: 4000, timerProgressBar: true });
}

async function enablePushNotifications() {
    if (!("Notification" in window)) return false;
    if (Notification.permission === "granted") return true;
    if (Notification.permission === "denied") return false;
    var permission = await Notification.requestPermission();
    return permission === "granted";
}

document.getElementById("notificationBellBtn")
    ?.addEventListener("click", async function () {
        var enabled = await enablePushNotifications();
        if (enabled && window.ensureWebpushrSubscription) {
            window.ensureWebpushrSubscription();
        }
    });
