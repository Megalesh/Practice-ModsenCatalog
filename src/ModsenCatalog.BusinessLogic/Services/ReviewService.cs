using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Events;
using ModsenCatalog.BusinessLogic.Interfaces;

namespace ModsenCatalog.BusinessLogic.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IEventPublisher _eventPublisher;

    public ReviewService(
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IEventPublisher eventPublisher)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Review> CreateReviewAsync(Guid userId, Guid productId, int rating, string comment)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Товар не найден.");

        if (await _reviewRepository.ExistsByUserAndProductAsync(userId, productId))
            throw new InvalidOperationException("Вы уже оставили отзыв на этот товар.");

        if (rating < 1 || rating > 5)
            throw new ArgumentException("Рейтинг должен быть от 1 до 5.");

        var review = new Review
        {
            UserId = userId,
            ProductId = productId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow
        };

        await _reviewRepository.AddAsync(review);

        await RecalculateProductAverageRatingAsync(productId);

        _eventPublisher.Publish(new ReviewAddedEvent
        {
            ProductName = product.Name
        });

        return review;
    }

    public async Task UpdateReviewAsync(Review review)
    {
        if (review.Rating < 1 || review.Rating > 5)
            throw new ArgumentException("Рейтинг должен быть от 1 до 5.");

        var existingReview = await _reviewRepository.GetByIdAsync(review.Id);
        if (existingReview == null)
            throw new KeyNotFoundException("Отзыв не найден.");

        await _reviewRepository.UpdateAsync(review);

        await RecalculateProductAverageRatingAsync(existingReview.ProductId);
    }

    public async Task DeleteReviewAsync(Guid reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            throw new KeyNotFoundException("Отзыв не найден.");

        var productId = review.ProductId;
        var productName = (await _productRepository.GetByIdAsync(productId))?.Name ?? "Unknown";

        await _reviewRepository.DeleteAsync(review);

        await RecalculateProductAverageRatingAsync(productId);

        _eventPublisher.Publish(new ReviewDeletedEvent
        {
            ProductName = productName
        });
    }

    public async Task<Review?> GetReviewByIdAsync(Guid id)
    {
        return await _reviewRepository.GetByIdAsync(id);
    }

    public async Task<(IEnumerable<Review> Items, int TotalCount)> GetReviewsByProductAsync(Guid productId, int pageNumber, int pageSize)
    {
        var items = await _reviewRepository.GetByProductIdAsync(productId, pageNumber, pageSize);
        var totalCount = await _reviewRepository.GetCountByProductIdAsync(productId);

        return (items, totalCount);
    }

    private async Task RecalculateProductAverageRatingAsync(Guid productId)
    {
        var avgRating = await _reviewRepository.GetAverageRatingForProductAsync(productId);
        await _productRepository.UpdateAverageRatingAsync(productId, avgRating ?? 0.0);
    }
}