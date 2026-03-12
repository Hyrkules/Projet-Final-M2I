// Fonction JS pour créer le graphique en camembert
function createPieChart(data, colors) {
    const ctx = document.getElementById('assetAllocationPieChart').getContext('2d');

    // Si un graphique existe déjà sur cet élément, on le détruit pour repartir sur une base propre
    let chart = Chart.getChart("assetAllocationPieChart");
    if (chart != undefined) {
        chart.destroy();
    }

    // Configuration de l'ombre portée (halos autour du camembert)
    const originalDraw = Chart.controllers.pie.prototype.draw;
    Chart.controllers.pie.prototype.draw = function () {
        ctx.save();
        ctx.shadowColor = 'rgba(0, 0, 0, 0.5)';
        ctx.shadowBlur = 10;
        ctx.shadowOffsetX = 0;
        ctx.shadowOffsetY = 4;
        originalDraw.apply(this, arguments);
        ctx.restore();
    };

    // Création du graphique Chart.js
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: data.labels,
            datasets: [{
                data: data.values,
                backgroundColor: colors, // Utilise les couleurs définies
                borderWidth: 1, // Bordure subtile entre les parts
                borderColor: 'rgba(255, 255, 255, 0.05)', // Couleur de la bordure
                offset: data.offset // Applique le décalage (offset) pour l'effet pizza
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false, // Permet d'adapter la taille au conteneur HTML
            plugins: {
                // Style de la légende
                legend: {
                    position: 'bottom', // Légende en bas
                    labels: {
                        color: '#848e9c', // Couleur var(--text-muted) d'origine
                        font: {
                            size: 11,
                            family: 'monospace' // Police monospace style trading
                        },
                        padding: 15, // Espace entre la légende et le graphique
                        usePointStyle: true, // Utilise des points à la place des carrés de légende
                        pointStyle: 'circle' // Points circulaires
                    }
                },
                // Style des infobulles au survol
                tooltip: {
                    backgroundColor: 'rgba(21, 25, 33, 0.9)', // Fond var(--card-bg) d'origine
                    titleColor: '#ffffff',
                    bodyColor: '#e0e3eb',
                    borderColor: 'rgba(193, 164, 97, 0.4)', // Bordure var(--gold) d'origine
                    borderWidth: 1,
                    cornerRadius: 6,
                    padding: 12,
                    callbacks: {
                        // Personnalise le texte de l'infobulle (Ajoute %)
                        label: function (context) {
                            let label = context.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed !== null) {
                                label += context.parsed + '%';
                            }
                            return label;
                        }
                    }
                }
            },
            // Style global du graphique
            elements: {
                arc: {
                    // Élargit la part au survol (Effet de grossissement)
                    hoverOffset: 15,
                }
            },
            animation: {
                // Animation d'apparition d'origine (Rotation + Scaling)
                animateRotate: true,
                animateScale: true,
                duration: 1500, // Durée de l'animation
                easing: 'easeOutQuart' // Type d'animation
            }
        }
    });
}