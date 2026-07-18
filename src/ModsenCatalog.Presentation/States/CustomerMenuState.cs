using ModsenCatalog.BusinessLogic.DTOs;
using ModsenCatalog.BusinessLogic.Entities;
using ModsenCatalog.BusinessLogic.Interfaces;
using ModsenCatalog.Presentation.UI;

namespace ModsenCatalog.Presentation.States;

public class CustomerMenuState : IMenuState
{
    private readonly MenuContext _context;
    private readonly ConsoleHelper _console;

    private IProductService ProductService => _context.GetService<IProductService>();
    private ICategoryService CategoryService => _context.GetService<ICategoryService>();
    private IReviewService ReviewService => _context.GetService<IReviewService>();

    public CustomerMenuState(MenuContext context)
    {
        _context = context;
        _console = _context.GetService<ConsoleHelper>();
    }

    public void DisplayMenu()
    {
        _console.WriteTitle($"КАТАЛОГ ТОВАРОВ [{_context.CurrentUser?.Username}]");
        Console.WriteLine("1. Все товары (Пагинация)");
        Console.WriteLine("2. Поиск товара по названию");
        Console.WriteLine("3. Фильтр по категории");
        Console.WriteLine("4. Мои отзывы (Оставить / Удалить)");
        Console.WriteLine("0. Выход из системы");
        Console.Write("Выберите действие: ");
    }

    public IMenuState? HandleInput(string input)
    {
        switch (input)
        {
            case "1": ListAllProducts(); break;
            case "2": SearchProducts(); break;
            case "3": FilterByCategory(); break;
            case "4": HandleMyReviews(); break;
            case "0":
                _context.SetCurrentUser(null);
                return new MainMenuState(_context);
            default:
                _console.WriteError("Неверный выбор.");
                break;
        }
        return null;
    }

    private void ListAllProducts()
    {
        int page = 1;
        int pageSize = 5;
        bool exitList = false;

        while (!exitList)
        {
            Console.Clear();
            _console.WriteTitle("КАТАЛОГ ТОВАРОВ");

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
                    Console.WriteLine($"Название: {p.Name}");
                    Console.WriteLine($"Цена: {p.Price:C} | Рейтинг: {p.AverageRating:F1}/5.0");
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

    private void SearchProducts()
    {
        string term = _console.ReadNonEmptyLine("Введите название для поиска: ");

        var result = ProductService.GetProductsAsync(new ProductSearchParameters
        {
            SearchTerm = term,
            PageSize = 10
        }).GetAwaiter().GetResult();

        Console.Clear();
        _console.WriteTitle($"РЕЗУЛЬТАТЫ ПОИСКА: '{term}'");

        if (!result.Items.Any())
        {
            _console.WriteInfo("Ничего не найдено.");
        }
        else
        {
            foreach (var p in result.Items)
            {
                Console.WriteLine($"ID: {p.Id} | Name: {p.Name} | Price: {p.Price:C} | Rating: {p.AverageRating:F1}");
            }
        }
        _console.WaitForEnter();
    }

    private void FilterByCategory()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("ФИЛЬТР ПО КАТЕГОРИИ");

            var categories = CategoryService.GetAllCategoriesAsync().GetAwaiter().GetResult();
            Console.WriteLine("Доступные категории:");
            foreach (var c in categories)
            {
                Console.WriteLine($"- ID: {c.Id} | Name: {c.Name}");
            }

            Guid catId = Guid.Parse(_console.ReadNonEmptyLine("\nВведите ID категории: "));

            var result = ProductService.GetProductsAsync(new ProductSearchParameters
            {
                CategoryId = catId,
                PageSize = 10
            }).GetAwaiter().GetResult();

            Console.Clear();
            _console.WriteTitle($"ТОВАРЫ В КАТЕГОРИИ");

            if (!result.Items.Any())
            {
                _console.WriteInfo("В этой категории нет товаров.");
            }
            else
            {
                foreach (var p in result.Items)
                {
                    Console.WriteLine($"ID: {p.Id} | Name: {p.Name} | Price: {p.Price:C} | Rating: {p.AverageRating:F1}");
                }
            }
            _console.WaitForEnter();
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
            _console.WaitForEnter();
        }
    }

    private void HandleMyReviews()
    {
        bool back = false;
        while (!back)
        {
            Console.Clear();
            _console.WriteTitle("МОИ ОТЗЫВЫ");
            Console.WriteLine("1. Оставить новый отзыв");
            Console.WriteLine("2. Посмотреть мои отзывы");
            Console.WriteLine("3. Удалить мой отзыв");
            Console.WriteLine("0. Назад");
            Console.Write("Выбор: ");

            string choice = Console.ReadLine()?.Trim() ?? "";
            switch (choice)
            {
                case "1": CreateReview(); break;
                case "2": ViewMyReviews(); break;
                case "3": DeleteMyReview(); break;
                case "0": back = true; break;
                default: _console.WriteError("Неверный ввод."); Thread.Sleep(1000); break;
            }
        }
    }

    private void CreateReview()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("НОВЫЙ ОТЗЫВ");

            var products = ProductService.GetProductsAsync(new ProductSearchParameters { PageSize = 5 }).GetAwaiter().GetResult();
            Console.WriteLine("Последние добавленные товары (для примера ID):");
            foreach (var p in products.Items)
                Console.WriteLine($"ID: {p.Id} | Name: {p.Name}");

            Guid prodId = Guid.Parse(_console.ReadNonEmptyLine("\nВведите ID товара для отзыва: "));

            var product = ProductService.GetProductByIdAsync(prodId).GetAwaiter().GetResult();
            if (product == null)
            {
                _console.WriteError("Товар не найден.");
                _console.WaitForEnter();
                return;
            }

            int rating = _console.ReadInt("Оценка (1-5): ");
            if (rating < 1 || rating > 5)
            {
                _console.WriteError("Оценка должна быть от 1 до 5.");
                _console.WaitForEnter();
                return;
            }

            string comment = _console.ReadNonEmptyLine("Комментарий: ");

            ReviewService.CreateReviewAsync(_context.CurrentUser!.Id, prodId, rating, comment).GetAwaiter().GetResult();

            _console.WriteSuccess("Отзыв успешно добавлен!");
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
        }
        _console.WaitForEnter();
    }

    private void ViewMyReviews()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("МОИ ОТЗЫВЫ");

            var myReviews = ReviewService.GetReviewsByUserIdAsync(_context.CurrentUser!.Id).GetAwaiter().GetResult();

            if (!myReviews.Any())
            {
                _console.WriteInfo("Вы еще не оставили ни одного отзыва.");
            }
            else
            {
                foreach (var r in myReviews)
                {
                    var prod = ProductService.GetProductByIdAsync(r.ProductId).GetAwaiter().GetResult();
                    string prodName = prod?.Name ?? "Unknown";

                    Console.WriteLine($"Товар: {prodName} (ID: {r.ProductId})");
                    Console.WriteLine($"Ваша оценка: {r.Rating}/5");
                    Console.WriteLine($"Комментарий: {r.Comment}");
                    Console.WriteLine($"ID отзыва (для удаления): {r.Id}");
                    Console.WriteLine(new string('-', 40));
                }
            }
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
        }
        _console.WaitForEnter();
    }

    private void DeleteMyReview()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("УДАЛЕНИЕ ОТЗЫВА");

            Guid reviewId = Guid.Parse(_console.ReadNonEmptyLine("Введите ID вашего отзыва для удаления: "));

            var review = ReviewService.GetReviewByIdAsync(reviewId).GetAwaiter().GetResult();

            if (review == null)
            {
                _console.WriteError("Отзыв не найден.");
                _console.WaitForEnter();
                return;
            }

            if (review.UserId != _context.CurrentUser!.Id)
            {
                _console.WriteError("Ошибка безопасности: Вы не можете удалить чужой отзыв!");
                _console.WaitForEnter();
                return;
            }

            Console.Write("Вы уверены, что хотите удалить этот отзыв? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            ReviewService.DeleteReviewAsync(reviewId).GetAwaiter().GetResult();
            _console.WriteSuccess("Отзыв удален. Рейтинг товара пересчитан.");
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
        }
        _console.WaitForEnter();
    }
}