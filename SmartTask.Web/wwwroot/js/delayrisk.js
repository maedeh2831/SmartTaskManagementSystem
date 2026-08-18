// ==========================================================
//              SmartTask Delay Risk UI
// ==========================================================

document.addEventListener("DOMContentLoaded", function () {

    // ===== Persian digits helper =====
    function toFaDigits(str) {
        var fa = "\u06F0\u06F1\u06F2\u06F3\u06F4\u06F5\u06F6\u06F7\u06F8\u06F9";
        return String(str).replace(/[0-9]/g, function (d) { return fa[+d]; });
    }

    // Apply Persian digits to stat values
    document.querySelectorAll(".delayrisk-summary .fa-digits, .risk-factor-value .fa-digits, .dashboard-donut-label .fa-digits").forEach(function (el) {
        el.textContent = toFaDigits(el.textContent);
    });


    // ===== Animate Risk Donut =====
    var donut = document.querySelector(".dashboard-donut[data-progress]");
    if (donut) {
        var progress = parseFloat(donut.dataset.progress) || 0;
        var circle = donut.querySelector(".donut-value");
        var circumference = 2 * Math.PI * 52;
        circle.style.strokeDasharray = circumference;
        circle.style.strokeDashoffset = circumference;
        requestAnimationFrame(function () {
            var offset = circumference - (progress / 100) * circumference;
            circle.style.strokeDashoffset = offset;
        });
    }

    // ===== Generate AI Narrative =====
    var btn = document.getElementById("generateNarrativeBtn");
    if (!btn) return;

    var box = document.getElementById("narrativeBox");
    var projectId = btn.dataset.projectId;
    var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    var token = tokenEl ? tokenEl.value : "";

    btn.addEventListener("click", function () {
        // Show loading state
        box.innerHTML =
            '<div class="ai-loading">' +
                '<div class="ai-spinner"></div>' +
                '<p>در حال تحلیل وضعیت پروژه...</p>' +
            '</div>';

        btn.classList.add("loading");
        btn.disabled = true;

        fetch("/DelayRisk/GenerateNarrative", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: "projectId=" + projectId + "&__RequestVerificationToken=" + encodeURIComponent(token)
        })
        .then(function (r) { return r.json(); })
        .then(function (data) {
            if (!data.success) {
                box.innerHTML =
                    '<div class="ai-error">' +
                        '<i class="fa-solid fa-triangle-exclamation"></i>' +
                        '<p>' + (data.message || "خطا در تولید تحلیل.") + '</p>' +
                    '</div>';
            } else {
                box.innerHTML = '<p>' + data.narrative + '</p>';
            }
        })
        .catch(function () {
            box.innerHTML =
                '<div class="ai-error">' +
                    '<i class="fa-solid fa-triangle-exclamation"></i>' +
                    '<p>ارتباط با سرور برقرار نشد.</p>' +
                '</div>';
        })
        .finally(function () {
            btn.classList.remove("loading");
            btn.disabled = false;
        });
    });

});