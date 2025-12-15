using ScheduleSystem.Storage;

namespace ScheduleSystem.Tests.TestHelpers;

public abstract class TestBase : IDisposable
{
    private static bool _isInitialized = false;
    
    protected TestBase()
    {
        if (!_isInitialized)
        {
            DataContext.Initialize("test-data");
            _isInitialized = true;
        }
        ClearTestData();
    }
    
    protected void ClearTestData()
    {
        DataContext.Rooms = new List<ScheduleSystem.Models.Room>();
        DataContext.Teachers = new List<ScheduleSystem.Models.Teacher>();
        DataContext.Groups = new List<ScheduleSystem.Models.Group>();
        DataContext.Courses = new List<ScheduleSystem.Models.Course>();
        DataContext.Sessions = new List<ScheduleSystem.Models.Session>();
        DataContext.Users = new List<ScheduleSystem.Models.User>();
        
        try
        {
            DataContext.SaveAll();
        }
        catch { }
    }
    
    public void Dispose()
    {
        ClearTestData();
        
        try
        {
            if (Directory.Exists("test-data"))
            {
                Directory.Delete("test-data", true);
            }
        }
        catch { }
    }
}