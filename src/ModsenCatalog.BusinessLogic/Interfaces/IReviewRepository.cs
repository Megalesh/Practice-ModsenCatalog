using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<bool> ExistsByUserAndProductAsync(Guid userId, Guid productId);
    Task<Review?> GetByUserAndProductAsync(Guid userId, Guid productId);
    Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId, int pageNumber, int pageSize);
    Task<int> GetCountByProductIdAsync(Guid productId);
    Task<double?> GetAverageRatingForProductAsync(Guid productId);
}