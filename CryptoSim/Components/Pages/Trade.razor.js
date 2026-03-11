// 1. DÉCLARATION GLOBALE (Indispensable pour éviter le "not defined")
let dotNetHelper = null;
let currentSocket = null;

// 2. Initialisation du pont Blazor

function updateTickerStats(data) {
    const price = parseFloat(data.c);
    
    // Mise à jour du DOM
    const priceElem = document.getElementById('live-price-val');
    if (priceElem) priceElem.innerText = `${price.toFixed(2)} €`;

    // Correction de l'erreur : On vérifie si dotNetHelper existe avant de l'appeler
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('UpdateCurrentPrice', price);
    }
}

