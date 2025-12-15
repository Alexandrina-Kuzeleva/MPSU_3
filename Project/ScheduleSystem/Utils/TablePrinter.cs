using ScheduleSystem.Models;
using ScheduleSystem.Storage;

namespace ScheduleSystem.Utils;

public static class TablePrinter
{
    public static void PrintSessions(List<Session> sessions)
    {
        if (!sessions.Any())
        {
            Console.WriteLine("No sessions to display.");
            return;
        }

        Console.WriteLine($@"{"Date",-10} {"Day",-4} {"Time",-12} {"Course",-24} 
            {"Teacher",-24} {"Group",-10} {"Room",-9} {"Notes"}".TrimStart()
        );
        Console.WriteLine(new string('-', 140));

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

            Console.WriteLine(
                $@"{s.Date:yyyy-MM-dd}  {s.DayShort}  {s.TimeRange,-12} {course,-25}  
                    {teacher,-22}  {group,-10}  {room,-8}  {s.Notes}.TrimStart()"
            );
        }

        Console.WriteLine($"\nTotal: {sessions.Count} session(s)");
    }
}