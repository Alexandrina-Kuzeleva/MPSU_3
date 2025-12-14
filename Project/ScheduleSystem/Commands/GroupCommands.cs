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
            case "list":    List(args); break;
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

    static void List(string[] args)
    {
        var year = ArgsParser.GetInt(args, "--year");
        var minSize = ArgsParser.GetInt(args, "--min-size");
        var maxSize = ArgsParser.GetInt(args, "--max-size");
        var codeLike = ArgsParser.GetValue(args, "--code-like");
        var sortBy = ArgsParser.GetValue(args, "--sort") ?? "code";
        var limit = ArgsParser.GetInt(args, "--limit");
        var reverse = ArgsParser.HasFlag(args, "--reverse") || ArgsParser.HasFlag(args, "--desc");

        var query = DataContext.Groups.AsQueryable();

        if (year.HasValue)
            query = query.Where(g => g.Year == year);
        
        if (minSize.HasValue)
            query = query.Where(g => g.Size >= minSize.Value);
        
        if (maxSize.HasValue)
            query = query.Where(g => g.Size <= maxSize.Value);
        
        if (!string.IsNullOrEmpty(codeLike))
            query = query.Where(g => g.Code.Contains(codeLike, StringComparison.OrdinalIgnoreCase));

        query = sortBy.ToLower() switch
        {
            "size" => query.OrderBy(g => g.Size),
            "year" => query.OrderBy(g => g.Year ?? int.MaxValue),
            _ => query.OrderBy(g => g.Code)
        };

        if (reverse)
            query = query.Reverse();

        var groups = query.ToList();

        if (!groups.Any())
        {
            Console.WriteLine("No groups found.");
            return;
        }

        if (limit.HasValue)
            groups = groups.Take(limit.Value).ToList();

        Console.WriteLine($"Found {groups.Count} group(s):");
        Console.WriteLine("ID  | Code         | Size | Year");
        Console.WriteLine(new string('-', 40));
        
        foreach (var g in groups)
            Console.WriteLine($"{g.Id,3} | {g.Code,-12} | {g.Size,4} | {g.Year?.ToString() ?? "-"}");
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