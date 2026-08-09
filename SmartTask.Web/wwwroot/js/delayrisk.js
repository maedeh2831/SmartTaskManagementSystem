document.addEventListener("DOMContentLoaded", function () {

    //  Animate Risk Donut 
    const donut = document.querySelector(".dashboard-donut[data-progress]");
    if (donut) {
        const progress = parseFloat(donut.dataset.progress) || 0;
        const circle = donut.querySelector(".donut-value");
        const circumference = 2 * Math.PI * 52;
        circle.style.strokeDasharray = circumference;
        circle.style.strokeDashoffset = circumference;
        requestAnimationFrame(() => {
            const offset = circumference - (progress / 100) * circumference;
            circle.style.strokeDashoffset = offset;
        });
    }

    //  Generate AI Narrative 
    const btn = document.getElementById("generateNarrativeBtn");
    if (!btn) return;

    const box = document.getElementById("narrativeBox");
    const projectId = btn.dataset.projectId;
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    btn.addEventListener("click", async function () {
        box.innerHTML = `
            <div class="ai-loading" style="padding:10px 0;">
                <div class="ai-spinner"></div>
                <p>در حال تحلیل وضعیت پروژه...</p>
            </div>`;

        btn.disabled = true;

        try {
            const response = await fetch("/DelayRisk/GenerateNarrative", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: `projectId=${projectId}&__RequestVerificationToken=${encodeURIComponent(token)}`
            });

            const data = await response.json();

            if (!data.success) {
                box.innerHTML = `<div class="ai-error" style="padding:10px 0;"><i class="fa-solid fa-triangle-exclamation"></i><p>${data.message}</p></div>`;
            } else {
                box.innerHTML = `<p>${data.narrative}</p>`;
            }
        } catch (err) {
            box.innerHTML = `<div class="ai-error" style="padding:10px 0;"><i class="fa-solid fa-triangle-exclamation"></i><p>ارتباط با سرور برقرار نشد.</p></div>`;
        } finally {
            btn.disabled = false;
        }
    });

});