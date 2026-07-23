using ModsenCatalog.BusinessLogic.DTOs;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Enums;
using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.Presentation.UI;

namespace ModsenCatalog.Presentation.States;

public class AdminMenuState : IMenuState
{
    private readonly MenuContext _context;
    private readonly ConsoleHelper _console;

    private IUserService UserService => _context.GetService<IUserService>();
    private ICategoryService CategoryService => _context.GetService<ICategoryService>();
    private IProductService ProductService => _context.GetService<IProductService>();
    private IReviewService ReviewService => _context.GetService<IReviewService>();

    private const int DefaultPageSize = 10;

    public AdminMenuState(MenuContext context)
    {
        _context = context;
        _console = _context.GetService<ConsoleHelper>();
    }

    public void DisplayMenu()
    {
        Console.Clear();
        _console.WriteTitle($"МЕНЮ АДМИНИСТРАТОРА [{_context.CurrentUser?.Username}]");
        Console.WriteLine("1. Управление пользователями");
        Console.WriteLine("2. Управление категориями");
        Console.WriteLine("3. Управление товарами");
        Console.WriteLine("4. Просмотр отзывов");
        Console.WriteLine("0. Выход из системы");
        Console.Write("Выберите действие: ");
    }

    public IMenuState? HandleInput(string input)
    {
        switch (input)
        {
            case "1": HandleUsersManagement(); break;
            case "2": HandleCategoriesManagement(); break;
            case "3": HandleProductsManagement(); break;
            case "4": HandleReviewsView(); break;
            case "0":
                _context.SetCurrentUser(null);
                return new MainMenuState(_context);
            default:
                _console.WriteError("Неверный выбор.");
                _console.WaitForEnter();
                break;
        }
        return null;
    }

    private void HandleUsersManagement()
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            _console.WriteTitle("УПРАВЛЕНИЕ ПОЛЬЗОВАТЕЛЯМИ");
            Console.WriteLine("1. Список всех пользователей");
            Console.WriteLine("2. Создать нового пользователя");
            Console.WriteLine("3. Изменить роль пользователя");
            Console.WriteLine("4. Удалить пользователя");
            Console.WriteLine("0. Назад");
            Console.Write("Выбор: ");

            string choice = Console.ReadLine()?.Trim() ?? "";
            switch (choice)
            {
                case "1": ListUsers(); break;
                case "2": CreateUser(); break;
                case "3": ChangeUserRole(); break;
                case "4": DeleteUser(); break;
                case "0": back = true; break;
                default: _console.WriteError("Неверный ввод."); _console.WaitForEnter(); break;
            }
        }
    }

    private void ListUsers()
    {
        var users = UserService.GetAllUsersAsync().GetAwaiter().GetResult();

        _console.ShowPagedListAndSelect(
            users,
            "СПИСОК ВСЕХ ПОЛЬЗОВАТЕЛЕЙ",
            (index, user) => Console.WriteLine($"{index}. [{user.Role}] {user.Username} ({user.Email})"),
            pageSize: 10
        );
    }

    private void CreateUser()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("СОЗДАНИЕ ПОЛЬЗОВАТЕЛЯ");

            string username = _console.ReadNonEmptyLine("Имя пользователя: ");
            string email = _console.ReadNonEmptyLine("Email: ");
            string password = _console.ReadPassword("Пароль: ");

            Console.WriteLine("\nДоступные роли:");
            Console.WriteLine("0 - Customer");
            Console.WriteLine("1 - Manager");
            Console.WriteLine("2 - Admin");

            UserRole role = (UserRole)_console.ReadInt("Введите номер роли (0-2): ");

            if (!Enum.IsDefined(typeof(UserRole), role))
            {
                _console.WriteError("Некорректная роль.");
                _console.WaitForEnter();
                return;
            }

            UserService.CreateUserAsync(username, email, password, role).GetAwaiter().GetResult();
            _console.WriteSuccess("Пользователь успешно создан!");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void ChangeUserRole()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("ИЗМЕНЕНИЕ РОЛИ ПОЛЬЗОВАТЕЛЯ");

            var users = UserService.GetAllUsersAsync().GetAwaiter().GetResult();

            var userToEdit = _console.ShowPagedListAndSelect(
                users,
                "ВЫБЕРИТЕ ПОЛЬЗОВАТЕЛЯ ДЛЯ ИЗМЕНЕНИЯ РОЛИ",
                (index, user) => Console.WriteLine($"{index}. [{user.Role}] {user.Username} ({user.Email})"),
                pageSize: DefaultPageSize
            );

            if (userToEdit == null) return;

            Console.WriteLine($"\nТекущая роль: {userToEdit.Role}");
            Console.WriteLine("Новая роль:");
            Console.WriteLine("0 - Customer");
            Console.WriteLine("1 - Manager");
            Console.WriteLine("2 - Admin");

            UserRole newRole = (UserRole)_console.ReadInt("Введите номер новой роли: ");

            if (!Enum.IsDefined(typeof(UserRole), newRole))
            {
                _console.WriteError("Некорректная роль.");
                _console.WaitForEnter();
                return;
            }

            UserService.ChangeUserRoleAsync(userToEdit.Id, newRole).GetAwaiter().GetResult();
            _console.WriteSuccess("Роль успешно изменена!");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void DeleteUser()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("УДАЛЕНИЕ ПОЛЬЗОВАТЕЛЯ");

            var users = UserService.GetAllUsersAsync().GetAwaiter().GetResult();

            var userToDelete = _console.ShowPagedListAndSelect(
                users,
                "ВЫБЕРИТЕ ПОЛЬЗОВАТЕЛЯ ДЛЯ УДАЛЕНИЯ",
                (index, user) => Console.WriteLine($"{index}. [{user.Role}] {user.Username} ({user.Email})"),
                pageSize: DefaultPageSize
            );

            if (userToDelete == null) return;

            Console.Write($"Вы уверены, что хотите удалить '{userToDelete.Username}'? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            UserService.DeleteUserAsync(userToDelete.Id).GetAwaiter().GetResult();
            _console.WriteSuccess("Пользователь удален.");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void HandleCategoriesManagement()
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            _console.WriteTitle("УПРАВЛЕНИЕ КАТЕГОРИЯМИ");
            Console.WriteLine("1. Список категорий");
            Console.WriteLine("2. Создать категорию");
            Console.WriteLine("3. Редактировать категорию");
            Console.WriteLine("4. Удалить категорию");
            Console.WriteLine("0. Назад");
            Console.Write("Выбор: ");

            string choice = Console.ReadLine()?.Trim() ?? "";
            switch (choice)
            {
                case "1": ListCategories(); break;
                case "2": CreateCategory(); break;
                case "3": EditCategory(); break;
                case "4": DeleteCategory(); break;
                case "0": back = true; break;
                default: _console.WriteError("Неверный ввод."); _console.WaitForEnter(); break;
            }
        }
    }

    private void ListCategories()
    {
        var cats = CategoryService.GetAllCategoriesAsync().GetAwaiter().GetResult();

        _console.ShowPagedListAndSelect(
            cats,
            "СПИСОК КАТЕГОРИЙ",
            (index, cat) =>
            {
                Console.WriteLine($"{index}. {cat.Name}");
                Console.WriteLine($"   Описание: {cat.Description}");
            },
            pageSize: DefaultPageSize
        );
    }

    private void CreateCategory()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("СОЗДАНИЕ КАТЕГОРИИ");
            string name = _console.ReadNonEmptyLine("Название категории: ");
            string desc = Console.ReadLine() ?? "";

            CategoryService.CreateCategoryAsync(name, desc).GetAwaiter().GetResult();
            _console.WriteSuccess("Категория создана!");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void EditCategory()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("РЕДАКТИРОВАНИЕ КАТЕГОРИИ");

            var cats = CategoryService.GetAllCategoriesAsync().GetAwaiter().GetResult();

            var catToEdit = _console.ShowPagedListAndSelect(
                cats,
                "ВЫБЕРИТЕ КАТЕГОРИЮ ДЛЯ РЕДАКТИРОВАНИЯ",
                (index, cat) => Console.WriteLine($"{index}. {cat.Name}"),
                pageSize: DefaultPageSize
            );

            if (catToEdit == null) return;

            Console.WriteLine($"Текущее имя: '{catToEdit.Name}'. Оставьте пустым, чтобы не менять.");
            string newName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newName)) catToEdit.Name = newName;

            Console.WriteLine($"Текущее описание: '{catToEdit.Description}'. Оставьте пустым, чтобы не менять.");
            string newDesc = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newDesc)) catToEdit.Description = newDesc;

            CategoryService.UpdateCategoryAsync(catToEdit).GetAwaiter().GetResult();
            _console.WriteSuccess("Категория обновлена!");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void DeleteCategory()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("УДАЛЕНИЕ КАТЕГОРИИ");

            var cats = CategoryService.GetAllCategoriesAsync().GetAwaiter().GetResult();
            var deletableCats = cats.Where(c => !c.Name.Equals("Архив", StringComparison.OrdinalIgnoreCase));

            var catToDelete = _console.ShowPagedListAndSelect(
                deletableCats,
                "ВЫБЕРИТЕ КАТЕГОРИЮ ДЛЯ УДАЛЕНИЯ",
                (index, cat) => Console.WriteLine($"{index}. {cat.Name}"),
                pageSize: DefaultPageSize
            );

            if (catToDelete == null) return;

            Console.Write("Внимание! Товары будут перенесены в 'Архив'. Продолжить? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            CategoryService.DeleteCategoryAsync(catToDelete.Id).GetAwaiter().GetResult();
            _console.WriteSuccess("Категория удалена. Товары перенесены в Архив.");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void HandleProductsManagement()
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            _console.WriteTitle("УПРАВЛЕНИЕ ТОВАРАМИ");
            Console.WriteLine("1. Список товаров");
            Console.WriteLine("2. Создать товар");
            Console.WriteLine("3. Редактировать товар");
            Console.WriteLine("4. Удалить товар");
            Console.WriteLine("0. Назад");
            Console.Write("Выбор: ");

            string choice = Console.ReadLine()?.Trim() ?? "";
            switch (choice)
            {
                case "1": ListProducts(); break;
                case "2": CreateProduct(); break;
                case "3": EditProduct(); break;
                case "4": DeleteProduct(); break;
                case "0": back = true; break;
                default: _console.WriteError("Неверный ввод."); _console.WaitForEnter(); break;
            }
        }
    }

    private void ListProducts()
    {
        var result = ProductService.GetProductsAsync(new ProductSearchParameters { PageSize = 100 }).GetAwaiter().GetResult();

        _console.ShowPagedListAndSelect(
            result.Items,
            "СПИСОК ТОВАРОВ",
            (index, prod) => Console.WriteLine($"{index}. {prod.Name} | Цена: {prod.Price:C} | Рейтинг: {prod.AverageRating:F1}"),
            pageSize: DefaultPageSize
        );
    }

    private void CreateProduct()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("СОЗДАНИЕ ТОВАРА");

            string name = _console.ReadNonEmptyLine("Название товара: ");
            string desc = Console.ReadLine() ?? "";
            decimal price = _console.ReadDecimal("Цена: ");

            var cats = CategoryService.GetAllCategoriesAsync().GetAwaiter().GetResult();
            var selectedCat = _console.ShowPagedListAndSelect(
                cats,
                "ВЫБЕРИТЕ КАТЕГОРИЮ ДЛЯ ТОВАРА",
                (index, cat) => Console.WriteLine($"{index}. {cat.Name}"),
                pageSize: DefaultPageSize
            );

            if (selectedCat == null) return;

            ProductService.CreateProductAsync(name, desc, price, selectedCat.Id).GetAwaiter().GetResult();
            _console.WriteSuccess("Товар создан!");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void EditProduct()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("РЕДАКТИРОВАНИЕ ТОВАРА");

            var result = ProductService.GetProductsAsync(new ProductSearchParameters { PageSize = 100 }).GetAwaiter().GetResult();

            var prodToEdit = _console.ShowPagedListAndSelect(
                result.Items,
                "ВЫБЕРИТЕ ТОВАР ДЛЯ РЕДАКТИРОВАНИЯ",
                (index, prod) => Console.WriteLine($"{index}. {prod.Name} | Цена: {prod.Price:C}"),
                pageSize: DefaultPageSize
            );

            if (prodToEdit == null) return;

            Console.WriteLine($"Текущая цена: {prodToEdit.Price:C}. Новая цена (Enter для пропуска): ");
            string priceInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(priceInput))
            {
                prodToEdit.Price = decimal.Parse(priceInput, System.Globalization.CultureInfo.InvariantCulture);
            }

            Console.WriteLine($"Текущее название: {prodToEdit.Name}. Новое название (Enter для пропуска): ");
            string nameInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(nameInput))
            {
                prodToEdit.Name = nameInput;
            }

            ProductService.UpdateProductAsync(prodToEdit).GetAwaiter().GetResult();
            _console.WriteSuccess("Товар обновлен!");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void DeleteProduct()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("УДАЛЕНИЕ ТОВАРА");

            var result = ProductService.GetProductsAsync(new ProductSearchParameters { PageSize = 100 }).GetAwaiter().GetResult();

            var prodToDelete = _console.ShowPagedListAndSelect(
                result.Items,
                "ВЫБЕРИТЕ ТОВАР ДЛЯ УДАЛЕНИЯ",
                (index, prod) => Console.WriteLine($"{index}. {prod.Name} | Цена: {prod.Price:C}"),
                pageSize: DefaultPageSize
            );

            if (prodToDelete == null) return;

            Console.Write("Вы уверены? Это действие необратимо. (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            ProductService.DeleteProductAsync(prodToDelete.Id).GetAwaiter().GetResult();
            _console.WriteSuccess("Товар удален.");
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
        _console.WaitForEnter();
    }

    private void HandleReviewsView()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("ПРОСМОТР ОТЗЫВОВ");

            var products = ProductService.GetProductsAsync(new ProductSearchParameters { PageSize = 100 }).GetAwaiter().GetResult().Items;

            var selectedProd = _console.ShowPagedListAndSelect(
                products,
                "ВЫБЕРИТЕ ТОВАР ДЛЯ ПРОСМОТРА ОТЗЫВОВ",
                (index, p) => Console.WriteLine($"{index}. {p.Name} (Рейтинг: {p.AverageRating:F1})"),
                pageSize: DefaultPageSize
            );

            if (selectedProd == null) return;

            int page = 1;
            int reviewPageSize = 5;
            bool exitList = false;

            while (!exitList)
            {
                Console.Clear();
                _console.WriteTitle($"ОТЗЫВЫ НА: {selectedProd.Name}");

                var reviewResult = ReviewService.GetReviewsByProductAsync(selectedProd.Id, page, reviewPageSize).GetAwaiter().GetResult();
                int totalPages = (int)Math.Ceiling(reviewResult.TotalCount / (double)reviewPageSize);

                if (!reviewResult.Items.Any())
                {
                    _console.WriteInfo("Отзывов пока нет.");
                }
                else
                {
                    foreach (var r in reviewResult.Items)
                    {
                        Console.WriteLine($"[{r.Rating}/5] Дата: {r.CreatedAt:dd.MM.yyyy}");
                        Console.WriteLine($"   {r.Comment}");
                        Console.WriteLine(new string('-', 40));
                    }
                }

                Console.WriteLine($"\nСтраница {page} из {Math.Max(1, totalPages)}");
                Console.WriteLine("[N]ext | [P]rev | [Q]uit to menu");
                Console.Write("Действие: ");

                string cmd = Console.ReadLine()?.Trim().ToUpper() ?? "";
                if (cmd == "N" && page < totalPages) page++;
                else if (cmd == "P" && page > 1) page--;
                else if (cmd == "Q") exitList = true;
            }
        }
        catch (Exception ex)
        {
            _console.WriteError($"Ошибка: {ex.Message}");
        }
    }
}