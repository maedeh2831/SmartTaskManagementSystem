function initAgileDashboard() {

    const primaryColor = "#5B5FEF";
    const dangerColor = "#EF4444";

    // ===== Burndown Chart =====
    const burndownCanvas = document.getElementById("burndownChart");
    if (burndownCanvas && window.__burndownData && window.__burndownData.length) {
        const data = window.__burndownData;

        new Chart(burndownCanvas, {
            type: "line",
            data: {
                labels: data.map(d => d.date),
                datasets: [
                    {
                        label: "خط ایده‌آل",
                        data: data.map(d => d.ideal),
                        borderColor: "#94A3B8",
                        borderDash: [6, 6],
                        pointRadius: 0,
                        tension: 0
                    },
                    {
                        label: "روند واقعی",
                        data: data.map(d => d.actual),
                        borderColor: primaryColor,
                        backgroundColor: "rgba(91,95,239,0.08)",
                        fill: true,
                        pointRadius: 3,
                        tension: 0.25,
                        spanGaps: false
                    }
                ]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { position: "bottom", labels: { font: { family: "Vazirmatn" } } }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: { display: true, text: "Story Point باقی‌مانده" }
                    }
                }
            }
        });
    }

    // ===== Velocity Chart =====
    const velocityCanvas = document.getElementById("velocityChart");
    if (velocityCanvas && window.__velocityData && window.__velocityData.length) {
        const data = window.__velocityData;

        const hasPoints = data.some(d => (d.plannedPoints || d.completedPoints || d.PlannedPoints || d.CompletedPoints) > 0);
        const planned = data.map(d => d.plannedPoints ?? d.PlannedPoints ?? 0);
        const completed = data.map(d => d.completedPoints ?? d.CompletedPoints ?? 0);
        const labels = data.map(d => d.sprintName ?? d.SprintName ?? '???');

        console.log('[Velocity] labels:', labels, 'planned:', planned, 'completed:', completed);

        new Chart(velocityCanvas, {
            type: "bar",
            data: {
                labels: labels,
                datasets: [
                    {
                        label: "برنامه‌ریزی‌شده",
                        data: planned,
                        backgroundColor: "#C7D2FE",
                        borderRadius: 6
                    },
                    {
                        label: "تکمیل‌شده",
                        data: completed,
                        backgroundColor: primaryColor,
                        borderRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: { position: "bottom", labels: { font: { family: "Vazirmatn" } } }
                },
                scales: {
                    y: { beginAtZero: true, title: { display: true, text: "Story Point" } }
                }
            }
        });
    }

}

// Run immediately — script is loaded after DOM is ready (in @section Scripts)
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAgileDashboard);
} else {
    initAgileDashboard();
}