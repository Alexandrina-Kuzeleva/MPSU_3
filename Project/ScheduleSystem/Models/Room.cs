namespace ScheduleSystem.Models;

public record Room(
    int Id,
    string Code,
    int Capacity,
    string? Building = null,
    string? AttributesJson = null)
{
    public override string ToString() => $"{Code} ({Capacity} мест)";
}