using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class InitCommand
{
    public static void Run(string[] args)
    {
        var path = ArgsParser.GetValue(args, "--db") ?? "app-data";

        DataContext.Initialize(path);
        DataContext.SaveAll();

        Console.WriteLine($"Database initialized at: {Path.GetFullPath(path)}");
    }
}