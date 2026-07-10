using Dapper;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.DataAccess.Context;

namespace ModsenCatalog.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly DbConnectionFactory _dbFactory;

    public CategoryRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        const string sql = "SELECT * FROM Categories WHERE Name = @Name";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Name = name });
    }

    public async Task<bool> HasProductsAsync(Guid categoryId)
    {
        const string sql = "SELECT COUNT(1) FROM Products WHERE CategoryId = @CategoryId";
        await using var connection = _dbFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { CategoryId = categoryId });

        return count > 0;
    }

    public async Task MoveProductsToCategoryAsync(Guid sourceCategoryId, Guid targetCategoryId)
    {
        const string sql = @"
            UPDATE Products 
            SET CategoryId = @TargetCategoryId 
            WHERE CategoryId = @SourceCategoryId";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new
        {
            SourceCategoryId = sourceCategoryId,
            TargetCategoryId = targetCategoryId
        });
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Categories WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Categories";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryAsync<Category>(sql);
    }

    public async Task AddAsync(Category entity)
    {
        const string sql = @"
            INSERT INTO Categories (Id, Name, Description)
            VALUES (@Id, @Name, @Description)";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task UpdateAsync(Category entity)
    {
        const string sql = @"
            UPDATE Categories 
            SET Name = @Name, Description = @Description 
            WHERE Id = @Id";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(Category entity)
    {
        const string sql = "DELETE FROM Categories WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new { Id = entity.Id });
    }
}