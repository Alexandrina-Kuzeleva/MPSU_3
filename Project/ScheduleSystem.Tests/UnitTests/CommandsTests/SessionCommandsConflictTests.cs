using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.CommandsTests;

public class SessionCommandsConflictTests : IDisposable
{
    private readonly string _testDataPath;
    
    public SessionCommandsConflictTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }

    private void SetupTestData()
    {
        DataContext.Rooms.Add(new Room(1, "A-101", 30));
        DataContext.Rooms.Add(new Room(2, "B-201", 40));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov"));
        DataContext.Teachers.Add(new Teacher(2, "Petrov"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 25));
        DataContext.Groups.Add(new Group(2, "IT-2024", 30));
        DataContext.Courses.Add(new Course(1, "Math"));
        DataContext.Courses.Add(new Course(2, "Physics"));
        DataContext.SaveAll();
    }
    
    [Fact]
    public void AddSession_WithRoomConflict_ThrowsException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        
        string[] args = {
            "session", "add",
            "--course", "2",
            "--teacher", "2",
            "--group", "2",
            "--room", "1",
            "--date", "2025-11-27",
            "--start", "11:00",
            "--end", "12:30"
        };
        
        var exception = Assert.Throws<InvalidOperationException>(() => SessionCommands.Run(args));
        Assert.Contains("Conflict detected", exception.Message);
    }
    
    [Fact]
    public void AddSession_WithTeacherConflict_ThrowsException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        
        string[] args = {
            "session", "add",
            "--course", "2",
            "--teacher", "1",
            "--group", "2",
            "--room", "2",
            "--date", "2025-11-27",
            "--start", "11:00",
            "--end", "12:30"
        };
        
        var exception = Assert.Throws<InvalidOperationException>(() => SessionCommands.Run(args));
        Assert.Contains("Conflict detected", exception.Message);
        Assert.Contains("Teacher", exception.Message);
    }
    
    [Fact]
    public void AddSession_WithGroupConflict_ThrowsException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        
        string[] args = {
            "session", "add",
            "--course", "2",
            "--teacher", "2",
            "--group", "1",
            "--room", "2",
            "--date", "2025-11-27",
            "--start", "11:00",
            "--end", "12:30"
        };
        
        var exception = Assert.Throws<InvalidOperationException>(() => SessionCommands.Run(args));
        Assert.Contains("Conflict detected", exception.Message);
        Assert.Contains("Group", exception.Message);
    }
    
    [Fact]
    public void UpdateSession_CreatesRoomConflict_ThrowsException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        
        DataContext.Sessions.Add(new Session(2, 2, 2, 2, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        
        string[] args = {
            "session", "update", "2",
            "--room", "1"
        };
        
        var exception = Assert.Throws<InvalidOperationException>(() => SessionCommands.Run(args));
        Assert.Contains("Conflict detected", exception.Message);
    }
    
    [Fact]
    public void ListSessions_WithConflictsOnly_ShowsConflictingSessions()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        
        DataContext.Sessions.Add(new Session(2, 2, 1, 2, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(11, 0), new TimeOnly(12, 30)));
        
        DataContext.Sessions.Add(new Session(3, 1, 2, 1, 1, 
            new DateOnly(2025, 11, 28), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        
        using var sw = new StringWriter();
        Console.SetOut(sw);
        
        string[] args = { "session", "list", "--conflicts-only" };
        SessionCommands.Run(args);
        
        var output = sw.ToString();
        Assert.Contains("2025-11-27", output);
        Assert.DoesNotContain("2025-11-28", output);
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