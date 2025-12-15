using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.CommandsTests;

public class RoomCommandsTests : IDisposable
{
    private readonly string _testDataPath;
    
    public RoomCommandsTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }

    [Fact]
    public void AddRoom_ValidData_CreatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "room", "add", "--code", "A-101", "--capacity", "30", "--building", "Main" };
        RoomCommands.Run(args);
        Assert.Single(DataContext.Rooms);
        var room = DataContext.Rooms[0];
        Assert.Equal("A-101", room.Code);
        Assert.Equal(30, room.Capacity);
        Assert.Equal("Main", room.Building);
    }
    
    [Fact]
    public void AddRoom_MissingCode_ThrowsArgumentException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "room", "add", "--capacity", "30" };
        var exception = Assert.Throws<ArgumentException>(() => RoomCommands.Run(args));
        Assert.Contains("Missing --code", exception.Message);
    }
    
    [Fact]
    public void ListRooms_Empty_ShowsNoRoomsMessage()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "room", "list" };
        RoomCommands.Run(args);
        Assert.Contains("No rooms", sw.ToString());
    }
    
    [Fact]
    public void ListRooms_WithData_ShowsAllRooms()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.Rooms.Add(new Room(2, "B-201", 50, "Secondary"));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "room", "list" };
        RoomCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("A-101", output);
        Assert.Contains("B-201", output);
        Assert.Contains("30", output);
        Assert.Contains("50", output);
    }
    
    [Fact]
    public void ShowRoom_ById_ReturnsCorrectRoom()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "room", "show", "1" };
        RoomCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("Room: A-101", output);
        Assert.Contains("Capacity: 30", output);
        Assert.Contains("Building: Main", output);
    }
    
    [Fact]
    public void ShowRoom_ByCode_ReturnsCorrectRoom()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "room", "show", "A-101" };
        RoomCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("Room: A-101", output);
        Assert.Contains("id=1", output);
    }
    
    [Fact]
    public void UpdateRoom_ById_UpdatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.SaveAll();
        string[] args = { "room", "update", "1", "--code", "A-102", "--capacity", "40", "--building", "Updated" };
        RoomCommands.Run(args);
        var room = DataContext.Rooms[0];
        Assert.Equal("A-102", room.Code);
        Assert.Equal(40, room.Capacity);
        Assert.Equal("Updated", room.Building);
    }
    
    [Fact]
    public void UpdateRoom_ByCode_UpdatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.SaveAll();
        string[] args = { "room", "update", "A-101", "--capacity", "35" };
        RoomCommands.Run(args);
        var room = DataContext.Rooms[0];
        Assert.Equal(35, room.Capacity);
        Assert.Equal("Main", room.Building);
    }
    
    [Fact]
    public void DeleteRoom_ById_RemovesRoom()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.SaveAll();
        string[] args = { "room", "delete", "1" };
        RoomCommands.Run(args);
        Assert.Empty(DataContext.Rooms);
    }
    
    [Fact]
    public void DeleteRoom_ByCode_RemovesRoom()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Rooms.Add(new Room(1, "A-101", 30, "Main"));
        DataContext.SaveAll();
        string[] args = { "room", "delete", "A-101" };
        RoomCommands.Run(args);
        Assert.Empty(DataContext.Rooms);
    }
    
    [Fact]
    public void DeleteRoom_NonExistent_ThrowsKeyNotFoundException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "room", "delete", "999" };
        Assert.Throws<KeyNotFoundException>(() => RoomCommands.Run(args));
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