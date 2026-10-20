using Microsoft.AspNetCore.Mvc;
using CustomerReviewInsights.Api.DTOs;
using CustomerReviewInsights.Api.Services;

namespace CustomerReviewInsights.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetAllAsync(cancellationToken);
        return Ok(reviews);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReviewResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var review = await reviewService.GetByIdAsync(id, cancellationToken);
        if (review is null)
        {
            return NotFound(new ApiErrorResponse
            {
                Error = "Review not found.",
                Detail = $"No review exists with id {id}."
            });
        }

        return Ok(review);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReviewResponse>> Create(
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        // ArgumentException (validation) → 400. ReviewAnalysisException (Gemini) → 502.
        // Database and other unexpected exceptions → 500 via GlobalExceptionHandler.
        var created = await reviewService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
