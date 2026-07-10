using BCrypt.Net;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Enums;
using ModsenCatalog.BusinessLogic.Events;
using ModsenCatalog.BusinessLogic.Interfaces;

namespace ModsenCatalog.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository userRepository;
    private readonly IEventPublisher eventPublisher;

    private const int MaxLoginAttempts = 3;

    public AuthService(IUserRepository UserRepository, IEventPublisher EventPublisher)
    {
        userRepository = UserRepository;
        eventPublisher = EventPublisher;
    }

    public async Task<User> RegisterAsync(string username, string email, string password)
    {
        if (await userRepository.ExistsByEmailAsync(email))
            throw new InvalidOperationException("Пользователь с таким Email уже существует.");

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(user);

        return user;
    }

    public async Task<User> LoginAsync(string usernameOrEmail, string password)
    {
        var user = await userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);

        if (user == null)
            throw new UnauthorizedAccessException("Неверный логин или пароль.");

        if (user.IsLocked && user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            var timeLeft = user.LockedUntil.Value - DateTime.UtcNow;
            throw new UnauthorizedAccessException(
                $"Аккаунт заблокирован. Попробуйте через {timeLeft.Minutes} мин.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= MaxLoginAttempts)
            {
                user.IsLocked = true;
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }

            await userRepository.UpdateAsync(user);
            throw new UnauthorizedAccessException("Неверный логин или пароль.");
        }

        user.FailedLoginAttempts = 0;
        user.IsLocked = false;
        user.LockedUntil = null;
        await userRepository.UpdateAsync(user);

        eventPublisher.Publish(new UserLoggedInEvent
        {
            Username = user.Username
        });

        return user;
    }

    public void Logout()
    {
        Console.WriteLine("Вы успешно вышли из системы.");
    }
}

