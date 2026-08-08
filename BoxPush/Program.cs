using System.Data;
using System.Net;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Xml.Schema;

namespace BoxPush;

internal static class Program
{
    // 地图元素常量
    private const int Wall = 9;          // 墙
    private const int Air = 0;           // 空地
    private const int Box = 1;           // 箱子
    private const int Player = 2;        // 玩家
    private const int Target = 3;        // 目标点
    private const int BoxOnTarget = 4;   // 箱子在目标上（临时标记）
    
    private static List<int[][]> _lvls = new();          // 所有关卡
    private static List<int[][]> _lvlOriginal = new();   // 原始关卡备份（用于重置）
    private static int _currentLvlIndex;                 // 当前关卡索引
    private static bool _levelCompleted;                 // 当前关卡是否完成
    private static bool _gameCompleted;                  // 是否全部通关
    
    static void Main(string[] args)
    {
        _lvls = LoadLevel();
        if (_gameCompleted) goto Stop;
        _lvlOriginal = DeepCopy(_lvls);
        Console.CursorVisible = false; // 隐藏光标
    
        ColoredPrint("推箱子\n\n", ConsoleColor.Cyan);
        Console.WriteLine("使用 W/A/S/D 控制角色移动");
        Console.WriteLine("按 R 重置当前关卡");
        Console.WriteLine("按 ESC 退出游戏");
        Console.WriteLine("请切换为英文输入法\n");
        Console.Write("显示为");
        ColoredPrint("蓝色", ConsoleColor.Cyan);
        Console.WriteLine("的玩家或箱子表示它们位于目标点上");
        Console.Write("按任意键开始...\n");
        ConsoleKeyInfo key = Console.ReadKey(true);
        if (key.Key == ConsoleKey.Escape) return;
    
        while (!_gameCompleted)
        {
            int[][] currentLvl = _lvls[_currentLvlIndex];
            HashSet<(int, int)> targetPlace = GetTargetPlace(ref currentLvl);
        
            Console.Clear();
            PrintLevel(currentLvl, targetPlace);
            while (!_levelCompleted)
            {
                HandleKey(ref currentLvl, targetPlace, out var pressR);
                if (pressR)
                {
                    GetTargetPlace(ref currentLvl);
                    PrintLevel(currentLvl, targetPlace);
                    continue;
                }
                PrintLevel(currentLvl, targetPlace);
                var boxPlace = GetBoxPlace(currentLvl); 
                if (targetPlace.SetEquals(boxPlace)) _levelCompleted = true;
            }

            if (_gameCompleted) goto Stop;

            _currentLvlIndex++;
            if (_currentLvlIndex >= _lvls.Count)
            {
                _gameCompleted = true;
            }
            _levelCompleted = false;
            ColoredPrint($"第 {_currentLvlIndex} 关完成！\n", ConsoleColor.Cyan);
            if (_currentLvlIndex < _lvls.Count)
            {
                Console.WriteLine("按任意键进入下一关...");
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) goto Stop;
            }
        }
    
        ColoredPrint("恭喜你通关了所有关卡！\n", ConsoleColor.Cyan);
    
        Stop:
        Console.WriteLine("按任意键退出...");
        Console.ReadKey(true);
        return;
    }

    // 深拷贝整个关卡列表
    static List<int[][]> DeepCopy(List<int[][]> sourceList)
    {
        List<int[][]> result = new();

        foreach (var i in sourceList)
        {
            int[][] newLvl = new int[i.Length][];
            int index = 0;
            foreach (var row in i)
            {
                int[] newRow = new int[row.Length];
                
                for (int j = 0; j < row.Length; j++)
                {
                    newRow[j] = row[j];
                }
                newLvl[index] = newRow;
                index++;
            }

            result.Add(newLvl);
        }

        return result;
    }

    // 深拷贝单个关卡（用于重置）
    static int[][] DeepCopy(int[][] sourceArray)
    {
        int[][] result = new int[sourceArray.Length][];

        foreach (var i in sourceArray)
        {
            int[] newRow = new int[i.Length];
            
            for (int j = 0; j < i.Length; j++)
            {
                newRow[j] = i[j];
            }
            result[sourceArray.IndexOf(i)] = newRow;
        }

        return result;
    }

    // 获取所有目标点的坐标，并将地图中的 BoxOnTarget 转换为普通箱子
    static HashSet<(int, int)> GetTargetPlace(ref int[][] lvl)
    {
        HashSet<(int, int)> result = new();
        
        for (int i = 0; i < lvl.Length; i++)
        {
            if (lvl[i].Contains(Target) || lvl[i].Contains(BoxOnTarget))
            {
                for (int j = 0; j < lvl[i].Length; j++)
                {
                    if (lvl[i][j] == Target || lvl[i][j] == BoxOnTarget) result.Add((i, j));
                    if (lvl[i][j] == BoxOnTarget) lvl[i][j] = Box;
                }
            }
        }
        
        return result;
    }
    
    // 从 lvls.json 加载关卡数据
    static List<int[][]> LoadLevel()
    {
        List<int[][]> lvls = new();
        const string filePath = @".\lvls.json";
        
        if (!File.Exists(filePath))
        {
            _gameCompleted = true;
            ColoredPrint("找不到 lvls.json 文件！\n", ConsoleColor.Red);
            return lvls;
        }
        
        string json = File.ReadAllText(filePath);
        lvls = JsonSerializer.Deserialize<List<int[][]>>(json)!;

        return lvls;
    }
    
    // 打印当前关卡地图，并高亮显示位于目标点上的玩家或箱子
    static void PrintLevel(int[][] lvl, HashSet<(int, int)> targetPlace)
    {
        Console.SetCursorPosition(0, 0);
        Console.Write("第 ");
        Console.Write(_currentLvlIndex + 1);
        Console.WriteLine(" 关");
        Console.WriteLine("");
        
        for (int i = 0; i < lvl.Length; i++)
        {
            for (int j = 0; j < lvl[i].Length; j++)
            {
                string symbol;
                switch (lvl[i][j])
                {
                    case Wall:
                        symbol = "墙";
                        break;
                    case Air:
                        symbol = "  ";
                        break;
                    case Box:
                        symbol = "箱";
                        break;
                    case Player:
                        symbol = "人";
                        break;
                    case Target:
                        symbol = "目";
                        break;
                    default:
                        symbol = "??";
                        break;
                }

                // 如果玩家或箱子在目标点上，显示为青色
                ConsoleColor color = targetPlace.Contains((i, j)) && lvl[i][j] is Player or Box ? ConsoleColor.Cyan : ConsoleColor.White;

                ColoredPrint(symbol, color);
            }
            Console.WriteLine("");
        }
    }

    // 处理键盘输入，移动玩家或推箱子
    static void HandleKey(ref int[][] cLvl, HashSet<(int, int)> targetPlace, out bool pressR)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);
        int playerIndex, rowIndex;
        switch (key.Key)
        {
            case ConsoleKey.W or ConsoleKey.UpArrow: // 上
                foreach (var i in cLvl)
                {
                    if (i.Contains(Player))
                    {
                        playerIndex = i.IndexOf(Player);
                        rowIndex = cLvl.IndexOf(i);
                        switch (cLvl[rowIndex - 1][playerIndex])
                        {
                            case Air or Target:
                                cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                cLvl[rowIndex - 1][playerIndex] = Player;
                                break;
                            
                            case Box:
                                if (cLvl[rowIndex - 2][playerIndex] == Air || cLvl[rowIndex - 2][playerIndex] == Target)
                                {
                                    cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                    cLvl[rowIndex - 1][playerIndex] = Player;
                                    cLvl[rowIndex - 2][playerIndex] = Box;
                                }

                                break;
                        }
                        break;
                    }
                }
                break;
            
            case ConsoleKey.A or ConsoleKey.LeftArrow: // 左
                foreach (var i in cLvl)
                {
                    if (i.Contains(Player))
                    {
                        playerIndex = i.IndexOf(Player);
                        rowIndex = cLvl.IndexOf(i);
                        switch (cLvl[rowIndex][playerIndex - 1])
                        {
                            case Air or Target:
                                cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                cLvl[rowIndex][playerIndex - 1] = Player;
                                break;
                            
                            case Box:
                                if (cLvl[rowIndex][playerIndex - 2] == Air || cLvl[rowIndex][playerIndex - 2] == Target)
                                {
                                    cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                    cLvl[rowIndex][playerIndex - 1] = Player;
                                    cLvl[rowIndex][playerIndex - 2] = Box;
                                }
                                break;
                        }
                        break;
                    }
                }
                break;
            
            case ConsoleKey.S or ConsoleKey.DownArrow: // 下
                foreach (var i in cLvl)
                {
                    if (i.Contains(Player))
                    {
                        playerIndex = i.IndexOf(Player);
                        rowIndex = cLvl.IndexOf(i);
                        switch (cLvl[rowIndex + 1][playerIndex])
                        {
                            case Air or Target:
                                cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                cLvl[rowIndex + 1][playerIndex] = Player;
                                break;
                            
                            case Box:
                                if (cLvl[rowIndex + 2][playerIndex] == Air || cLvl[rowIndex + 2][playerIndex] == Target)
                                {
                                    cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                    cLvl[rowIndex + 1][playerIndex] = Player;
                                    cLvl[rowIndex + 2][playerIndex] = Box;
                                }
                                break;
                        }
                        break;
                    }
                }
                break;
            
            case ConsoleKey.D or ConsoleKey.RightArrow: // 右
                foreach (var i in cLvl)
                {
                    if (i.Contains(Player))
                    {
                        playerIndex = i.IndexOf(Player);
                        rowIndex = cLvl.IndexOf(i);
                        switch (cLvl[rowIndex][playerIndex + 1])
                        {
                            case Air or Target:
                                cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                cLvl[rowIndex][playerIndex + 1] = Player;
                                break;
                            
                            case Box:
                                if (cLvl[rowIndex][playerIndex + 2] == Air || cLvl[rowIndex][playerIndex + 2] == Target)
                                {
                                    cLvl[rowIndex][playerIndex] = targetPlace.Contains((rowIndex, playerIndex)) ? Target : Air;
                                    cLvl[rowIndex][playerIndex + 1] = Player;
                                    cLvl[rowIndex][playerIndex + 2] = Box;
                                }
                                break;
                        }
                        break;
                    }
                }
                break;
            
            case ConsoleKey.R: // 重置当前关卡
                cLvl = DeepCopy(_lvlOriginal[_currentLvlIndex]);
                pressR = true;
                return;
            
            case ConsoleKey.Escape: // 退出游戏
                _levelCompleted = true;
                _gameCompleted = true;
                break;
        }

        pressR = false;
    }

    // 带颜色的控制台输出
    static void ColoredPrint(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.Write(text);
        Console.ResetColor();
    }

    // 获取当前所有箱子的坐标
    static HashSet<(int, int)> GetBoxPlace(int[][] lvl)
    {
        HashSet<(int, int)> result = new();

        for (int i = 0; i < lvl.Length; i++)
        {
            if (lvl[i].Contains(Box))
            {
                for (int j = 0; j < lvl[i].Length; j++)
                {
                    if (lvl[i][j] == Box) result.Add((i, j));
                }
            }
        }
        
        return result;
    }
}