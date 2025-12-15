using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.CommandsTests;

public class GroupCommandsTests : IDisposable
{
    private readonly string _testDataPath;
    
    public GroupCommandsTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }

    [Fact]
    public void AddGroup_ValidData_CreatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "group", "add", "--code", "CS-2025", "--size", "25", "--year", "2025" };
        GroupCommands.Run(args);
        Assert.Single(DataContext.Groups);
        var group = DataContext.Groups[0];
        Assert.Equal("CS-2025", group.Code);
        Assert.Equal(25, group.Size);
        Assert.Equal(2025, group.Year);
    }
    
    [Fact]
    public void AddGroup_WithoutYear_CreatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "group", "add", "--code", "IT-2024", "--size", "30" };
        GroupCommands.Run(args);
        var group = DataContext.Groups[0];
        Assert.Equal("IT-2024", group.Code);
        Assert.Equal(30, group.Size);
        Assert.Null(group.Year);
    }
    
    [Fact]
    public void AddGroup_MissingCode_ThrowsArgumentException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "group", "add", "--size", "30" };
        var exception = Assert.Throws<ArgumentException>(() => GroupCommands.Run(args));
        Assert.Contains("Missing --code", exception.Message);
    }
    
    [Fact]
    public void ListGroups_Empty_ShowsNoGroupsMessage()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "group", "list" };
        GroupCommands.Run(args);
        Assert.Contains("No groups", sw.ToString());
    }
    
    [Fact]
    public void ListGroups_WithData_ShowsAllGroups()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.Groups.Add(new Group(2, "IT-2024", 30, 2024));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "group", "list" };
        GroupCommands.Run(args);        
        var output = sw.ToString();
        Assert.Contains("CS-2025", output);
        Assert.Contains("IT-2024", output);
        Assert.Contains("25", output);
        Assert.Contains("30", output);
        Assert.Contains("2025", output);
    }
    
    [Fact]
    public void ShowGroup_ById_ReturnsCorrectGroup()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.SaveAll();       
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "group", "show", "1" };        
        GroupCommands.Run(args);        
        var output = sw.ToString();
        Assert.Contains("ID: 1", output);
        Assert.Contains("Code: CS-2025", output);
        Assert.Contains("Size: 25", output);
        Assert.Contains("Year: 2025", output);
    }
    
    [Fact]
    public void ShowGroup_ByCode_ReturnsCorrectGroup()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.SaveAll();       
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "group", "show", "CS-2025" };        
        GroupCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("Code: CS-2025", output);
        Assert.Contains("ID: 1", output);
    }
    
    [Fact]
    public void UpdateGroup_ById_UpdatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.SaveAll();
        string[] args = { "group", "update", "1", "--code", "CS-2026", "--size", "28", "--year", "2026" };
        GroupCommands.Run(args);
        var group = DataContext.Groups[0];
        Assert.Equal("CS-2026", group.Code);
        Assert.Equal(28, group.Size);
        Assert.Equal(2026, group.Year);
    }
    
    [Fact]
    public void UpdateGroup_RemoveYear_SetsYearToNull()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.SaveAll();
        string[] args = { "group", "update", "1", "--year", "" };
        GroupCommands.Run(args);
        var group = DataContext.Groups[0];
        Assert.Null(group.Year);
    }
    
    [Fact]
    public void DeleteGroup_ById_RemovesGroup()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Groups.Add(new Group(1, "CS-2025", 25, 2025));
        DataContext.SaveAll();
        string[] args = { "group", "delete", "1" };
        GroupCommands.Run(args);
        Assert.Empty(DataContext.Groups);
    }
    
    [Fact]
    public void DeleteGroup_NonExistent_ThrowsKeyNotFoundException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "group", "delete", "999" };
        Assert.Throws<KeyNotFoundException>(() => GroupCommands.Run(args));
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