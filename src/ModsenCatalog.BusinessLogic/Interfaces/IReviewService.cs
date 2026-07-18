using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface IReviewService
{
    Task<Review> CreateReviewAsync(Guid userId, Guid productId, int rating, string comment);

    Task UpdateReviewAsync(Review review);

    Task DeleteReviewAsync(Guid reviewId);

    Task<Review?> GetReviewByIdAsync(Guid id);

    Task<(IEnumerable<Review> Items, int TotalCount)> GetReviewsByProductAsync(Guid productId, int pageNumber, int pageSize);
    Task<IEnumerable<Review>> GetReviewsByUserIdAsync(Guid userId);
}