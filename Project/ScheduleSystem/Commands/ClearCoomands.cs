using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;


namespace ScheduleSystem.Commands;

public static class ClearCommands
{
    public static void Run()
    {
        var path = DataContext.BasePath;

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"База данных уже пуста (папка не существует: {path})");
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("       DATABASE CLEARED");
            Console.WriteLine($"       Папка удалена: {Path.GetFullPath(path)}");
            Console.WriteLine("       Все данные стёрты без возможности восстановления!");
            Console.WriteLine();
            Console.ResetColor();
            Console.WriteLine("Готово! Теперь можно запустить:");
            Console.WriteLine("   dotnet run -- init");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Ошибка при удалении: {ex.Message}");
            Console.ResetColor();
        }
    }
}