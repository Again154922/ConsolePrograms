namespace Test;

public static class MainApp
{
    public static void Main(string[] args)
    {
        int a = 1;
        int b = 2;
        List<int> swapped = Swap(a, b);
    }

    public static List<int> Swap(int a, int b)
    {
        List<int> result = new List<int>(){ b, a };
        return result;
    }
}