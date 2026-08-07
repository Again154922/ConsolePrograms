namespace RandomPi;

internal static class Program
{
    static Random _random = new();
    
    static void Main()
    {
        int count = 0;
        int crossCount = 0;
        int time;
        while (true)
        {
            try
            {
                Console.Write("Enter number of iterations >>> "); 
                time = int.Parse(Console.ReadLine() ?? "");
                break;
            }
            catch (Exception)
            {
                Console.WriteLine("Error: Invalid input. Please enter a valid number.");
            }
        }
        for (int i = 0; i < time; i++)
        {
            GetRandomNeedle(out double[] start, out double[] end);
            if (IsCrossingLine(start, end))
            {
                crossCount++;
            }

            count++;
            Console.WriteLine($"Count: {count}\tCrossCount: {crossCount}\tPi: {(double)count / crossCount}");
        }
    }

    static void GetRandomNeedle(out double[] start, out double[] end)
    {
        const double len = 0.5;
        double angle = _random.NextDouble() * Math.PI;
        double startx = _random.NextDouble() * 10;
        double starty = _random.NextDouble() * 10;
        double endx = startx + len * Math.Cos(angle);
        double endy = starty + len * Math.Sin(angle);
        start = [startx, starty];
        end = [endx, endy];
    }

    static bool IsCrossingLine(double[] start, double[] end)
    {
        return Math.Floor((decimal)start[1]) != Math.Floor((decimal)end[1]);
    }
}

