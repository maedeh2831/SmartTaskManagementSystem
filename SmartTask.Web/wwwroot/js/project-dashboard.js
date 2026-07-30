document.addEventListener("DOMContentLoaded", function () {

    // ===== Animate Donut Chart =====
    const donut = document.querySelector(".dashboard-donut");

    if (donut) {
        const progress = parseFloat(donut.dataset.progress) || 0;
        const circle = donut.querySelector(".donut-value");
        const circumference = 2 * Math.PI * 52; // r=52

        circle.style.strokeDasharray = circumference;
        circle.style.strokeDashoffset = circumference;

        requestAnimationFrame(() => {
            const offset = circumference - (progress / 100) * circumference;
            circle.style.strokeDashoffset = offset;
        });
    }

    // ===== Animate Bar Chart =====
    document.querySelectorAll(".dashboard-bar-fill").forEach(bar => {
        const targetWidth = bar.style.width;
        bar.style.width = "0";
        requestAnimationFrame(() => {
            setTimeout(() => {
                bar.style.width = targetWidth;
            }, 100);
        });
    });

});