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
    const container = document.getElementById('portfolioPerformanceChart');
    if (!container || typeof Chart === 'undefined') return;

    container.innerHTML = '';
    const canvas = document.createElement('canvas');
    container.appendChild(canvas);
    const ctx = canvas.getContext('2d');

    // 1. Déterminer les limites réelles pour placer le 0
    const min = Math.min(...data.values);
    const max = Math.max(...data.values);

    // 2. Calculer le pourcentage vertical où se trouve le 0
    // Formule : (Max / (Max - Min))
    let zeroPoint = 0.5; // Par défaut au milieu
    if (max !== min) {
        zeroPoint = max / (max - min);
    }

    // Inverser pour le canvas (le haut du canvas est 0, le bas est 1)
    const stop = 1 - zeroPoint;

    // 3. Créer le dégradé de REMPLISSAGE (Fill)
    const fillGradient = ctx.createLinearGradient(0, 0, 0, container.clientHeight);
    fillGradient.addColorStop(0, 'rgba(38, 166, 154, 0.7)');  // Vert vif en haut
    fillGradient.addColorStop(stop, 'rgba(38, 166, 154, 0.1)'); // Vert léger au 0
    fillGradient.addColorStop(stop, 'rgba(239, 83, 80, 0.1)');  // Rouge léger au 0
    fillGradient.addColorStop(1, 'rgba(239, 83, 80, 0.7)');    // Rouge vif en bas

    // 4. Créer le dégradé de la LIGNE (Border)
    const borderGradient = ctx.createLinearGradient(0, 0, 0, container.clientHeight);
    borderGradient.addColorStop(0, '#26a69a');    // Vert
    borderGradient.addColorStop(stop, '#26a69a'); // Vert jusqu'au 0
    borderGradient.addColorStop(stop, '#ef5350'); // Rouge dès le 0
    borderGradient.addColorStop(1, '#ef5350');    // Rouge

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.dates,
            datasets: [{
                data: data.values,
                fill: true,
                backgroundColor: fillGradient,
                borderColor: borderGradient,
                borderWidth: 4,
                pointRadius: 0,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: {
                    grid: {
                        // On met en avant la ligne du 0 avec une couleur blanche
                        color: (context) => context.tick.value === 0 ? 'rgba(255, 255, 255, 0.8)' : 'rgba(132, 142, 156, 0.1)',
                        lineWidth: (context) => context.tick.value === 0 ? 2 : 1
                    },
                    ticks: { color: '#848e9c' }
                },
                x: { display: false } // Pour un look plus clean
            }
        }
    });
};