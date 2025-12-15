using System.Text.Json;
using ScheduleSystem.Models;

namespace ScheduleSystem.Storage;

public static class JsonStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void LoadAll()
    {
        DataContext.Rooms = Load<List<Room>>("rooms.json") ?? new();
        DataContext.Teachers = Load<List<Teacher>>("teachers.json") ?? new();
        DataContext.Groups = Load<List<Group>>("groups.json") ?? new();
        DataContext.Courses = Load<List<Course>>("courses.json") ?? new();
        DataContext.Sessions = Load<List<Session>>("sessions.json") ?? new();

    }

    public static void SaveAll()
    {
        Save("rooms.json", DataContext.Rooms);
        Save("teachers.json", DataContext.Teachers);
        Save("groups.json", DataContext.Groups);
        Save("courses.json", DataContext.Courses);
        Save("sessions.json", DataContext.Sessions);
    }

    public static void Backup(string filePath)
    {
        var allData = new
        {
            Rooms    = DataContext.Rooms,
            Teachers = DataContext.Teachers,
            Groups   = DataContext.Groups,
            Courses  = DataContext.Courses,
            Sessions = DataContext.Sessions,
        };

        File.WriteAllText(filePath, JsonSerializer.Serialize(allData, Options));
        Console.WriteLine($"Backup saved to {Path.GetFullPath(filePath)}");
    }

    public static void Restore(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Backup file not found: {filePath}");

        var json = File.ReadAllText(filePath);
        var backup = JsonSerializer.Deserialize<BackupData>(json, Options)
                    ?? throw new InvalidOperationException("Corrupted backup file");

        DataContext.Rooms = backup.Rooms ?? new();
        DataContext.Teachers = backup.Teachers ?? new();
        DataContext.Groups = backup.Groups ?? new();
        DataContext.Courses = backup.Courses ?? new();
        DataContext.Sessions = backup.Sessions ?? new();

        SaveAll();
        Console.WriteLine($"Restored from {Path.GetFullPath(filePath)}");
    }

    private class BackupData
    {
        public List<Room>? Rooms { get; set; }
        public List<Teacher>? Teachers { get; set; }
        public List<Group>? Groups { get; set; }
        public List<Course>? Courses { get; set; }
        public List<Session>? Sessions { get; set; }
    }

    private static void Save<T>(string fileName, T data)
    {
        var path = Path.Combine(DataContext.BasePath, fileName);
        File.WriteAllText(path, JsonSerializer.Serialize(data, Options));
    }

    private static T? Load<T>(string fileName)
    {
        var path = Path.Combine(DataContext.BasePath, fileName);
        return File.Exists(path)
            ? JsonSerializer.Deserialize<T>(File.ReadAllText(path), Options)
            : default;
    }
}