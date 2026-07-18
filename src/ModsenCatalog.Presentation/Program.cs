using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModsenCatalog.BusinessLogic.Events;
using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.BusinessLogic.Services;
using ModsenCatalog.DataAccess.Context;
using ModsenCatalog.DataAccess.Repositories;
using ModsenCatalog.Presentation.States;
using ModsenCatalog.Presentation.UI;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();

services.AddSingleton<DbConnectionFactory>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ICategoryRepository, CategoryRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
services.AddScoped<IReviewRepository, ReviewRepository>();

services.AddSingleton<IEventPublisher, EventPublisher>();

services.AddScoped<IAuthService, AuthService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<IProductService, ProductService>();
services.AddScoped<IReviewService, ReviewService>();

services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton<ConsoleHelper>();
services.AddTransient<MainMenuState>();
services.AddTransient<AdminMenuState>();
services.AddTransient<ManagerMenuState>();
services.AddTransient<CustomerMenuState>();

var serviceProvider = services.BuildServiceProvider();

var eventPublisher = serviceProvider.GetRequiredService<IEventPublisher>();
var consoleHelper = serviceProvider.GetRequiredService<ConsoleHelper>();

eventPublisher.Subscribe<UserLoggedInEvent>(e =>
{
    consoleHelper.WriteSuccess($"Добро пожаловать, {e.Username}!");
});

eventPublisher.Subscribe<PriceChangedEvent>(e =>
{
    consoleHelper.WriteWarning($"Цена на '{e.ProductName}' изменена: {e.OldPrice:C} -> {e.NewPrice:C}");
});

eventPublisher.Subscribe<ProductDeletedEvent>(e =>
{
    consoleHelper.WriteError($"Товар '{e.ProductName}' был удален.");
});

eventPublisher.Subscribe<ReviewAddedEvent>(e =>
{
    consoleHelper.WriteSuccess($"Спасибо за отзыв на товар '{e.ProductName}'!");
});

eventPublisher.Subscribe<ReviewDeletedEvent>(e =>
{
    consoleHelper.WriteWarning($"Ваш отзыв на товар '{e.ProductName}' удален.");
});

eventPublisher.Subscribe<UserRoleChangedEvent>(e =>
{
    consoleHelper.WriteInfo($"Роль пользователя '{e.Username}' изменена с {e.OldRole} на {e.NewRole}.");
});

eventPublisher.Subscribe<CategoryDeletedEvent>(e =>
{
    consoleHelper.WriteError($"Категория '{e.CategoryName}' удалена. Товары перенесены в Архив.");
});

try
{
    var authservice = serviceProvider.GetRequiredService<IAuthService>();
    var menuContext = new MenuContext(serviceProvider);

    Console.WriteLine("=== MODSEN CATALOG ===");

    bool isRunning = true;
    while (isRunning)
    {
        try
        {
            var currentState = menuContext.CurrentState;
            currentState.DisplayMenu();

            string input = Console.ReadLine()?.Trim() ?? "";

            var nextState = currentState.HandleInput(input);

            if (nextState != null)
            {
                menuContext.SetState(nextState);
            }

            if (menuContext.ShouldExit)
            {
                isRunning = false;
            }
        }
        catch (Exception ex)
        {
            consoleHelper.WriteError($"Ошибка выполнения: {ex.Message}");
        }
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Критическая ошибка: {ex.Message}");
    Console.ResetColor();
}
finally
{
    Console.WriteLine("Приложение завершено. Нажмите любую клавишу...");
    Console.ReadKey();
}