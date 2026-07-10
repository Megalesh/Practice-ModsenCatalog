using BCrypt.Net;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Enums;
using ModsenCatalog.BusinessLogic.Events;
using ModsenCatalog.BusinessLogic.Interfaces;

namespace ModsenCatalog.BusinessLogic.Services;

public class UserService : IUserService
{
    private readonly IUserRepository userRepository;
    private readonly IEventPublisher eventPublisher;

    public UserService(IUserRepository UserRepository, IEventPublisher EventPublisher)
    {
        userRepository = UserRepository;
        eventPublisher = EventPublisher;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await userRepository.GetAllAsync();
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await userRepository.GetByIdAsync(id);
    }

    public async Task<User> CreateUserAsync(string username, string email, string password, UserRole role)
    {
        if (await userRepository.ExistsByEmailAsync(email))
            throw new InvalidOperationException("Пользователь с таким Email уже существует.");

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user);
        return user;
    }

    public async Task UpdateUserAsync(User user)
    {
        var existingUser = await userRepository.GetByUsernameOrEmailAsync(user.Email);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            throw new InvalidOperationException("Этот Email уже используется другим пользователем.");
        }

        await userRepository.UpdateAsync(user);
    }

    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден.");

        await userRepository.DeleteAsync(user);
    }

    public async Task ChangeUserRoleAsync(Guid userId, UserRole newRole)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException("Пользователь не найден.");

        var oldRole = user.Role;

        if (oldRole == newRole) return;

        user.Role = newRole;
        await userRepository.UpdateAsync(user);

        eventPublisher.Publish(new UserRoleChangedEvent
        {
            Username = user.Username,
            OldRole = oldRole.ToString(),
            NewRole = newRole.ToString()
        });
    }
}