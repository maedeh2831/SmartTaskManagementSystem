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
});