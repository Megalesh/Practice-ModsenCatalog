using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(Guid id);
    Task<Category> CreateCategoryAsync(string name, string description);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(Guid categoryId);
}