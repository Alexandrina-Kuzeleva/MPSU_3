using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class BackupCommands
{
    public static void Backup(string[] args)
    {
        var outPath = ArgsParser.GetValue(args, "--out") 
                      ?? throw new ArgumentException("Missing --out path");

        JsonStorage.Backup(outPath);
    }

    public static void Restore(string[] args)
    {
        var fromPath = ArgsParser.GetValue(args, "--from") 
                       ?? throw new ArgumentException("Missing --from path");

        JsonStorage.Restore(fromPath);
    }
}