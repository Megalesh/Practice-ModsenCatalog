using Dapper;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.DataAccess.Context;

namespace ModsenCatalog.DataAccess.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly DbConnectionFactory _dbFactory;

    public ReviewRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> ExistsByUserAndProductAsync(Guid userId, Guid productId)
    {
        const string sql = "SELECT COUNT(1) FROM Reviews WHERE UserId = @UserId AND ProductId = @ProductId";
        await using var connection = _dbFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId, ProductId = productId });

        return count > 0;
    }

    public async Task<Review?> GetByUserAndProductAsync(Guid userId, Guid productId)
    {
        const string sql = "SELECT * FROM Reviews WHERE UserId = @UserId AND ProductId = @ProductId";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Review>(sql, new { UserId = userId, ProductId = productId });
    }

    public async Task<IEnumerable<Review>> GetByProductIdAsync(Guid productId, int pageNumber, int pageSize)
    {
        const string sql = @"
            SELECT * FROM Reviews 
            WHERE ProductId = @ProductId 
            ORDER BY CreatedAt DESC 
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        int offset = (pageNumber - 1) * pageSize;
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryAsync<Review>(sql, new
        {
            ProductId = productId,
            Offset = offset,
            PageSize = pageSize
        });
    }

    public async Task<int> GetCountByProductIdAsync(Guid productId)
    {
        const string sql = "SELECT COUNT(1) FROM Reviews WHERE ProductId = @ProductId";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(sql, new { ProductId = productId });
    }

    public async Task<Review?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Reviews WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Review>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Review>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Reviews";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryAsync<Review>(sql);
    }

    public async Task AddAsync(Review entity)
    {
        const string sql = @"
            INSERT INTO Reviews (Id, Rating, Comment, UserId, ProductId, CreatedAt)
            VALUES (@Id, @Rating, @Comment, @UserId, @ProductId, @CreatedAt)";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task UpdateAsync(Review entity)
    {
        const string sql = @"
            UPDATE Reviews 
            SET Rating = @Rating, 
                Comment = @Comment 
            WHERE Id = @Id";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(Review entity)
    {
        const string sql = "DELETE FROM Reviews WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new { Id = entity.Id });
    }

    public async Task<double?> GetAverageRatingForProductAsync(Guid productId)
    {
        const string sql = "SELECT AVG(CAST(Rating AS FLOAT)) FROM Reviews WHERE ProductId = @ProductId";
        await using var connection = _dbFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<double?>(sql, new { ProductId = productId });
    }

    public async Task<IEnumerable<Review>> GetByUserIdAsync(Guid userId)
    {
        const string sql = "SELECT * FROM Reviews WHERE UserId = @UserId ORDER BY CreatedAt DESC";
        await using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<Review>(sql, new { UserId = userId });
    }
}