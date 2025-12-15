using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.CommandsTests;

public class SessionCommandsTests : IDisposable
{
    private readonly string _testDataPath;
    
    public SessionCommandsTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }

    private void SetupTestData()
    {
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I.", "ivanov@edu"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.SaveAll();
    }
    
    [Fact]
    public void AddSession_ValidData_CreatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        string[] args = { 
            "session", "add",
            "--course", "1",
            "--teacher", "1",
            "--group", "1",
            "--room", "1",
            "--date", "2025-11-27",
            "--start", "10:00",
            "--end", "11:30",
            "--notes", "Lecture 1"
        };
        SessionCommands.Run(args);
        Assert.Single(DataContext.Sessions);
        var session = DataContext.Sessions[0];
        Assert.Equal(1, session.CourseId);
        Assert.Equal(1, session.TeacherId);
        Assert.Equal(1, session.GroupId);
        Assert.Equal(1, session.RoomId);
        Assert.Equal(new DateOnly(2025, 11, 27), session.Date);
        Assert.Equal(new TimeOnly(10, 0), session.Start);
        Assert.Equal(new TimeOnly(11, 30), session.End);
        Assert.Equal("Lecture 1", session.Notes);
    }
    
    [Fact]
    public void AddSession_WithoutDate_UsesToday()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        string[] args = { 
            "session", "add",
            "--course", "1",
            "--teacher", "1",
            "--group", "1",
            "--room", "1",
            "--start", "10:00",
            "--end", "11:30"
        };
        SessionCommands.Run(args);
        var session = DataContext.Sessions[0];
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), session.Date);
    }
    
    [Fact]
    public void AddSession_InvalidTime_ThrowsArgumentException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        string[] args = { 
            "session", "add",
            "--course", "1",
            "--teacher", "1",
            "--group", "1",
            "--room", "1",
            "--date", "2025-11-27",
            "--start", "12:00",
            "--end", "11:00"
        };
        var exception = Assert.Throws<ArgumentException>(() => SessionCommands.Run(args));
        Assert.Contains("Start time must be before end time", exception.Message);
    }
    
    [Fact]
    public void AddSession_MissingRequiredArgs_ThrowsArgumentException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        string[] args = { 
            "session", "add",
            "--course", "1",
            "--teacher", "1",
            "--group", "1",
            "--room", "1"
        };
        var exception = Assert.Throws<ArgumentException>(() => SessionCommands.Run(args));
        Assert.Contains("Missing --start", exception.Message);
    }
    
    [Fact]
    public void ListSessions_Empty_ShowsNoSessionsMessage()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "session", "list" };
        SessionCommands.Run(args);
        Assert.Contains("No sessions found", sw.ToString());
    }
    
    [Fact]
    public void ListSessions_WithData_ShowsAllSessions()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30),
            "Test"));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "session", "list" };
        SessionCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("2025-11-27", output);
        Assert.Contains("10:00-11:30", output);
        Assert.Contains("Algorithms", output);
        Assert.Contains("Ivanov I.I.", output);
        Assert.Contains("CS-2025", output);
    }
    
    [Fact]
    public void ShowSession_ById_ReturnsCorrectSession()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30),
            "Lecture 1"));
        DataContext.SaveAll();
        
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "session", "show", "1" };
        SessionCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("Session ID: 1", output);
        Assert.Contains("Date: 2025-11-27", output);
        Assert.Contains("Time: 10:00-11:30", output);
        Assert.Contains("Course: Algorithms", output);
        Assert.Contains("Teacher: Ivanov I.I.", output);
        Assert.Contains("Group: CS-2025", output);
        Assert.Contains("Room: A-101", output);
        Assert.Contains("Notes: Lecture 1", output);
    }
    
    [Fact]
    public void UpdateSession_ById_UpdatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30),
            "Old notes"));
        DataContext.SaveAll();
        string[] args = { 
            "session", "update", "1",
            "--date", "2025-11-28",
            "--start", "14:00",
            "--end", "15:30",
            "--notes", "Updated notes"
        };
        SessionCommands.Run(args);
        var session = DataContext.Sessions[0];
        Assert.Equal(new DateOnly(2025, 11, 28), session.Date);
        Assert.Equal(new TimeOnly(14, 0), session.Start);
        Assert.Equal(new TimeOnly(15, 30), session.End);
        Assert.Equal("Updated notes", session.Notes);
    }
    
    [Fact]
    public void DeleteSession_ById_RemovesSession()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        string[] args = { "session", "delete", "1" };
        SessionCommands.Run(args);
        Assert.Empty(DataContext.Sessions);
    }
    
    [Fact]
    public void FindConflicts_NoConflicts_ShowsNoConflictsMessage()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "session", "conflicts" };
        SessionCommands.Run(args);
        Assert.Contains("No conflicts found", sw.ToString());
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