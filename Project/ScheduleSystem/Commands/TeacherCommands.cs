using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class TeacherCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: sched teacher <add|list|show|delete>");

        var action = args[1].ToLower();
        switch (action)
        {
            case "add":     Add(args); break;
            case "list":    List(); break;
            case "show":
                if (args.Length < 3) 
                    throw new ArgumentException("Usage: sched teacher show <id>");
                Show(int.Parse(args[2]));
                break;
            case "delete":
                if (args.Length < 3) 
                    throw new ArgumentException("Usage: sched teacher delete <id>");
                Delete(int.Parse(args[2]));
                break;
            case "update":
                if (args.Length < 3) throw new ArgumentException("Usage: sched teacher update <id> [--name new] [--email new]");
                Update(int.Parse(args[2]), args);
                break;
            default: throw new ArgumentException($"Unknown teacher action: {action}");
        }
    }

    static void Add(string[] args)
    {
        var name = ArgsParser.GetValue(args, "--name") ?? throw new ArgumentException("Missing --name");
        var email = ArgsParser.GetValue(args, "--email");

        var teacher = new Teacher(
            Id: DataContext.NextId<Teacher>(),
            Name: name,
            Email: email
        );

        DataContext.Teachers.Add(teacher);
        DataContext.SaveAll();

        Console.WriteLine($"Teacher {teacher.Name} (id={teacher.Id}) created.");
    }

    static void List()
    {
        if (!DataContext.Teachers.Any())
        {
            Console.WriteLine("No teachers.");
            return;
        }

        foreach (var t in DataContext.Teachers.OrderBy(t => t.Name))
            Console.WriteLine($"{t.Id,3} | {t.Name,-30} | {t.Email ?? "-"}");
    }

    static void Show(int id)
    {
        var t = DataContext.Teachers.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Teacher {id} not found");

        Console.WriteLine($"ID: {t.Id}");
        Console.WriteLine($"Name: {t.Name}");
        Console.WriteLine($"Email: {t.Email ?? "—"}");
    }

    static void Update(int id, string[] args)
    {
        var teacher = DataContext.Teachers.FirstOrDefault(t => t.Id == id)
                    ?? throw new KeyNotFoundException($"Teacher {id} not found");

        var newName = ArgsParser.GetValue(args, "--name");
        var newEmail = ArgsParser.GetValue(args, "--email");

        var updatedTeacher = new Teacher(
            Id: teacher.Id,
            Name: newName ?? teacher.Name,
            Email: newEmail ?? teacher.Email
        );

        DataContext.Teachers.Remove(teacher);
        DataContext.Teachers.Add(updatedTeacher);
        DataContext.SaveAll();
        Console.WriteLine($"Teacher {updatedTeacher.Name} (id={updatedTeacher.Id}) updated.");
    }

    static void Delete(int id)
    {
        var t = DataContext.Teachers.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Teacher {id} not found");

        DataContext.Teachers.Remove(t);
        DataContext.SaveAll();
        Console.WriteLine($"Teacher {t.Name} deleted.");
    }
}