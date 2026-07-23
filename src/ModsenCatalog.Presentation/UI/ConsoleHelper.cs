using ModsenCatalog.BusinessLogic.Entities;

namespace ModsenCatalog.Presentation.UI;

public class ConsoleHelper
{
    public void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void WriteTitle(string title)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine($"=== {title} ===");
        Console.ResetColor();
    }

    public string ReadNonEmptyLine(string prompt)
    {
        string input;
        do
        {
            Console.Write(prompt);
            input = Console.ReadLine()?.Trim() ?? "";
            if (string.IsNullOrEmpty(input))
                WriteError("Ввод не может быть пустым.");
        } while (string.IsNullOrEmpty(input));

        return input;
    }

    public int ReadInt(string prompt)
    {
        int result;
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out result))
                return result;

            WriteError("Некорректный ввод. Ожидается целое число.");
        }
    }

    public decimal ReadDecimal(string prompt)
    {
        decimal result;
        while (true)
        {
            Console.Write(prompt);
            if (decimal.TryParse(Console.ReadLine(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result))
                return result;

            WriteError("Некорректный ввод. Ожидается число.");
        }
    }

    public string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var password = new System.Text.StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password.Remove(password.Length - 1, 1);
                Console.Write("\b \b");
            }
            else if (key.Key != ConsoleKey.Backspace)
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        Console.WriteLine();

        return password.ToString();
    }

    public void WaitForEnter(string message = "\nНажмите Enter, чтобы продолжить...")
    {
        Console.WriteLine(message);
        Console.ReadLine();
        Console.Clear();
    }

    public T? SelectFromList<T>(IEnumerable<T> items, string prompt = "Выберите номер: ") where T : class
    {
        var list = items.ToList();
        if (!list.Any())
        {
            WriteInfo("Список пуст.");
            return null;
        }

        for (int i = 0; i < list.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {list[i]}");
        }

        int choice = ReadInt(prompt);

        if (choice < 1 || choice > list.Count)
        {
            WriteError("Неверный выбор.");
            return null;
        }

        return list[choice - 1];
    }

    public T? ShowPagedListAndSelect<T>(
        IEnumerable<T> items,
        string title,
        Action<int, T> itemDisplayAction,
        int pageSize = 5) where T : class
    {
        var list = items.ToList();
        if (!list.Any())
        {
            WriteInfo("Список пуст.");
            WaitForEnter();
            return null;
        }

        int currentPage = 1;
        int totalPages = (int)Math.Ceiling(list.Count / (double)pageSize);
        bool isSelectionMode = true;
        T? selected = null;

        while (isSelectionMode)
        {
            Console.Clear();
            WriteTitle(title);

            int startIndex = (currentPage - 1) * pageSize;
            int endIndex = Math.Min(startIndex + pageSize, list.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                int displayNumber = i + 1;
                itemDisplayAction(displayNumber, list[i]);
            }

            Console.WriteLine($"\nСтраница {currentPage} из {totalPages} (Всего: {list.Count})");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("[N]ext страница | [P]rev страница | [Q]uit (Назад в меню)");

            string input = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (input == "N")
            {
                if (currentPage < totalPages) currentPage++;
            }
            else if (input == "P")
            {
                if (currentPage > 1) currentPage--;
            }
            else if (input == "Q")
            {
                return null;
            }
            else
            {
                if (int.TryParse(input, out int choice))
                {
                    if (choice >= 1 && choice <= list.Count)
                    {
                        selected = list[choice - 1];
                        isSelectionMode = false;
                    }
                    else
                    {
                        WriteError($"Введите число от 1 до {list.Count}, или N/P/Q.");
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    WriteError("Неверный ввод. Используйте N, P, Q или номер элемента.");
                    Thread.Sleep(1000);
                }
            }
        }

        return selected;
    }
}