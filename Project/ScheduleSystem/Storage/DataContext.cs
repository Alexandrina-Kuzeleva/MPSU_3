using ScheduleSystem.Models;

namespace ScheduleSystem.Storage;

public static class DataContext
{
    public static string BasePath { get; private set; } = "app-data";

    public static List<Room>     Rooms     { get; set; } = new();
    public static List<Teacher>  Teachers  { get; set; } = new();
    public static List<Group>    Groups    { get; set; } = new();
    public static List<Course>   Courses   { get; set; } = new();
    public static List<Session>  Sessions  { get; set; } = new();
    public static List<User>     Users     { get; set; } = new();

    public static User? CurrentUser { get; set; } = null;

    public static int NextId<T>() where T : class
    {
        return typeof(T) switch
        {
            var t when t == typeof(Room)     => Rooms.Count     == 0 ? 1 : Rooms.Max(x => x.Id)     + 1,
            var t when t == typeof(Teacher)  => Teachers.Count  == 0 ? 1 : Teachers.Max(x => x.Id)  + 1,
            var t when t == typeof(Group)    => Groups.Count    == 0 ? 1 : Groups.Max(x => x.Id)    + 1,
            var t when t == typeof(Course)   => Courses.Count   == 0 ? 1 : Courses.Max(x => x.Id)   + 1,
            var t when t == typeof(Session)  => Sessions.Count  == 0 ? 1 : Sessions.Max(x => x.Id)  + 1,
            _ => throw new NotSupportedException($"No ID generator for {typeof(T)}")
        };
    }

    public static void Initialize(string? customPath = null)
    {
        BasePath = customPath ?? "app-data";
        Directory.CreateDirectory(BasePath);
        JsonStorage.LoadAll();
    }

    public static void SaveAll() => JsonStorage.SaveAll();
}