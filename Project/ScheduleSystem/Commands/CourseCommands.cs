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
            case "show":    if (args.Length < 3) throw new ArgumentException("Usage: sched course show <id|code>"); Show(args[2]); break;
            case "delete":  
                if (args.Length < 3) throw new ArgumentException("Usage: sched course delete <id|code>");
                Delete(args[2]); 
                break;
            case "update":
                if (args.Length < 3) throw new ArgumentException("Usage: sched course update <id> [--title new] [--code new] [--duration N]");
                Update(args[2], args);
                break;
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

    static void Show(string identifier)
    {
        Course? course = null;

        if (int.TryParse(identifier, out int id))
        {
            course = DataContext.Courses.FirstOrDefault(c => c.Id == id);
        }

        if (course == null)
        {
            course = DataContext.Courses.FirstOrDefault(c => 
                c.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        if (course == null)
        {
            throw new KeyNotFoundException($"Course not found: '{identifier}' (neither ID nor code)");
        }

        Console.WriteLine($"ID: {course.Id}");
        Console.WriteLine($"Title: {course.Title}");
        Console.WriteLine($"Code: {course.Code ?? "—"}");
        Console.WriteLine($"Duration: {course.DurationMinutes} minutes");
    }

    static void Update(string identifier, string[] args)
    {
        Course? course = null;
        if (int.TryParse(identifier, out int id))
        {
            course = DataContext.Courses.FirstOrDefault(c => c.Id == id);
        }
        if (course == null && !string.IsNullOrEmpty(identifier))
        {
            course = DataContext.Courses.FirstOrDefault(c => 
                c.Code != null && c.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }
        if (course == null)
            throw new KeyNotFoundException($"Course not found: '{identifier}'");

        var newTitle = ArgsParser.GetValue(args, "--title");
        var newCode = ArgsParser.GetValue(args, "--code");
        var newDurationStr = ArgsParser.GetValue(args, "--duration");

        var updatedCourse = new Course(
            Id: course.Id,
            Title: newTitle ?? course.Title,
            Code: newCode ?? course.Code,
            DurationMinutes: int.TryParse(newDurationStr, out int duration) ? duration : course.DurationMinutes
        );

        DataContext.Courses.Remove(course);
        DataContext.Courses.Add(updatedCourse);
        DataContext.SaveAll();
        Console.WriteLine($"Course \"{updatedCourse.Title}\" (id={updatedCourse.Id}) updated.");
    }

    static void Delete(string identifier)
    {
        Course? course = null;

        if (int.TryParse(identifier, out int id))
        {
            course = DataContext.Courses.FirstOrDefault(c => c.Id == id);
        }

        if (course == null)
        {
            course = DataContext.Courses.FirstOrDefault(c => 
                c.Code.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        }

        if (course == null)
        {
            throw new KeyNotFoundException($"Course not found: '{identifier}' (neither ID nor code)");
        }

        DataContext.Courses.Remove(course);
        DataContext.SaveAll();
        Console.WriteLine($"Course \"{course.Title}\" deleted.");
    }
}