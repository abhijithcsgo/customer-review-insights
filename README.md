# Customer Review Insights

AI-assisted customer review analysis for product teams. Users paste free-text feedback; the backend classifies sentiment and category, generates a short summary with Google Gemini, stores the result in SQL Server, and shows history in a React dashboard.

> **Live AI analysis requires your own Gemini API key and available provider quota.** Without a key, review submission fails at analysis and nothing is saved.

## Problem statement

Support and product teams receive unstructured customer feedback that is hard to triage at scale. This application turns each review into structured signals (sentiment, category, summary) so teams can scan history quickly without manual tagging.

## Key features

- Submit customer reviews from a responsive web UI
- Gemini structured JSON analysis (sentiment, category, ≤10-word summary)
- Persist original text + AI fields in one `CustomerReviews` table
- Review history with search, sentiment badges, and sticky table scrolling
- Validation on client and server (max 5,000 characters)
- Automated backend tests with a mocked AI service
- Swagger UI in Development

## Technology stack (verified from this repo)

| Layer | Technology | Notes |
| --- | --- | --- |
| Frontend | React 19, TypeScript ~5.8, Vite 6 | `client/` |
| Backend | ASP.NET Core **.NET 9** | `net9.0` |
| ORM | EF Core 9.0.10 + SQL Server provider | Migrations included |
| AI | Google Gemini REST API | Default model `gemini-3.5-flash-lite` |
| API docs | Swashbuckle / Swagger | Development only |
| Tests | xUnit, Moq, EF InMemory | 29 tests |

## Architecture overview

```text
React (Vite :5173)
    │  HTTP JSON
    ▼
ASP.NET Core API (:5056)
    ├── Controllers / DTOs / validation
    ├── GeminiReviewAnalysisService  →  Google Gemini
    └── AppDbContext (EF Core)       →  SQL Server (Database=ReviewIQ)
```

Failed AI analysis does **not** create a database row.

## Repository structure

```text
./
├── client/                                 # React + Vite frontend
│   ├── src/
│   │   ├── api/client.ts                   # Central API client (VITE_API_BASE_URL)
│   │   ├── components/
│   │   ├── types/
│   │   ├── App.tsx
│   │   └── index.css
│   ├── .env.example
│   └── package.json
├── server/
│   ├── CustomerReviewInsights.Api/
│   │   ├── Controllers/ReviewsController.cs
│   │   ├── Configuration/GeminiOptions.cs
│   │   ├── Data/ + Migrations/
│   │   ├── DTOs/ Models/ Services/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   ├── CustomerReviewInsights.Tests/
│   └── CustomerReviewInsights.sln
├── docs/
│   ├── LOCAL_SETUP.md
│   ├── CONFIGURATION.md
│   ├── API_REFERENCE.md
│   ├── TESTING.md
│   ├── DEPLOYMENT.md
│   └── sample-reviews.txt
├── .env.example
├── .gitignore
└── README.md
```

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- Microsoft SQL Server (Express / Developer / LocalDB)
- [`dotnet-ef`](https://learn.microsoft.com/ef/core/cli/dotnet) global tool
- Gemini API key from [Google AI Studio](https://aistudio.google.com/apikey)

Optional: Visual Studio 2022 with ASP.NET workload.

## Quick start

```powershell
# 1) Backend restore + database
dotnet restore .\server\CustomerReviewInsights.sln
dotnet ef database update --project .\server\CustomerReviewInsights.Api\CustomerReviewInsights.Api.csproj

# 2) Gemini key (User Secrets already have a UserSecretsId)
cd .\server\CustomerReviewInsights.Api
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
cd ..\..

# 3) Frontend env
copy .\client\.env.example .\client\.env

# 4) Terminal A — API
cd .\server\CustomerReviewInsights.Api
dotnet run --launch-profile http

# 5) Terminal B — UI
cd .\client
npm install
npm run dev
```

- UI: http://localhost:5173  
- API: http://localhost:5056  
- Swagger: http://localhost:5056/swagger  

Detailed Windows steps: [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md)

### Visual Studio

1. Open `server/CustomerReviewInsights.sln`
2. Set **CustomerReviewInsights.Api** as Startup Project
3. Launch profile **http** → F5
4. Run `npm run dev` in `client/` separately

## Configuration

See [`docs/CONFIGURATION.md`](docs/CONFIGURATION.md).

| Setting | Required | Notes |
| --- | --- | --- |
| `Gemini:ApiKey` / `Gemini__ApiKey` | Yes for live AI | User Secrets or host env — **never** in React |
| `ConnectionStrings:DefaultConnection` | Yes | Default targets `localhost\SQLEXPRESS`, DB name `ReviewIQ` |
| `VITE_API_BASE_URL` | Recommended | Frontend only; defaults to `http://localhost:5056` |

## Database

EF Core migration creates:

- Database name: `ReviewIQ` (connection string; not renamed with the C# projects)
- Table: `CustomerReviews` (`Id`, `ReviewText`, `Sentiment`, `Category`, `Summary`, `CreatedAt` UTC)

```powershell
dotnet ef database update --project .\server\CustomerReviewInsights.Api\CustomerReviewInsights.Api.csproj
```

## API endpoints

Full details: [`docs/API_REFERENCE.md`](docs/API_REFERENCE.md)

| Method | Route | Success |
| --- | --- | --- |
| `GET` | `/api/reviews` | `200` list (newest first) |
| `GET` | `/api/reviews/{id}` | `200` or `404` |
| `POST` | `/api/reviews` | `201` created; `400` validation; `502` AI failure |

`POST` body:

```json
{ "reviewText": "The application crashes whenever I open order history." }
```

## Using the application

1. Open the UI and wait for history to load.
2. Paste feedback (max 5,000 characters) and submit.
3. Wait for Gemini analysis; on success the row appears immediately.
4. Use search to filter by text, sentiment, category, or summary.
5. Click truncated review text to view the full message.

Sample texts: [`docs/sample-reviews.txt`](docs/sample-reviews.txt)

## Tests

```powershell
dotnet test .\server\CustomerReviewInsights.sln
```

Details: [`docs/TESTING.md`](docs/TESTING.md)

## Troubleshooting

| Issue | What to check |
| --- | --- |
| UI cannot load reviews | API running? `VITE_API_BASE_URL`? CORS (localhost:5173 only)? |
| Submit → “API key not configured” | `dotnet user-secrets set "Gemini:ApiKey" "..."` in Api project |
| Submit → timeout / 502 | Model availability/quota; default `gemini-3.5-flash-lite`; logs show status without secrets |
| EF / SQL errors | SQL service running? Server name `SQLEXPRESS`? Migration applied? |
| CORS in production | Update `WithOrigins` in `Program.cs` — currently localhost only |

## Production deployment

See [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md).

High level:

1. Host static `client/dist` with production `VITE_API_BASE_URL`
2. Publish API (`dotnet publish`) to a .NET 9 host
3. Point connection string at hosted SQL Server; run migrations
4. Supply `Gemini__ApiKey` via host secrets
5. Update CORS origins for the real frontend URL

Hosted AI, database, and app hosting may incur provider charges.

## Security considerations

- Gemini key stays on the backend only
- Do not commit `client/.env`, User Secrets, or local overrides
- `VITE_*` values are public in the browser bundle
- Rotate any key that may have been exposed
- Prefer Production environment (Swagger off) and HTTPS in deployment

## Git / contribution notes

- Root `.gitignore` excludes `node_modules/`, `dist/`, `bin/`, `obj/`, `.vs/`, local `.env` files (keeps `.env.example`)
- `.gitignore` does **not** untrack files already committed — use `git rm --cached` if needed
- Keep PRs focused; do not commit secrets or machine-specific paths

## Documentation index

| Doc | Description |
| --- | --- |
| [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md) | New machine setup |
| [`docs/CONFIGURATION.md`](docs/CONFIGURATION.md) | Settings reference |
| [`docs/API_REFERENCE.md`](docs/API_REFERENCE.md) | Endpoints |
| [`docs/TESTING.md`](docs/TESTING.md) | Automated + manual tests |
| [`docs/DEPLOYMENT.md`](docs/DEPLOYMENT.md) | Hosting guide |
| [`docs/sample-reviews.txt`](docs/sample-reviews.txt) | Manual sample inputs |
| [`docs/SECURITY.md`](docs/SECURITY.md) | Security notes |
