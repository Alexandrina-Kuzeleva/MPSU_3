using ScheduleSystem.Models;
using ScheduleSystem.Services;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class SessionCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: sched session <add|list|show|update|delete|conflicts>");

        var action = args[1].ToLower();
        switch (action)
        {
            case "add":           Add(args); break;
            case "list":          List(args); break;
            case "show":          Show(int.Parse(args[2])); break;
            case "update":        Update(int.Parse(args[2]), args); break;
            case "delete":        Delete(int.Parse(args[2])); break;
            case "conflicts":     FindConflicts(); break;
            default: throw new ArgumentException("Unknown session action");
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

        var courseId = int.Parse(ArgsParser.GetValue(args, "--course")!);
        var teacherId = int.Parse(ArgsParser.GetValue(args, "--teacher")!);
        var groupId = int.Parse(ArgsParser.GetValue(args, "--group")!);
        var roomId = int.Parse(ArgsParser.GetValue(args, "--room")!);
        var dateStr = ArgsParser.GetValue(args, "--date");
        var startStr = ArgsParser.GetValue(args, "--start") ?? throw new ArgumentException("Missing --start");
        var endStr = ArgsParser.GetValue(args, "--end") ?? throw new ArgumentException("Missing --end");
        var notes = ArgsParser.GetValue(args, "--notes") ?? "";
        var force = ArgsParser.HasFlag(args, "--force");

        var date = DateOnly.Parse(dateStr ?? DateTime.Today.ToString("yyyy-MM-dd"));
        var start = TimeOnly.Parse(startStr);
        var end = TimeOnly.Parse(endStr);

        if (start >= end) throw new ArgumentException("Start time must be before end time");

        var dowStr = ArgsParser.GetValue(args, "--dow");
        var fromStr = ArgsParser.GetValue(args, "--from");
        var toStr = ArgsParser.GetValue(args, "--to");

        List<Session> sessionsToAdd;

        if (dowStr != null && fromStr != null && toStr != null)
        {
            var dow = ParseDayOfWeek(dowStr);
            var from = DateOnly.Parse(fromStr);
            var to = DateOnly.Parse(toStr);

            sessionsToAdd = RecurrenceService.GenerateRecurring(
                courseId, teacherId, groupId, roomId, start, end, dow, from, to, notes, force);
        }
        else
        {
            var session = new Session(
                Id: DataContext.NextId<Session>(),
                CourseId: courseId, TeacherId: teacherId, GroupId: groupId, RoomId: roomId,
                Date: date, Start: start, End: end, Notes: notes);

            var (conflict, msg) = ConflictService.Check(session);
            if (conflict && !force)
                throw new InvalidOperationException($"Conflict detected: {msg}\nUse --force to add anyway.");

            if (conflict && force)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"⚠ Warning: Adding session with conflict: {msg}");
                Console.ResetColor();
            }

            sessionsToAdd = new() { session };
        }

        foreach (var s in sessionsToAdd)
            DataContext.Sessions.Add(s);

        DataContext.SaveAll();
        Console.WriteLine($"Session(s) created: {sessionsToAdd.Count} record(s) added.");
    }

    static void AddInteractive()
    {
        Console.WriteLine("=== Create New Session ===");
        Console.WriteLine("Leave empty to cancel or use default values.");
        Console.WriteLine();
        
        try
        {
            Console.WriteLine("Available courses:");
            foreach (var c in DataContext.Courses.Take(10))
                Console.WriteLine($"  [{c.Id}] {c.Title} ({c.Code})");
            if (DataContext.Courses.Count > 10)
                Console.WriteLine($"  ... and {DataContext.Courses.Count - 10} more");
            
            Console.Write($"\nSelect course ID (1-{DataContext.Courses.Count}): ");
            var courseInput = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(courseInput))
            {
                Console.WriteLine("Cancelled.");
                return;
            }
            if (!int.TryParse(courseInput, out int courseId) || !DataContext.Courses.Any(c => c.Id == courseId))
            {
                Console.WriteLine("Invalid course ID.");
                return;
            }
            
            Console.WriteLine("\nAvailable teachers:");
            foreach (var t in DataContext.Teachers.Take(10))
                Console.WriteLine($"  [{t.Id}] {t.Name}");
            if (DataContext.Teachers.Count > 10)
                Console.WriteLine($"  ... and {DataContext.Teachers.Count - 10} more");
            
            Console.Write($"\nSelect teacher ID (1-{DataContext.Teachers.Count}): ");
            var teacherInput = Console.ReadLine();
            if (!int.TryParse(teacherInput, out int teacherId) || !DataContext.Teachers.Any(t => t.Id == teacherId))
            {
                Console.WriteLine("Invalid teacher ID.");
                return;
            }
            
            Console.WriteLine("\nAvailable groups:");
            foreach (var g in DataContext.Groups.Take(10))
                Console.WriteLine($"  [{g.Id}] {g.Code} ({g.Size} students)");
            if (DataContext.Groups.Count > 10)
                Console.WriteLine($"  ... and {DataContext.Groups.Count - 10} more");
            
            Console.Write($"\nSelect group ID (1-{DataContext.Groups.Count}): ");
            var groupInput = Console.ReadLine();
            if (!int.TryParse(groupInput, out int groupId) || !DataContext.Groups.Any(g => g.Id == groupId))
            {
                Console.WriteLine("Invalid group ID.");
                return;
            }
            
            Console.WriteLine("\nAvailable rooms:");
            foreach (var r in DataContext.Rooms.Take(10))
                Console.WriteLine($"  [{r.Id}] {r.Code} ({r.Capacity} seats, {r.Building})");
            if (DataContext.Rooms.Count > 10)
                Console.WriteLine($"  ... and {DataContext.Rooms.Count - 10} more");
            
            Console.Write($"\nSelect room ID (1-{DataContext.Rooms.Count}): ");
            var roomInput = Console.ReadLine();
            if (!int.TryParse(roomInput, out int roomId) || !DataContext.Rooms.Any(r => r.Id == roomId))
            {
                Console.WriteLine("Invalid room ID.");
                return;
            }
            
            Console.Write("\nDate (yyyy-mm-dd) [today]: ");
            var dateStr = Console.ReadLine();
            var date = string.IsNullOrWhiteSpace(dateStr) 
                ? DateOnly.FromDateTime(DateTime.Today) 
                : DateOnly.Parse(dateStr);
            
            Console.Write("Start time (HH:mm) [10:00]: ");
            var startStr = Console.ReadLine();
            var start = string.IsNullOrWhiteSpace(startStr) 
                ? new TimeOnly(10, 0) 
                : TimeOnly.Parse(startStr);
            
            Console.Write("End time (HH:mm) [11:30]: ");
            var endStr = Console.ReadLine();
            var end = string.IsNullOrWhiteSpace(endStr) 
                ? new TimeOnly(11, 30) 
                : TimeOnly.Parse(endStr);
            
            if (start >= end)
            {
                Console.WriteLine("Error: Start time must be before end time.");
                return;
            }
            
            Console.Write("Notes (optional): ");
            var notes = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(notes)) notes = "";
            
            var session = new Session(
                Id: DataContext.NextId<Session>(),
                CourseId: courseId,
                TeacherId: teacherId,
                GroupId: groupId,
                RoomId: roomId,
                Date: date,
                Start: start,
                End: end,
                Notes: notes
            );
            
            var (conflict, msg) = ConflictService.Check(session);
            if (conflict)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n⚠ Warning: Conflict detected: {msg}");
                Console.ResetColor();
                
                Console.Write("Force create anyway? (y/N): ");
                var forceInput = Console.ReadLine();
                if (!forceInput?.ToLower().StartsWith("y") ?? true)
                {
                    Console.WriteLine("Session creation cancelled.");
                    return;
                }
            }
            
            DataContext.Sessions.Add(session);
            DataContext.SaveAll();
            
            Console.WriteLine($"Session created with ID {session.Id}");
            Console.WriteLine($"   Date: {date:yyyy-MM-dd} ({date.DayOfWeek})");
            Console.WriteLine($"   Time: {start:HH:mm}-{end:HH:mm}");
        }
        catch (FormatException)
        {
            Console.WriteLine("Error: Invalid date or time format.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void Update(int id, string[] args)
    {
        var session = DataContext.Sessions.FirstOrDefault(s => s.Id == id)
                      ?? throw new KeyNotFoundException($"Session {id} not found");

        var newCourseId = ArgsParser.GetInt(args, "--course");
        var newTeacherId = ArgsParser.GetInt(args, "--teacher");
        var newGroupId = ArgsParser.GetInt(args, "--group");
        var newRoomId = ArgsParser.GetInt(args, "--room");
        var newDateStr = ArgsParser.GetValue(args, "--date");
        var newStartStr = ArgsParser.GetValue(args, "--start");
        var newEndStr = ArgsParser.GetValue(args, "--end");
        var newNotes = ArgsParser.GetValue(args, "--notes");
        var force = ArgsParser.HasFlag(args, "--force");

        var updatedSession = new Session(
            Id: session.Id,
            CourseId: newCourseId ?? session.CourseId,
            TeacherId: newTeacherId ?? session.TeacherId,
            GroupId: newGroupId ?? session.GroupId,
            RoomId: newRoomId ?? session.RoomId,
            Date: newDateStr != null ? DateOnly.Parse(newDateStr) : session.Date,
            Start: newStartStr != null ? TimeOnly.Parse(newStartStr) : session.Start,
            End: newEndStr != null ? TimeOnly.Parse(newEndStr) : session.End,
            Notes: newNotes ?? session.Notes
        );

        if (updatedSession.Start >= updatedSession.End) 
            throw new ArgumentException("Start time must be before end time");

        var (conflict, msg) = ConflictService.Check(updatedSession);
        if (conflict && !force)
            throw new InvalidOperationException($"Conflict detected: {msg}\nUse --force to update anyway.");

        if (conflict && force)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ Warning: Updating session with conflict: {msg}");
            Console.ResetColor();
        }

        DataContext.Sessions.Remove(session);
        DataContext.Sessions.Add(updatedSession);
        DataContext.SaveAll();
        
        Console.WriteLine($"Session {id} updated.");
    }

    static void List(string[] args)
    {
        var groupId = ArgsParser.GetInt(args, "--group");
        var teacherId = ArgsParser.GetInt(args, "--teacher");
        var roomId = ArgsParser.GetInt(args, "--room");
        var dateStr = ArgsParser.GetValue(args, "--date");
        var fromStr = ArgsParser.GetValue(args, "--from");
        var toStr = ArgsParser.GetValue(args, "--to");
        var dayStr = ArgsParser.GetValue(args, "--day");
        var timeStr = ArgsParser.GetValue(args, "--time");
        var limit = ArgsParser.GetInt(args, "--limit");
        var reverse = ArgsParser.HasFlag(args, "--reverse") || ArgsParser.HasFlag(args, "--desc");

        var query = DataContext.Sessions.AsQueryable();

        if (groupId.HasValue) query = query.Where(s => s.GroupId == groupId);
        if (teacherId.HasValue) query = query.Where(s => s.TeacherId == teacherId);
        if (roomId.HasValue) query = query.Where(s => s.RoomId == roomId);
        if (dateStr != null) query = query.Where(s => s.Date == DateOnly.Parse(dateStr));
        if (fromStr != null) query = query.Where(s => s.Date >= DateOnly.Parse(fromStr));
        if (toStr != null) query = query.Where(s => s.Date <= DateOnly.Parse(toStr));
        
        if (dayStr != null)
        {
            var day = ParseDayOfWeek(dayStr);
            query = query.Where(s => s.Date.DayOfWeek == day);
        }
        
        if (timeStr != null)
        {
            var times = timeStr.Split('-');
            if (times.Length == 2)
            {
                var startTime = TimeOnly.Parse(times[0]);
                var endTime = TimeOnly.Parse(times[1]);
                query = query.Where(s => s.Start >= startTime && s.End <= endTime);
            }
        }

        if (ArgsParser.HasFlag(args, "--conflicts-only"))
        {
            var conflictingSessions = new List<Session>();
            foreach (var session in query)
            {
                var (hasConflict, _) = ConflictService.Check(session, checkGroup: false);
                if (hasConflict) conflictingSessions.Add(session);
            }
            query = conflictingSessions.AsQueryable();
        }

        query = query.OrderBy(s => s.Date).ThenBy(s => s.Start);

        if (reverse)
            query = query.Reverse();

        var list = query.ToList();

        if (limit.HasValue)
            list = list.Take(limit.Value).ToList();

        if (!list.Any())
        {
            Console.WriteLine("No sessions found.");
            return;
        }

        TablePrinter.PrintSessions(list);
    }

    static void Show(int id)
    {
        var s = DataContext.Sessions.FirstOrDefault(x => x.Id == id)
                ?? throw new KeyNotFoundException($"Session {id} not found");

        var course = DataContext.Courses.FirstOrDefault(c => c.Id == s.CourseId)?.Title ?? "?";
        var teacher = DataContext.Teachers.FirstOrDefault(t => t.Id == s.TeacherId)?.Name ?? "?";
        var group = DataContext.Groups.FirstOrDefault(g => g.Id == s.GroupId)?.Code ?? "?";
        var room = DataContext.Rooms.FirstOrDefault(r => r.Id == s.RoomId)?.Code ?? "?";

        Console.WriteLine($"Session ID: {s.Id}");
        Console.WriteLine($"Date: {s.Date:yyyy-MM-dd} ({s.DayShort})");
        Console.WriteLine($"Time: {s.TimeRange}");
        Console.WriteLine($"Course: {course}");
        Console.WriteLine($"Teacher: {teacher}");
        Console.WriteLine($"Group: {group}");
        Console.WriteLine($"Room: {room}");
        Console.WriteLine($"Notes: {s.Notes}");
    }

    static void Delete(int id)
    {
        var session = DataContext.Sessions.FirstOrDefault(s => s.Id == id)
                      ?? throw new KeyNotFoundException($"Session {id} not found");

        DataContext.Sessions.Remove(session);
        DataContext.SaveAll();
        Console.WriteLine($"Session {id} deleted.");
    }

    static DayOfWeek ParseDayOfWeek(string input)
    {
        return input.Trim().ToUpper() switch
        {
            "MON" or "MONDAY" or "ПН" or "ПОНЕДЕЛЬНИК" => DayOfWeek.Monday,
            "TUE" or "TUESDAY" or "ВТ" or "ВТОРНИК"   => DayOfWeek.Tuesday,
            "WED" or "WEDNESDAY" or "СР" or "СРЕДА"   => DayOfWeek.Wednesday,
            "THU" or "THURSDAY" or "ЧТ" or "ЧЕТВЕРГ"  => DayOfWeek.Thursday,
            "FRI" or "FRIDAY" or "ПТ" or "ПЯТНИЦА"    => DayOfWeek.Friday,
            "SAT" or "SATURDAY" or "СБ" or "СУББОТА"  => DayOfWeek.Saturday,
            "SUN" or "SUNDAY" or "ВС" or "ВОСКРЕСЕНЬЕ" => DayOfWeek.Sunday,
            _ => Enum.Parse<DayOfWeek>(input, true)
        };
    }

    static void FindConflicts()
    {
        var conflicts = ConflictService.FindAllConflicts();
        if (!conflicts.Any())
            Console.WriteLine("No conflicts found.");
        else
            foreach (var c in conflicts)
                Console.WriteLine("CONFLICT: " + c);
    }
}