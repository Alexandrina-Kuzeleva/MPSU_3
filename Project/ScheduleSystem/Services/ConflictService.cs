using ScheduleSystem.Models;
using ScheduleSystem.Storage;

namespace ScheduleSystem.Services;

public static class ConflictService
{
    public static (bool hasConflict, string message) Check(Session newSession, bool checkGroup = true)
    {
        foreach (var existing in DataContext.Sessions)
        {
            if (existing.Id == newSession.Id) continue;
            if (existing.Date != newSession.Date) continue;

            bool timeOverlap = newSession.Start < existing.End && existing.Start < newSession.End;
            if (!timeOverlap) continue;

            if (existing.RoomId == newSession.RoomId)
                return (true, $"Room {GetRoomCode(existing.RoomId)} is occupied {existing.Start:HH:mm}-{existing.End:HH:mm} (session id={existing.Id})");

            if (existing.TeacherId == newSession.TeacherId)
                return (true, $"Teacher {GetTeacherName(existing.TeacherId)} is busy {existing.Start:HH:mm}-{existing.End:HH:mm} (session id={existing.Id})");

            if (checkGroup && existing.GroupId == newSession.GroupId)
                return (true, $"Group {GetGroupCode(existing.GroupId)} has another class at this time (session id={existing.Id})");
        }

        return (false, "");
    }

    public static List<string> FindAllConflicts()
    {
        var conflicts = new List<string>();
        var checkedIds = new HashSet<int>();

        foreach (var s1 in DataContext.Sessions)
        {
            if (checkedIds.Contains(s1.Id)) continue;

            foreach (var s2 in DataContext.Sessions)
            {
                if (s1.Id >= s2.Id || s1.Date != s2.Date) continue;

                if (s1.OverlapsWith(s2))
                {
                    if (s1.RoomId == s2.RoomId)
                        conflicts.Add($"ROOM CONFLICT on {s1.Date:yyyy-MM-dd} {s1.TimeRange} vs {s2.TimeRange} | {GetRoomCode(s1.RoomId)}");
                    if (s1.TeacherId == s2.TeacherId)
                        conflicts.Add($"TEACHER CONFLICT on {s1.Date:yyyy-MM-dd} | {GetTeacherName(s1.TeacherId)}");
                    if (s1.GroupId == s2.GroupId)
                        conflicts.Add($"GROUP CONFLICT on {s1.Date:yyyy-MM-dd} | {GetGroupCode(s1.GroupId)}");

                    checkedIds.Add(s2.Id);
                }
            }
            checkedIds.Add(s1.Id);
        }

        return conflicts;
    }

    private static string GetRoomCode(int id) => DataContext.Rooms.FirstOrDefault(r => r.Id == id)?.Code ?? $"[Room {id}]";
    private static string GetTeacherName(int id) => DataContext.Teachers.FirstOrDefault(t => t.Id == id)?.Name ?? $"[Teacher {id}]";
    private static string GetGroupCode(int id) => DataContext.Groups.FirstOrDefault(g => g.Id == id)?.Code ?? $"[Group {id}]";
}