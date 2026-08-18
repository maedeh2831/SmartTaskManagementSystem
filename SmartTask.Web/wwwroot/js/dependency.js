// ==========================================================
//              SmartTask Dependency UI
// ==========================================================

document.addEventListener("DOMContentLoaded", function () {

    // ===== Persian digits helper =====
    function toFaDigits(str) {
        var fa = "۰۱۲۳۴۵۶۷۸۹";
        return String(str).replace(/[0-9]/g, function (d) { return fa[+d]; });
    }

    // Apply Persian digits to stat values
    document.querySelectorAll(".dependency-stats-bar .fa-digits").forEach(function (el) {
        el.textContent = toFaDigits(el.textContent);
    });


    // ===== Tab System =====
    var tabBtns = document.querySelectorAll(".dependency-tab-btn[data-tab]");
    var tabPanels = document.querySelectorAll(".dependency-tab-panel");

    tabBtns.forEach(function (btn) {
        btn.addEventListener("click", function () {
            // Switch active tab
            tabBtns.forEach(function (b) {
                b.classList.remove("active");
                b.setAttribute("aria-selected", "false");
            });
            btn.classList.add("active");
            btn.setAttribute("aria-selected", "true");

            // Switch active panel
            tabPanels.forEach(function (p) { p.classList.add("d-none"); });
            var targetPanel = document.getElementById(btn.dataset.tab);
            if (targetPanel) {
                targetPanel.classList.remove("d-none");
            }

            // Lazy-load graph on first view
            if (btn.dataset.tab === "graphView" && typeof window.initDependencyGraph === "function") {
                window.initDependencyGraph();
            }
        });
    });


    // ===== Risk List Search =====
    var searchInput = document.getElementById("dependencySearchInput");
    var riskCards = document.querySelectorAll("#dependencyRiskList .dependency-risk-card");

    if (searchInput && riskCards.length > 0) {
        searchInput.addEventListener("input", function () {
            var q = this.value.trim().toLowerCase();
            var visible = 0;

            riskCards.forEach(function (card) {
                var title = (card.dataset.title || "").toLowerCase();
                var match = !q || title.indexOf(q) !== -1;
                card.style.display = match ? "" : "none";
                if (match) visible++;
            });

            // Show/hide empty state
            var list = document.getElementById("dependencyRiskList");
            var noResult = document.getElementById("dependencyNoResult");
            if (visible === 0 && riskCards.length > 0) {
                if (!noResult) {
                    noResult = document.createElement("div");
                    noResult.id = "dependencyNoResult";
                    noResult.className = "workspace-empty";
                    noResult.innerHTML = '<div class="workspace-empty-icon"><i class="fa-solid fa-magnifying-glass"></i></div><h3>نتیجه‌ای یافت نشد</h3><p>عبارت جستجوی خود را تغییر دهید.</p>';
                    list.parentNode.insertBefore(noResult, list.nextSibling);
                }
                noResult.style.display = "";
                list.style.display = "none";
            } else {
                if (noResult) noResult.style.display = "none";
                list.style.display = "";
            }
        });
    }

});
