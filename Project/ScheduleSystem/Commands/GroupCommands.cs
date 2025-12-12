using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class GroupCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: sched group <add|list|show|delete>");

        var action = args[1].ToLower();
        switch (action)
        {
            case "add":     Add(args); break;
            case "list":    List(); break;
            case "show":    Show(int.Parse(args[2])); break;
            case "delete":  Delete(int.Parse(args[2])); break;
            default: throw new ArgumentException($"Unknown group action: {action}");
        }
    }

    static void Add(string[] args)
    {
        var code = ArgsParser.GetValue(args, "--code") ?? throw new ArgumentException("Missing --code");
        var size = int.Parse(ArgsParser.GetValue(args, "--size") ?? "0");
        var year = ArgsParser.GetInt(args, "--year");

        var group = new Group(
            Id: DataContext.NextId<Group>(),
            Code: code,
            Size: size,
            Year: year
        );

        DataContext.Groups.Add(group);
        DataContext.SaveAll();

        Console.WriteLine($"Group {group.Code} (id={group.Id}) created.");
    }

    static void List()
    {
        if (!DataContext.Groups.Any())
        {
            Console.WriteLine("No groups.");
            return;
        }

        foreach (var g in DataContext.Groups.OrderBy(g => g.Code))
            Console.WriteLine($"{g.Id,3} | {g.Code,-12} | {g.Size,3} students | Year {g.Year?.ToString() ?? "-"}");
    }

    static void Show(int id)
    {
        var g = DataContext.Groups.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Group {id} not found");

        Console.WriteLine($"ID: {g.Id}");
        Console.WriteLine($"Code: {g.Code}");
        Console.WriteLine($"Size: {g.Size}");
        Console.WriteLine($"Year: {g.Year?.ToString() ?? "—"}");
    }

    static void Delete(int id)
    {
        var g = DataContext.Groups.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Group {id} not found");

        DataContext.Groups.Remove(g);
        DataContext.SaveAll();
        Console.WriteLine($"Group {g.Code} deleted.");
    }
}