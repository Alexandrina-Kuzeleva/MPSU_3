using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;
using System.Text.Json;

namespace ScheduleSystem.Services;

public static class ReportService
{
    private static List<Session> FilterSessions(
        Func<Session, bool> predicate,
        DateOnly? from = null,
        DateOnly? to = null)
    {
        return DataContext.Sessions
            .Where(predicate)
            .Where(s => !from.HasValue || s.Date >= from.Value)
            .Where(s => !to.HasValue || s.Date <= to.Value)
            .OrderBy(s => s.Date)
            .ThenBy(s => s.Start)
            .ToList();
    }

    public static void PrintGroupReport(int groupId, DateOnly? from = null, 
        DateOnly? to = null, string format = "text")
    {
        var sessions = FilterSessions(s => s.GroupId == groupId, from, to);
        var groupName = DataContext.Groups.FirstOrDefault(g => g.Id == groupId)?.Code 
            ?? $"Group {groupId}";

        PrintReportHeader($"Schedule for group {groupName}", from, to, sessions);

        if (format == "csv") 
            ExportToCsv(sessions, $"report_group_{groupId}.csv");
        else if (format == "json") 
            ExportToJson(sessions, $"report_group_{groupId}.json");
        else 
            TablePrinter.PrintSessions(sessions);
    }

    public static void PrintTeacherReport(int teacherId, DateOnly? from = null, 
        DateOnly? to = null, string format = "text")
    {
        var sessions = FilterSessions(s => s.TeacherId == teacherId, from, to);
        var teacherName = DataContext.Teachers.FirstOrDefault(t => t.Id == teacherId)?.Name 
            ?? $"Teacher {teacherId}";

        PrintReportHeader($"Schedule for teacher {teacherName}", from, to, sessions);

        if (format == "csv") 
            ExportToCsv(sessions, $"report_teacher_{teacherId}.csv");
        else if (format == "json") 
            ExportToJson(sessions, $"report_teacher_{teacherId}.json");
        else 
            TablePrinter.PrintSessions(sessions);
    }

    public static void PrintRoomReport(int roomId, DateOnly? from = null, 
        DateOnly? to = null, string format = "text")
    {
        var sessions = FilterSessions(s => s.RoomId == roomId, from, to);
        var roomCode = DataContext.Rooms.FirstOrDefault(r => r.Id == roomId)?.Code 
            ?? $"Room {roomId}";

        PrintReportHeader($"Schedule for room {roomCode}", from, to, sessions);

        if (format == "csv") 
            ExportToCsv(sessions, $"report_room_{roomId}.csv");
        else if (format == "json") 
            ExportToJson(sessions, $"report_room_{roomId}.json");
        else 
            TablePrinter.PrintSessions(sessions);
    }

    public static void PrintDayReport(DateOnly date, string format = "text")
    {
        var sessions = DataContext.Sessions
            .Where(s => s.Date == date)
            .OrderBy(s => s.Start)
            .ToList();

        PrintReportHeader($"Schedule for {date:yyyy-MM-dd} ({date.DayOfWeek})", 
            null, null, sessions);

        if (format == "csv") 
            ExportToCsv(sessions, $"report_day_{date:yyyy-MM-dd}.csv");
        else if (format == "json") 
            ExportToJson(sessions, $"report_day_{date:yyyy-MM-dd}.json");
        else 
            TablePrinter.PrintSessions(sessions);
    }

    private static void PrintReportHeader(string title, DateOnly? from, 
        DateOnly? to, List<Session> sessions)
    {
        if (!sessions.Any())
        {
            Console.WriteLine("No sessions found.");
            return;
        }

        Console.WriteLine($"\n=== {title} ===");
        if (from.HasValue || to.HasValue)
            Console.WriteLine(
                $"Period: {(from?.ToString("yyyy-MM-dd") ?? "beginning")} – " +
                $"{(to?.ToString("yyyy-MM-dd") ?? "end")}"
            );
        Console.WriteLine($"Total sessions: {sessions.Count}\n");
    }

    public static void PrintGroupWeekReport(int groupId, DateOnly? from = null, 
        DateOnly? to = null)
    {
        var sessions = FilterSessions(s => s.GroupId == groupId, from, to);
        var groupName = DataContext.Groups.FirstOrDefault(g => g.Id == groupId)?.Code 
            ?? $"Group {groupId}";

        if (!sessions.Any())
        {
            Console.WriteLine("No sessions found.");
            return;
        }

        Console.WriteLine($"Weekly schedule for group: {groupName}");
        if (from.HasValue || to.HasValue)
            Console.WriteLine(
                $"Period: {(from?.ToString("yyyy-MM-dd") ?? "beginning")} – " +
                $"{(to?.ToString("yyyy-MM-dd") ?? "end")}"
            );
        Console.WriteLine();

        var groupedByDate = sessions
            .GroupBy(s => s.Date)
            .OrderBy(g => g.Key);

        foreach (var dayGroup in groupedByDate)
        {
            var date = dayGroup.Key;
            Console.WriteLine($"{date:yyyy-MM-dd} ({date.DayOfWeek}):");
            
            var daySessions = dayGroup.OrderBy(s => s.Start).ToList();
            
            foreach (var s in daySessions)
            {
                var course = DataContext.Courses.FirstOrDefault(
                    c => c.Id == s.CourseId)?.Title ?? "?";
                var teacher = DataContext.Teachers.FirstOrDefault(
                    t => t.Id == s.TeacherId)?.Name ?? "?";
                var room = DataContext.Rooms.FirstOrDefault(
                    r => r.Id == s.RoomId)?.Code ?? "?";

                Console.WriteLine($"  {s.TimeRange} - {course}");
                Console.WriteLine($"    Teacher: {teacher}, Room: {room}");
                if (!string.IsNullOrEmpty(s.Notes))
                    Console.WriteLine($"    Notes: {s.Notes}");
            }
            Console.WriteLine();
        }

        Console.WriteLine($"Total sessions: {sessions.Count}");
    }

    private static void ExportToCsv(List<Session> sessions, string filename)
    {
        var lines = new List<string>
        {
            "Date,Day,Time,Course,Teacher,Group,Room,Notes"
        };

        foreach (var s in sessions)
        {
            var course = DataContext.Courses.FirstOrDefault(
                c => c.Id == s.CourseId)?.Title ?? "—";
            var teacher = DataContext.Teachers.FirstOrDefault(
                t => t.Id == s.TeacherId)?.Name ?? "—";
            var group = DataContext.Groups.FirstOrDefault(
                g => g.Id == s.GroupId)?.Code ?? "—";
            var room = DataContext.Rooms.FirstOrDefault(
                r => r.Id == s.RoomId)?.Code ?? "—";

            lines.Add(
                $"{s.Date:yyyy-MM-dd},{s.DayShort},{s.TimeRange},{course}," +
                $"{teacher},{group},{room},{s.Notes.Replace(",", " ")}"
            );
        }

        File.WriteAllLines(filename, lines);
        Console.WriteLine(
            $"Exported {sessions.Count} rows → {filename}"
        );
    }

    private static void ExportToJson(List<Session> sessions, string filename)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filename, JsonSerializer.Serialize(sessions, options));
        Console.WriteLine($"Exported → {filename}");
    }
}