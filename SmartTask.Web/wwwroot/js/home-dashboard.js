document.addEventListener('DOMContentLoaded', function () {
    if (typeof homeChartData === 'undefined' || typeof Chart === 'undefined') return;
    if (!homeChartData.length) return;

    const canvas = document.getElementById('myTaskStatusChart');
    if (!canvas) return;

    new Chart(canvas, {
        type: 'doughnut',
        data: {
            labels: homeChartData.map(x => x.label),
            datasets: [{
                data: homeChartData.map(x => x.value),
                backgroundColor: ['#4F46E5', '#22C55E', '#F59E0B', '#EF4444', '#0EA5E9']
            }]
        },
        options: { plugins: { legend: { position: 'bottom' } } }
    });
});