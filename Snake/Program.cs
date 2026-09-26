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
    private static bool _gameStart;
    private static Lock _lock = new();
    
    private static Task _timer = null!;
    private static Task _input = null!;
    private static bool _move;
    private static bool _didMove;
    private static int _dir = Right;
    private static List<(int, int)> _snake = new() { (8, 9), (8, 8), (8, 7) };
    private static (int, int) _food;

    private static Random _random = new();
    
    private static async Task Main(string[] args)
    {
        Init(ref _map, _snake, ref _food);

        while (_gameStart)
        {
            lock (_lock)
            {
                if (_move)
                {
                    Move(ref _map, _dir, ref _snake, ref _food);
                    _move = false;
                    _didMove = true;
                }
            }
            if (_didMove)
            {
                ShowMap(_map);
                _didMove = false;
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
        string? input = Console.ReadLine();
        int speed = int.Parse(input is "1" or "2" or "3" or "4" or "5" ? input : "3") switch
        {
            1 => 1000,
            2 => 750,
            3 => 500,
            4 => 250,
            5 => 100,
            _ => throw new Exception()
        };

        Console.CursorVisible = false;
        
        map = 
        [
            [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],
        ];
        SetFood(_random, map, ref food);
        SetMap(ref map, snake, food);
        ShowMap(map);

        Console.WriteLine("按方向键或WASD控制蛇的移动");
        Console.WriteLine("按Esc键退出游戏");
        Console.WriteLine("按任意键开始游戏");
        Console.ReadKey(true);
        Console.Clear();
        ShowMap(map);
        
        _timer = Task.Run(async () =>
        {
            do
            {
                await Task.Delay(speed);
                lock (_lock) _move = true;
            } while (_gameStart);
        });
        
        _input = Task.Run(async () =>
        {
            do
            {
                await Task.Delay(10);
                if (!Console.KeyAvailable) continue;
                var key = Console.ReadKey(true).Key;
                lock (_lock)
                {
                    _dir = key switch
                    {
                        ConsoleKey.UpArrow or ConsoleKey.W =>
                            _map[_snake[0].Item1 - 1][_snake[0].Item2] is Empty or Food ||
                            (_snake[0].Item1 - 1, _snake[0].Item2) == snake[^1]
                                ? Up
                                : _dir,
                        ConsoleKey.LeftArrow or ConsoleKey.A =>
                            _map[_snake[0].Item1][_snake[0].Item2 - 1] is Empty or Food ||
                            (_snake[0].Item1, _snake[0].Item2 - 1) == snake[^1]
                                ? Left
                                : _dir,
                        ConsoleKey.DownArrow or ConsoleKey.S =>
                            _map[_snake[0].Item1 + 1][_snake[0].Item2] is Empty or Food ||
                            (_snake[0].Item1 + 1, _snake[0].Item2) == snake[^1]
                                ? Down
                                : _dir,
                        ConsoleKey.RightArrow or ConsoleKey.D =>
                            _map[_snake[0].Item1][_snake[0].Item2 + 1] is Empty or Food ||
                            (_snake[0].Item1, _snake[0].Item2 + 1) == snake[^1]
                                ? Right
                                : _dir,
                        ConsoleKey.Escape => 0,
                        _ => _dir
                    };
                }
            } while (_gameStart);
        });
        
        _gameStart = true;
    }

    private static void Move(ref int[][] map, int dir, ref List<(int, int)> snake, ref (int, int) food)
    {
        switch (dir)
        {
            case Up:
                if (map[snake[0].Item1 - 1][snake[0].Item2] is Body or Wall && snake.IndexOf((snake[0].Item1 - 1, snake[0].Item2)) != snake.Count - 1)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1 - 1][snake[0].Item2] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else if (GetEmpty(map).Count != 0)
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1 - 1, snake[0].Item2));
                
                break;
            
            case Left:
                if (map[snake[0].Item1][snake[0].Item2 - 1] is Body or Wall && snake.IndexOf((snake[0].Item1, snake[0].Item2 - 1)) != snake.Count - 1)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1][snake[0].Item2 - 1] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else if (GetEmpty(map).Count != 0)
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1, snake[0].Item2 - 1));
                
                break;
            
            case Down:
                if (map[snake[0].Item1 + 1][snake[0].Item2] is Body or Wall && snake.IndexOf((snake[0].Item1 + 1, snake[0].Item2)) != snake.Count - 1)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1 + 1][snake[0].Item2] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else if (GetEmpty(map).Count != 0)
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1 + 1, snake[0].Item2));

                break;
            
            case Right:
                if (map[snake[0].Item1][snake[0].Item2 + 1] is Body or Wall && snake.IndexOf((snake[0].Item1, snake[0].Item2 + 1)) != snake.Count - 1)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1][snake[0].Item2 + 1] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else if (GetEmpty(map).Count != 0)
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1, snake[0].Item2 + 1));
                
                break;
            
            default:
                _gameStart = false;
                return;
        }
        
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
        Task.WaitAll([_input, _timer], 100);
        Console.WriteLine("按任意键退出...");
        Console.ReadKey(true);
    }

    private static void SetMap(ref int[][] map, List<(int, int)> snake, (int, int) food)
    {
        map =
        [
            [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],
        ];
        
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
        for (int i = 1; i < 16; i++)
        {
            for (int j = 1; j < 16; j++)
            {
                if (map[i][j] == Empty)
                {
                    empty.Add((i, j));
                }
            }
        }
        return empty;
    }
}