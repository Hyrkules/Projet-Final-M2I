let currentSocket = null; // Stocke le socket actuel globalement

window.createCryptoChart = async (containerId, symbol = 'BTCUSDT') => {
    const container = document.getElementById(containerId);
    if (!container) return;

    // 1. Nettoyage : Fermer le socket précédent s'il existe
    if (currentSocket) {
        currentSocket.close();
    }

    container.innerHTML = ''; // Reset le graphique visuellement

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

    // 2. Historique dynamique selon le symbole
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

    // 3. WebSocket combiné (Graphique + Stats 24h)
    // On utilise @ticker pour avoir le vrai volume 24h et la variation précise
    currentSocket = new WebSocket(`wss://stream.binance.com:9443/stream?streams=${symbol.toLowerCase()}@kline_1h/${symbol.toLowerCase()}@ticker`);

    currentSocket.onmessage = (event) => {
        const response = JSON.parse(event.data);
        const stream = response.stream;
        const data = response.data;

        // Mise à jour des bougies
        if (stream.includes('@kline')) {
            const candle = data.k;
            candleSeries.update({
                time: candle.t / 1000,
                open: parseFloat(candle.o), high: parseFloat(candle.h),
                low: parseFloat(candle.l), close: parseFloat(candle.c),
            });
        }

        // Mise à jour des Stats HTML (Ticker 24h)
        if (stream.includes('@ticker')) {
            const price = parseFloat(data.c).toFixed(2);
            const vol24h = parseFloat(data.v);
            const change = parseFloat(data.P).toFixed(2);

            const priceElem = document.getElementById('live-price-val');
            const highElem = document.getElementById('live-high');
            const lowElem = document.getElementById('live-low');
            const volElem = document.getElementById('live-vol-24h');
            const changeElem = document.getElementById('live-price-change');

            if (priceElem) priceElem.innerText = `${price} €`;
            if (highElem) highElem.innerText = `${parseFloat(data.h).toFixed(2)} €`;
            if (lowElem) lowElem.innerText = `${parseFloat(data.l).toFixed(2)} €`;

            if (volElem) {
                volElem.innerText = vol24h >= 1000 ? `${(vol24h / 1000).toFixed(2)}K` : vol24h.toFixed(2);
            }

            if (changeElem) {
                changeElem.innerText = `${change >= 0 ? '+' : ''}${change}%`;
                changeElem.className = `price-change ${change >= 0 ? 'positive' : 'negative'}`;
            }
        }
    };

    const resizeObserver = new ResizeObserver(entries => {
        if (entries.length > 0) chart.applyOptions({ width: entries[0].contentRect.width });
    });
    resizeObserver.observe(container);
};