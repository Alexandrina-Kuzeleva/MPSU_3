using System;

namespace ScheduleSystem.Models;

public record Session(
    int Id,
    int CourseId,
    int TeacherId,
    int GroupId,
    int RoomId,
    DateOnly Date,
    TimeOnly Start,
    TimeOnly End,
    string Notes = "")
{
    public bool OverlapsWith(Session other)
 {
     if (Date != other.Date) return false;
     return Start < other.End && other.Start < End;
 }

 public string TimeRange => $"{Start:HH:mm}-{End:HH:mm}";
 public string DayShort => Date.DayOfWeek.ToString()[..3].ToUpper();
}