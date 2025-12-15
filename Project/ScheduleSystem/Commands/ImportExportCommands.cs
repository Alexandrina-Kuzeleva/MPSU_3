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
        var force = ArgsParser.HasFlag(args, "--force");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var lines = File.ReadAllLines(filePath);

        int success = 0;
        int errors = 0;

        if (lines.Length < 2)
        {
            Console.WriteLine("CSV import complete. Success: 0, Errors: 0");
            return;
        }

        var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

        switch (entity.ToLower())
        {
            case "sessions":
                if (mode == "replace") DataContext.Sessions.Clear();

                foreach (var line in dataLines)
                {
                    try
                    {
                        var v = ParseCsvLine(line);
                        if (v.Length < 6) throw new Exception("Not enough columns");
                        if (string.IsNullOrWhiteSpace(v[2])) throw new Exception();
                        if (string.IsNullOrWhiteSpace(v[3])) throw new Exception();
                        if (string.IsNullOrWhiteSpace(v[4])) throw new Exception();
                        if (string.IsNullOrWhiteSpace(v[5])) throw new Exception();

                        var date = DateOnly.Parse(v[0]);
                        var time = v[1].Split('-');
                        if (time.Length != 2) throw new Exception("Invalid time format");

                        var session = new Session(
                            Id: DataContext.NextId<Session>(),
                            CourseId: GetOrCreateCourseId(v[2]),
                            TeacherId: GetOrCreateTeacherId(v[3]),
                            GroupId: GetOrCreateGroupId(v[4]),
                            RoomId: GetOrCreateRoomId(v[5]),
                            Date: date,
                            Start: TimeOnly.Parse(time[0]),
                            End: TimeOnly.Parse(time[1]),
                            Notes: v.Length > 6 ? v[6] : ""
                        );

                        var conflict = ConflictService.Check(session);

                        if (!conflict.hasConflict || force)
                        {
                            if (conflict.hasConflict && force)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Warning: {conflict.message}");
                                Console.ResetColor();
                            }

                            DataContext.Sessions.Add(session);
                            success++;
                        }
                        else
                        {
                            errors++;
                        }
                    }
                    catch
                    {
                        errors++;
                    }
                }
                break;

            case "rooms":
                if (mode == "replace") DataContext.Rooms.Clear();

                foreach (var line in dataLines)
                {
                    try
                    {
                        var v = ParseCsvLine(line);
                        if (string.IsNullOrWhiteSpace(v[0])) throw new Exception();
                        if (!int.TryParse(v[1], out int cap) || cap <= 0) throw new Exception();
                        if (DataContext.Rooms.Any(r =>
                            r.Code.Equals(v[0], StringComparison.OrdinalIgnoreCase)))
                            throw new Exception();

                        DataContext.Rooms.Add(new Room(
                            DataContext.NextId<Room>(),
                            v[0],
                            cap,
                            v.Length > 2 ? v[2] : null
                        ));

                        success++;
                    }
                    catch
                    {
                        errors++;
                    }
                }
                break;

            case "teachers":
                if (mode == "replace") DataContext.Teachers.Clear();

                foreach (var line in dataLines)
                {
                    try
                    {
                        var v = ParseCsvLine(line);
                        if (string.IsNullOrWhiteSpace(v[0])) throw new Exception();

                        if (v.Length > 1 && !string.IsNullOrEmpty(v[1]) && !v[1].Contains("@"))
                            throw new Exception();

                        DataContext.Teachers.Add(new Teacher(
                            DataContext.NextId<Teacher>(),
                            v[0],
                            v.Length > 1 ? v[1] : null
                        ));

                        success++;
                    }
                    catch
                    {
                        errors++;
                    }
                }
                break;

            case "groups":
                if (mode == "replace") DataContext.Groups.Clear();

                foreach (var line in dataLines)
                {
                    try
                    {
                        var v = ParseCsvLine(line);
                        if (string.IsNullOrWhiteSpace(v[0])) throw new Exception();
                        if (!int.TryParse(v[1], out int size) || size <= 0) throw new Exception();
                        if (!int.TryParse(v[2], out int year)) throw new Exception();

                        DataContext.Groups.Add(new Group(
                            DataContext.NextId<Group>(),
                            v[0],
                            size,
                            year
                        ));

                        success++;
                    }
                    catch
                    {
                        errors++;
                    }
                }
                break;

            case "courses":
                if (mode == "replace") DataContext.Courses.Clear();

                foreach (var line in dataLines)
                {
                    try
                    {
                        var v = ParseCsvLine(line);
                        if (string.IsNullOrWhiteSpace(v[0])) throw  new Exception();
                        if (string.IsNullOrWhiteSpace(v[1])) throw  new Exception();
                        if (!int.TryParse(v[2], out int dur)) throw  new Exception();

                        DataContext.Courses.Add(new Course(
                            DataContext.NextId<Course>(),
                            v[0],
                            v.Length > 1 ? v[1] : null,
                            dur
                        ));

                        success++;
                    }
                    catch
                    {
                        errors++;
                    }
                }
                break;

            default:
                throw new ArgumentException($"Entity {entity} not supported");
        }

        DataContext.SaveAll();
        Console.WriteLine($"CSV import complete. Success: {success}, Errors: {errors}");
    }

    private static void ExportCsv(string[] args)
    {
        var outPath = ArgsParser.GetValue(args, "--out") ?? throw new ArgumentException("Missing --out");
        var entity = ArgsParser.GetValue(args, "--entity") ?? throw new ArgumentException("Missing --entity");
        var fromStr = ArgsParser.GetValue(args, "--from");
        var toStr = ArgsParser.GetValue(args, "--to");
        var building = ArgsParser.GetValue(args, "--building");
        var minCapacity = ArgsParser.GetInt(args, "--min-capacity");
        var maxCapacity = ArgsParser.GetInt(args, "--max-capacity");
        var nameLike = ArgsParser.GetValue(args, "--name-like");
        var year = ArgsParser.GetInt(args, "--year");
        var titleLike = ArgsParser.GetValue(args, "--title-like");
        var limit = ArgsParser.GetInt(args, "--limit");

        DateOnly? from = fromStr != null ? DateOnly.Parse(fromStr) : null;
        DateOnly? to = toStr != null ? DateOnly.Parse(toStr) : null;

        var lines = new List<string>();

        switch (entity.ToLower())
        {
            case "sessions":
                lines.Add("Date,Time,Course,Teacher,Group,Room,Notes");
                
                var sessionsQuery = DataContext.Sessions.AsQueryable();
                
                if (from.HasValue) sessionsQuery = sessionsQuery.Where(s => s.Date >= from.Value);
                if (to.HasValue) sessionsQuery = sessionsQuery.Where(s => s.Date <= to.Value);
                
                var sessions = sessionsQuery
                    .OrderBy(s => s.Date)
                    .ThenBy(s => s.Start)
                    .ToList();

                if (limit.HasValue)
                    sessions = sessions.Take(limit.Value).ToList();

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
                lines.Add("Code,Capacity,Building,AttributesJson");
                
                var roomsQuery = DataContext.Rooms.AsQueryable();
                
                if (!string.IsNullOrEmpty(building))
                    roomsQuery = roomsQuery.Where(r => r.Building != null && r.Building.Contains(building, StringComparison.OrdinalIgnoreCase));
                if (minCapacity.HasValue)
                    roomsQuery = roomsQuery.Where(r => r.Capacity >= minCapacity.Value);
                if (maxCapacity.HasValue)
                    roomsQuery = roomsQuery.Where(r => r.Capacity <= maxCapacity.Value);
                
                var rooms = roomsQuery
                    .OrderBy(r => r.Code)
                    .ToList();

                if (limit.HasValue)
                    rooms = rooms.Take(limit.Value).ToList();

                foreach (var r in rooms)
                {
                    lines.Add($"{EscapeCsv(r.Code)},{r.Capacity},{EscapeCsv(r.Building ?? "")},{EscapeCsv(r.AttributesJson ?? "")}");
                }
                break;

            case "teachers":
                lines.Add("Name,Email");
                
                var teachersQuery = DataContext.Teachers.AsQueryable();
                
                if (!string.IsNullOrEmpty(nameLike))
                    teachersQuery = teachersQuery.Where(t => t.Name.Contains(nameLike, StringComparison.OrdinalIgnoreCase));
                
                var teachers = teachersQuery
                    .OrderBy(t => t.Name)
                    .ToList();

                if (limit.HasValue)
                    teachers = teachers.Take(limit.Value).ToList();

                foreach (var t in teachers)
                {
                    lines.Add($"{EscapeCsv(t.Name)},{EscapeCsv(t.Email ?? "")}");
                }
                break;

            case "groups":
                lines.Add("Code,Size,Year");
                
                var groupsQuery = DataContext.Groups.AsQueryable();
                
                if (year.HasValue)
                    groupsQuery = groupsQuery.Where(g => g.Year == year);
                
                var groups = groupsQuery
                    .OrderBy(g => g.Code)
                    .ToList();

                if (limit.HasValue)
                    groups = groups.Take(limit.Value).ToList();

                foreach (var g in groups)
                {
                    lines.Add($"{EscapeCsv(g.Code)},{g.Size},{g.Year?.ToString() ?? ""}");
                }
                break;

            case "courses":
                lines.Add("Title,Code,Duration");
                
                var coursesQuery = DataContext.Courses.AsQueryable();
                
                if (!string.IsNullOrEmpty(titleLike))
                    coursesQuery = coursesQuery.Where(c => c.Title.Contains(titleLike, StringComparison.OrdinalIgnoreCase));
                
                var courses = coursesQuery
                    .OrderBy(c => c.Title)
                    .ToList();

                if (limit.HasValue)
                    courses = courses.Take(limit.Value).ToList();

                foreach (var c in courses)
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