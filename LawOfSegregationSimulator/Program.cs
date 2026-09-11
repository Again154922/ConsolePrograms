namespace LauOfSegregationSimulator;

internal static class Program
{
    static void Main(string[] args)
    {
        input:
        Console.Write("样本数 >>> ");
        int sampleNum;
        try
        {
            sampleNum = int.Parse(Console.ReadLine() ?? "");
            if (sampleNum <= 0) throw new Exception();
        }
        catch (Exception)
        {
            goto input;
        }
        
        Console.WriteLine("亲本基因型: DdFf x DdFf");
        Console.WriteLine("D/d基因: 性状1");
        Console.WriteLine("F/f基因: 性状2");
        Console.WriteLine("开始模拟...");
        long startTime = GetTimeStamp();
        
        var random = new Random();
        string[] gamete = ["DF", "Df", "dF", "df"];

        List<string> f1 = new();
        for (int i = 0; i < sampleNum; i++)
        {
            string parentF = gamete[random.Next(0, 4)];
            string parentM = gamete[random.Next(0, 4)];
            string d = parentM[0] == 'd' ? parentF[0] + parentM[0].ToString() : parentM[0] + parentF[0].ToString();
            string f = parentM[1] == 'f' ? parentF[1] + parentM[1].ToString() : parentM[1] + parentF[1].ToString();
            f1.Add(d + f);
        }

        double DF = f1.Count(x => x[0] == 'D' && x[2] == 'F');
        double Df = f1.Count(x => x[0] == 'D' && x[2] == 'f');
        double dF = f1.Count(x => x[0] == 'd' && x[2] == 'F');
        double df = f1.Count(x => x[0] == 'd' && x[2] == 'f');
        double[] result = [DF, Df, dF, df];
        
        Console.WriteLine($"经过{GetTimeStamp() - startTime}毫秒模拟完成, 结果如下:");
        Console.WriteLine($"性状1,2均显:\t\t{DF}");
        Console.WriteLine($"性状1显, 性状2隐:\t{Df}");
        Console.WriteLine($"性状1隐, 性状2显:\t{dF}");
        Console.WriteLine($"性状1,2均隐:\t\t{df}");
        Console.WriteLine("通过对每个统计值计算 值/最小值, 可得比例大致为: " +
                          $"{Math.Round(DF / result.Min())}:" +
                          $"{Math.Round(Df / result.Min())}:" +
                          $"{Math.Round(dF / result.Min())}:" +
                          $"{Math.Round(df / result.Min())}");
    }

    static long GetTimeStamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}