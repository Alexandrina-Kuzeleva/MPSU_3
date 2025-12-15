using ScheduleSystem.Models;
using ScheduleSystem.Storage;

namespace ScheduleSystem.Services;

public static class RecurrenceService
{
    public static List<Session> GenerateRecurring(
        int courseId, int teacherId, int groupId, int roomId,
        TimeOnly start, TimeOnly end,
        DayOfWeek dayOfWeek,
        DateOnly fromDate,
        DateOnly toDate,
        string notes = "",
        bool force = false)
    {
        var sessions = new List<Session>();

        var current = fromDate;
        int daysToAdd = ((int)dayOfWeek - (int)current.DayOfWeek + 7) % 7;
        if (daysToAdd > 0 || current.DayOfWeek != dayOfWeek)
            current = current.AddDays(daysToAdd == 0 ? 7 : daysToAdd);
        if (start >= end)
            throw new ArgumentException("Invalid session time range");

        while (current <= toDate)
        {
            var session = new Session(
                Id: DataContext.NextId<Session>(),
                CourseId: courseId,
                TeacherId: teacherId,
                GroupId: groupId,
                RoomId: roomId,
                Date: current,
                Start: start,
                End: end,
                Notes: notes + (notes.Length > 0 ? " " : "") + $"(weekly on {dayOfWeek})"
            );

            var (conflict, msg) = ConflictService.Check(session);
            if (conflict)
                throw new InvalidOperationException($"Cannot create recurring session on {current:yyyy-MM-dd}: {msg}");

            sessions.Add(session);
            current = current.AddDays(7);
        }

        return sessions;
    }
}