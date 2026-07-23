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

    private const int DefaultPageSize = 5;

    public CustomerMenuState(MenuContext context)
    {
        _context = context;
        _console = _context.GetService<ConsoleHelper>();
    }

    public void DisplayMenu()
    {
        Console.Clear();
        _console.WriteTitle($"КАТАЛОГ ТОВАРОВ [{_context.CurrentUser?.Username}]");
        Console.WriteLine("1. Все товары");
        Console.WriteLine("2. Поиск товара по названию");
        Console.WriteLine("3. Фильтр по категории");
        Console.WriteLine("4. Мои отзывы");
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
                _console.WaitForEnter();
                break;
        }
        return null;
    }

    private void ListAllProducts()
    {
        var result = ProductService.GetProductsAsync(new ProductSearchParameters { PageSize = 100 }).GetAwaiter().GetResult();

        _console.ShowPagedListAndSelect(
            result.Items,
            "КАТАЛОГ ТОВАРОВ",
            (index, p) =>
            {
                Console.WriteLine($"{index}. {p.Name}");
                Console.WriteLine($"   Цена: {p.Price:C} | Рейтинг: {p.AverageRating:F1}/5.0");
            },
            pageSize: DefaultPageSize
        );
    }

    private void SearchProducts()
    {
        string term = _console.ReadNonEmptyLine("Введите название для поиска: ");

        var result = ProductService.GetProductsAsync(new ProductSearchParameters
        {
            SearchTerm = term,
            PageSize = 100
        }).GetAwaiter().GetResult();

        if (!result.Items.Any())
        {
            _console.WriteInfo("Ничего не найдено.");
            _console.WaitForEnter();
            return;
        }

        _console.ShowPagedListAndSelect(
            result.Items,
            $"РЕЗУЛЬТАТЫ ПОИСКА: '{term}'",
            (index, p) => Console.WriteLine($"{index}. {p.Name} | Цена: {p.Price:C} | Рейтинг: {p.AverageRating:F1}"),
            pageSize: DefaultPageSize
        );
    }

    private void FilterByCategory()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("ФИЛЬТР ПО КАТЕГОРИИ");

            var categories = CategoryService.GetAllCategoriesAsync().GetAwaiter().GetResult();

            var selectedCat = _console.ShowPagedListAndSelect(
                categories,
                "ВЫБЕРИТЕ КАТЕГОРИЮ",
                (index, c) => Console.WriteLine($"{index}. {c.Name}"),
                pageSize: DefaultPageSize
            );

            if (selectedCat == null) return;

            var result = ProductService.GetProductsAsync(new ProductSearchParameters
            {
                CategoryId = selectedCat.Id,
                PageSize = 100
            }).GetAwaiter().GetResult();

            if (!result.Items.Any())
            {
                _console.WriteInfo("В этой категории нет товаров.");
                _console.WaitForEnter();
                return;
            }

            _console.ShowPagedListAndSelect(
                result.Items,
                $"ТОВАРЫ В КАТЕГОРИИ: {selectedCat.Name}",
                (index, p) => Console.WriteLine($"{index}. {p.Name} | Цена: {p.Price:C} | Рейтинг: {p.AverageRating:F1}"),
                pageSize: DefaultPageSize
            );
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
                default: _console.WriteError("Неверный ввод."); _console.WaitForEnter(); break;
            }
        }
    }

    private void CreateReview()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("НОВЫЙ ОТЗЫВ");

            var products = ProductService.GetProductsAsync(new ProductSearchParameters { PageSize = 100 }).GetAwaiter().GetResult().Items;

            var selectedProd = _console.ShowPagedListAndSelect(
                products,
                "ВЫБЕРИТЕ ТОВАР ДЛЯ ОТЗЫВА",
                (index, p) => Console.WriteLine($"{index}. {p.Name} | Рейтинг: {p.AverageRating:F1}"),
                pageSize: DefaultPageSize
            );

            if (selectedProd == null) return;

            var existingReview = ReviewService.GetReviewsByUserIdAsync(_context.CurrentUser!.Id).GetAwaiter().GetResult()
                .FirstOrDefault(r => r.ProductId == selectedProd.Id);

            if (existingReview != null)
            {
                _console.WriteError("Вы уже оставили отзыв на этот товар! Вы можете изменить или удалить его в разделе 'Мои отзывы'.");
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

            ReviewService.CreateReviewAsync(_context.CurrentUser!.Id, selectedProd.Id, rating, comment).GetAwaiter().GetResult();

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
                _console.WaitForEnter();
                return;
            }

            var displayItems = new List<ReviewDisplayItem>();

            foreach (var review in myReviews)
            {
                var product = ProductService.GetProductByIdAsync(review.ProductId).GetAwaiter().GetResult();
                string productName = product?.Name ?? "Товар удален";

                displayItems.Add(new ReviewDisplayItem(review, productName));
            }

            var selectedItem = _console.ShowPagedListAndSelect(
                displayItems,
                "СПИСОК ВАШИХ ОТЗЫВОВ",
                (index, item) =>
                {
                    Console.WriteLine($"{index}. Товар: {item.ProductName}");
                    Console.WriteLine($"   Оценка: {item.Review.Rating}/5 | Дата: {item.Review.CreatedAt:dd.MM.yyyy}");
                    Console.WriteLine($"   Текст: {item.Review.Comment.Substring(0, Math.Min(40, item.Review.Comment.Length))}...");
                },
                pageSize: DefaultPageSize
            );

            if (selectedItem != null)
            {
                Console.Clear();
                _console.WriteTitle("ПОДРОБНОСТИ ОТЗЫВА");

                Console.WriteLine($"Товар: {selectedItem.ProductName}");
                Console.WriteLine($"Ваша оценка: {selectedItem.Review.Rating}/5");
                Console.WriteLine($"Дата: {selectedItem.Review.CreatedAt:dd.MM.yyyy HH:mm}");
                Console.WriteLine($"Комментарий:\n{selectedItem.Review.Comment}");

                _console.WaitForEnter();
            }
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
            _console.WaitForEnter();
        }
    }

    private void DeleteMyReview()
    {
        try
        {
            Console.Clear();
            _console.WriteTitle("УДАЛЕНИЕ МОЕГО ОТЗЫВА");

            var myReviews = ReviewService.GetReviewsByUserIdAsync(_context.CurrentUser!.Id).GetAwaiter().GetResult();

            if (!myReviews.Any())
            {
                _console.WriteInfo("У вас нет отзывов для удаления.");
                _console.WaitForEnter();
                return;
            }

            var displayItems = new List<ReviewDisplayItem>();

            foreach (var review in myReviews)
            {
                var product = ProductService.GetProductByIdAsync(review.ProductId).GetAwaiter().GetResult();
                string productName = product?.Name ?? "Товар удален";
                displayItems.Add(new ReviewDisplayItem(review, productName));
            }

            var selectedItem = _console.ShowPagedListAndSelect(
                displayItems,
                "ВЫБЕРИТЕ ОТЗЫВ ДЛЯ УДАЛЕНИЯ",
                (index, item) =>
                {
                    Console.WriteLine($"{index}. {item.ProductName} | Оценка: {item.Review.Rating}/5");
                },
                pageSize: DefaultPageSize
            );

            if (selectedItem == null) return;

            Console.Write("Вы уверены, что хотите удалить этот отзыв? (y/n): ");
            if (Console.ReadLine()?.Trim().ToLower() != "y") return;

            ReviewService.DeleteReviewAsync(selectedItem.Review.Id).GetAwaiter().GetResult();
            _console.WriteSuccess("Отзыв удален. Рейтинг товара пересчитан.");
        }
        catch (Exception ex)
        {
            _console.WriteError(ex.Message);
            _console.WaitForEnter();
        }
    }
}