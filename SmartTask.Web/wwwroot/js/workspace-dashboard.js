document.addEventListener("DOMContentLoaded", function () {

    const primaryColor = "#5B5FEF";
    const chartColors = ["#5B5FEF", "#22C55E", "#F59E0B", "#EF4444", "#06B6D4", "#8B5CF6"];

    const statusCanvas = document.getElementById("projectStatusChart");
    if (statusCanvas && typeof projectStatusLabels !== "undefined" && projectStatusLabels.length) {
        new Chart(statusCanvas, {
            type: "doughnut",
            data: {
                labels: projectStatusLabels,
                datasets: [{
                    data: projectStatusValues,
                    backgroundColor: chartColors,
                    borderWidth: 0
                }]
            },
            options: {
                plugins: {
                    legend: { position: "bottom", labels: { font: { family: "Vazirmatn" } } }
                },
                cutout: "65%"
            }
        });
    }

    const activityCanvas = document.getElementById("activityChart");
    if (activityCanvas && typeof activityLabels !== "undefined") {
        new Chart(activityCanvas, {
            type: "line",
            data: {
                labels: activityLabels,
                datasets: [{
                    label: "فعالیت",
                    data: activityValues,
                    borderColor: primaryColor,
                    backgroundColor: "rgba(91,95,239,0.1)",
                    fill: true,
                    tension: 0.4,
                    pointRadius: 4,
                    pointBackgroundColor: primaryColor
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: {
                    y: { beginAtZero: true, ticks: { precision: 0 } }
                }
            }
        });
    }

});