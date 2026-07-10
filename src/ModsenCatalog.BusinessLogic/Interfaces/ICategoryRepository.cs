using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
    Task<bool> HasProductsAsync(Guid categoryId);
    Task MoveProductsToCategoryAsync(Guid sourceCategoryId, Guid targetCategoryId);
}