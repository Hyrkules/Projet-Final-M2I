# CryptoSim 💹

Plateforme de trading de cryptomonnaies fictives en environnement simulé, construite avec une architecture microservices .NET.

## Fonctionnalités

- Authentification via JWT
- Prix de cryptos fictives mis à jour en temps réel
- Passage d'ordres d'achat / vente
- Gestion d'un portefeuille virtuel (solde, actifs, performance)
- Historique des transactions et des cours

## Architecture

5 services indépendants, chacun avec sa propre base de données :

```
┌─────────────────────────────────────┐
│       Blazor Frontend (5000)        │
└─────────────────┬───────────────────┘
                  │ HTTP   
                  ▼ 
┌─────────────────────────────────────┐
│            Gateway (5005)           │
└────┬────────┬────────┬────────┬─────┘
     │        │        │        │  
     ▼        ▼        ▼        ▼
┌────────┐ ┌────────┐ ┌───────────┐ ┌───────┐
│  Auth  │ │ Market │ │ Portfolio │ │ Order │
│  5001  │ │  5002  │ │   5003    │ │  5004 │
└────────┘ └────────┘ └───────────┘ └───────┘
  users_db  market_db  portfolio_db  orders_db
```

| Service | Responsabilité |
|---|---|
| **AuthService** | Inscription, connexion, émission JWT |
| **MarketService** | Génération des prix, historique des cours |
| **PortfolioService** | Portefeuille, transactions, performance |
| **OrderService** | Passage et suivi des ordres |
| **Blazor Frontend** | Interface utilisateur |

## Stack technique

- **Frontend** : Blazor Server (.NET 8)
- **Backend** : ASP.NET Core Web API (.NET 8)
- **Base de données** : PostgreSQL
- **Auth** : JWT (HMAC-SHA256)
- **Conteneurisation** : Docker & Docker Compose

## Lancement

### Prérequis

- Docker Desktop
- Git

### Démarrage

```bash
git clone https://github.com/Hyrkules/Projet-Final-M2I.git
cd cryptosim
docker compose up --build
```

L'application est accessible sur **http://localhost:5000**

La Swagger UI de la Gateway est accessible sur **http://localhost:5005/swagger**

## Commandes utiles

```bash
# Relancer un service spécifique
docker compose up --build cryptosim-gateway

# Suivre les logs en temps réel
docker logs cryptosim-blazor -f

# Arrêter tous les services
docker compose down
```
