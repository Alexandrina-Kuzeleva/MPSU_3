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
            case "show":    
                if (args.Length < 3) throw new ArgumentException("Usage: sched group show <id|code>");
                Show(args[2]); 
                break;
            case "delete":  
                if (args.Length < 3) throw new ArgumentException("Usage: sched group delete <id|code>");
                Delete(args[2]); 
                break;
            case "update":
                if (args.Length < 3) throw new ArgumentException("Usage: sched group update <id> [--code new] [--size new] [--year new]");
                Update(args[2],args);
                break;
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

    static void Show(string identifier)
    {
        Group? group = null;

        if (int.TryParse(identifier, out int id))
        {
            group = DataContext.Groups.FirstOrDefault(g => g.Id == id);
        }

        if (group == null)
        {
            group = DataContext.Groups.FirstOrDefault(g => 
                g.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        if (group == null)
        {
            throw new KeyNotFoundException($"Group not found: '{identifier}' (neither ID nor code)");
        }

        Console.WriteLine($"ID: {group.Id}");
        Console.WriteLine($"Code: {group.Code}");
        Console.WriteLine($"Size: {group.Size}");
        Console.WriteLine($"Year: {group.Year?.ToString() ?? "—"}");
    }

    static void Update(string identifier, string[] args)
    {
        Group? group = null;
        if (int.TryParse(identifier, out int id))
        {
            group = DataContext.Groups.FirstOrDefault(g => g.Id == id);
        }
        if (group == null)
        {
            group = DataContext.Groups.FirstOrDefault(g => g.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }
        if (group == null)
            throw new KeyNotFoundException($"Group not found: '{identifier}'");

        var newCode = ArgsParser.GetValue(args, "--code");
        var newSizeStr = ArgsParser.GetValue(args, "--size");
        var newYearStr = ArgsParser.GetValue(args, "--year");

        int? year = group.Year;
        if (newYearStr != null)
        {
            if (int.TryParse(newYearStr, out int newYear))
                year = newYear;
            else if (newYearStr == "")
                year = null;
        }

        var updatedGroup = new Group(
            Id: group.Id,
            Code: newCode ?? group.Code,
            Size: int.TryParse(newSizeStr, out int size) ? size : group.Size,
            Year: year
        );

        DataContext.Groups.Remove(group);
        DataContext.Groups.Add(updatedGroup);
        DataContext.SaveAll();
        Console.WriteLine($"Group {updatedGroup.Code} (id={updatedGroup.Id}) updated.");
    }

    static void Delete(string identifier)
    {
        Group? group = null;

        if (int.TryParse(identifier, out int id))
        {
            group = DataContext.Groups.FirstOrDefault(g => g.Id == id);
        }

        if (group == null)
        {
            group = DataContext.Groups.FirstOrDefault(g => 
                g.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        if (group == null)
        {
            throw new KeyNotFoundException($"Group not found: '{identifier}' (neither ID nor code)");
        }

        DataContext.Groups.Remove(group);
        DataContext.SaveAll();
        Console.WriteLine($"Group {group.Code} deleted.");
    }
}