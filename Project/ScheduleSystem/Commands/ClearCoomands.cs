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
            Console.WriteLine(
                $"Database is already empty (folder does not exist: {path})"
            );
            return;
        }

        try
        {
            Directory.Delete(path, recursive: true);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("DATABASE CLEARED");
            Console.WriteLine($"Folder deleted: {Path.GetFullPath(path)}");
            Console.WriteLine(
                "All data has been permanently erased with no recovery option!"
            );
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error while deleting: {ex.Message}");
            Console.ResetColor();
        }
    }
}