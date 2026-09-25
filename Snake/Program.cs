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
    
    private static bool _move;
    private static int _dir = Right;
    private static List<(int, int)> _snake = new() { (8, 9), (8, 8), (8, 7) };
    private static (int, int) _food;

    private static Random _random = new();
    
    private static void Main(string[] args)
    {
        Init(ref _map, _snake, ref _food);

        while (_gameStart)
        {
            if (_move)
            {
                _move = false;
                Move(ref _map, _dir, ref _snake, ref _food);
            }
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
        int speed = int.Parse(Console.ReadLine() ?? "3") switch
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

        int[][] mapCopy = map;
        Task Timer = Task.Run(async () =>
        {
            Console.WriteLine("按方向键或WASD控制蛇的移动");
            Console.WriteLine("按任意键开始游戏");
            Console.ReadKey(true);
            Console.Clear();
            ShowMap(mapCopy);
            do
            {
                await Task.Delay(speed);
                _move = true;
            } while (_gameStart);
        });
        
        Task Input = Task.Run(() =>
        {
            do
            {
                var key = Console.ReadKey(true).Key;
                _dir = key switch
                {
                    ConsoleKey.UpArrow or ConsoleKey.W => _map[_snake[0].Item1 - 1][_snake[0].Item2] is Empty or Food
                        ? Up
                        : _dir,
                    ConsoleKey.LeftArrow or ConsoleKey.A => _map[_snake[0].Item1][_snake[0].Item2 - 1] is Empty or Food
                        ? Left
                        : _dir,
                    ConsoleKey.DownArrow or ConsoleKey.S => _map[_snake[0].Item1 + 1][_snake[0].Item2] is Empty or Food
                        ? Down
                        : _dir,
                    ConsoleKey.RightArrow or ConsoleKey.D => _map[_snake[0].Item1][_snake[0].Item2 + 1] is Empty or Food
                        ? Right
                        : _dir,
                    _ => _dir
                };
            } while (_gameStart);
        });
        
        _gameStart = true;
    }

    private static void Move(ref int[][] map, int dir, ref List<(int, int)> snake, ref (int, int) food)
    {
        switch (dir)
        {
            case Up:
                if (map[snake[0].Item1 - 1][snake[0].Item2] is Body or Wall)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1 - 1][snake[0].Item2] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1 - 1, snake[0].Item2));
                
                break;
            
            case Left:
                if (map[snake[0].Item1][snake[0].Item2 - 1] is Body or Wall)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1][snake[0].Item2 - 1] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1, snake[0].Item2 - 1));
                
                break;
            
            case Down:
                if (map[snake[0].Item1 + 1][snake[0].Item2] is Body or Wall)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1 + 1][snake[0].Item2] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1 + 1, snake[0].Item2));

                break;
            
            case Right:
                if (map[snake[0].Item1][snake[0].Item2 + 1] is Body or Wall)
                {
                    _gameStart = false;
                    return;
                }
                
                if (map[snake[0].Item1][snake[0].Item2 + 1] != Food)
                    snake.RemoveAt(snake.Count - 1);
                else
                    SetFood(_random, map, ref food);
                snake.Insert(0, (snake[0].Item1, snake[0].Item2 + 1));
                
                break;
        }
        
        SetMap(ref map, snake, food);
        
        ShowMap(map);
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
        int x, y;
        do
        {
            x = random.Next(1, 15);
            y = random.Next(1, 15);
        } while(map[x][y] != Empty);
        food = (x, y);
    }
}