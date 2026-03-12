window.createPieChart = (data, colors) => {
    if (typeof Chart === 'undefined') {
        setTimeout(() => window.createPieChart(data, colors), 50);
        return;
    }

    const canvas = document.getElementById('assetAllocationPieChart');
    if (!canvas) return;

    const ctx = canvas.getContext('2d');

    const existingChart = Chart.getChart("assetAllocationPieChart");
    if (existingChart) existingChart.destroy();

    new Chart(ctx, {
        type: 'pie',
        // --- ACTIVATION DU PLUGIN DATALABELS ---
        plugins: [ChartDataLabels],
        data: {
            labels: data.labels,
            datasets: [{
                data: data.values,
                backgroundColor: colors,
                borderWidth: 1, // Bordure très subtile d'origine
                borderColor: 'rgba(255, 255, 255, 0.05)' // Semi-transparente
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            layout: {
                padding: 10
            },
            plugins: {
                // Configuration des étiquettes (Datalabels)
                datalabels: {
                    color: '#fff',
                    font: {
                        weight: 'bold',
                        size: 14,
                        family: 'monospace'
                    },
                    formatter: (value, context) => {
                        // Affiche le label de la crypto + le %
                        return context.chart.data.labels[context.dataIndex] + '\n' + value + '%';
                    },
                    anchor: 'center',
                    align: 'center',
                    display: (context) => {
                        // Cache les labels si la part est minuscule (< 10%)
                        return context.dataset.data[context.dataIndex] > 10;
                    },
                    textAlign: 'center'
                },
                legend: {
                    // Supprime la légende en bas d'origine
                    display: false
                }
            },
            elements: {
                arc: {
                    // halo lumineux au survol (Glow effect d'origine)
                    hoverOffset: 15,
                    borderWidth: 2,
                    borderColor: colors
                }
            },
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 1500,
                easing: 'easeOutQuart'
            }
        }
    });
};

window.createLineChart = (data) => {
    if (typeof Chart === 'undefined') {
        setTimeout(() => window.createLineChart(data), 50);
        return;
    }

    const canvas = document.createElement('canvas');
    const container = document.getElementById('portfolioPerformanceChart');
    if (!container) return;

    // Nettoyage du conteneur
    container.innerHTML = '';
    container.appendChild(canvas);

    const ctx = canvas.getContext('2d');

    // Création du dégradé (Vert -> Transparent)
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(38, 166, 154, 0.4)'); // Ton vert #26a69a
    gradient.addColorStop(1, 'rgba(38, 166, 154, 0)');

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.dates,
            datasets: [{
                label: 'Valeur du Portefeuille',
                data: data.values,
                fill: true,
                backgroundColor: gradient,
                borderColor: '#26a69a',
                borderWidth: 3,
                pointRadius: 0, // Cache les points pour un look "TradingView"
                pointHoverRadius: 6,
                pointHoverBackgroundColor: '#26a69a',
                pointHoverBorderColor: '#fff',
                pointHoverBorderWidth: 2,
                tension: 0.4 // Courbe lisse
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }, // Pas besoin de légende pour une seule ligne
                tooltip: {
                    mode: 'index',
                    intersect: false,
                    backgroundColor: '#1c212d',
                    titleColor: '#848e9c',
                    bodyColor: '#fff',
                    bodyFont: { weight: 'bold' },
                    callbacks: {
                        label: (context) => ` ${context.parsed.y.toLocaleString()} €`
                    }
                }
            },
            scales: {
                x: {
                    grid: { display: false },
                    ticks: { color: '#848e9c', maxRotation: 0 }
                },
                y: {
                    grid: { color: 'rgba(132, 142, 156, 0.1)' },
                    ticks: {
                        color: '#848e9c',
                        callback: (value) => value + ' €'
                    }
                }
            }
        }
    });
};