let currentSocket = null;

window.createCryptoChart = async (containerId, symbol = 'BTCUSDT') => {
    console.log("✅ createCryptoChart lancé");
    const container = document.getElementById(containerId);
    if (!container) return;

    if (currentSocket) {
        currentSocket.close();
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
        timeScale: { borderVisible: false, timeVisible: true },
    });

    const candleSeries = chart.addCandlestickSeries({
        upColor: '#26a69a', downColor: '#ef5350',
        borderVisible: false, wickUpColor: '#26a69a', wickDownColor: '#ef5350',
    });

    try {
        const response = await fetch(`https://api.binance.com/api/v3/klines?symbol=${symbol}&interval=1h&limit=100`);
        const rawData = await response.json();
        const formattedData = rawData.map(d => ({
            time: d[0] / 1000,
            open: parseFloat(d[1]), high: parseFloat(d[2]),
            low: parseFloat(d[3]), close: parseFloat(d[4]),
        }));
        candleSeries.setData(formattedData);
        chart.timeScale().fitContent();
    } catch (e) { console.error("Erreur historique:", e); }

    // MODIFICATION ICI : On ajoute @depth5 à la liste des streams
    const streams = `${symbol.toLowerCase()}@kline_1h/${symbol.toLowerCase()}@ticker/${symbol.toLowerCase()}@depth5`;
    currentSocket = new WebSocket(`wss://stream.binance.com:9443/stream?streams=${streams}`);

    currentSocket.onmessage = (event) => {
        const response = JSON.parse(event.data);
        const stream = response.stream;
        const data = response.data;
       

        // 1. Mise à jour des bougies
        if (stream.includes('@kline')) {
            const candle = data.k;
            candleSeries.update({
                time: candle.t / 1000,
                open: parseFloat(candle.o), high: parseFloat(candle.h),
                low: parseFloat(candle.l), close: parseFloat(candle.c),
            });
        }

        // 2. Mise à jour des Stats HTML (Ticker 24h)
        if (stream.includes('@ticker')) {
            updateTickerStats(data);
        }

        // 3. NOUVEAU : Mise à jour du Carnet d'ordres (Order Book)
        if (stream.includes('@depth')) {
            updateOrderBookUI(data);
        }
    };

    const resizeObserver = new ResizeObserver(entries => {
        if (entries.length > 0) chart.applyOptions({ width: entries[0].contentRect.width });
    });
    resizeObserver.observe(container);
};

// Fonctions utilitaires pour garder createCryptoChart propre
function updateTickerStats(data) {
    const price = parseFloat(data.c);
    const change = parseFloat(data.P).toFixed(2);

    const priceElem = document.getElementById('live-price-val');
    const highElem = document.getElementById('live-high');
    const lowElem = document.getElementById('live-low');
    const volElem = document.getElementById('live-vol-24h');
    const changeElem = document.getElementById('live-price-change');

    if (priceElem) priceElem.innerText = `${price.toFixed(2)} $`;
    if (highElem) highElem.innerText = `${parseFloat(data.h).toFixed(2)} $`;
    if (lowElem) lowElem.innerText = `${parseFloat(data.l).toFixed(2)} $`;
    if (volElem) {
        const vol = parseFloat(data.v);
        volElem.innerText = vol >= 1000 ? `${(vol / 1000).toFixed(2)}K` : vol.toFixed(2);
    }
    if (changeElem) {
        changeElem.innerText = `${change >= 0 ? '+' : ''}${change}%`;
        changeElem.className = `price-change ${change >= 0 ? 'positive' : 'negative'}`;
    }
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('UpdateCurrentPrice', price);
    }
}

function updateOrderBookUI(data) {
    const asksContainer = document.querySelector('.asks');
    const bidsContainer = document.querySelector('.bids');
    if (!asksContainer || !bidsContainer) return;

    // Ventes (Asks) - On inverse pour avoir les prix bas en bas
    asksContainer.innerHTML = data.asks.reverse().map(ask => `
        <div class="ob-row ask-row">
            <span class="red">${parseFloat(ask[0]).toFixed(2)}</span>
            <span>${parseFloat(ask[1]).toFixed(4)}</span>
            <div class="depth-bg ask-bg" style="width: ${Math.min(parseFloat(ask[1]) * 500, 100)}%"></div>
        </div>
    `).join('');

    // Achats (Bids)
    bidsContainer.innerHTML = data.bids.map(bid => `
        <div class="ob-row bid-row green">
            <span class="green">${parseFloat(bid[0]).toFixed(2)}</span>
            <span>${parseFloat(bid[1]).toFixed(4)}</span>
            <div class="depth-bg bid-bg" style="width: ${Math.min(parseFloat(bid[1]) * 500, 100)}%"></div>
        </div>
    `).join('');
}