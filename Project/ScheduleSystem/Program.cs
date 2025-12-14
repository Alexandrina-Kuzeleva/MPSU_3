using ScheduleSystem.Commands;
using ScheduleSystem.Storage;

namespace ScheduleSystem;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                Console.WriteLine("sched — university schedule manager");
                Console.WriteLine("Commands: init, room, teacher, group, course, session, report, backup, restore, import, export, clear");
                return 1;
            }

            string cmd = args[0].ToLower();

            DataContext.Initialize();

            switch (cmd)
            {
                case "init":      InitCommand.Run(args); break;
                case "room":      RoomCommands.Run(args); break;
                case "teacher":   TeacherCommands.Run(args); break;
                case "group":     GroupCommands.Run(args); break;
                case "course":    CourseCommands.Run(args); break;
                case "session":   SessionCommands.Run(args); break;
                case "report":    ReportCommands.Run(args); break;
                case "backup":    BackupCommands.Backup(args); break;
                case "restore":   BackupCommands.Restore(args); break;
                case "import":    ImportExportCommands.Run(args); break;
                case "export":    ImportExportCommands.Run(args); break;
                case "clear":     ClearCommands.Run(); break;

                default:
                    Console.WriteLine($"Unknown command: {cmd}");
                    return 1;
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"ERROR: {ex.Message}");
            Console.ResetColor();
            return ex switch
            {
                ArgumentException or FormatException => 2,
                KeyNotFoundException => 3,
                InvalidOperationException => 4,
                FileNotFoundException => 5,
                _ => 1
            };
        }
    }
}