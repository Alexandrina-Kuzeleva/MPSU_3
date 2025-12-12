using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class CourseCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: sched course <add|list|show|delete>");

        var action = args[1].ToLower();
        switch (action)
        {
            case "add":     Add(args); break;
            case "list":    List(); break;
            case "show":    Show(int.Parse(args[2])); break;
            case "delete":  Delete(int.Parse(args[2])); break;
            default: throw new ArgumentException($"Unknown course action: {action}");
        }
    }

    static void Add(string[] args)
    {
        var title = ArgsParser.GetValue(args, "--title") ?? throw new ArgumentException("Missing --title");
        var code = ArgsParser.GetValue(args, "--code");
        var duration = int.Parse(ArgsParser.GetValue(args, "--duration") ?? "90");

        var course = new Course(
            Id: DataContext.NextId<Course>(),
            Title: title,
            Code: code,
            DurationMinutes: duration
        );

        DataContext.Courses.Add(course);
        DataContext.SaveAll();

        Console.WriteLine($"Course \"{course.Title}\" (id={course.Id}) created.");
    }

    static void List()
    {
        if (!DataContext.Courses.Any())
        {
            Console.WriteLine("No courses.");
            return;
        }

        foreach (var c in DataContext.Courses.OrderBy(c => c.Title))
            Console.WriteLine($"{c.Id,3} | {c.Title,-40} | {c.Code ?? "-",-10} | {c.DurationMinutes} min");
    }

    static void Show(int id)
    {
        var c = DataContext.Courses.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Course {id} not found");

        Console.WriteLine($"ID: {c.Id}");
        Console.WriteLine($"Title: {c.Title}");
        Console.WriteLine($"Code: {c.Code ?? "—"}");
        Console.WriteLine($"Duration: {c.DurationMinutes} minutes");
    }

    static void Delete(int id)
    {
        var c = DataContext.Courses.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Course {id} not found");

        DataContext.Courses.Remove(c);
        DataContext.SaveAll();
        Console.WriteLine($"Course \"{c.Title}\" deleted.");
    }
}