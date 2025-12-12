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
            case "show":    Show(int.Parse(args[2])); break;
            case "delete":  Delete(int.Parse(args[2])); break;
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

    static void Show(int id)
    {
        var room = DataContext.Rooms.FirstOrDefault(r => r.Id == id)
                   ?? throw new KeyNotFoundException($"Room {id} not found");

        Console.WriteLine($"Room: {room.Code} (id={room.Id})");
        Console.WriteLine($"Capacity: {room.Capacity}");
        Console.WriteLine($"Building: {room.Building ?? "—"}");
    }

    static void Delete(int id)
    {
        var room = DataContext.Rooms.FirstOrDefault(r => r.Id == id)
                   ?? throw new KeyNotFoundException($"Room {id} not found");

        DataContext.Rooms.Remove(room);
        DataContext.SaveAll();
        Console.WriteLine($"Room {room.Code} deleted.");
    }

}