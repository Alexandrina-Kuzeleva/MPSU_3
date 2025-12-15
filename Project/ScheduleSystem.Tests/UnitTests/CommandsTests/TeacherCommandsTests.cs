using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.CommandsTests;

public class TeacherCommandsTests : IDisposable
{
    private readonly string _testDataPath;
    
    public TeacherCommandsTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }

    [Fact]
    public void AddTeacher_ValidData_CreatesSuccessfully()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "teacher", "add", "--name", "Ivanov I.I.", "--email", "ivanov@university.edu" };
        TeacherCommands.Run(args);
        Assert.Single(DataContext.Teachers);
        var teacher = DataContext.Teachers[0];
        Assert.Equal("Ivanov I.I.", teacher.Name);
        Assert.Equal("ivanov@university.edu", teacher.Email);
    }
    
    [Fact]
    public void AddTeacher_WithoutEmail_CreatesSuccessfully()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "teacher", "add", "--name", "Petrov P.P." };
        TeacherCommands.Run(args);
        var teacher = DataContext.Teachers[0];
        Assert.Equal("Petrov P.P.", teacher.Name);
        Assert.Null(teacher.Email);
    }
    
    [Fact]
    public void AddTeacher_MissingName_ThrowsArgumentException()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "teacher", "add", "--email", "test@test.com" };
        var exception = Assert.Throws<ArgumentException>(() => TeacherCommands.Run(args));
        Assert.Contains("Missing --name", exception.Message);
    }
    
    [Fact]
    public void ListTeachers_Empty_ShowsNoTeachersMessage()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "teacher", "list" };
        TeacherCommands.Run(args);
        Assert.Contains("No teachers", sw.ToString());
    }
    
    [Fact]
    public void ListTeachers_WithData_ShowsAllTeachers()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I.", "ivanov@edu"));
        DataContext.Teachers.Add(new Teacher(2, "Petrov P.P.", "petrov@edu"));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "teacher", "list" };
        TeacherCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("Ivanov I.I.", output);
        Assert.Contains("Petrov P.P.", output);
        Assert.Contains("ivanov@edu", output);
    }
    
    [Fact]
    public void ShowTeacher_ById_ReturnsCorrectTeacher()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I.", "ivanov@edu"));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "teacher", "show", "1" };
        TeacherCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("ID: 1", output);
        Assert.Contains("Name: Ivanov I.I.", output);
        Assert.Contains("Email: ivanov@edu", output);
    }
    
    [Fact]
    public void UpdateTeacher_ById_UpdatesSuccessfully()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I.", "old@email.com"));
        DataContext.SaveAll();
        string[] args = { "teacher", "update", "1", "--name", "Ivanov Ivan", "--email", "new@email.com" };
        TeacherCommands.Run(args);
        var teacher = DataContext.Teachers[0];
        Assert.Equal("Ivanov Ivan", teacher.Name);
        Assert.Equal("new@email.com", teacher.Email);
    }
    
    [Fact]
    public void DeleteTeacher_ById_RemovesTeacher()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Teachers.Add(new Teacher(1, "Ivanov I.I.", "ivanov@edu"));
        DataContext.SaveAll();
        string[] args = { "teacher", "delete", "1" };
        TeacherCommands.Run(args);
        Assert.Empty(DataContext.Teachers);
    }
    
    [Fact]
    public void DeleteTeacher_NonExistent_ThrowsKeyNotFoundException()
    {
            if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "teacher", "delete", "999" };
        Assert.Throws<KeyNotFoundException>(() => TeacherCommands.Run(args));
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