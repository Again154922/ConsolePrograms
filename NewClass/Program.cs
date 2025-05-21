namespace NewClass;

public static class Program
{
    public static void Main(string[] args)
    { 
        long startTime = long.Parse(GetTimeStamp()); 
        Person p1 = new Person(18, "John", Gender.Male, "Beijing");
        Person p2 = new Person(10, "Peter", Gender.Female, "Shanghai");
        p1.Introduction();
        p2.Introduction();
        long stopTime = long.Parse(GetTimeStamp());
        Console.WriteLine("run time: " + (stopTime - startTime) + "ms");
    }

    private static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds).ToString();
    }
}
