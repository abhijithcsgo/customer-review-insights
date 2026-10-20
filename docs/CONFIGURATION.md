# Configuration Reference

All settings below are verified against the current source code. Use **placeholders only** — never commit real API keys or passwords.

## Summary table

| Setting | Purpose | Required | Where to configure | Example placeholder |
| --- | --- | --- | --- | --- |
| `Gemini:ApiKey` | Authenticates calls to Google Gemini | **Yes** (for live analysis) | User Secrets (dev), env var / host secrets (prod) | `YOUR_GEMINI_API_KEY` |
| `Gemini:Model` | Gemini model id for `generateContent` | No (has default) | `appsettings.json`, User Secrets, env | `gemini-3.5-flash-lite` |
| `Gemini:BaseUrl` | Gemini REST API base URL | No (has default) | `appsettings.json`, User Secrets, env | `https://generativelanguage.googleapis.com/v1beta` |
| `Gemini:TimeoutSeconds` | `HttpClient` timeout for Gemini calls | No (has default) | `appsettings.json`, User Secrets, env | `45` |
| `ConnectionStrings:DefaultConnection` | SQL Server connection | **Yes** | `appsettings.json`, User Secrets, env | See below |
| `ASPNETCORE_ENVIRONMENT` | Hosting environment (`Development` enables Swagger) | No | launchSettings / host env | `Development` |
| `VITE_API_BASE_URL` | Frontend base URL for API calls | No (defaults to `http://localhost:5056`) | `client/.env` (dev), build-time env (prod) | `http://localhost:5056` |

Environment variable equivalents use `__` for nested keys (Windows / ASP.NET Core):

| Configuration key | Environment variable |
| --- | --- |
| `Gemini:ApiKey` | `Gemini__ApiKey` |
| `Gemini:Model` | `Gemini__Model` |
| `Gemini:BaseUrl` | `Gemini__BaseUrl` |
| `Gemini:TimeoutSeconds` | `Gemini__TimeoutSeconds` |
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |

## Local development vs production

| Concern | Local development | Production |
| --- | --- | --- |
| Gemini API key | .NET User Secrets | Host secret store / env vars (never in source or frontend) |
| SQL connection | Windows auth to Express is fine | Prefer SQL auth or managed identity; restrict network access |
| Frontend API URL | `client/.env` → `VITE_API_BASE_URL=http://localhost:5056` | Set to the **public HTTPS API URL** at build time |
| Swagger | Enabled when `ASPNETCORE_ENVIRONMENT=Development` | Disabled outside Development (current `Program.cs`) |
| CORS | Hardcoded for `http://localhost:5173` and `http://127.0.0.1:5173` | Must update origins in `Program.cs` (or introduce config) before deploying |

## Backend defaults (`appsettings.json`)

File: `server/CustomerReviewInsights.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ReviewIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Gemini": {
    "ApiKey": "",
    "Model": "gemini-3.5-flash-lite",
    "BaseUrl": "https://generativelanguage.googleapis.com/v1beta",
    "TimeoutSeconds": 45
  }
}
```

`Gemini:ApiKey` is intentionally empty in source. Supply it via User Secrets or environment variables.

## .NET User Secrets

Project: `server/CustomerReviewInsights.Api/CustomerReviewInsights.Api.csproj`

`UserSecretsId` is already set to `1b9ea0f4-2f9d-4037-988b-1a0e22fa0ae5`.  
**`dotnet user-secrets init` is only needed** if you create a new project without an ID. For this repo, skip init and set values directly.

```powershell
cd server\CustomerReviewInsights.Api

dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=ReviewIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

List keys (shows values — do not share the output):

```powershell
dotnet user-secrets list
```

Remove a secret:

```powershell
dotnet user-secrets remove "Gemini:ApiKey"
```

Clear all secrets for this project:

```powershell
dotnet user-secrets clear
```

User Secrets apply when `ASPNETCORE_ENVIRONMENT=Development` (default for the `http` launch profile).

## Frontend configuration (Vite)

File template: `client/.env.example`  
Local file (not committed): `client/.env`

```env
VITE_API_BASE_URL=http://localhost:5056
```

Code reads this in `client/src/api/client.ts`. Variables prefixed with `VITE_` are embedded into the frontend bundle. **Do not store secrets in Vite env files.**

Production builds must set `VITE_API_BASE_URL` to the deployed API origin **before** `npm run build`.

## Root `.env.example`

`./.env.example` documents backend-oriented placeholders for shells that export environment variables. The ASP.NET app does **not** load a `.env` file by itself; use User Secrets or real environment variables.

## CORS note

CORS origins are currently fixed in `server/CustomerReviewInsights.Api/Program.cs`:

- `http://localhost:5173`
- `http://127.0.0.1:5173`

For a hosted frontend, update `WithOrigins(...)` to your production origin(s) and redeploy the API.
