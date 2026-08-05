document.addEventListener('DOMContentLoaded', function () {
    if (typeof adminChartData === 'undefined' || typeof Chart === 'undefined') return;

    const userCanvas = document.getElementById('userGrowthChart');
    if (userCanvas && adminChartData.userGrowth?.length) {
        new Chart(userCanvas, {
            type: 'line',
            data: {
                labels: adminChartData.userGrowth.map(x => x.label),
                datasets: [{
                    label: 'کاربر جدید',
                    data: adminChartData.userGrowth.map(x => x.value),
                    borderColor: '#4F46E5',
                    backgroundColor: 'rgba(79,70,229,0.1)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }

    const workspaceCanvas = document.getElementById('workspaceGrowthChart');
    if (workspaceCanvas && adminChartData.workspaceGrowth?.length) {
        new Chart(workspaceCanvas, {
            type: 'line',
            data: {
                labels: adminChartData.workspaceGrowth.map(x => x.label),
                datasets: [{
                    label: 'فضای کاری جدید',
                    data: adminChartData.workspaceGrowth.map(x => x.value),
                    borderColor: '#0EA5E9',
                    backgroundColor: 'rgba(14,165,233,0.1)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }
});