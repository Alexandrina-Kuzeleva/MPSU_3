namespace ScheduleSystem.Models;

public record Course(
    int Id,
    string Title,
    string? Code = null,
    int DurationMinutes = 90)
{
    public override string ToString() => $"{Title} ({Code ?? ""})".Trim();
}