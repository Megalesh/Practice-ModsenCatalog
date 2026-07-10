using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.DTOs;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface IProductService
{
    Task<Product> CreateProductAsync(string name, string description, decimal price, Guid categoryId);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Guid productId);

    Task<Product?> GetProductByIdAsync(Guid id);

    Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(ProductSearchParameters parameters);
}