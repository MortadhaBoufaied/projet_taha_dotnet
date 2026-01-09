function renderPriorityChart(canvasId, data) {
  const ctx = document.getElementById(canvasId);
  if (!ctx) return;
  new Chart(ctx, {
    type: 'doughnut',
    data: {
      labels: ['High', 'Medium', 'Low', 'Unknown'],
      datasets: [{
        data: [data.high, data.medium, data.low, data.unknown],
        backgroundColor: ['#198754', '#ffc107', '#dc3545', '#6c757d']
      }]
    },
    options: { responsive: true }
  });
}

function renderCompanyChart(canvasId, dict) {
  const ctx = document.getElementById(canvasId);
  if (!ctx) return;
  const labels = Object.keys(dict);
  const values = Object.values(dict);
  new Chart(ctx, {
    type: 'bar',
    data: {
      labels,
      datasets: [{ label: 'Clients', data: values, backgroundColor: '#0d6efd' }]
    },
    options: {
      responsive: true,
      plugins: { legend: { display: false } },
      scales: { y: { beginAtZero: true } }
    }
  });
}

function renderMonthChart(canvasId, dict) {
  const ctx = document.getElementById(canvasId);
  if (!ctx) return;
  const labels = Object.keys(dict);
  const values = Object.values(dict);
  new Chart(ctx, {
    type: 'line',
    data: {
      labels,
      datasets: [{ label: 'Nouveaux clients', data: values, borderColor: '#20c997', tension: 0.25 }]
    },
    options: { responsive: true }
  });
}
