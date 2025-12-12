namespace ScheduleSystem.Models;

public record User(
    string Username,
    string PasswordHash,
    string Role
    ) 
{
    public bool IsAdmin => Role.Equals("admin", StringComparison.OrdinalIgnoreCase);
}