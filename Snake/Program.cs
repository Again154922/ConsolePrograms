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
    
    private static void Main(string[] args)
    {
        Init();

        while (_gameStart)
        {
            if (_move)
            {
                _move = false;
                Move(ref _map, _dir, ref _snake);
            }
        }

        Exit();
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

    private static void Init()
    {
        Console.CursorVisible = false;
        
        _map = 
        [
            [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 2, 2, 1, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9],
            [9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9],
        ];
        ShowMap(_map);

        Task Timer = Task.Run(async () =>
        {
            do
            {
                await Task.Delay(1000);
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
                    ConsoleKey.UpArrow or ConsoleKey.W => Up,
                    ConsoleKey.LeftArrow or ConsoleKey.A => Left,
                    ConsoleKey.DownArrow or ConsoleKey.S => Down,
                    ConsoleKey.RightArrow or ConsoleKey.D => Right,
                    _ => _dir
                };
            } while (_gameStart);
        });
        
        _gameStart = true;
    }

    private static void Move(ref int[][] map, int dir, ref List<(int, int)> snake)
    {
        switch (dir)
        {
            case Up:
                if (map[snake[0].Item1 - 1][snake[0].Item2] != Empty) _gameStart = false;
                
                snake.Insert(0, (snake[0].Item1 - 1, snake[0].Item2));
                snake.RemoveAt(snake.Count - 1);
                
                break;
            
            case Left:
                if (map[snake[0].Item1][snake[0].Item2 - 1] != Empty) _gameStart = false;
                
                snake.Insert(0, (snake[0].Item1, snake[0].Item2 - 1));
                snake.RemoveAt(snake.Count - 1);
                
                break;
            
            case Down:
                if (map[snake[0].Item1 + 1][snake[0].Item2] != Empty) _gameStart = false;
                
                snake.Insert(0, (snake[0].Item1 + 1, snake[0].Item2));
                snake.RemoveAt(snake.Count - 1);
                
                break;
            
            case Right:
                if (map[snake[0].Item1][snake[0].Item2 + 1] != Empty) _gameStart = false;
                
                snake.Insert(0, (snake[0].Item1, snake[0].Item2 + 1));
                snake.RemoveAt(snake.Count - 1);
                
                break;
        }
        
        SetMap(ref map, snake);
        
        ShowMap(map);
    }
    
    private static void Exit()
    {
        Console.WriteLine("按任意键退出...");
        Console.ReadKey();
    }

    private static void SetMap(ref int[][] map, List<(int, int)> snake)
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
            [9, 0, 0, 0, 0, 0, 0, 2, 2, 1, 0, 0, 0, 0, 0, 0, 9],
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
    }
}