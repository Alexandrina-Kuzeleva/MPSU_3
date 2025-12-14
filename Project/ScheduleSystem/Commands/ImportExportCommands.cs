using ScheduleSystem.Models;
using ScheduleSystem.Services;
using ScheduleSystem.Storage;
using ScheduleSystem.Utils;

namespace ScheduleSystem.Commands;

public static class ImportExportCommands
{
    public static void Run(string[] args)
    {
        if (args.Length < 2) throw new ArgumentException("Usage: sched <import|export> <csv|json> ...");

        var action = args[0].ToLower();
        var format = args[1].ToLower();

        switch (action)
        {
            case "import":
                if (format == "csv") ImportCsv(args);
                else if (format == "json") ImportJson(args);
                else throw new ArgumentException("Format must be csv or json");
                break;
                
            case "export":
                if (format == "csv") ExportCsv(args);
                else if (format == "json") ExportJson(args);
                else throw new ArgumentException("Format must be csv or json");
                break;
                
            default:
                throw new ArgumentException("Action must be import or export");
        }
    }

    private static void ImportCsv(string[] args)
    {
        var filePath = ArgsParser.GetValue(args, "--file") ?? throw new ArgumentException("Missing --file");
        var entity = ArgsParser.GetValue(args, "--entity") ?? throw new ArgumentException("Missing --entity");
        var mode = ArgsParser.GetValue(args, "--mode") ?? "append";

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 2) return;

        var headers = lines[0].Split(',');
        var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        int successCount = 0;
        int errorCount = 0;

        switch (entity.ToLower())
        {
            case "sessions":
                if (mode == "replace") DataContext.Sessions.Clear();
                
                foreach (var line in dataLines)
                {
                    try
                    {
                        var values = ParseCsvLine(line);
                        if (values.Length < 8) continue;

                        var session = new Session(
                            Id: DataContext.NextId<Session>(),
                            CourseId: GetOrCreateCourseId(values[3]),
                            TeacherId: GetOrCreateTeacherId(values[4]),
                            GroupId: GetOrCreateGroupId(values[5]),
                            RoomId: GetOrCreateRoomId(values[6]),
                            Date: DateOnly.Parse(values[0]),
                            Start: TimeOnly.Parse(values[1].Split('-')[0]),
                            End: TimeOnly.Parse(values[1].Split('-')[1]),
                            Notes: values.Length > 7 ? values[7] : ""
                        );

                        var conflictResult = ConflictService.Check(session);
                        if (!conflictResult.hasConflict)
                        {
                            DataContext.Sessions.Add(session);
                            successCount++;
                        }
                        else
                        {
                            errorCount++;
                        }
                    }
                    catch
                    {
                        errorCount++;
                    }
                }
                break;

            default:
                throw new ArgumentException($"Entity {entity} not supported for CSV import");
        }

        DataContext.SaveAll();
        Console.WriteLine($"CSV import complete. Success: {successCount}, Errors: {errorCount}");
    }

    private static void ExportCsv(string[] args)
    {
        var outPath = ArgsParser.GetValue(args, "--out") ?? throw new ArgumentException("Missing --out");
        var entity = ArgsParser.GetValue(args, "--entity") ?? throw new ArgumentException("Missing --entity");
        var fromStr = ArgsParser.GetValue(args, "--from");
        var toStr = ArgsParser.GetValue(args, "--to");

        DateOnly? from = fromStr != null ? DateOnly.Parse(fromStr) : null;
        DateOnly? to = toStr != null ? DateOnly.Parse(toStr) : null;

        var lines = new List<string>();

        switch (entity.ToLower())
        {
            case "sessions":
                lines.Add("Date,Time,Course,Teacher,Group,Room,Notes");
                
                var sessions = DataContext.Sessions
                    .Where(s => !from.HasValue || s.Date >= from.Value)
                    .Where(s => !to.HasValue || s.Date <= to.Value)
                    .OrderBy(s => s.Date)
                    .ThenBy(s => s.Start)
                    .ToList();

                foreach (var s in sessions)
                {
                    var course = DataContext.Courses.FirstOrDefault(c => c.Id == s.CourseId)?.Title ?? "";
                    var teacher = DataContext.Teachers.FirstOrDefault(t => t.Id == s.TeacherId)?.Name ?? "";
                    var group = DataContext.Groups.FirstOrDefault(g => g.Id == s.GroupId)?.Code ?? "";
                    var room = DataContext.Rooms.FirstOrDefault(r => r.Id == s.RoomId)?.Code ?? "";

                    lines.Add($"{s.Date:yyyy-MM-dd},{s.TimeRange},{EscapeCsv(course)},{EscapeCsv(teacher)},{EscapeCsv(group)},{EscapeCsv(room)},{EscapeCsv(s.Notes)}");
                }
                break;

            case "rooms":
                lines.Add("Code,Capacity,Building");
                foreach (var r in DataContext.Rooms.OrderBy(r => r.Code))
                {
                    lines.Add($"{EscapeCsv(r.Code)},{r.Capacity},{EscapeCsv(r.Building ?? "")}");
                }
                break;

            case "teachers":
                lines.Add("Name,Email");
                foreach (var t in DataContext.Teachers.OrderBy(t => t.Name))
                {
                    lines.Add($"{EscapeCsv(t.Name)},{EscapeCsv(t.Email ?? "")}");
                }
                break;

            case "groups":
                lines.Add("Code,Size,Year");
                foreach (var g in DataContext.Groups.OrderBy(g => g.Code))
                {
                    lines.Add($"{EscapeCsv(g.Code)},{g.Size},{g.Year?.ToString() ?? ""}");
                }
                break;

            case "courses":
                lines.Add("Title,Code,Duration");
                foreach (var c in DataContext.Courses.OrderBy(c => c.Title))
                {
                    lines.Add($"{EscapeCsv(c.Title)},{EscapeCsv(c.Code ?? "")},{c.DurationMinutes}");
                }
                break;

            default:
                throw new ArgumentException($"Unknown entity: {entity}");
        }

        File.WriteAllLines(outPath, lines);
        Console.WriteLine($"Exported {lines.Count - 1} {entity} to {Path.GetFullPath(outPath)}");
    }

    private static void ImportJson(string[] args)
    {
        var filePath = ArgsParser.GetValue(args, "--file") ?? throw new ArgumentException("Missing --file");
        
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var json = File.ReadAllText(filePath);
        var entity = ArgsParser.GetValue(args, "--entity");

        if (string.IsNullOrEmpty(entity))
        {
            BackupCommands.Restore(args);
        }
        else
        {
            ImportPartialJson(entity, json);
        }
    }

    private static void ExportJson(string[] args)
    {
        var outPath = ArgsParser.GetValue(args, "--out") ?? throw new ArgumentException("Missing --out");
        var entity = ArgsParser.GetValue(args, "--entity");

        if (string.IsNullOrEmpty(entity))
        {
            BackupCommands.Backup(args);
        }
        else
        {
            ExportPartialJson(entity, outPath);
        }
    }

    private static void ImportPartialJson(string entity, string json)
    {
        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        switch (entity.ToLower())
        {
            case "sessions":
                var sessions = System.Text.Json.JsonSerializer.Deserialize<List<Session>>(json, options) ?? new();
                foreach (var session in sessions)
                {
                    DataContext.Sessions.Add(session with { Id = DataContext.NextId<Session>() });
                }
                break;

            case "rooms":
                var rooms = System.Text.Json.JsonSerializer.Deserialize<List<Room>>(json, options) ?? new();
                DataContext.Rooms.AddRange(rooms.Select(r => r with { Id = DataContext.NextId<Room>() }));
                break;

            case "teachers":
                var teachers = System.Text.Json.JsonSerializer.Deserialize<List<Teacher>>(json, options) ?? new();
                DataContext.Teachers.AddRange(teachers.Select(t => t with { Id = DataContext.NextId<Teacher>() }));
                break;

            case "groups":
                var groups = System.Text.Json.JsonSerializer.Deserialize<List<Group>>(json, options) ?? new();
                DataContext.Groups.AddRange(groups.Select(g => g with { Id = DataContext.NextId<Group>() }));
                break;

            case "courses":
                var courses = System.Text.Json.JsonSerializer.Deserialize<List<Course>>(json, options) ?? new();
                DataContext.Courses.AddRange(courses.Select(c => c with { Id = DataContext.NextId<Course>() }));
                break;

            default:
                throw new ArgumentException($"Unknown entity: {entity}");
        }

        DataContext.SaveAll();
        Console.WriteLine($"Imported {entity} from JSON");
    }

    private static void ExportPartialJson(string entity, string outPath)
    {
        object data = entity.ToLower() switch
        {
            "sessions" => DataContext.Sessions,
            "rooms" => DataContext.Rooms,
            "teachers" => DataContext.Teachers,
            "groups" => DataContext.Groups,
            "courses" => DataContext.Courses,
            _ => throw new ArgumentException($"Unknown entity: {entity}")
        };

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        var json = System.Text.Json.JsonSerializer.Serialize(data, options);
        File.WriteAllText(outPath, json);

        var count = entity.ToLower() switch
        {
            "sessions" => DataContext.Sessions.Count,
            "rooms" => DataContext.Rooms.Count,
            "teachers" => DataContext.Teachers.Count,
            "groups" => DataContext.Groups.Count,
            "courses" => DataContext.Courses.Count,
            _ => 0
        };

        Console.WriteLine($"Exported {count} {entity} to {Path.GetFullPath(outPath)}");
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = "";

        for (int i = 0; i < line.Length; i++)
        {
            var ch = line[i];

            if (ch == '"' && (i == 0 || line[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
            }
            else if (ch == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += ch;
            }
        }

        result.Add(current);
        return result.Select(s => s.Replace("\"\"", "\"")).ToArray();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private static int GetOrCreateCourseId(string title)
    {
        var course = DataContext.Courses.FirstOrDefault(c => c.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        if (course != null) return course.Id;

        var newCourse = new Course(
            Id: DataContext.NextId<Course>(),
            Title: title,
            Code: "",
            DurationMinutes: 90
        );
        DataContext.Courses.Add(newCourse);
        return newCourse.Id;
    }

    private static int GetOrCreateTeacherId(string name)
    {
        var teacher = DataContext.Teachers.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (teacher != null) return teacher.Id;

        var newTeacher = new Teacher(
            Id: DataContext.NextId<Teacher>(),
            Name: name,
            Email: null
        );
        DataContext.Teachers.Add(newTeacher);
        return newTeacher.Id;
    }

    private static int GetOrCreateGroupId(string code)
    {
        var group = DataContext.Groups.FirstOrDefault(g => g.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (group != null) return group.Id;

        var newGroup = new Group(
            Id: DataContext.NextId<Group>(),
            Code: code,
            Size: 30,
            Year: null
        );
        DataContext.Groups.Add(newGroup);
        return newGroup.Id;
    }

    private static int GetOrCreateRoomId(string code)
    {
        var room = DataContext.Rooms.FirstOrDefault(r => r.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
        if (room != null) return room.Id;

        var newRoom = new Room(
            Id: DataContext.NextId<Room>(),
            Code: code,
            Capacity: 30,
            Building: null
        );
        DataContext.Rooms.Add(newRoom);
        return newRoom.Id;
    }
}