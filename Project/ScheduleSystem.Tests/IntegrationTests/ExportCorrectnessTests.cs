using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.IntegrationTests;

public class ExportCorrectnessTests : IDisposable
{
    private readonly string _testDataPath;
    
    public ExportCorrectnessTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }
    [Fact]
    public void ExportCsv_Sessions_CorrectHeadersAndData()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I.", "ivanov@univ.edu"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30),
            "Lecture 1 with, comma"));
        DataContext.SaveAll();

        var outputFile = "test_export_headers.csv";
        string[] args = { "export", "csv", "--entity", "sessions", "--out", outputFile };

        ImportExportCommands.Run(args);

        var lines = File.ReadAllLines(outputFile);
        Assert.Equal(2, lines.Length);
        Assert.Equal("Date,Time,Course,Teacher,Group,Room,Notes", lines[0]);
        
        var dataLine = lines[1];
        Assert.Contains("2025-11-27", dataLine);
        Assert.Contains("10:00-11:30", dataLine);
        Assert.Contains("Algorithms", dataLine);
        Assert.Contains("Ivanov I.I.", dataLine);
        Assert.Contains("CS-2025", dataLine);
        Assert.Contains("A-101", dataLine);
        Assert.Contains("Lecture 1 with, comma", dataLine);
        Assert.Contains("\"Lecture 1 with, comma\"", dataLine);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportCsv_Sessions_EmptyData_ExportsOnlyHeaders()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        var outputFile = "test_export_empty.csv";
        string[] args = { "export", "csv", "--entity", "sessions", "--out", outputFile };

        ImportExportCommands.Run(args);

        var lines = File.ReadAllLines(outputFile);
        Assert.Single(lines);
        Assert.Equal("Date,Time,Course,Teacher,Group,Room,Notes", lines[0]);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportCsv_Sessions_WithDateFilter_CorrectRange()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Course(1, "Math"));

        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27), new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.Sessions.Add(new Session(2, 1, 1, 1, 1,
            new DateOnly(2025, 12, 1), new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.Sessions.Add(new Session(3, 1, 1, 1, 1,
            new DateOnly(2025, 12, 5), new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();

        var outputFile = "test_export_filtered.csv";
        string[] args = { 
            "export", "csv", "--entity", "sessions", 
            "--out", outputFile,
            "--from", "2025-12-01",
            "--to", "2025-12-03"
        };

        ImportExportCommands.Run(args);

        var lines = File.ReadAllLines(outputFile);
        Assert.Equal(2, lines.Length);
        Assert.Contains("2025-12-01", lines[1]);
        Assert.DoesNotContain("2025-11-27", string.Join("", lines));
        Assert.DoesNotContain("2025-12-05", string.Join("", lines));

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportCsv_Rooms_CorrectFormat()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main Building"));
        DataContext.Rooms.Add(new Room(2, "B-201", 50, "Secondary"));
        DataContext.SaveAll();

        var outputFile = "test_rooms_export.csv";
        string[] args = { "export", "csv", "--entity", "rooms", "--out", outputFile };

        ImportExportCommands.Run(args);

        var lines = File.ReadAllLines(outputFile);
        Assert.Equal(3, lines.Length);
        Assert.Equal("Code,Capacity,Building,AttributesJson", lines[0]);
        Assert.Contains("A-101", lines[1]);
        Assert.Contains("30", lines[1]);
        Assert.Contains("Main Building", lines[1]);
        Assert.Contains("B-201", lines[2]);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportCsv_Teachers_CorrectFormat()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I.", "ivanov@edu"));
        DataContext.Teachers.Add(new Teacher(2, "Petrov P.P.", null));
        DataContext.SaveAll();

        var outputFile = "test_teachers_export.csv";
        string[] args = { "export", "csv", "--entity", "teachers", "--out", outputFile };

        ImportExportCommands.Run(args);

        var lines = File.ReadAllLines(outputFile);
        Assert.Equal(3, lines.Length);
        Assert.Equal("Name,Email", lines[0]);
        Assert.Contains("Ivanov I.I.", lines[1]);
        Assert.Contains("ivanov@edu", lines[1]);
        Assert.Contains("Petrov P.P.", lines[2]);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportCsv_Groups_CorrectFormat()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.Groups.Add(new Group(2, "IT-2024", 30, null));
        DataContext.SaveAll();

        var outputFile = "test_groups_export.csv";
        string[] args = { "export", "csv", "--entity", "groups", "--out", outputFile };

        ImportExportCommands.Run(args);

        var lines = File.ReadAllLines(outputFile);
        Assert.Equal(3, lines.Length);
        Assert.Equal("Code,Size,Year", lines[0]);
        Assert.Contains("CS-2025", lines[1]);
        Assert.Contains("25", lines[1]);
        Assert.Contains("2025", lines[1]);
        Assert.Contains("IT-2024", lines[2]);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportCsv_Courses_CorrectFormat()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.Courses.Add(new Course(2, "Mathematics", null, 120));
        DataContext.SaveAll();

        var outputFile = "test_courses_export.csv";
        string[] args = { "export", "csv", "--entity", "courses", "--out", outputFile };

        ImportExportCommands.Run(args);

        var lines = File.ReadAllLines(outputFile);
        Assert.Equal(3, lines.Length);
        Assert.Equal("Title,Code,Duration", lines[0]);
        Assert.Contains("Algorithms", lines[1]);
        Assert.Contains("CS101", lines[1]);
        Assert.Contains("90", lines[1]);
        Assert.Contains("Mathematics", lines[2]);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportCsv_Sessions_SpecialCharacters_EscapedCorrectly()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov, I.I.", "ivanov@edu"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Course(1, "Algorithms \"Advanced\"", "CS101", 90));
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30),
            "Lecture with \"quotes\" and, commas"));
        DataContext.SaveAll();

        var outputFile = "test_special_chars.csv";
        string[] args = { "export", "csv", "--entity", "sessions", "--out", outputFile };

        ImportExportCommands.Run(args);

        var content = File.ReadAllText(outputFile);
        Assert.Contains("\"Ivanov, I.I.\"", content);
        Assert.Contains("\"Algorithms \"\"Advanced\"\"\"", content);
        Assert.Contains("\"Lecture with \"\"quotes\"\" and, commas\"", content);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportJson_Sessions_CorrectFormat()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Course(1, "Math"));
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();

        var outputFile = "test_json_export.json";
        string[] args = { "export", "json", "--entity", "sessions", "--out", outputFile };

        ImportExportCommands.Run(args);

        var json = File.ReadAllText(outputFile);
        Assert.Contains("\"Id\": 1", json);
        Assert.Contains("\"CourseId\": 1", json);
        Assert.Contains("\"Date\": \"2025-11-27\"", json);
        Assert.Contains("\"Start\": \"10:00:00\"", json);
        Assert.Contains("\"End\": \"11:30:00\"", json);

        File.Delete(outputFile);
    }

    [Fact]
    public void ExportJson_FullBackup_CorrectStructure()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Course(1, "Math"));
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();

        var outputFile = "test_full_backup.json";
        string[] args = { "export", "json", "--out", outputFile };

        ImportExportCommands.Run(args);

        var json = File.ReadAllText(outputFile);
        Assert.Contains("\"rooms\"", json);
        Assert.Contains("\"teachers\"", json);
        Assert.Contains("\"groups\"", json);
        Assert.Contains("\"courses\"", json);
        Assert.Contains("\"sessions\"", json);

        File.Delete(outputFile);
    }
    public void Dispose()
    {
        try
        {
            Thread.Sleep(50);
            
            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, true);
            }
        }
        catch{}
    }
}