using ScheduleSystem.Services;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class ReportCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: sched report <group|teacher|room|day> ...");

        var type = args[1].ToLower();
        var format = ArgsParser.GetValue(args, "--format") ?? "text";

        DateOnly? from = ArgsParser.GetDate(args, "--from");
        DateOnly? to = ArgsParser.GetDate(args, "--to");

        switch (type)
        {
            case "group":
                var groupId = int.Parse(ArgsParser.GetValue(args, "--group")!);
                ReportService.PrintGroupReport(groupId, from, to, format);
                break;

            case "teacher":
                var teacherId = int.Parse(ArgsParser.GetValue(args, "--teacher")!);
                ReportService.PrintTeacherReport(teacherId, from, to, format);
                break;

            ;

            case "room":
                var roomId = int.Parse(ArgsParser.GetValue(args, "--room")!);
                ReportService.PrintRoomReport(roomId, from, to, format);
                break;

            case "day":
                var date = DateOnly.Parse(ArgsParser.GetValue(args, "--date")!);
                ReportService.PrintDayReport(date, format);
                break;

            default:
                throw new ArgumentException($"Unknown report type: {type}. Use group|teacher|room|day");
        }
    }
}