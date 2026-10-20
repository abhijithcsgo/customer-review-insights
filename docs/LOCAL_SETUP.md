# Local Setup Guide (Windows)

Step-by-step instructions to run **Customer Review Insights** on a new Windows machine.

## 1. Install prerequisites

| Software | Verified / expected | Download |
| --- | --- | --- |
| .NET SDK | **9.x** (project targets `net9.0`) | https://dotnet.microsoft.com/download |
| Node.js + npm | Node **20+** recommended (verified with Node 22 / npm 10) | https://nodejs.org/ |
| SQL Server | Express, Developer, or LocalDB | https://www.microsoft.com/sql-server |
| EF Core tools | `dotnet-ef` global tool | See command below |
| Git | Any recent version | https://git-scm.com/ |
| Visual Studio 2022 (optional) | Workload: ASP.NET and web development | https://visualstudio.microsoft.com/ |

Verify installs:

```powershell
dotnet --version
node --version
npm --version
dotnet tool install --global dotnet-ef
# or update: dotnet tool update --global dotnet-ef
```

Ensure the SQL Server service is running (for Express, often `MSSQL$SQLEXPRESS`).

## 2. Clone the repository

```powershell
git clone <YOUR_REPOSITORY_URL>
cd <repo-root>
```

Example local path used during development: `D:\ReviewIQ`

## 3. Restore backend dependencies

```powershell
cd <repo-root>
dotnet restore .\server\CustomerReviewInsights.sln
```

## 4. Install frontend dependencies

```powershell
cd <repo-root>\client
npm install
```

## 5. Configure SQL Server and apply migrations

Default connection string (in `server/CustomerReviewInsights.Api/appsettings.json`):

```text
Server=localhost\SQLEXPRESS;Database=ReviewIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Override if needed (User Secrets):

```powershell
cd <repo-root>\server\CustomerReviewInsights.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=ReviewIQ;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Apply the existing EF Core migration (creates database `ReviewIQ` and table `CustomerReviews`):

```powershell
cd <repo-root>
dotnet ef database update --project .\server\CustomerReviewInsights.Api\CustomerReviewInsights.Api.csproj
```

The migration lives at:

`server/CustomerReviewInsights.Api/Data/Migrations/20261020034938_InitialCreate.cs`

## 6. Configure the Gemini API key

The API project already has a `UserSecretsId`. You do **not** need `dotnet user-secrets init` unless secrets were never initialized on this machine for that ID.

```powershell
cd <repo-root>\server\CustomerReviewInsights.Api
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
```

Optional overrides:

```powershell
dotnet user-secrets set "Gemini:Model" "gemini-3.5-flash-lite"
dotnet user-secrets set "Gemini:TimeoutSeconds" "45"
```

Get an API key from [Google AI Studio](https://aistudio.google.com/apikey).

> Live AI analysis requires a valid key and available quota. Without a key, `POST /api/reviews` fails with HTTP 502 and **no review is saved**.

## 7. Configure the frontend API URL

```powershell
cd <repo-root>\client
copy .env.example .env
```

`.env` should contain:

```env
VITE_API_BASE_URL=http://localhost:5056
```

`VITE_*` variables are exposed to browser code. **Never** put the Gemini API key here.

## 8. Start the backend

Terminal 1:

```powershell
cd <repo-root>\server\CustomerReviewInsights.Api
dotnet run --launch-profile http
```

Or open `server/CustomerReviewInsights.sln` in Visual Studio, set **CustomerReviewInsights.Api** as startup project, select **http**, press F5.

- API: http://localhost:5056  
- Swagger (Development only): http://localhost:5056/swagger  

## 9. Start the frontend

Terminal 2:

```powershell
cd <repo-root>\client
npm run dev
```

- UI: http://localhost:5173  

## 10. Verify the application

1. Open http://localhost:5173 — history table should load (may be empty).
2. Open http://localhost:5056/swagger — try `GET /api/reviews`.
3. Submit a short review in the UI (for example: `The app crashes when I open order history.`).
4. Confirm a new row appears with sentiment, category, summary, and timestamp.

Manual sample texts: [`sample-reviews.txt`](./sample-reviews.txt)

## 11. Run automated tests

```powershell
cd <repo-root>
dotnet test .\server\CustomerReviewInsights.sln
```

Tests mock the AI service and do not call Gemini or require a live key.

## Visual Studio tip

1. Open `server/CustomerReviewInsights.sln`
2. Set **CustomerReviewInsights.Api** as Startup Project
3. Choose launch profile **http**
4. Run the API with F5 / Ctrl+F5
5. Keep `npm run dev` running in a separate terminal for the React app
