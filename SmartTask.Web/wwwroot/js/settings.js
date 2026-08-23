document.addEventListener("DOMContentLoaded", function () {
    const tabButtons = document.querySelectorAll(".settings-tab-btn");
    const tabPanels = document.querySelectorAll(".settings-tab-panel");
    const STORAGE_KEY = "smarttask_settings_active_tab";

    function activateTab(tabName) {
        tabButtons.forEach(function (btn) {
            btn.classList.toggle("active", btn.dataset.tab === tabName);
        });
        tabPanels.forEach(function (panel) {
            panel.classList.toggle("active", panel.dataset.tabPanel === tabName);
        });
    }

    tabButtons.forEach(function (btn) {
        btn.addEventListener("click", function () {
            const tabName = btn.dataset.tab;
            activateTab(tabName);
            localStorage.setItem(STORAGE_KEY, tabName);
        });
    });


    document.querySelectorAll(".settings-tab-panel form").forEach(function (form) {
        form.addEventListener("submit", function () {
            const panel = form.closest(".settings-tab-panel");
            if (panel) {
                localStorage.setItem(STORAGE_KEY, panel.dataset.tabPanel);
            }
        });
    });

    const savedTab = localStorage.getItem(STORAGE_KEY);
    const initialTab = (savedTab && document.querySelector('.settings-tab-btn[data-tab="' + savedTab + '"]'))
        ? savedTab
        : "account";

    activateTab(initialTab);

    // ===== Security: Active Devices =====
    var logoutAllBtn = document.getElementById("logoutAllDevicesBtn");
    var devicesList = document.getElementById("activeDevicesList");
    var devicesContainer = document.getElementById("devicesContainer");
    var devicesLoading = document.getElementById("devicesLoading");

    if (logoutAllBtn && devicesList) {
        logoutAllBtn.addEventListener("click", function () {
            var isVisible = devicesList.style.display !== "none";
            if (isVisible) {
                devicesList.style.display = "none";
                return;
            }
            devicesList.style.display = "block";
            devicesLoading.style.display = "block";
            devicesContainer.innerHTML = "";

            fetch("/Settings/GetActiveSessions", {
                headers: { "X-Requested-With": "XMLHttpRequest" }
            })
            .then(function (res) {
                if (!res.ok) throw new Error("HTTP " + res.status);
                return res.json();
            })
            .then(function (sessions) {
                devicesLoading.style.display = "none";
                if (!sessions || sessions.length === 0) {
                    devicesContainer.innerHTML = '<div style="padding:16px;color:var(--muted);font-size:13px;">هیچ نشست فعالی یافت نشد.</div>';
                    return;
                }
                var html = '';
                sessions.forEach(function (s) {
                    var icon = s.operatingSystem.indexOf("Windows") !== -1 ? "fa-brands fa-windows" :
                               s.operatingSystem.indexOf("macOS") !== -1 ? "fa-brands fa-apple" :
                               s.operatingSystem.indexOf("Linux") !== -1 ? "fa-brands fa-linux" :
                               s.operatingSystem.indexOf("Android") !== -1 ? "fa-brands fa-android" :
                               s.operatingSystem.indexOf("iOS") !== -1 ? "fa-brands fa-apple" : "fa-solid fa-desktop";
                    var currentBadge = s.isCurrent ? '<span style="font-size:10px;background:var(--primary-light);color:var(--primary);padding:2px 8px;border-radius:999px;font-weight:700;">دستگاه فعلی</span>' : '';
                    html += '<div class="settings-device-item" style="display:flex;align-items:center;gap:12px;padding:12px 16px;border:1px solid var(--border);border-radius:10px;font-size:13px;">'
                        + '<i class="' + icon + '" style="font-size:20px;color:var(--primary);width:24px;text-align:center;"></i>'
                        + '<div style="flex:1;min-width:0;">'
                        + '<div style="font-weight:700;color:var(--text);">' + s.deviceInfo + ' — ' + s.operatingSystem + '</div>'
                        + '<div style="font-size:11px;color:var(--muted);margin-top:2px;">' + s.ipAddress + ' · آخرین فعالیت: ' + s.lastActivity + '</div>'
                        + '</div>'
                        + currentBadge
                        + '</div>';
                });
                devicesContainer.innerHTML = html;

                // Add logout all button
                var hasOthers = sessions.some(function (s) { return !s.isCurrent; });
                if (hasOthers) {
                    var logoutAllAction = document.createElement("div");
                    logoutAllAction.style.cssText = "padding:12px 16px;border-top:1px solid var(--border);";
                    logoutAllAction.innerHTML = '<button type="button" id="confirmLogoutAllBtn" style="width:100%;padding:10px;border:none;border-radius:8px;background:var(--danger);color:#fff;font-size:13px;font-weight:700;font-family:Vazirmatn;cursor:pointer;transition:background .2s;">'
                        + '<i class="fa-solid fa-right-from-bracket" style="margin-inline-start:6px;"></i>'
                        + 'خروج از همه دستگاه‌ها'
                        + '</button>';
                    devicesContainer.appendChild(logoutAllAction);

                    document.getElementById("confirmLogoutAllBtn").addEventListener("click", function () {
                        Swal.fire({
                            title: "خروج از همه دستگاه‌ها",
                            text: "آیا از خروج از تمام دستگاه‌ها مطمئن هستید؟ تمام نشست‌های دیگر غیرفعال خواهند شد.",
                            icon: "question",
                            showCancelButton: true,
                            confirmButtonText: "بله، خارج شو",
                            cancelButtonText: "انصراف",
                            confirmButtonColor: "#EF4444",
                            cancelButtonColor: "#64748B"
                        }).then(function (result) {
                            if (result.isConfirmed) {
                                var token = document.querySelector('input[name="__RequestVerificationToken"]');
                                var formData = new FormData();
                                if (token) formData.append("__RequestVerificationToken", token.value);
                                fetch("/Settings/LogoutAllDevices", {
                                    method: "POST",
                                    body: formData,
                                    headers: { "X-Requested-With": "XMLHttpRequest" }
                                })
                                .then(function (res) {
                                    if (!res.ok) throw new Error("HTTP " + res.status);
                                    return res.json();
                                })
                                .then(function (data) {
                                    if (data.success) {
                                        showSuccess(data.message);
                                        // Reload the sessions list
                                        logoutAllBtn.click();
                                        setTimeout(function () { logoutAllBtn.click(); }, 300);
                                    } else {
                                        showError(data.message || "خطا در انجام عملیات.");
                                    }
                                })
                                .catch(function () { showError("خطا در ارتباط با سرور."); });
                            }
                        });
                    });
                }
            })
            .catch(function () {
                devicesLoading.style.display = "none";
                devicesContainer.innerHTML = '<div style="padding:16px;color:var(--danger);font-size:13px;">خطا در بارگذاری اطلاعات دستگاه‌ها.</div>';
            });
        });
    }

    // ===== Security: Delete Account =====
    var deleteAccountBtn = document.getElementById("deleteAccountBtn");
    if (deleteAccountBtn) {
        deleteAccountBtn.addEventListener("click", function () {
            Swal.fire({
                title: "حذف حساب کاربری",
                text: "آیا از حذف دائمی حساب کاربری خود مطمئن هستید؟ این عمل غیرقابل بازگشت است.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "بله، حساب را حذف کن",
                cancelButtonText: "انصراف",
                confirmButtonColor: "#EF4444",
                cancelButtonColor: "#64748B"
            }).then(function (result) {
                if (result.isConfirmed) {
                    var token = document.querySelector('input[name="__RequestVerificationToken"]');
                    var formData = new FormData();
                    if (token) formData.append("__RequestVerificationToken", token.value);
                    fetch("/Settings/DeleteAccount", {
                        method: "POST",
                        body: formData,
                        headers: { "X-Requested-With": "XMLHttpRequest" }
                    })
                    .then(function (res) {
                        if (!res.ok) throw new Error("HTTP " + res.status);
                        return res.json();
                    })
                    .then(function (data) {
                        if (data.success) {
                            Swal.fire({
                                title: "حذف شد",
                                text: data.message,
                                icon: "success",
                                confirmButtonText: "باشه",
                                confirmButtonColor: "#6366F1"
                            }).then(function () {
                                window.location.href = "/Account/Login";
                            });
                        } else {
                            showError(data.message || "خطا در حذف حساب.");
                        }
                    })
                    .catch(function () { showError("خطا در ارتباط با سرور."); });
                }
            });
        });
    }
});