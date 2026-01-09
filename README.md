# AI Client Manager (MongoDB + OpenAI) — .NET 10

Projet généré et refactoré pour **MongoDB** (au lieu de MySQL) et enrichi avec:
- Authentification (ASP.NET Core Identity sur MongoDB)
- CRUD Clients + upload CV
- Notes/Interactions illimitées par client
- Recherche + filtre par priorité
- Analyse IA (OpenAI) : priorité, résumé, mots-clés
- Dashboard avec compteurs + graphiques (Chart.js)

**Packages fixés:** PdfPig (pour PDF) et System.IO.Packaging 8.0.1 (patch sécurité).

## Prérequis
- .NET SDK **10**
- MongoDB (local ou Atlas)

## Démarrage
1) Démarrer MongoDB local: `mongod` (par défaut sur `mongodb://localhost:27017`) 
2) Configurer `src/AiClientManager.Web/appsettings.json`:
   - `Mongo:ConnectionString`
   - `Mongo:DatabaseName`
   - (optionnel) `OpenAI:ApiKey`
3) Lancer:
```bash
cd src/AiClientManager.Web
dotnet restore
dotnet run
```
Puis ouvrir: http://localhost:5000

## Compte admin seed
- Email: `toutougay@gmail.com`
- Password: `toutougay`

## Endpoints
- UI: `/` (Dashboard), `/Clients`
- Swagger: `/swagger`
- Health MongoDB: `GET /health/db`
- API:
  - `GET /api/clients`
  - `POST /api/clients`
  - `POST /api/clients/{id}/analyze`
  - `GET /api/dashboard/summary`

> Si `OpenAI:Enabled=false` ou pas de clé API, l'analyse IA utilise un mode *heuristique* (sans réseau).
