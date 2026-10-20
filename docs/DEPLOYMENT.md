# Deployment Guide

This guide describes how to deploy **Customer Review Insights** for the first time. Nothing in this document claims that cloud resources already exist for this project.

> **Cost note:** Local development does not require paid cloud services. Hosted frontend, API, SQL Server, and Gemini usage may incur charges depending on provider, plan, and traffic. Prefer free/dev tiers for experiments and review their limits before production use.

## 1. Recommended architecture

Deploy three parts separately:

```text
Browser
  → React static site (HTML/JS/CSS from Vite build)
  → ASP.NET Core Web API (HTTPS)
  → SQL Server (CustomerReviews)
  → Google Gemini API (server-side only)
```

| Component | Role |
| --- | --- |
| Frontend | Static assets from `npm run build` |
| Backend | `CustomerReviewInsights.Api` published to a .NET 9 host |
| Database | SQL Server–compatible database with schema from EF migrations |
| Secrets | Gemini key + connection string in host secret store / env vars |

The Gemini API key must **never** ship in the frontend bundle.

## 2. Concrete example stack (optional)

One common Microsoft-centric combination:

| Piece | Example service | Notes |
| --- | --- | --- |
| Frontend | **Azure Static Web Apps** (Free plan available for hobby use) | Serve `client/dist`; Free plan has bandwidth/storage limits and no SLA |
| Backend | **Azure App Service** (.NET 9) | Free (F1) is for trials/learning only (CPU minutes/day, no production SLA) |
| Database | **Azure SQL Database** free offer | Monthly free compute/storage limits; auto-pause options; not a production SLA offer |
| Secrets | App Service Application Settings / Key Vault | Prefer Key Vault for production |

Alternatives: Cloudflare Pages / Netlify / S3+CDN for static files; any Linux/Windows host that runs ASP.NET Core 9; Amazon RDS SQL Server or self-managed SQL Server.

Always verify current pricing and quotas on the provider’s site before committing.

## 3. Frontend production build

```powershell
cd client
npm install
$env:VITE_API_BASE_URL = "https://YOUR-API-HOST.example.com"
npm run build
```

Output directory: `client/dist/`

Deploy the contents of `dist/` to your static host.

Critical:

- `VITE_API_BASE_URL` is baked in at **build time**
- Do not leave it as `http://localhost:5056` for production
- Never set Gemini keys in Vite env vars

## 4. Publish the ASP.NET Core API

```powershell
cd server\CustomerReviewInsights.Api
dotnet publish -c Release -o .\publish
```

Deploy the `publish` folder to App Service, IIS, Docker, or another .NET 9–compatible host.

Set environment:

```text
ASPNETCORE_ENVIRONMENT=Production
```

Swagger is only enabled when the environment is `Development` (`Program.cs`).

## 5. Production secrets and configuration

Set on the host (names must match ASP.NET configuration):

| Name | Example placeholder |
| --- | --- |
| `Gemini__ApiKey` | `YOUR_GEMINI_API_KEY` |
| `Gemini__Model` | `gemini-3.5-flash-lite` (optional) |
| `Gemini__TimeoutSeconds` | `45` (optional) |
| `ConnectionStrings__DefaultConnection` | SQL auth or managed identity connection string |

Rotate keys if they were ever committed or shared.

## 6. CORS

Current code allows only:

- `http://localhost:5173`
- `http://127.0.0.1:5173`

Before production, update `WithOrigins(...)` in `server/CustomerReviewInsights.Api/Program.cs` to your real frontend origin(s), for example `https://your-frontend.example.com`, then republish the API.

Without this change, browsers will block cross-origin calls from the hosted UI.

## 7. Database

1. Provision SQL Server / Azure SQL.
2. Allow the API host’s outbound IP / VNet access (firewall rules).
3. Prefer least-privilege SQL credentials or managed identity.
4. Apply schema from the repo root:

```powershell
dotnet ef database update --project .\server\CustomerReviewInsights.Api\CustomerReviewInsights.Api.csproj
```

Ensure the connection string points at the production database (not `localhost\SQLEXPRESS`) when running migrations against production.

Table created: `CustomerReviews` (see EF migration `InitialCreate`).

## 8. HTTPS, logging, and security practices

- Terminate TLS at the host / reverse proxy; prefer HTTPS-only frontend and API URLs.
- Keep `ASPNETCORE_ENVIRONMENT=Production` so Swagger is not exposed.
- Application logs should not include API keys or full untrusted review payloads in production sinks you cannot control.
- Rotate Gemini keys periodically; revoke compromised keys in Google AI Studio.
- Restrict SQL firewall to the API host only.

## 9. Post-deploy verification

| Check | How |
| --- | --- |
| Frontend loads | Open the public site URL |
| API responds | `GET https://YOUR-API/api/reviews` returns JSON array |
| Database | Successful GET/POST proves connectivity (no separate health endpoint exists) |
| Gemini | Submit a real review; expect `201` and a filled sentiment/category/summary |
| History | New rows appear in the UI after refresh/submit |
| Validation | Empty/oversized input returns `400` |
| AI failure | Invalid key → `502`, no new row |

## 10. Deployment troubleshooting

| Symptom | Likely cause |
| --- | --- |
| Browser CORS errors | Frontend origin not listed in `Program.cs` `WithOrigins` |
| UI calls localhost | `VITE_API_BASE_URL` not set for production build |
| HTTP 404 on API | Wrong base path / App Service site not publishing correctly |
| HTTP 500 on GET | Connection string / SQL firewall / migration not applied |
| HTTP 502 on POST | Gemini key missing/invalid, model unavailable, quota, or timeout |
| Gemini auth errors | Bad `Gemini__ApiKey` |
| Timeouts | Slow model or network; check `Gemini__Model` / `TimeoutSeconds` |

## 11. Deployment checklist

- [ ] Production SQL database created and reachable from API host  
- [ ] EF migration applied  
- [ ] `Gemini__ApiKey` set in host secrets (not in Git / not in frontend)  
- [ ] `ConnectionStrings__DefaultConnection` set for production  
- [ ] CORS origins updated for the real frontend URL  
- [ ] Frontend built with production `VITE_API_BASE_URL`  
- [ ] Frontend `dist/` deployed  
- [ ] API published (`dotnet publish`) and running on HTTPS  
- [ ] `ASPNETCORE_ENVIRONMENT=Production`  
- [ ] Smoke-tested GET + POST + UI history  
- [ ] Confirmed failed AI analysis does not insert rows  

## Manual steps that require your cloud account

These cannot be completed from this repository alone:

- Creating cloud subscriptions / resource groups  
- Configuring DNS and custom domains  
- Setting provider firewalls and managed identities  
- Paying for usage beyond free-tier limits  
- Obtaining and rotating your own Gemini API key  
