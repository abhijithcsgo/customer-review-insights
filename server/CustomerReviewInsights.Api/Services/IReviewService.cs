using CustomerReviewInsights.Api.DTOs;

namespace CustomerReviewInsights.Api.Services;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ReviewResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ReviewResponse> CreateAsync(CreateReviewRequest request, CancellationToken cancellationToken = default);
}
