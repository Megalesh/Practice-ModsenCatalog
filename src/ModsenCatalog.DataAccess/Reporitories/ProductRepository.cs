using Dapper;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.DataAccess.Context;
using System.Text;

namespace ModsenCatalog.DataAccess.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _dbFactory;

    public ProductRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        double? minRating = null,
        string sortBy = "date",
        bool isDescending = false)
    {
        var sql = new StringBuilder("SELECT * FROM Products WHERE 1=1");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql.Append(" AND Name LIKE @SearchTerm");
            parameters.Add("@SearchTerm", $"%{searchTerm}%");
        }
        if (categoryId.HasValue)
        {
            sql.Append(" AND CategoryId = @CategoryId");
            parameters.Add("@CategoryId", categoryId.Value);
        }
        if (minPrice.HasValue)
        {
            sql.Append(" AND Price >= @MinPrice");
            parameters.Add("@MinPrice", minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            sql.Append(" AND Price <= @MaxPrice");
            parameters.Add("@MaxPrice", maxPrice.Value);
        }
        if (minRating.HasValue)
        {
            sql.Append(" AND AverageRating >= @MinRating");
            parameters.Add("@MinRating", minRating.Value);
        }

        string orderByColumn = sortBy.ToLower() switch
        {
            "price" => "Price",
            "rating" => "AverageRating",
            "name" => "Name",
            _ => "CreatedAt"
        };

        string orderDirection = isDescending ? "DESC" : "ASC";
        sql.Append($" ORDER BY {orderByColumn} {orderDirection}");

        int offset = (pageNumber - 1) * pageSize;
        sql.Append(" OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

        parameters.Add("@Offset", offset);
        parameters.Add("@PageSize", pageSize);

        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryAsync<Product>(sql.ToString(), parameters);
    }

    public async Task<int> GetTotalCountAsync(
        string? searchTerm = null,
        Guid? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        double? minRating = null)
    {
        var sql = new StringBuilder("SELECT COUNT(1) FROM Products WHERE 1=1");
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sql.Append(" AND Name LIKE @SearchTerm");
            parameters.Add("@SearchTerm", $"%{searchTerm}%");
        }
        if (categoryId.HasValue)
        {
            sql.Append(" AND CategoryId = @CategoryId");
            parameters.Add("@CategoryId", categoryId.Value);
        }
        if (minPrice.HasValue)
        {
            sql.Append(" AND Price >= @MinPrice");
            parameters.Add("@MinPrice", minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            sql.Append(" AND Price <= @MaxPrice");
            parameters.Add("@MaxPrice", maxPrice.Value);
        }
        if (minRating.HasValue)
        {
            sql.Append(" AND AverageRating >= @MinRating");
            parameters.Add("@MinRating", minRating.Value);
        }

        await using var connection = _dbFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(sql.ToString(), parameters);
    }

    public async Task UpdateAverageRatingAsync(Guid productId, double newAverageRating)
    {
        const string sql = "UPDATE Products SET AverageRating = @NewRating WHERE Id = @ProductId";
        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new { ProductId = productId, NewRating = newAverageRating });
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Products WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Products";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryAsync<Product>(sql);
    }

    public async Task AddAsync(Product entity)
    {
        const string sql = @"
            INSERT INTO Products (Id, Name, Description, Price, AverageRating, CategoryId, CreatedAt)
            VALUES (@Id, @Name, @Description, @Price, @AverageRating, @CategoryId, @CreatedAt)";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task UpdateAsync(Product entity)
    {
        const string sql = @"
            UPDATE Products 
            SET Name = @Name, 
                Description = @Description, 
                Price = @Price, 
                AverageRating = @AverageRating, 
                CategoryId = @CategoryId 
            WHERE Id = @Id";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(Product entity)
    {
        const string sql = "DELETE FROM Products WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new { Id = entity.Id });
    }
}