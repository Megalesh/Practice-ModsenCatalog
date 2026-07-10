namespace ModsenCatalog.BusinessLogic.Services;

using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Events;
using ModsenCatalog.BusinessLogic.Interfaces;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository categoryRepository;
    private readonly IEventPublisher eventPublisher;

    private const string ArchiveCategoryName = "Архив";

    public CategoryService(
        ICategoryRepository CategoryRepository,
        IEventPublisher EventPublisher)
    {
        categoryRepository = CategoryRepository;
        eventPublisher = EventPublisher;
    }

    public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return await categoryRepository.GetAllAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(Guid id)
    {
        return await categoryRepository.GetByIdAsync(id);
    }

    public async Task<Category> CreateCategoryAsync(string name, string description)
    {
        var existing = await categoryRepository.GetByNameAsync(name);
        if (existing != null)
            throw new InvalidOperationException($"Категория с названием '{name}' уже существует.");

        var category = new Category
        {
            Name = name,
            Description = description
        };

        await categoryRepository.AddAsync(category);
        return category;
    }

    public async Task UpdateCategoryAsync(Category category)
    {
        var existing = await categoryRepository.GetByNameAsync(category.Name);
        if (existing != null && existing.Id != category.Id)
            throw new InvalidOperationException($"Категория с названием '{category.Name}' уже существует.");

        await categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            throw new KeyNotFoundException("Категория не найдена.");

        if (category.Name.Equals(ArchiveCategoryName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Нельзя удалить системную категорию 'Архив'.");

        bool hasProducts = await categoryRepository.HasProductsAsync(categoryId);

        if (hasProducts)
        {
            await MoveProductsToArchiveAsync(categoryId);
        }

        await categoryRepository.DeleteAsync(category);

        eventPublisher.Publish(new CategoryDeletedEvent
        {
            CategoryName = category.Name
        });
    }

    private async Task MoveProductsToArchiveAsync(Guid sourceCategoryId)
    {
        var archiveCategory = await categoryRepository.GetByNameAsync(ArchiveCategoryName);

        if (archiveCategory == null)
        {
            archiveCategory = new Category
            {
                Name = ArchiveCategoryName,
                Description = "Автоматически созданная категория для удаленных товаров"
            };
            await categoryRepository.AddAsync(archiveCategory);
        }

        await categoryRepository.MoveProductsToCategoryAsync(sourceCategoryId, archiveCategory.Id);
    }
}