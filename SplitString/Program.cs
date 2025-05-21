namespace SplitString;

public static class SplitString
{
    public static void Main(string[] args)
    {
        const string test = "(Hello World)";
        char[] separator = new[] { '(', ')', ' ' };
        List<string> parts = test.Split(separator).ToList();
        // foreach (var part in parts)
        // {
        //     Console.Write(part + "---");
        // }
        // Console.WriteLine(parts[1]);
        for (int i = 0; i < parts.Count; i++)
        {
            if (0 < i && i <= parts.Count-2)
            {
                Console.WriteLine("---" + parts[i] + "---");
            }
        }
    }
}