using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class RoomCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: sched room <add|list|show|update|delete>");

        var action = args[1].ToLower();
        switch (action)
        {
            case "add":     Add(args); break;
            case "list":    List(args); break;
            case "show":    
                if (args.Length < 3) throw new ArgumentException("Usage: sched room show <id|code>");
                Show(args[2]); 
                break;
            case "delete":  
                if (args.Length < 3) throw new ArgumentException("Usage: sched room delete <id|code>");
                Delete(args[2]); 
                break;
            case "update":
                if (args.Length < 3) throw new ArgumentException("Usage: sched room update <id|code> [--code new] [--capacity N] [--building B]");
                Update(args[2], args);
                break;
            default: throw new ArgumentException($"Unknown room action: {action}");
        }
    }

    static void Add(string[] args)
    {
        var code = ArgsParser.GetValue(args, "--code") ?? throw new ArgumentException("Missing --code");
        var capacity = int.Parse(ArgsParser.GetValue(args, "--capacity") ?? "0");
        var building = ArgsParser.GetValue(args, "--building");

        var room = new Room(
            Id: DataContext.NextId<Room>(),
            Code: code,
            Capacity: capacity,
            Building: building
        );

        DataContext.Rooms.Add(room);
        DataContext.SaveAll();

        Console.WriteLine($"Room {room.Code} (id={room.Id}) created.");
    }

    static void List(string[] args)
    {
        var building = ArgsParser.GetValue(args, "--building");
        var minCapacity = ArgsParser.GetInt(args, "--min-capacity");
        var maxCapacity = ArgsParser.GetInt(args, "--max-capacity");
        var codeLike = ArgsParser.GetValue(args, "--code-like");
        var sortBy = ArgsParser.GetValue(args, "--sort") ?? "code";
        var limit = ArgsParser.GetInt(args, "--limit");
        var reverse = ArgsParser.HasFlag(args, "--reverse") || ArgsParser.HasFlag(args, "--desc");

        var query = DataContext.Rooms.AsQueryable();

        if (!string.IsNullOrEmpty(building))
            query = query.Where(r => r.Building != null && r.Building.Contains(building, StringComparison.OrdinalIgnoreCase));
        
        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);
        
        if (maxCapacity.HasValue)
            query = query.Where(r => r.Capacity <= maxCapacity.Value);
        
        if (!string.IsNullOrEmpty(codeLike))
            query = query.Where(r => r.Code.Contains(codeLike, StringComparison.OrdinalIgnoreCase));

        query = sortBy.ToLower() switch
        {
            "capacity" => query.OrderBy(r => r.Capacity),
            "building" => query.OrderBy(r => r.Building ?? ""),
            _ => query.OrderBy(r => r.Code)
        };

        if (reverse)
            query = query.Reverse();

        var rooms = query.ToList();

        if (!rooms.Any())
        {
            Console.WriteLine("No rooms found.");
            return;
        }

        if (limit.HasValue)
            rooms = rooms.Take(limit.Value).ToList();

        Console.WriteLine($"Found {rooms.Count} room(s):");
        Console.WriteLine("ID  | Code       | Capacity | Building");
        Console.WriteLine(new string('-', 50));
        
        foreach (var r in rooms)
            Console.WriteLine($"{r.Id,3} | {r.Code,-10} | {r.Capacity,8} | {r.Building ?? "-"}");
    }

    static void Show(string identifier)
    {
        Room? room = null;

        if (int.TryParse(identifier, out int id))
        {
            room = DataContext.Rooms.FirstOrDefault(r => r.Id == id);
        }

        if (room == null)
        {
            room = DataContext.Rooms.FirstOrDefault(r => 
                r.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        if (room == null)
        {
            throw new KeyNotFoundException($"Room not found: '{identifier}' (neither ID nor code)");
        }

        Console.WriteLine($"Room: {room.Code} (id={room.Id})");
        Console.WriteLine($"Capacity: {room.Capacity} seats");
        Console.WriteLine($"Building: {room.Building ?? "—"}");
    }

    static void Update(string identifier, string[] args)
    {
        Room? room = null;
        if (int.TryParse(identifier, out int id))
        {
            room = DataContext.Rooms.FirstOrDefault(r => r.Id == id);
        }
        if (room == null)
        {
            room = DataContext.Rooms.FirstOrDefault(r => r.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }
        if (room == null)
            throw new KeyNotFoundException($"Room not found: '{identifier}'");

        var newCode = ArgsParser.GetValue(args, "--code");
        var newCapacityStr = ArgsParser.GetValue(args, "--capacity");
        var newBuilding = ArgsParser.GetValue(args, "--building");

        var updatedRoom = new Room(
            Id: room.Id,
            Code: newCode ?? room.Code,
            Capacity: int.TryParse(newCapacityStr, out int capacity) ? capacity : room.Capacity,
            Building: newBuilding ?? room.Building,
            AttributesJson: room.AttributesJson
        );

        DataContext.Rooms.Remove(room);
        DataContext.Rooms.Add(updatedRoom);
        DataContext.SaveAll();
        Console.WriteLine($"Room {updatedRoom.Code} (id={updatedRoom.Id}) updated.");
    }

    static void Delete(string identifier)
    {
        Room? room = null;

        if (int.TryParse(identifier, out int id))
        {
            room = DataContext.Rooms.FirstOrDefault(r => r.Id == id);
        }

        if (room == null)
        {
            room = DataContext.Rooms.FirstOrDefault(r => 
                r.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        if (room == null)
        {
            throw new KeyNotFoundException($"Room not found: '{identifier}' (neither ID nor code)");
        }

        DataContext.Rooms.Remove(room);
        DataContext.SaveAll();
        Console.WriteLine($"Room {room.Code} deleted.");
    }
}