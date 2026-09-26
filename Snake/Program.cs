using System.Diagnostics;

namespace Snake;

internal static class Program
{
    private const int Head = 1;
    private const int Body = 2;
    private const int Food = 3;
    private const int Empty = 0;
    private const int Wall = 9;
    
    private const int Up = -1;
    private const int Left = -2;
    private const int Down = -3;
    private const int Right = -4;

    private static int[][] _map = null!;
    private static int[][] _mapBase = null!;
    private static bool _gameStart;
    
    private static int _speed;
    private static int _dir = Right;
    private static List<(int, int)> _snake = new();
    private static (int, int) _food;

    private static Random _random = new();

    // 方向 -> 行列增量：四个方向共用一份查表，不用每个方向抄一遍一样的逻辑
    private static readonly Dictionary<int, (int Row, int Col)> Deltas = new()
    {
        [Up] = (-1, 0),
        [Left] = (0, -1),
        [Down] = (1, 0),
        [Right] = (0, 1),
    };
    
    private static async Task Main()
    {
        Init(ref _map, _snake, ref _food);

        // 单线程游戏循环：节拍用秒表自己算，不再需要计时线程、锁和标志位
        var clock = Stopwatch.StartNew();
        long lastMove = 0;

        while (_gameStart)
        {
            if (Console.KeyAvailable)
            {
                HandleKey(Console.ReadKey(true).Key);
            }

            if (clock.ElapsedMilliseconds - lastMove >= _speed)
            {
                lastMove = clock.ElapsedMilliseconds;
                Move(ref _map, _dir, ref _snake, ref _food);
                ShowMap(_map);
            }

            await Task.Delay(10);
        }

        Exit(_map);
    }

    private static void ShowMap(int[][] map)
    {
        Console.SetCursorPosition(0, 0);
        
        foreach (var row in map)
        {
            foreach (var cell in row)
            {
                Console.ForegroundColor = cell is Head or Body ? ConsoleColor.Cyan : ConsoleColor.White;
                Console.Write(cell switch
                {
                    Head => "头",
                    Body => "蛇",
                    Food => "食",
                    Empty => "  ",
                    Wall => "墙",
                    _ => throw new Exception()
                });
            }
            Console.WriteLine();
        }
    }

    private static void Init(ref int[][] map, List<(int, int)> snake, ref (int, int) food)
    {
        Console.Write("输入游戏难度(1-5),默认为3 >>> ");
        string? speedInput = Console.ReadLine();
        bool getSpeedSuccess = int.TryParse(speedInput, out int speed);
        if (!getSpeedSuccess || speed < 1 || speed > 5) speed = 3;
        _speed = speed switch
        {
            1 => 1000,
            2 => 750,
            3 => 500,
            4 => 250,
            5 => 100,
            _ => throw new Exception()
        };
        
        Console.Write("输入游戏地图大小(5-20),默认为15 >>> ");
        string? mapSizeInput = Console.ReadLine();
        bool getMapSizeSuccess = int.TryParse(mapSizeInput, out int mapSize);
        if (!getMapSizeSuccess || mapSize < 5 || mapSize > 20) mapSize = 15;
        SetBaseInfo(mapSize, ref _mapBase, ref _snake);

        Console.CursorVisible = false;

        SetMap(ref map, snake, (1, 1));
        map[1][1] = Empty;
        SetFood(_random, map, ref food);
        SetMap(ref map, snake, food);
        Console.Clear();
        ShowMap(map);

        Console.WriteLine("按方向键或WASD控制蛇的移动");
        Console.WriteLine("按Esc键退出游戏");
        Console.WriteLine("按任意键开始游戏");
        Console.ReadKey(true);
        Console.Clear();
        ShowMap(map);
        
        _gameStart = true;
    }

    // 按键 -> 方向：目标格可走（或那格是下一步要让位的尾巴）才转向，否则保持原方向
    private static void HandleKey(ConsoleKey key)
    {
        _dir = key switch
        {
            ConsoleKey.UpArrow or ConsoleKey.W => Turn(Up, _snake),
            ConsoleKey.LeftArrow or ConsoleKey.A => Turn(Left, _snake),
            ConsoleKey.DownArrow or ConsoleKey.S => Turn(Down, _snake),
            ConsoleKey.RightArrow or ConsoleKey.D => Turn(Right, _snake),
            ConsoleKey.Escape => 0,
            _ => _dir
        };
    }

    private static int Turn(int dir, List<(int, int)> snake)
    {
        if (!TryGetNext(snake[0], dir, out (int Row, int Col) next))
            return _dir;

        return _map[next.Row][next.Col] is Empty or Food || (next.Row, next.Col) == snake[^1]
            ? dir
            : _dir;
    }

    // 四个方向共用的一套计算：把方向换成下一格坐标，方向无效（0）时返回 false
    private static bool TryGetNext((int Row, int Col) head, int dir, out (int Row, int Col) next)
    {
        if (Deltas.TryGetValue(dir, out (int Row, int Col) delta))
        {
            next = (head.Row + delta.Row, head.Col + delta.Col);
            return true;
        }

        next = head;
        return false;
    }

    private static void Move(ref int[][] map, int dir, ref List<(int, int)> snake, ref (int, int) food)
    {
        // 方向为 0（按了 Esc）或不是有效方向时结束游戏
        if (!TryGetNext(snake[0], dir, out (int Row, int Col) next))
        {
            _gameStart = false;
            return;
        }

        // 撞墙或撞到自己就结束；尾巴下一步会让位，所以踩尾巴不算死
        if (map[next.Row][next.Col] is Body or Wall &&
            snake.IndexOf((next.Row, next.Col)) != snake.Count - 1)
        {
            _gameStart = false;
            return;
        }

        if (map[next.Row][next.Col] != Food)
            snake.RemoveAt(snake.Count - 1);
        else if (GetEmpty(map).Count != 0)
            SetFood(_random, map, ref food);
        snake.Insert(0, (next.Row, next.Col));

        SetMap(ref map, snake, food);
    }
    
    private static void Exit(int[][] map)
    {
        bool win = true;
        foreach (var i in map)
        {
            foreach (var j in i)
            {
                if (j == Empty)
                {
                    win = false;
                    break;
                }
            }
            if (!win) break;
        }
        Console.WriteLine(win ? "恭喜你，你赢了!" : "游戏结束!");
        Console.WriteLine("按任意键退出...");
        Console.ReadKey(true);
    }

    private static void SetMap(ref int[][] map, List<(int, int)> snake, (int, int) food)
    {
        map = _mapBase.Select(row => (int[])row.Clone()).ToArray();
        
        map[snake[0].Item1][snake[0].Item2] = Head;
        foreach (var pos in snake[1..])
        {
            map[pos.Item1][pos.Item2] = Body;
        }

        map[food.Item1][food.Item2] = Food;
    }
    
    private static void SetFood(Random random, int[][] map, ref (int, int) food)
    {
        List<(int, int)> emptyPositions = GetEmpty(map);
        food = emptyPositions[random.Next(emptyPositions.Count)];
    }
    
    private static List<(int, int)> GetEmpty(int[][] map)
    {
        List<(int, int)> empty = new();
        for (int i = 1; i < map.Length - 1; i++)
        {
            for (int j = 1; j < map[i].Length - 1; j++)
            {
                if (map[i][j] == Empty)
                {
                    empty.Add((i, j));
                }
            }
        }
        return empty;
    }

    private static void SetBaseInfo(int len, ref int[][] mapBase, ref List<(int, int)> snake)
    {
        mapBase = new int[len + 2][];
        
        for (int i = 0; i < len + 2; i++)
        {
            mapBase[i] = new int[len + 2];
        }

        for (int i = 0; i < len + 2; i++)
        {
            (mapBase[0][i], mapBase[^1][i]) = (9, 9);
        }

        for (int i = 1; i <= len; i++)
        {
            mapBase[i] = new int[len + 2];
            (mapBase[i][0], mapBase[i][^1]) = (9, 9);
        }

        snake.Add((len / 2 + 1, len / 2 + 1));
        snake.Add((len / 2 + 1, len / 2));
        snake.Add((len / 2 + 1, len / 2 - 1));
    }
}