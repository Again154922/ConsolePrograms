namespace MoreThreads;

public static class MoreThreads
{
    private static int Num { get; set; }

    public static async Task Main(string[] args)
    {
        for (int i = 0; i < 3; i++)
        {
            await Task.Run(AddNumAsync);
        }

        Console.WriteLine(Num);
    }

    static void AddNumAsync()
    {
        for (int i = 0; i < 100000000; i++)
        {
            Num++;
        }
    }
}