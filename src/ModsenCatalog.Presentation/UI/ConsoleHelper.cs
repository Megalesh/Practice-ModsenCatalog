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
}