internal static class Program
{
    static int _num, _real, _fake;
    static Dictionary<int, int> _phoneDict = new Dictionary<int, int>();
    static List<int>? _bulletList;
    
    public static void Main(string[] args)
    {
        Start:
        if (_real != 0 || _fake != 0) goto Again;
        
        Console.Clear();
        _num = 1;
        Console.Write("实弹数 >>> ");
        _real = int.Parse(Console.ReadLine()!);
        Console.Write("空包弹数 >>> ");
        _fake = int.Parse(Console.ReadLine()!);
        _bulletList = null;
        
        Again:
        Console.Write("键入数字:\n1. 射击\n2. 打电话\n3. 重开\n>>> ");
        int operation = int.Parse(Console.ReadLine()!);
        switch (operation)
        {
            case 1:
                Console.Write("键入数字:\n1. 实弹\n2. 空包弹\n>>> ");
                int shootOperation = int.Parse(Console.ReadLine()!);
                switch (shootOperation)
                {
                    case 1:
                        _real--;
                        break;
                    
                    case 2:
                        _fake--;
                        break;
                }
                
                _num++;
                foreach (var pair in _phoneDict)
                {
                    if (pair.Key < _num) _phoneDict.Remove(pair.Key);
                }
                
                int realCount1 = 0;
                int fakeCount1 = 0;
                foreach (var pair in _phoneDict)
                {
                    if (pair.Value == 1) realCount1++;
                    else if (pair.Value == 2) fakeCount1++;
                }
                
                if (realCount1 == _real)
                {
                    List<int> realList = new List<int>();
                    foreach (var pair in _phoneDict)
                    {
                        if (pair.Value == 1) realList.Add(pair.Key - _num + 1);
                    }

                    _bulletList = GenerateMarkerList(_real + _fake, realList);
                }

                if (fakeCount1 == _fake)
                {
                    List<int> fakeList = new List<int>();
                    foreach (var pair in _phoneDict)
                    {
                        if (pair.Value == 2) fakeList.Add(pair.Key - _num + 1);
                    }
                    
                    _bulletList = GenerateInverseMarkerList(_real + _fake, fakeList);
                }
                
                Output(1);
                break;
            
            case 2:
                Console.Write("轮次数 >>> ");
                int round = int.Parse(Console.ReadLine()!);
                Console.Write("键入数字:\n1. 实弹\n2. 空包弹\n>>> ");
                int phoneOperation = int.Parse(Console.ReadLine()!);
                _phoneDict.Add(round + _num, phoneOperation);

                int realCount = 0;
                int fakeCount = 0;
                foreach (var pair in _phoneDict)
                {
                    if (pair.Value == 1) realCount++;
                    else if (pair.Value == 2) fakeCount++;
                }
                
                if (realCount == _real)
                {
                    List<int> realList = new List<int>();
                    foreach (var pair in _phoneDict)
                    {
                        if (pair.Value == 1) realList.Add(pair.Key - _num + 1);
                    }

                    _bulletList = GenerateMarkerList(_real + _fake, realList);
                }

                if (fakeCount == _fake)
                {
                    List<int> fakeList = new List<int>();
                    foreach (var pair in _phoneDict)
                    {
                        if (pair.Value == 2) fakeList.Add(pair.Key - _num + 1);
                    }
                    
                    _bulletList = GenerateInverseMarkerList(_real + _fake, fakeList);
                }
                
                Output(2);
                break;
            
            case 3:
                _real = 0;
                _fake = 0;
                goto Start;
        }

        goto Start;
    }

    static void Output(int operation)
    {
        Console.Clear();
        Console.WriteLine("输出:");
        
        switch (operation)
        {
            case 1:
                Console.WriteLine($"剩余实弹:\t{_real}");
                Console.WriteLine($"剩余空包弹:\t{_fake}");
                if (_phoneDict.Count != 0)
                {
                    Console.WriteLine("已知:");
                    foreach (var pair in _phoneDict)
                    {
                        Console.WriteLine($"{pair.Key}{GetOdinaryNum(pair.Key)}:\t{(pair.Value == 1 ? "实弹" : "空包弹")}");
                    }
                }

                if (_bulletList != null && (_real != 0 || _fake != 0))
                {
                    Console.Write("剩余顺序:\t");
                    foreach (var i in _bulletList)
                    {
                        Console.Write($"{(i == 1 ? "真" : "假")}");
                    }
                }
                else if (_real != 0 || _fake != 0) Console.WriteLine($"下一发:\t\t{_num}{GetOdinaryNum(_num)}");
                else Console.WriteLine("本轮结束");
                Console.WriteLine("");
                break;
            
            case 2:
                Console.WriteLine($"剩余实弹:\t{_real}");
                Console.WriteLine($"剩余空包弹:\t{_fake}");
                Console.WriteLine("已知:");
                foreach (var pair in _phoneDict)
                {
                    Console.WriteLine($"{pair.Key}{GetOdinaryNum(pair.Key)}:\t{(pair.Value == 1 ? "实弹" : "空包弹")}");
                }
                
                if (_bulletList != null && (_real != 0 || _fake != 0))
                {
                    Console.Write("剩余顺序:\t");
                    foreach (var i in _bulletList)
                    {
                        Console.Write($"{(i == 1 ? "真" : "假")}");
                    }
                }
                else if (_real != 0 || _fake != 0) Console.WriteLine($"下一发:\t\t{_num}{GetOdinaryNum(_num)}");
                Console.WriteLine("");
                break;
        }
    }

    static string GetOdinaryNum(int num)
    {
        string result;
        if (num - num / 10 * 10 == 1 && num != 11) result = "st";
        else if (num - num / 10 * 10 == 2 && num != 12) result = "nd";
        else if (num - num / 10 * 10 == 3 && num != 13) result = "rd";
        else result = "th";
        return result;
    }

    static List<int> GenerateMarkerList(int n, List<int> l)
    {
        // 初始化为全 2，长度为 n
        List<int> result = new List<int>(new int[n]);
        for (int i = 0; i < n; i++)
            result[i] = 2;

        // 将 l 中每个元素对应的位置设为 1
        foreach (int num in l)
        {
            // 假设 num 有效（1 ≤ num ≤ n）
            int index = num - 1;
            result[index] = 1;
        }

        return result;
    }
    
    static List<int> GenerateInverseMarkerList(int n, List<int> l)
    {
        // 初始化为全 1
        List<int> result = new List<int>(new int[n]);
        for (int i = 0; i < n; i++)
            result[i] = 1;

        // 将 l 中每个元素对应的位置设为 2
        foreach (int num in l)
        {
            int index = num - 1;
            result[index] = 2;
        }

        return result;
    }
}