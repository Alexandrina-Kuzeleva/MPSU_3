namespace ScheduleSystem.Models;

public record Teacher(
    int Id,
    string Name,
    string? Email = null)
{
    public override string ToString() => Name;
}