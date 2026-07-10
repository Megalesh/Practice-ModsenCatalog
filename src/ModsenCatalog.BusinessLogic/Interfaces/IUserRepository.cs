using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<bool> ExistsByEmailAsync(string email);
    Task UpdateLoginAttemptsAsync(Guid userId, int attempts, bool isLocked, DateTime? lockedUntil);
}