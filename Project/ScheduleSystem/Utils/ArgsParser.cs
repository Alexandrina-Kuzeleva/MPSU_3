namespace ScheduleSystem.Utils;

public static class ArgsParser
{
    public static string? GetValue(string[] args, string option)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(option, StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < args.Length) return args[i + 1];
                return null;
            }
        }
        return null;
    }

    public static int? GetInt(string[] args, string option)
    {
        var val = GetValue(args, option);
        return int.TryParse(val, out int result) ? result : null;
    }

    public static DateOnly? GetDate(string[] args, string option)
    {
        var val = GetValue(args, option);
        return DateOnly.TryParse(val, out DateOnly date) ? date : null;
    }

    public static bool HasFlag(string[] args, string flag)
    {
        return args.Any(a => a.Equals(flag, StringComparison.OrdinalIgnoreCase));
    }
}