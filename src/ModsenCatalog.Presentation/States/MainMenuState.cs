using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.Presentation.UI;

namespace ModsenCatalog.Presentation.States;

public class MainMenuState : IMenuState
{
    private readonly MenuContext _context;
    private readonly ConsoleHelper _console;

    public MainMenuState(MenuContext context)
    {
        _context = context;
        _console = _context.GetService<ConsoleHelper>();
    }

    public void DisplayMenu()
    {
        Console.Clear();
        _console.WriteTitle("ГЛАВНОЕ МЕНЮ");
        Console.WriteLine("1. Войти в систему");
        Console.WriteLine("2. Зарегистрироваться");
        Console.WriteLine("0. Выход");
        Console.Write("Выберите действие: ");
    }

    public IMenuState? HandleInput(string input)
    {
        switch (input)
        {
            case "1":
                return HandleLogin();
            case "2":
                return HandleRegister();
            case "0":
                _context.Exit();
                return null;
            default:
                _console.WriteError("Неверный выбор. Попробуйте снова.");
                _console.WaitForEnter();

                return null;
        }
    }

    private IMenuState? HandleLogin()
    {
        try
        {
            var authService = _context.GetService<IAuthService>();

            string login = _console.ReadNonEmptyLine("Логин или Email: ");
            string password = _console.ReadPassword("Пароль: ");

            var user = authService.LoginAsync(login, password).Result;

            _context.SetCurrentUser(user);
            _console.WriteSuccess($"Вы вошли как: {user.Username} ({user.Role})");
            _console.WaitForEnter();

            return user.Role switch
            {
                BusinessLogic.Enums.UserRole.Admin => new AdminMenuState(_context),
                BusinessLogic.Enums.UserRole.Manager => new ManagerMenuState(_context),
                _ => new CustomerMenuState(_context)
            };
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
            _console.WaitForEnter();

            return null;
        }
    }

    private IMenuState? HandleRegister()
    {
        try
        {
            var authService = _context.GetService<IAuthService>();

            string username = _console.ReadNonEmptyLine("Придумайте имя пользователя: ");
            string email = _console.ReadNonEmptyLine("Email: ");
            string password = _console.ReadPassword("Пароль: ");

            var user = authService.RegisterAsync(username, email, password).Result;

            _console.WriteSuccess("Регистрация успешна! Теперь вы можете войти.");
            _console.WaitForEnter();

            return null;
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
            _console.WaitForEnter();

            return null;
        }
    }
}