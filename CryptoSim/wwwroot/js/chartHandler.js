window.cryptoChartInstance = {
    chart: null,
    interval: null,
    dotNetHelper: null
};

window.createCryptoChart = async (containerId, symbol = 'BBTC', helper) => {
    window.cryptoChartInstance.dotNetHelper = helper;
    const container = document.getElementById(containerId);
    if (!container) return;

    if (window.cryptoChartInstance.interval) {
        clearInterval(window.cryptoChartInstance.interval);
        window.cryptoChartInstance.interval = null;
    }

    if (window.cryptoChartInstance.chart && typeof window.cryptoChartInstance.chart.remove === 'function') {
        try { window.cryptoChartInstance.chart.remove(); } catch (e) { }
        window.cryptoChartInstance.chart = null;
    }

    container.innerHTML = '';

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

    const lineSeries = chart.addLineSeries({ color: '#C1A461', lineWidth: 3 });
    window.cryptoChartInstance.chart = chart;

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
            chart.timeScale().fitContent();
        }
    } catch (e) { console.error("Erreur historique:", e); }

    window.cryptoChartInstance.interval = setInterval(async () => {
        try {
            const response = await fetch(`http://localhost:${portAPI}/api/market/cryptos/${symbol}`);
            if (response.ok) {
                const data = await response.json();
                lineSeries.update({
                    time: Math.floor(new Date(data.lastUpdated || Date.now()).getTime() / 1000),
                    value: parseFloat(data.currentPrice)
                });
                updateTickerStatsFromLocal(data);
                generateFakeOrderBook(parseFloat(data.currentPrice));
            }
        } catch (e) { console.error("Erreur polling:", e); }
    }, 3000);
};

function updateTickerStatsFromLocal(data) {
    if (!data) return;

    const priceElem = document.getElementById('live-price-val');
    if (priceElem) priceElem.innerText = `${parseFloat(data.currentPrice).toFixed(2)} $`;

    const highElem = document.getElementById('live-high');
    if (highElem) highElem.innerText = `${parseFloat(data.high24h || data.currentPrice).toFixed(2)} $`;

    const lowElem = document.getElementById('live-low');
    if (lowElem) lowElem.innerText = `${parseFloat(data.low24h || data.currentPrice).toFixed(2)} $`;

    const volElem = document.getElementById('live-vol-24h');
    if (volElem) volElem.innerText = `${Math.floor(data.volume24h || 0).toLocaleString()} $`;

    const changeElem = document.getElementById('live-price-change');
    if (changeElem && data.priceChangePercent !== undefined) {
        const change = parseFloat(data.priceChangePercent);
        changeElem.innerText = `${change >= 0 ? '+' : ''}${change.toFixed(2)}%`;
        changeElem.className = `price-change ${change >= 0 ? 'positive' : 'negative'}`;
    }

    if (window.cryptoChartInstance.dotNetHelper) {
        window.cryptoChartInstance.dotNetHelper.invokeMethodAsync('UpdateCurrentPrice', parseFloat(data.currentPrice));
    }
}

function generateFakeOrderBook(currentPrice) {
    const asks = [];
    const bids = [];

    for (let i = 1; i <= 8; i++) {
        const askPrice = currentPrice * (1 + (i * 0.0025));
        const bidPrice = currentPrice * (1 - (i * 0.0025));
        const quantity = (Math.random() * 2 + 0.1).toFixed(4);
        asks.push([askPrice.toFixed(2), quantity]);
        bids.push([bidPrice.toFixed(2), quantity]);
    }

    updateOrderBookUI({ asks: asks.reverse(), bids });
}

function updateOrderBookUI(data) {
    const container = document.querySelector('.order-book-container');
    if (!container) return;

    const totalBidQty = data.bids.reduce((s, b) => s + parseFloat(b[1]), 0);
    const totalAskQty = data.asks.reduce((s, a) => s + parseFloat(a[1]), 0);
    const total = totalBidQty + totalAskQty;
    const bidPct = total > 0 ? (totalBidQty / total * 100).toFixed(1) : 50;
    const askPct = total > 0 ? (totalAskQty / total * 100).toFixed(1) : 50;

    const rows = data.bids.map((bid, i) => {
        const ask = data.asks[i] || ['0', '0'];
        const bidQty = parseFloat(bid[1]);
        const askQty = parseFloat(ask[1]);

        // Dans generateFakeOrderBook, les classes text-green/text-red ne marchent pas
        // Utilise des styles inline à la place
        return `
    <div class="ob-row">
        <span style="color:var(--white)">${bidQty.toFixed(4)}</span>
        <span style="color:#26a69a">${parseFloat(bid[0]).toFixed(2)}</span>
        <span class="ob-separator"></span>
        <span style="color:#ef5350">${parseFloat(ask[0]).toFixed(2)}</span>
        <span style="color:var(--white)">${askQty.toFixed(4)}</span>
    </div>`;
    }).join('');

    container.innerHTML = `
    <div class="ob-global-gauge">
        <div class="ob-global-bid" style="width: ${bidPct}%"></div>
        <div class="ob-global-ask" style="width: ${askPct}%"></div>
    </div>
        <div class="ob-header">
            <span style="color:var(--white)">QTÉ</span>
            <span style="color:var(--white)">ACHAT</span>
            <span></span>
            <span style="color:var(--white)">VENTE</span>
            <span style="color:var(--white)">QTÉ</span>
        </div>
        ${rows}
    `;
}