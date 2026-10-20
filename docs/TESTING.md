# Testing Guide

## Projects

| Item | Path |
| --- | --- |
| Solution | `server/CustomerReviewInsights.sln` |
| API project | `server/CustomerReviewInsights.Api/CustomerReviewInsights.Api.csproj` |
| Test project | `server/CustomerReviewInsights.Tests/CustomerReviewInsights.Tests.csproj` |

## Restore and run all backend tests

```powershell
cd <repo-root>
dotnet restore .\server\CustomerReviewInsights.sln
dotnet test .\server\CustomerReviewInsights.sln
```

Run only the test project:

```powershell
dotnet test .\server\CustomerReviewInsights.Tests\CustomerReviewInsights.Tests.csproj
```

## What the tests cover

| File | Coverage |
| --- | --- |
| `CreateReviewRequestValidationTests.cs` | Required text, max length (5,000), valid payload |
| `ReviewAnalysisValidationTests.cs` | Sentiment/category allow-lists, summary word count, JSON parse success/failure |
| `ReviewServiceTests.cs` | Persist after mocked AI success; **no** persist when AI throws; newest-first ordering |

External dependencies for automated tests:

- **No Gemini API key** required — AI is mocked with Moq
- **No SQL Server** required — EF Core InMemory provider is used in service tests

## Interpreting results

Successful run looks like:

```text
Passed!  - Failed: 0, Passed: 20, Skipped: 0, Total: 20
```

A non-zero exit code means one or more tests failed. Open the failing assertion message for details.

> Documentation note: the suite was executed successfully in this environment with **20 passed**. Re-run locally after changes.

## Frontend checks (not unit-tested)

```powershell
cd <repo-root>\client
npm install
npm run build
```

There is no automated frontend test suite in this repository.

## Manual end-to-end test (AI + SQL)

Prerequisites: SQL Server migrated, Gemini key in User Secrets, API + Vite running.

1. Open http://localhost:5173
2. Confirm history loads (`GET /api/reviews`)
3. Submit sample reviews from [`sample-reviews.txt`](./sample-reviews.txt)
4. Confirm each success adds a row with sentiment badge, category, summary, timestamp
5. Submit empty text → client validation blocks submit
6. Temporarily remove/break the API key → expect HTTP 502 / UI error and **no new DB row**

Useful Swagger checks: http://localhost:5056/swagger
