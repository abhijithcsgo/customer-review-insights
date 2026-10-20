# API Reference

Base URL (local): `http://localhost:5056`  
Controller: `server/CustomerReviewInsights.Api/Controllers/ReviewsController.cs`  
Route prefix: `api/reviews`  
JSON property naming: camelCase

Swagger UI is available in Development at `/swagger`.

Common error body shape (`ApiErrorResponse`):

```json
{
  "error": "string",
  "detail": "string | null",
  "validationErrors": {
    "fieldName": ["message"]
  }
}
```

---

## GET `/api/reviews`

**Purpose:** Return all saved reviews, newest first (`CreatedAt` descending).

**Headers:** `Accept: application/json` (optional)

**Request body:** none

**Success — `200 OK`:**

```json
[
  {
    "id": 1,
    "reviewText": "The application crashes whenever I open order history.",
    "sentiment": "Negative",
    "category": "Bug",
    "summary": "App crashes when opening order history",
    "createdAt": "2026-10-20T10:00:00Z"
  }
]
```

**Errors:**

| Status | When |
| --- | --- |
| `500` | Unexpected database/server failure |

**Implementation:** `ReviewsController.GetAll` → `IReviewService.GetAllAsync`

---

## GET `/api/reviews/{id}`

**Purpose:** Return a single review by integer id.

**Success — `200 OK`:** same object shape as one element above.

**Errors:**

| Status | When |
| --- | --- |
| `404` | No review with that id |
| `500` | Unexpected failure |

**Example `404`:**

```json
{
  "error": "Review not found.",
  "detail": "No review exists with id 999.",
  "validationErrors": null
}
```

**Implementation:** `ReviewsController.GetById` → `IReviewService.GetByIdAsync`

---

## POST `/api/reviews`

**Purpose:** Validate review text, analyze with Gemini (structured JSON), persist one `CustomerReviews` row, return the saved record.

**Headers:**

- `Content-Type: application/json`
- `Accept: application/json`

**Request:**

```json
{
  "reviewText": "The application crashes whenever I open order history."
}
```

**Validation rules** (`CreateReviewRequest` + service checks):

| Rule | Detail |
| --- | --- |
| Required | `reviewText` must be present and non-empty after trim |
| Max length | 5,000 characters (`ReviewAnalysisConstants.MaxReviewLength`) |
| AI output | Sentiment ∈ Positive / Neutral / Negative |
| AI output | Category ∈ Pricing / Bug / Usability / Feature Request / Other |
| AI output | Summary required, ≤ 200 characters, ≤ 10 words |
| Persistence | Review is **not** saved if AI call or validation fails |

**Success — `201 Created`:**

```json
{
  "id": 1,
  "reviewText": "The application crashes whenever I open order history.",
  "sentiment": "Negative",
  "category": "Bug",
  "summary": "App crashes when opening order history",
  "createdAt": "2026-10-20T10:00:00Z"
}
```

`Location` header points to `/api/reviews/{id}`.

**Errors:**

| Status | When | Typical `error` |
| --- | --- | --- |
| `400` | Model validation or `ArgumentException` (empty / too long) | `Validation failed.` / `Invalid review.` |
| `502` | Gemini missing key, timeout, provider error, invalid AI JSON | `Review analysis failed.` |
| `500` | Unexpected server error | `Unable to create the review.` |

**Example validation failure (`400`):**

```json
{
  "error": "Validation failed.",
  "detail": "One or more fields are invalid.",
  "validationErrors": {
    "ReviewText": ["Review text is required."]
  }
}
```

**Example AI failure (`502`):**

```json
{
  "error": "Review analysis failed.",
  "detail": "Gemini API key is not configured. Set Gemini:ApiKey via user secrets or environment variables.",
  "validationErrors": null
}
```

**Implementation:** `ReviewsController.Create` → `ReviewService.CreateAsync` → `IReviewAnalysisService.AnalyzeAsync` (`GeminiReviewAnalysisService`)

---

## Notes

- There is **no** dedicated health-check endpoint in the current codebase. Use `GET /api/reviews` or Swagger for smoke tests.
- CORS allows only `http://localhost:5173` and `http://127.0.0.1:5173` (see `Program.cs`).
- Gemini model/schema configuration: `GeminiReviewAnalysisService` + `GeminiOptions`.
