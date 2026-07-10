using ModsenCatalog.BusinessLogic.DTOs;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Events;
using ModsenCatalog.BusinessLogic.Interfaces;

namespace ModsenCatalog.BusinessLogic.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository productRepository;
    private readonly ICategoryRepository categoryRepository;
    private readonly IEventPublisher eventPublisher;

    public ProductService(
        IProductRepository ProductRepository,
        ICategoryRepository CategoryRepository,
        IEventPublisher EventPublisher)
    {
        this.productRepository = ProductRepository;
        this.categoryRepository = CategoryRepository;
        this.eventPublisher = EventPublisher;
    }

    public async Task<Product> CreateProductAsync(string name, string description, decimal price, Guid categoryId)
    {
        var category = await categoryRepository.GetByIdAsync(categoryId);
        if (category == null)
            throw new KeyNotFoundException("Указанная категория не существует.");

        var product = new Product
        {
            Name = name,
            Description = description,
            Price = price,
            CategoryId = categoryId,
            CreatedAt = DateTime.UtcNow,
            AverageRating = 0.0
        };

        await productRepository.AddAsync(product);
        return product;
    }

    public async Task UpdateProductAsync(Product product)
    {
        if (product.CategoryId != Guid.Empty)
        {
            var category = await categoryRepository.GetByIdAsync(product.CategoryId);
            if (category == null)
                throw new KeyNotFoundException("Указанная категория не существует.");
        }

        var existingProduct = await productRepository.GetByIdAsync(product.Id);
        if (existingProduct == null)
            throw new KeyNotFoundException("Товар не найден.");

        if (existingProduct.Price != product.Price)
        {
            eventPublisher.Publish(new PriceChangedEvent
            {
                ProductName = product.Name,
                OldPrice = existingProduct.Price,
                NewPrice = product.Price
            });
        }

        await productRepository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        var product = await productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException("Товар не найден.");

        await productRepository.DeleteAsync(product);

        eventPublisher.Publish(new ProductDeletedEvent
        {
            ProductName = product.Name
        });
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
    {
        return await productRepository.GetByIdAsync(id);
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(ProductSearchParameters parameters)
    {
        var items = await productRepository.GetPagedAsync(
            parameters.PageNumber,
            parameters.PageSize,
            parameters.SearchTerm,
            parameters.CategoryId,
            parameters.MinPrice,
            parameters.MaxPrice,
            parameters.MinRating,
            parameters.SortBy,
            parameters.IsDescending
        );

        var totalCount = await productRepository.GetTotalCountAsync(
            parameters.SearchTerm,
            parameters.CategoryId,
            parameters.MinPrice,
            parameters.MaxPrice,
            parameters.MinRating
        );

        return (items, totalCount);
    }
}