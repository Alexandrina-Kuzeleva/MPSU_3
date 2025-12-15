using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Services;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.ServicesTests;

public class SessionTimeValidationTests : IDisposable
{
    private readonly string _testDataPath;
    
    public SessionTimeValidationTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }

    private void SetupTestData()
    {
        DataContext.Rooms.Add(new Room(1, "A-101", 30));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov"));
        DataContext.Groups.Add(new Group(1, "CS-2025", 30));
        DataContext.Courses.Add(new Course(1, "Math"));
        DataContext.SaveAll();
    }
    
    [Fact]
    public void Session_StartBeforeEnd_IsValid()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session = new Session(
            Id: 1,
            CourseId: 1,
            TeacherId: 1,
            GroupId: 1,
            RoomId: 1,
            Date: new DateOnly(2025, 11, 27),
            Start: new TimeOnly(10, 0),
            End: new TimeOnly(11, 30),
            Notes: ""
        );
        
        Assert.True(session.Start < session.End);
    }
    
    [Fact]
    public void Session_StartEqualsEnd_IsInvalid()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session = new Session(
            Id: 1,
            CourseId: 1,
            TeacherId: 1,
            GroupId: 1,
            RoomId: 1,
            Date: new DateOnly(2025, 11, 27),
            Start: new TimeOnly(10, 0),
            End: new TimeOnly(10, 0),
            Notes: ""
        );
        
        Assert.False(session.Start < session.End);
    }
    
    [Fact]
    public void Session_StartAfterEnd_IsInvalid()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session = new Session(
            Id: 1,
            CourseId: 1,
            TeacherId: 1,
            GroupId: 1,
            RoomId: 1,
            Date: new DateOnly(2025, 11, 27),
            Start: new TimeOnly(12, 0),
            End: new TimeOnly(11, 0),
            Notes: ""
        );
        
        Assert.False(session.Start < session.End);
    }
    
    [Fact]
    public void Session_OneMinuteDifference_IsValid()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session = new Session(
            Id: 1,
            CourseId: 1,
            TeacherId: 1,
            GroupId: 1,
            RoomId: 1,
            Date: new DateOnly(2025, 11, 27),
            Start: new TimeOnly(10, 0),
            End: new TimeOnly(10, 1),
            Notes: ""
        );
        
        Assert.True(session.Start < session.End);
    }
    
    [Fact]
    public void SessionCommands_AddWithEqualTimes_ThrowsException()
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
            "--end", "10:00"
        };
        
        var exception = Assert.Throws<ArgumentException>(() => SessionCommands.Run(args));
        Assert.Contains("Start time must be before end time", exception.Message);
    }
    
    [Fact]
    public void SessionCommands_AddWithStartAfterEnd_ThrowsException()
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
            "--start", "14:00",
            "--end", "13:00"
        };
        
        var exception = Assert.Throws<ArgumentException>(() => SessionCommands.Run(args));
        Assert.Contains("Start time must be before end time", exception.Message);
    }
    
    [Fact]
    public void SessionCommands_AddWithValidTime_Succeeds()
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
            "--end", "11:30"
        };
        
        SessionCommands.Run(args);
        
        Assert.Single(DataContext.Sessions);
        var session = DataContext.Sessions[0];
        Assert.Equal(new TimeOnly(10, 0), session.Start);
        Assert.Equal(new TimeOnly(11, 30), session.End);
    }
    
    [Fact]
    public void SessionCommands_UpdateWithInvalidTime_ThrowsException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        
        string[] args = {
            "session", "update", "1",
            "--start", "12:00",
            "--end", "11:00"
        };
        
        var exception = Assert.Throws<ArgumentException>(() => SessionCommands.Run(args));
        Assert.Contains("Start time must be before end time", exception.Message);
    }
    
    [Fact]
    public void SessionCommands_UpdateWithValidTime_Succeeds()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        DataContext.Sessions.Add(new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        DataContext.SaveAll();
        
        string[] args = {
            "session", "update", "1",
            "--start", "14:00",
            "--end", "15:30"
        };
        
        SessionCommands.Run(args);
        
        var session = DataContext.Sessions[0];
        Assert.Equal(new TimeOnly(14, 0), session.Start);
        Assert.Equal(new TimeOnly(15, 30), session.End);
    }
    
    [Fact]
    public void RecurrenceService_GenerateWithInvalidTime_ThrowsException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupTestData();
        
        Assert.Throws<ArgumentException>(() =>
            RecurrenceService.GenerateRecurring(
                1, 1, 1, 1,
                new TimeOnly(12, 0), new TimeOnly(11, 0),
                DayOfWeek.Monday,
                new DateOnly(2025, 11, 1),
                new DateOnly(2025, 11, 30)
            ));
    }
    
    [Fact]
    public void Session_OverlapsWith_SameTime_ReturnsTrue()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session1 = new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var session2 = new Session(
            2, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        Assert.True(session1.OverlapsWith(session2));
    }
    
    [Fact]
    public void Session_OverlapsWith_PartialOverlap_ReturnsTrue()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session1 = new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var session2 = new Session(
            2, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(11, 0), new TimeOnly(12, 30));
        
        Assert.True(session1.OverlapsWith(session2));
    }
    
    [Fact]
    public void Session_OverlapsWith_NoOverlap_ReturnsFalse()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session1 = new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var session2 = new Session(
            2, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(12, 0), new TimeOnly(13, 30));
        
        Assert.False(session1.OverlapsWith(session2));
    }
    
    [Fact]
    public void Session_OverlapsWith_DifferentDate_ReturnsFalse()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        var session1 = new Session(
            1, 1, 1, 1, 1,
            new DateOnly(2025, 11, 27),
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var session2 = new Session(
            2, 1, 1, 1, 1,
            new DateOnly(2025, 11, 28),
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        Assert.False(session1.OverlapsWith(session2));
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