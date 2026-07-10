using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        double? minRating = null,
        string sortBy = "date",
        bool isDescending = false);

    Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        double? minRating = null);

    Task UpdateAverageRatingAsync(Guid productId, double newAverageRating);
}