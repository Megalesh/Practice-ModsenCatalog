using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Enums;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface IUserService
{
    Task<IEnumerable<User>> GetAllUsersAsync();

    Task<User?> GetUserByIdAsync(Guid id);

    Task<User> CreateUserAsync(string username, string email, string password, UserRole role);

    Task UpdateUserAsync(User user);

    Task DeleteUserAsync(Guid userId);

    Task ChangeUserRoleAsync(Guid userId, UserRole newRole);
}