using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Services;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.ServicesTests;

public class ConflictServiceTests : IDisposable
{
    private readonly string _testDataPath;
    
    public ConflictServiceTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }
    private void SetupBasicData()
    {
        DataContext.Rooms.Add(new Room(1, "A-101", 30));
        DataContext.Rooms.Add(new Room(2, "B-201", 40));
        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I."));
        DataContext.Teachers.Add(new Teacher(2, "Petrov P.P."));
        DataContext.Groups.Add(new Group(1, "CS-2025", 25));
        DataContext.Groups.Add(new Group(2, "IT-2024", 30));
        DataContext.Courses.Add(new Course(1, "Algorithms"));
        DataContext.Courses.Add(new Course(2, "Mathematics"));
        DataContext.SaveAll();
    }
    
    [Fact]
    public void Check_RoomConflict_SameRoomOverlapping_ReturnsConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 1, 2, 2, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(11, 0), new TimeOnly(12, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.True(result.hasConflict);
        Assert.Contains("Room A-101 is occupied", result.message);
    }
    
    [Fact]
    public void Check_RoomConflict_SameRoomSameTime_ReturnsConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 2, 2, 2, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.True(result.hasConflict);
        Assert.Contains("Room A-101 is occupied", result.message);
    }
    
    [Fact]
    public void Check_RoomConflict_SameRoomDifferentDate_NoConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 1, 2, 2, 1, 
            new DateOnly(2025, 11, 28), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.False(result.hasConflict);
        Assert.Empty(result.message);
    }
    
    [Fact]
    public void Check_RoomConflict_SameRoomNonOverlapping_NoConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 1, 2, 2, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(12, 0), new TimeOnly(13, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.False(result.hasConflict);
        Assert.Empty(result.message);
    }
    
    [Fact]
    public void Check_TeacherConflict_SameTeacherOverlapping_ReturnsConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 2, 1, 2, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(11, 0), new TimeOnly(12, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.True(result.hasConflict);
        Assert.Contains("Teacher Ivanov I.I. is busy", result.message);
    }
    
    [Fact]
    public void Check_TeacherConflict_SameTeacherDifferentRooms_ReturnsConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 2, 1, 2, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 30), new TimeOnly(12, 0));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.True(result.hasConflict);
        Assert.Contains("Teacher Ivanov I.I. is busy", result.message);
    }
    
    [Fact]
    public void Check_GroupConflict_SameGroupOverlapping_ReturnsConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 2, 2, 1, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(11, 0), new TimeOnly(12, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.True(result.hasConflict);
        Assert.Contains("Group CS-2025 has another class", result.message);
    }
    
    [Fact]
    public void Check_GroupConflict_CheckGroupFalse_NoGroupConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 2, 2, 1, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(11, 0), new TimeOnly(12, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession, checkGroup: false);
        
        Assert.False(result.hasConflict);
    }
    
    [Fact]
    public void Check_MultipleConflicts_RoomAndTeacherConflict_ReturnsFirstConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var existing = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        var newSession = new Session(2, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(11, 0), new TimeOnly(12, 30));
        
        DataContext.Sessions.Add(existing);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(newSession);
        
        Assert.True(result.hasConflict);
        Assert.Contains("Room A-101 is occupied", result.message);
    }
    
    [Fact]
    public void Check_SameSessionId_SkipsSelfCheck_NoConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        var session = new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30));
        
        DataContext.Sessions.Add(session);
        DataContext.SaveAll();
        
        var result = ConflictService.Check(session);
        
        Assert.False(result.hasConflict);
        Assert.Empty(result.message);
    }
    
    [Fact]
    public void FindAllConflicts_MultipleConflicts_ReturnsAll()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        
        DataContext.Sessions.Add(new Session(2, 2, 1, 2, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(11, 0), new TimeOnly(12, 30)));
        
        DataContext.Sessions.Add(new Session(3, 1, 2, 1, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 30), new TimeOnly(12, 0)));
        
        DataContext.SaveAll();
        
        var conflicts = ConflictService.FindAllConflicts();
        
        Assert.NotEmpty(conflicts);
        Assert.Contains("ROOM CONFLICT", string.Join(" ", conflicts));
        Assert.Contains("TEACHER CONFLICT", string.Join(" ", conflicts));
        Assert.Contains("GROUP CONFLICT", string.Join(" ", conflicts));
    }
    
    [Fact]
    public void FindAllConflicts_NoConflicts_ReturnsEmpty()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
        DataContext.Sessions.Add(new Session(1, 1, 1, 1, 1, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(10, 0), new TimeOnly(11, 30)));
        
        DataContext.Sessions.Add(new Session(2, 2, 2, 2, 2, 
            new DateOnly(2025, 11, 27), 
            new TimeOnly(12, 0), new TimeOnly(13, 30)));
        
        DataContext.SaveAll();
        
        var conflicts = ConflictService.FindAllConflicts();
        
        Assert.Empty(conflicts);
    }
    
    [Fact]
    public void SessionCommands_AddWithRoomConflict_ThrowsException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
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
        Assert.Contains("Room A-101 is occupied", exception.Message);
    }
    
    [Fact]
    public void SessionCommands_AddWithForceFlag_AddsDespiteConflict()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        SetupBasicData();
        
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
            "--end", "12:30",
            "--force"
        };
        
        using var sw = new StringWriter();
        Console.SetOut(sw);
        
        SessionCommands.Run(args);
        
        Assert.Equal(2, DataContext.Sessions.Count);
        var output = sw.ToString();
        Assert.Contains("Session(s) created", output);
        Assert.Contains("Warning", output);
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