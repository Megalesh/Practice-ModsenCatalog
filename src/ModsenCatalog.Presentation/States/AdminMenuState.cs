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

    public AdminMenuState(MenuContext context)
    {
        _context = context;
        _console = _context.GetService<ConsoleHelper>();
    }

    public void DisplayMenu()
    {
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
                case "1": ListUsersWithPagination(); break;
                case "2": CreateUser(); break;
                case "3": ChangeUserRole(); break;
                case "4": DeleteUser(); break;
                case "0": back = true; break;
                default: 
                    _console.WriteError("Неверный ввод.");
                    _console.WaitForEnter();

                    break;
            }
        }
    }

    private void ListUsersWithPagination()
    {
        int page = 1;
        int pageSize = 10;
        bool exitList = false;

        while (!exitList)
        {
            Console.Clear();
            _console.WriteTitle("СПИСОК ПОЛЬЗОВАТЕЛЕЙ");

            var users = UserService.GetAllUsersAsync().GetAwaiter().GetResult();
            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            int totalPages = (int)Math.Ceiling(users.Count() / (double)pageSize);

            if (!pagedUsers.Any())
            {
                _console.WriteInfo("Список пуст.");
            }
            else
            {
                foreach (var u in pagedUsers)
                {
                    Console.WriteLine($"[{u.Id}] {u.Username} ({u.Email}) | Роль: {u.Role}");
                }
            }

            Console.WriteLine($"\nСтраница {page} из {totalPages}");
            Console.WriteLine("[N]ext, [P]rev, [Q]uit to menu");
            Console.Write("Действие: ");

            string cmd = Console.ReadLine()?.Trim().ToUpper() ?? "";
            if (cmd == "N" && page < totalPages) page++;
            else if (cmd == "P" && page > 1) page--;
            else if (cmd == "Q") exitList = true;
        }
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
            _console.WriteTitle("ИЗМЕНЕНИЕ РОЛИ");

            var users = UserService.GetAllUsersAsync().GetAwaiter().GetResult();
            if (!users.Any())
            {
                _console.WriteError("Нет пользователей для редактирования.");
                _console.WaitForEnter();
                return;
            }

            Console.WriteLine("Список пользователей:");
            foreach (var u in users)
            {
                Console.WriteLine($"ID: {u.Id} | User: {u.Username} | Current Role: {u.Role}");
            }

            Guid userId = Guid.Parse(_console.ReadNonEmptyLine("\nВведите ID пользователя для изменения роли: "));

            if (!users.Any(u => u.Id == userId))
            {
                _console.WriteError("Пользователь с таким ID не найден.");
                _console.WaitForEnter();
                return;
            }

            Console.WriteLine("\nНовая роль:");
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

            UserService.ChangeUserRoleAsync(userId, newRole).GetAwaiter().GetResult();
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
            Console.WriteLine("Список пользователей:");
            foreach (var u in users)
            {
                Console.WriteLine($"ID: {u.Id} | User: {u.Username}");
            }

            Guid userId = Guid.Parse(_console.ReadNonEmptyLine("\nВведите ID пользователя для удаления: "));

            Console.Write("Вы уверены? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            UserService.DeleteUserAsync(userId).GetAwaiter().GetResult();
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
                default: _console.WriteError("Неверный ввод."); Thread.Sleep(1000); break;
            }
        }
    }

    private void ListCategories()
    {
        Console.Clear();
        _console.WriteTitle("СПИСОК КАТЕГОРИЙ");
        var cats = CategoryService.GetAllCategoriesAsync().GetAwaiter().GetResult();

        if (!cats.Any())
        {
            _console.WriteInfo("Категорий нет.");
        }
        else
        {
            foreach (var c in cats)
            {
                Console.WriteLine($"ID: {c.Id} | Name: {c.Name}");
                Console.WriteLine($"   Desc: {c.Description}");
                Console.WriteLine(new string('-', 50));
            }
        }
        _console.WaitForEnter();
    }

    private void CreateCategory()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("СОЗДАНИЕ КАТЕГОРИИ");
            string name = _console.ReadNonEmptyLine("Название категории: ");
            string desc = Console.ReadLine() ?? ""; // Описание может быть пустым

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
            Console.WriteLine("Доступные категории:");
            foreach (var c in cats) Console.WriteLine($"ID: {c.Id} | Name: {c.Name}");

            Guid id = Guid.Parse(_console.ReadNonEmptyLine("\nВведите ID категории: "));
            var cat = CategoryService.GetCategoryByIdAsync(id).GetAwaiter().GetResult();

            if (cat == null)
            {
                _console.WriteError("Категория не найдена.");
                _console.WaitForEnter();
                return;
            }

            Console.WriteLine($"Текущее имя: '{cat.Name}'. Оставьте пустым, чтобы не менять.");
            string newName = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newName)) cat.Name = newName;

            Console.WriteLine($"Текущее описание: '{cat.Description}'. Оставьте пустым, чтобы не менять.");
            string newDesc = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(newDesc)) cat.Description = newDesc;

            CategoryService.UpdateCategoryAsync(cat).GetAwaiter().GetResult();
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
            var deletableCats = cats.Where(c => !c.Name.Equals("Архив", StringComparison.OrdinalIgnoreCase)).ToList();

            if (!deletableCats.Any())
            {
                _console.WriteError("Нет категорий для удаления.");
                _console.WaitForEnter();
                return;
            }

            Console.WriteLine("Доступные категории для удаления:");
            foreach (var c in deletableCats) Console.WriteLine($"ID: {c.Id} | Name: {c.Name}");

            Guid id = Guid.Parse(_console.ReadNonEmptyLine("\nВведите ID категории для удаления: "));

            Console.Write("Внимание! Товары будут перенесены в 'Архив'. Продолжить? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            CategoryService.DeleteCategoryAsync(id).GetAwaiter().GetResult();
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
            Console.WriteLine("1. Список товаров (Пагинация)");
            Console.WriteLine("2. Создать товар");
            Console.WriteLine("3. Редактировать товар");
            Console.WriteLine("4. Удалить товар");
            Console.WriteLine("0. Назад");
            Console.Write("Выбор: ");

            string choice = Console.ReadLine()?.Trim() ?? "";
            switch (choice)
            {
                case "1": ListProductsWithPagination(); break;
                case "2": CreateProduct(); break;
                case "3": EditProduct(); break;
                case "4": DeleteProduct(); break;
                case "0": back = true; break;
                default: _console.WriteError("Неверный ввод."); Thread.Sleep(1000); break;
            }
        }
    }

    private void ListProductsWithPagination()
    {
        int page = 1;
        int pageSize = 5;
        bool exitList = false;

        while (!exitList)
        {
            Console.Clear();
            _console.WriteTitle("СПИСОК ТОВАРОВ");

            var result = ProductService.GetProductsAsync(new ProductSearchParameters
            {
                PageNumber = page,
                PageSize = pageSize
            }).GetAwaiter().GetResult();

            int totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

            if (!result.Items.Any())
            {
                _console.WriteInfo("Товаров нет.");
            }
            else
            {
                foreach (var p in result.Items)
                {
                    Console.WriteLine($"ID: {p.Id}");
                    Console.WriteLine($"Name: {p.Name}");
                    Console.WriteLine($"Price: {p.Price:C} | Rating: {p.AverageRating:F1}");
                    Console.WriteLine(new string('-', 30));
                }
            }

            Console.WriteLine($"\nСтраница {page} из {Math.Max(1, totalPages)} (Всего: {result.TotalCount})");
            Console.WriteLine("[N]ext, [P]rev, [Q]uit to menu");
            Console.Write("Действие: ");

            string cmd = Console.ReadLine()?.Trim().ToUpper() ?? "";
            if (cmd == "N" && page < totalPages) page++;
            else if (cmd == "P" && page > 1) page--;
            else if (cmd == "Q") exitList = true;
        }
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
            Console.WriteLine("\nДоступные категории:");
            foreach (var c in cats) Console.WriteLine($"ID: {c.Id} | Name: {c.Name}");

            Guid catId = Guid.Parse(_console.ReadNonEmptyLine("\nВведите ID категории: "));

            ProductService.CreateProductAsync(name, desc, price, catId).GetAwaiter().GetResult();
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

            Guid id = Guid.Parse(_console.ReadNonEmptyLine("Введите ID товара для редактирования: "));
            var prod = ProductService.GetProductByIdAsync(id).GetAwaiter().GetResult();

            if (prod == null)
            {
                _console.WriteError("Товар не найден.");
                _console.WaitForEnter();
                return;
            }

            Console.WriteLine($"Текущая цена: {prod.Price:C}. Новая цена (Enter для пропуска): ");
            string priceInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(priceInput))
            {
                prod.Price = decimal.Parse(priceInput, System.Globalization.CultureInfo.InvariantCulture);
            }

            Console.WriteLine($"Текущее название: {prod.Name}. Новое название (Enter для пропуска): ");
            string nameInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(nameInput))
            {
                prod.Name = nameInput;
            }

            ProductService.UpdateProductAsync(prod).GetAwaiter().GetResult();
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

            Guid id = Guid.Parse(_console.ReadNonEmptyLine("Введите ID товара для удаления: "));

            Console.Write("Вы уверены? Это действие необратимо. (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            ProductService.DeleteProductAsync(id).GetAwaiter().GetResult();
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

            Guid prodId = Guid.Parse(_console.ReadNonEmptyLine("Введите ID товара для просмотра отзывов: "));

            var prod = ProductService.GetProductByIdAsync(prodId).GetAwaiter().GetResult();
            if (prod == null)
            {
                _console.WriteError("Товар не найден.");
                _console.WaitForEnter();
                return;
            }

            int page = 1;
            int pageSize = 5;
            bool exitList = false;

            while (!exitList)
            {
                Console.Clear();
                _console.WriteTitle($"ОТЗЫВЫ НА ТОВАР: {prod.Name}");

                var result = ReviewService.GetReviewsByProductAsync(prodId, page, pageSize).GetAwaiter().GetResult();
                int totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

                if (!result.Items.Any())
                {
                    _console.WriteInfo("Отзывов пока нет.");
                }
                else
                {
                    foreach (var r in result.Items)
                    {
                        Console.WriteLine($"[{r.Rating}/5] Пользователь ID: {r.UserId}");
                        Console.WriteLine($"Дата: {r.CreatedAt:dd.MM.yyyy}");
                        Console.WriteLine($"Комментарий: {r.Comment}");
                        Console.WriteLine(new string('-', 40));
                    }
                }

                Console.WriteLine($"\nСтраница {page} из {Math.Max(1, totalPages)}");
                Console.WriteLine("[N]ext, [P]rev, [Q]uit to menu");
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