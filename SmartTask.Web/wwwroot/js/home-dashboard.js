// ==========================================================
//              SmartTask Home Dashboard Chart
// ==========================================================

document.addEventListener('DOMContentLoaded', function () {
    if (typeof homeChartData === 'undefined' || typeof Chart === 'undefined') return;
    if (!homeChartData.length) return;

    var canvas = document.getElementById('myTaskStatusChart');
    if (!canvas) return;

    // Design system colors
    var colors = {
        primary: '#5B5FEF',
        success: '#22C55E',
        warning: '#F59E0B',
        danger:  '#EF4444',
        info:    '#0EA5E9'
    };

    new Chart(canvas, {
        type: 'doughnut',
        data: {
            labels: homeChartData.map(function (x) { return x.label; }),
            datasets: [{
                data: homeChartData.map(function (x) { return x.value; }),
                backgroundColor: [colors.primary, colors.success, colors.warning, colors.danger, colors.info],
                borderWidth: 2,
                borderColor: getComputedStyle(document.documentElement).getPropertyValue('--card').trim() || '#FFFFFF',
                hoverOffset: 4
            }]
        },
        options: {
            cutout: '65%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 16,
                        usePointStyle: true,
                        pointStyleWidth: 8,
                        font: {
                            family: '"Vazirmatn", sans-serif',
                            size: 12
                        }
                    }
                }
            },
            responsive: true,
            maintainAspectRatio: true
        }
    });
});