document.addEventListener('DOMContentLoaded', function () {
    initReportTabs();
    initReportCharts();
});

function initReportTabs() {
    const tabButtons = document.querySelectorAll('.report-tab-btn');
    const tabPanels = document.querySelectorAll('.report-tab-panel');

    tabButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            tabButtons.forEach(b => b.classList.remove('active'));
            tabPanels.forEach(p => p.classList.remove('active'));

            this.classList.add('active');
            document.getElementById('tab-' + this.dataset.tab).classList.add('active');
        });
    });
}

function initReportCharts() {
    if (typeof reportData === 'undefined' || typeof Chart === 'undefined') return;

    const palette = ['#4F46E5', '#22C55E', '#F59E0B', '#EF4444', '#0EA5E9', '#A855F7'];

    const statusCanvas = document.getElementById('taskStatusChart');
    if (statusCanvas && reportData.taskStatus?.length) {
        new Chart(statusCanvas, {
            type: 'doughnut',
            data: {
                labels: reportData.taskStatus.map(x => x.label),
                datasets: [{
                    data: reportData.taskStatus.map(x => x.value),
                    backgroundColor: palette
                }]
            },
            options: { plugins: { legend: { position: 'bottom' } } }
        });
    }

    const priorityCanvas = document.getElementById('taskPriorityChart');
    if (priorityCanvas && reportData.taskPriority?.length) {
        new Chart(priorityCanvas, {
            type: 'bar',
            data: {
                labels: reportData.taskPriority.map(x => x.label),
                datasets: [{
                    label: 'تعداد Task',
                    data: reportData.taskPriority.map(x => x.value),
                    backgroundColor: '#4F46E5',
                    borderRadius: 8
                }]
            },
            options: {
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
            }
        });
    }

    const timeLogCanvas = document.getElementById('timeLogChart');
    if (timeLogCanvas && reportData.timeLog?.length) {
        new Chart(timeLogCanvas, {
            type: 'bar',
            data: {
                labels: reportData.timeLog.map(x => x.label),
                datasets: [{
                    label: 'دقیقه ثبت‌شده',
                    data: reportData.timeLog.map(x => x.value),
                    backgroundColor: '#0EA5E9',
                    borderRadius: 8
                }]
            },
            options: {
                indexAxis: 'y',
                plugins: { legend: { display: false } },
                scales: { x: { beginAtZero: true } }
            }
        });
    }
}