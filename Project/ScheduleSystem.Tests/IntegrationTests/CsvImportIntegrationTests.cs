using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.IntegrationTests;

public class CsvImportIntegrationTests : IDisposable
{
    private readonly string _testDataPath;
    
    public CsvImportIntegrationTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }

    [Fact]
    public void ImportCsv_Sessions_MixedValidInvalid_ReportsCorrectly()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var csvContent = @"Date,Time,Course,Teacher,Group,Room,Notes
2025-11-27,10:00-11:30,Mathematics,Ivanov I.I.,CS-2025,A-101,Lecture 1
2025-11-27,INVALID_TIME,Physics,Petrov P.P.,IT-2024,B-201,Bad time format
2025-11-27,13:00-14:30,,Sidorov S.S.,AI-2026,C-301,Missing course
2025-11-28,09:00-10:30,Chemistry,Kuznetsov K.K.,BIO-2025,D-401,Lab session
2025-11-28,14:00-15:30,Biology,Smirnov S.S.,BIO-2025,D-401,Another valid";

        var filePath = "test_mixed.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "sessions", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 3", output);
        Assert.Contains("Errors: 2", output);
        Assert.Equal(3, DataContext.Sessions.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Sessions_EmptyFile_DoesNothing()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        var csvContent = @"Date,Time,Course,Teacher,Group,Room,Notes";
        
        var filePath = "test_empty.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "sessions", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 0", output);
        Assert.Contains("Errors: 0", output);
        Assert.Empty(DataContext.Sessions);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Sessions_InvalidDateFormat_FailsGracefully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        var csvContent = @"Date,Time,Course,Teacher,Group,Room,Notes
INVALID_DATE,10:00-11:30,Math,Ivanov,CS-2025,A-101,Bad date
2025-11-27,10:00-11:30,Math,Ivanov,CS-2025,A-101,Good date";

        var filePath = "test_bad_date.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "sessions", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 1", output);
        Assert.Contains("Errors: 1", output);
        Assert.Single(DataContext.Sessions);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Sessions_WithConflicts_ReportsConflicts()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Models.Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Models.Teacher(1, "Ivanov"));
        DataContext.Groups.Add(new Models.Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Models.Course(1, "Math"));
        DataContext.Sessions.Add(new Models.Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();

        var csvContent = @"Date,Time,Course,Teacher,Group,Room,Notes
2025-11-27,11:00-12:30,Math,Ivanov,CS-2025,A-101,Conflict with existing
2025-11-27,13:00-14:30,Math,Ivanov,CS-2025,B-201,No conflict (different room)
2025-11-27,11:00-12:30,Math,Petrov,CS-2025,A-101,Another conflict";

        var filePath = "test_conflicts.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "sessions", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 1", output);
        Assert.Contains("Errors: 2", output);
        Assert.Equal(2, DataContext.Sessions.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Sessions_WithForceFlag_IgnoresConflicts()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Models.Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Models.Teacher(1, "Ivanov"));
        DataContext.Groups.Add(new Models.Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Models.Course(1, "Math"));
        DataContext.Sessions.Add(new Models.Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();

        var csvContent = @"Date,Time,Course,Teacher,Group,Room,Notes
2025-11-27,11:00-12:30,Math,Ivanov,CS-2025,A-101,Conflict 1
2025-11-27,13:00-14:30,Math,Ivanov,CS-2025,A-101,Conflict 2";

        var filePath = "test_force.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "sessions", "--file", filePath, "--force" };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 2", output);
        Assert.Contains("Errors: 0", output);
        Assert.Contains("Warning", output);
        Assert.Equal(3, DataContext.Sessions.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Sessions_ReplaceMode_ClearsExisting()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Models.Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Models.Teacher(1, "Ivanov"));
        DataContext.Groups.Add(new Models.Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Models.Course(1, "Math"));
        DataContext.Sessions.Add(new Models.Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();

        var csvContent = @"Date,Time,Course,Teacher,Group,Room,Notes
2025-11-28,10:00-11:30,Math,Ivanov,CS-2025,A-101,New session";

        var filePath = "test_replace.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "sessions", "--file", filePath, "--mode", "replace" };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 1", output);
        Assert.Single(DataContext.Sessions);
        Assert.Equal(new DateOnly(2025, 11, 28), DataContext.Sessions[0].Date);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Rooms_MixedValidInvalid_ReportsCorrectly()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        var csvContent = @"Code,Capacity,Building
A-101,30,Main Building
B-201,INVALID,Secondary
,50,Tertiary
C-301,40,Library";

        var filePath = "test_rooms_mixed.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "rooms", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 2", output);
        Assert.Contains("Errors: 2", output);
        Assert.Equal(2, DataContext.Rooms.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Teachers_MixedValidInvalid_ReportsCorrectly()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        var csvContent = @"Name,Email
Ivanov I.I.,ivanov@university.edu
Petrova A.V.,invalid-email
,Sidorov S.S.
Kuznetsov K.K.,kuznetsov@university.edu";

        var filePath = "test_teachers_mixed.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "teachers", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 2", output);
        Assert.Contains("Errors: 2", output);
        Assert.Equal(2, DataContext.Teachers.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Groups_MixedValidInvalid_ReportsCorrectly()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        var csvContent = @"Code,Size,Year
CS-2025,25,2025
IT-2024,INVALID,2024
AI-2026,20,INVALID_YEAR
,30,2023
BIO-2025,22,2025";

        var filePath = "test_groups_mixed.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "groups", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 2", output);
        Assert.Contains("Errors: 3", output);
        Assert.Equal(2, DataContext.Groups.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_Courses_MixedValidInvalid_ReportsCorrectly()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        var csvContent = @"Title,Code,Duration
Algorithms,CS101,90
Mathematics,MATH101,INVALID
Physics,,120
,PHYS101,90
Database Systems,CS201,120";

        var filePath = "test_courses_mixed.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "courses", "--file", filePath };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 2", output);
        Assert.Contains("Errors: 3", output);
        Assert.Equal(2, DataContext.Courses.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_AllEntities_ComplexScenario_HandlesCorrectly()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        DataContext.Rooms.Add(new Models.Room(1, "EXISTING", 30));
        DataContext.SaveAll();

        var csvContent = @"Code,Capacity,Building
A-101,30,Main
B-201,INVALID,Secondary
C-301,40,Library
EXISTING,50,Duplicate";

        var filePath = "test_complex.csv";
        File.WriteAllText(filePath, csvContent);

        string[] args = { "import", "csv", "--entity", "rooms", "--file", filePath, "--mode", "append" };

        using var sw = new StringWriter();
        Console.SetOut(sw);

        ImportExportCommands.Run(args);

        var output = sw.ToString();
        Assert.Contains("Success: 2", output);
        Assert.Contains("Errors: 2", output);
        Assert.Equal(3, DataContext.Rooms.Count);

        File.Delete(filePath);
    }

    [Fact]
    public void ImportCsv_FileNotFound_ThrowsFileNotFoundException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        string[] args = { "import", "csv", "--entity", "sessions", "--file", "nonexistent.csv" };

        var exception = Assert.Throws<FileNotFoundException>(() => ImportExportCommands.Run(args));
        Assert.Contains("File not found", exception.Message);
    }

    [Fact]
    public void ImportCsv_MissingEntity_ThrowsArgumentException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

        string[] args = { "import", "csv", "--file", "test.csv" };

        var exception = Assert.Throws<ArgumentException>(() => ImportExportCommands.Run(args));
        Assert.Contains("Missing --entity", exception.Message);
    }

    [Fact]
    public void ImportCsv_UnknownEntity_ThrowsArgumentException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        
        var filePath = "test.csv";
        File.WriteAllText(filePath, "Header\nData");
        
        string[] args = { "import", "csv", "--entity", "unknown", "--file", filePath };

        var exception = Assert.Throws<ArgumentException>(() => ImportExportCommands.Run(args));
        Assert.Contains("not supported", exception.Message);

        File.Delete(filePath);
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