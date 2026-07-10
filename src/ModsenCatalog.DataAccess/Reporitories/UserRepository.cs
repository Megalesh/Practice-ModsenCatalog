using Dapper;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.DataAccess.Context;

namespace ModsenCatalog.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbConnectionFactory _dbFactory;

    public UserRepository(DbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        const string sql = @"
            SELECT * FROM Users 
            WHERE Username = @UsernameOrEmail OR Email = @UsernameOrEmail";

        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { UsernameOrEmail = usernameOrEmail });
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

        await using var connection = _dbFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });

        return count > 0;
    }

    public async Task UpdateLoginAttemptsAsync(Guid userId, int attempts, bool isLocked, DateTime? lockedUntil)
    {
        const string sql = @"
            UPDATE Users 
            SET FailedLoginAttempts = @Attempts, 
                IsLocked = @IsLocked, 
                LockedUntil = @LockedUntil 
            WHERE Id = @UserId";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Attempts = attempts,
            IsLocked = isLocked,
            LockedUntil = lockedUntil
        });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        const string sql = "SELECT * FROM Users WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = "SELECT * FROM Users";
        await using var connection = _dbFactory.CreateConnection();

        return await connection.QueryAsync<User>(sql);
    }

    public async Task AddAsync(User entity)
    {
        const string sql = @"
            INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedAt, FailedLoginAttempts, IsLocked, LockedUntil)
            VALUES (@Id, @Username, @Email, @PasswordHash, @Role, @CreatedAt, @FailedLoginAttempts, @IsLocked, @LockedUntil)";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task UpdateAsync(User entity)
    {
        const string sql = @"
            UPDATE Users 
            SET Username = @Username, 
                Email = @Email, 
                PasswordHash = @PasswordHash, 
                Role = @Role,
                FailedLoginAttempts = @FailedLoginAttempts,
                IsLocked = @IsLocked,
                LockedUntil = @LockedUntil
            WHERE Id = @Id";

        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, entity);
    }

    public async Task DeleteAsync(User entity)
    {
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        await using var connection = _dbFactory.CreateConnection();

        await connection.ExecuteAsync(sql, new { Id = entity.Id });
    }
}