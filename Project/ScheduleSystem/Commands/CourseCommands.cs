using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class CourseCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) 
            throw new ArgumentException(
                "Usage: sched course <add|list|show|delete>"
            );

        var action = args[1].ToLower();
        switch (action)
        {
            case "add": Add(args); break;
            case "list": List(args); break;
            case "show":    
                if (args.Length < 3) 
                    throw new ArgumentException(
                        "Usage: sched course show <id|code>"
                    ); 
                Show(args[2]); 
                break;
            case "delete":  
                if (args.Length < 3) 
                    throw new ArgumentException(
                        "Usage: sched course delete <id|code>"
                    );
                Delete(args[2]); 
                break;
            case "update":
                if (args.Length < 3) 
                    throw new ArgumentException(
                        "Usage: sched course update <id>" + 
                        "[--title new] [--code new] [--duration N]");
                Update(args[2], args);
                break;
            default: 
                throw new ArgumentException($"Unknown course action: {action}");
        }
    }

    static void Add(string[] args)
    {
        bool hasArgs = args.Any(a => a.StartsWith("--"));
        
        if (!hasArgs)
        {
            AddInteractive();
            return;
        }

        var title = ArgsParser.GetValue(args, "--title") ?? 
            throw new ArgumentException("Missing --title");
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

    static void AddInteractive()
    {
        Console.WriteLine("Create New Course");
        Console.WriteLine("Leave empty to cancel.");
        Console.WriteLine();
        
        try
        {
            Console.Write("Course title (e.g., Algorithms): ");
            var title = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Cancelled.");
                return;
            }
            
            Console.Write("Course code (optional, e.g., CS101): ");
            var code = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(code)) code = null;
            
            Console.Write("Duration in minutes (default: 90): ");
            var durationStr = Console.ReadLine();
            if (!int.TryParse(durationStr, out int duration) || duration <= 0)
            {
                Console.WriteLine("Invalid duration. Using default: 90");
                duration = 90;
            }
            
            var course = new Course(
                Id: DataContext.NextId<Course>(),
                Title: title,
                Code: code,
                DurationMinutes: duration
            );
            
            DataContext.Courses.Add(course);
            DataContext.SaveAll();
            
            Console.WriteLine($"Course '{title}' created with ID {course.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    static void List(string[] args)
    {
        var titleLike = ArgsParser.GetValue(args, "--title-like");
        var codeLike = ArgsParser.GetValue(args, "--code-like");
        var minDuration = ArgsParser.GetInt(args, "--min-duration");
        var maxDuration = ArgsParser.GetInt(args, "--max-duration");
        var sortBy = ArgsParser.GetValue(args, "--sort") ?? "title";
        var limit = ArgsParser.GetInt(args, "--limit");
        var reverse = ArgsParser.HasFlag(args, "--reverse") || 
            ArgsParser.HasFlag(args, "--desc");

        var query = DataContext.Courses.AsQueryable();

        if (!string.IsNullOrEmpty(titleLike))
            query = query.Where(c => c.Title.Contains(
                titleLike, StringComparison.OrdinalIgnoreCase
            ));
        
        if (!string.IsNullOrEmpty(codeLike))
            query = query.Where(c => c.Code != null && c.Code.Contains(
                codeLike, StringComparison.OrdinalIgnoreCase
            ));
        
        if (minDuration.HasValue)
            query = query.Where(c => c.DurationMinutes >= minDuration.Value);
        
        if (maxDuration.HasValue)
            query = query.Where(c => c.DurationMinutes <= maxDuration.Value);

        query = sortBy.ToLower() switch
        {
            "code" => query.OrderBy(c => c.Code ?? ""),
            "duration" => query.OrderBy(c => c.DurationMinutes),
            _ => query.OrderBy(c => c.Title)
        };

        if (reverse)
            query = query.Reverse();

        var courses = query.ToList();

        if (!courses.Any())
        {
            Console.WriteLine("No courses found.");
            return;
        }

        if (limit.HasValue)
            courses = courses.Take(limit.Value).ToList();

        Console.WriteLine($"Found {courses.Count} course(s):");
        Console.WriteLine($"{ "ID",-5}| { "Title",-40}| { "Code",-10}| {"Duration"}");
        Console.WriteLine(new string('-', 80));
        
        foreach (var c in courses)
            Console.WriteLine(
                $"{c.Id,3} | {c.Title,-40} | {c.Code ?? "-",-9} | {c.DurationMinutes,8} min"
            );
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
            throw new KeyNotFoundException(
                $"Course not found: '{identifier}' (neither ID nor code)"
            );
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
                c.Code != null && c.Code.Equals(
                    identifier, StringComparison.OrdinalIgnoreCase
                ));
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
            DurationMinutes: int.TryParse(newDurationStr, out int duration) ? 
                duration : course.DurationMinutes
        );

        DataContext.Courses.Remove(course);
        DataContext.Courses.Add(updatedCourse);
        DataContext.SaveAll();
        Console.WriteLine(
            $"Course \"{updatedCourse.Title}\" (id={updatedCourse.Id}) updated."
        );
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
            throw new KeyNotFoundException(
                $"Course not found: '{identifier}' (neither ID nor code)"
            );
        }

        DataContext.Courses.Remove(course);
        DataContext.SaveAll();
        Console.WriteLine($"Course \"{course.Title}\" deleted.");
    }
}