using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.BusinessLogic.Interfaces;

public interface IAuthService
{
    Task<User> RegisterAsync(string username, string email, string password);

    Task<User> LoginAsync(string usernameOrEmail, string password);

    void Logout();
}