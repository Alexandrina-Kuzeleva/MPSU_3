using ScheduleSystem.Commands;
using ScheduleSystem.Models;
using ScheduleSystem.Storage;
using ScheduleSystem.Tests.TestHelpers;
using Xunit;

namespace ScheduleSystem.Tests.UnitTests.CommandsTests;

public class CourseCommandsTests : IDisposable
{
    private readonly string _testDataPath;
    
    public CourseCommandsTests()
    {
        _testDataPath = Path.Combine(Path.GetTempPath(), $"ScheduleSystemTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDataPath);
        
        DataContext.Initialize(_testDataPath);
    }
    
    [Fact]
    public void AddCourse_ValidData_CreatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "course", "add", "--title", "Algorithms", "--code", "CS101", "--duration", "90" };
        CourseCommands.Run(args);
        Assert.Single(DataContext.Courses);
        var course = DataContext.Courses[0];
        Assert.Equal("Algorithms", course.Title);
        Assert.Equal("CS101", course.Code);
        Assert.Equal(90, course.DurationMinutes);
    }
    
    [Fact]
    public void AddCourse_WithoutCode_CreatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "course", "add", "--title", "Mathematics", "--duration", "120" };
        CourseCommands.Run(args);
        var course = DataContext.Courses[0];
        Assert.Equal("Mathematics", course.Title);
        Assert.Null(course.Code);
        Assert.Equal(120, course.DurationMinutes);
    }
    
    [Fact]
    public void AddCourse_DefaultDuration_Uses90Minutes()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "course", "add", "--title", "Physics" };
        CourseCommands.Run(args);
        var course = DataContext.Courses[0];
        Assert.Equal(90, course.DurationMinutes);
    }
    
    [Fact]
    public void AddCourse_MissingTitle_ThrowsArgumentException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "course", "add", "--code", "CS101" };
        var exception = Assert.Throws<ArgumentException>(() => CourseCommands.Run(args));
        Assert.Contains("Missing --title", exception.Message);
    }
    
    [Fact]
    public void ListCourses_Empty_ShowsNoCoursesMessage()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "course", "list" };
        CourseCommands.Run(args);
        Assert.Contains("No courses", sw.ToString());
    }
    
    [Fact]
    public void ListCourses_WithData_ShowsAllCourses()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.Courses.Add(new Course(2, "Mathematics", "MATH101", 120));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "course", "list" };
        CourseCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("Algorithms", output);
        Assert.Contains("Mathematics", output);
        Assert.Contains("CS101", output);
        Assert.Contains("90", output);
        Assert.Contains("120", output);
    }
    
    [Fact]
    public void ShowCourse_ById_ReturnsCorrectCourse()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.SaveAll();     
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "course", "show", "1" };
        CourseCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("ID: 1", output);
        Assert.Contains("Title: Algorithms", output);
        Assert.Contains("Code: CS101", output);
        Assert.Contains("Duration: 90", output);
    }
    
    [Fact]
    public void ShowCourse_ByCode_ReturnsCorrectCourse()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.SaveAll();
        using var sw = new StringWriter();
        Console.SetOut(sw);
        string[] args = { "course", "show", "CS101" };
        CourseCommands.Run(args);
        var output = sw.ToString();
        Assert.Contains("Algorithms", output);
        Assert.Contains("Code: CS101", output);
    }
    
    [Fact]
    public void UpdateCourse_ById_UpdatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.SaveAll();
        string[] args = { "course", "update", "1", "--title", "Advanced Algorithms", "--code", "CS201", "--duration", "120" };
        CourseCommands.Run(args);
        var course = DataContext.Courses[0];
        Assert.Equal("Advanced Algorithms", course.Title);
        Assert.Equal("CS201", course.Code);
        Assert.Equal(120, course.DurationMinutes);
    }
    
    [Fact]
    public void UpdateCourse_ByCode_UpdatesSuccessfully()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.SaveAll();
        string[] args = { "course", "update", "CS101", "--duration", "180" };
        CourseCommands.Run(args);
        var course = DataContext.Courses[0];
        Assert.Equal(180, course.DurationMinutes);
    }
    
    [Fact]
    public void DeleteCourse_ById_RemovesCourse()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.SaveAll();
        string[] args = { "course", "delete", "1" };
        CourseCommands.Run(args);
        Assert.Empty(DataContext.Courses);
    }
    
    [Fact]
    public void DeleteCourse_ByCode_RemovesCourse()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        DataContext.Courses.Add(new Course(1, "Algorithms", "CS101", 90));
        DataContext.SaveAll();
        string[] args = { "course", "delete", "CS101" };
        CourseCommands.Run(args);
        Assert.Empty(DataContext.Courses);
    }
    
    [Fact]
    public void DeleteCourse_NonExistent_ThrowsKeyNotFoundException()
    {
        if (Console.Out == null || Console.Out.GetType().GetProperty("BaseStream")?.GetValue(Console.Out) == null)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        string[] args = { "course", "delete", "999" };
        Assert.Throws<KeyNotFoundException>(() => CourseCommands.Run(args));
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