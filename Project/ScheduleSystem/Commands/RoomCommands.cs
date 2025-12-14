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
            case "list":    List(); break;
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
                Update(args[2]);
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

    static void List()
    {
        if (!DataContext.Rooms.Any())
        {
            Console.WriteLine("No rooms.");
            return;
        }

        foreach (var r in DataContext.Rooms.OrderBy(r => r.Code))
            Console.WriteLine($"{r.Id,3} | {r.Code,-10} | {r.Capacity,3} seats | {r.Building ?? "-"}");
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

    static void Update(string identifier)
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

        var newCode = ArgsParser.GetValue(Environment.GetCommandLineArgs(), "--code");
        var newCapacityStr = ArgsParser.GetValue(Environment.GetCommandLineArgs(), "--capacity");
        var newBuilding = ArgsParser.GetValue(Environment.GetCommandLineArgs(), "--building");

        if (newCode != null) room = room with { Code = newCode };
        if (int.TryParse(newCapacityStr, out int newCapacity)) room = room with { Capacity = newCapacity };
        if (newBuilding != null) room = room with { Building = newBuilding };

        DataContext.SaveAll();
        Console.WriteLine($"Room {room.Code} (id={room.Id}) updated.");
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