namespace ScheduleSystem.Models;

public record Group(
    int Id,
    string Code,
    int Size,
    int? Year = null)
{
    public override string ToString() => Code;
}