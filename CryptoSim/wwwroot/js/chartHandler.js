window.cryptoChartInstance = {
    chart: null,
    interval: null,
    dotNetHelper: null
};

window.createCryptoChart = async (containerId, symbol = 'BBTC', helper) => {
    window.cryptoChartInstance.dotNetHelper = helper;
    const container = document.getElementById(containerId);
    if (!container) return;

    // 1. NETTOYAGE
    if (window.cryptoChartInstance.interval) {
        clearInterval(window.cryptoChartInstance.interval);
        window.cryptoChartInstance.interval = null;
    }

    // On détruit l'ancien graphique s'il existe
    if (window.cryptoChartInstance.chart && typeof window.cryptoChartInstance.chart.remove === 'function') {
        try {
            window.cryptoChartInstance.chart.remove();
        } catch (e) {
            console.warn("Erreur lors de la suppression du graphique:", e);
        }
        window.cryptoChartInstance.chart = null;
    }

    container.innerHTML = '';

    // 2. CRÉATION DU GRAPHIQUE
    const chart = LightweightCharts.createChart(container, {
        width: container.clientWidth,
        height: 350,
        layout: { background: { color: 'transparent' }, textColor: '#848e9c' },
        grid: {
            vertLines: { color: 'rgba(193, 164, 97, 0.05)' },
            horzLines: { color: 'rgba(193, 164, 97, 0.05)' },
        },
        timeScale: { borderVisible: false, timeVisible: true, secondsVisible: true },
    });

    const lineSeries = chart.addLineSeries({
        color: '#C1A461',
        lineWidth: 3,
    });

    window.cryptoChartInstance.chart = chart;

    // 3. CHARGEMENT INITIAL (Historique)
    const portAPI = "5005";
    try {
        const hRes = await fetch(`http://localhost:${portAPI}/api/market/history/${symbol}?limit=100`);
        if (hRes.ok) {
            const rawData = await hRes.json();
            const formattedData = rawData.map(d => ({
                time: Math.floor(new Date(d.recordedAt).getTime() / 1000),
                value: parseFloat(d.price)
            })).sort((a, b) => a.time - b.time);
            lineSeries.setData(formattedData);
            if (formattedData.length > 0) {
                const lastPrice = formattedData[formattedData.length - 1].value;
                if (window.cryptoChartInstance.dotNetHelper) {
                    window.cryptoChartInstance.dotNetHelper.invokeMethodAsync('UpdateCurrentPrice', lastPrice);
                }
            }
            chart.timeScale().fitContent();
        }
    } catch (e) { console.error("Erreur historique:", e); }

    // 4. BOUCLE DE MISE À JOUR (Polling)
    window.cryptoChartInstance.interval = setInterval(async () => {
        try {
            const response = await fetch(`http://localhost:${portAPI}/api/market/cryptos/${symbol}`);
            if (response.ok) {
                const data = await response.json();

                lineSeries.update({
                    time: Math.floor(new Date(data.lastUpdated || Date.now()).getTime() / 1000),
                    value: parseFloat(data.currentPrice)
                });

                // Mise à jour du texte HTML (ça, ça marchait déjà)
                document.getElementById('live-price-val').innerText = `${data.currentPrice.toFixed(2)} $`;

                // --- CORRECTION ICI ---
                // On utilise le chemin complet vers le helper stocké au début
                if (window.cryptoChartInstance.dotNetHelper) {
                    window.cryptoChartInstance.dotNetHelper.invokeMethodAsync('UpdateCurrentPrice', parseFloat(data.currentPrice));
                }

                updateTickerStatsFromLocal(data, symbol);
            }
        } catch (e) { console.error("Erreur polling:", e); }
    }, 3000);
};

function updateTickerStatsFromLocal(data, symbol) {
    if (!data) return;

    // Prix principal
    const priceElem = document.getElementById('live-price-val');
    if (priceElem) priceElem.innerText = `${parseFloat(data.currentPrice).toFixed(2)} $`;

    // High 24h
    const highElem = document.getElementById('live-high');
    if (highElem) highElem.innerText = `${parseFloat(data.high24h || data.currentPrice).toFixed(2)} $`;

    // Low 24h
    const lowElem = document.getElementById('live-low');
    if (lowElem) lowElem.innerText = `${parseFloat(data.low24h || data.currentPrice).toFixed(2)} $`;

    // Volume 24h
    const volElem = document.getElementById('live-vol-24h');
    if (volElem) volElem.innerText = `${Math.floor(data.volume24h || 0).toLocaleString()} $`;

    // Variation % (Optionnel - évite l'erreur si changeElem n'existe pas)
    const changeElem = document.getElementById('live-price-change');
    if (changeElem && data.priceChangePercent !== undefined) {
        const change = parseFloat(data.priceChangePercent);
        changeElem.innerText = `${change >= 0 ? '+' : ''}${change.toFixed(2)}%`;
        changeElem.className = `price-change ${change >= 0 ? 'positive' : 'negative'}`;
    }

    // 3. Notification à Blazor pour le C#
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('UpdateCurrentPrice', data.currentPrice);
    }
}