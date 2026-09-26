using System.Diagnostics;
using System.Text;

namespace Snake;

/// <summary>
/// 自动游玩：让程序自己把场地填满。
/// 偶数边长用「行内蛇形 + 首列折返」构造一条覆盖全场的环路；
/// 奇数边长用「环形套环」构造覆盖全场少一格的环路（中心那格留作最后一口），
/// 因为奇×奇网格（二分图，两侧格数差 1）在数学上不存在哈密顿回路。
/// 蛇只在这条环路上走，且只在「环路序号朝食物单调前进、不越过食物」时抄近路，
/// 因此永远不会把自己围死，最终必然填满全场。
/// </summary>
public static class AutoMode
{
    private const int Empty = 0, Head = 1, Body = 2, Food = 3, Wall = 9;
    private static readonly int[] DR = { -1, 0, 1, 0 };
    private static readonly int[] DC = { 0, 1, 0, -1 };      // 0=上 1=右 2=下 3=左

    private static int _len;
    private static int _tickMs;
    private static int[][] _map = Array.Empty<int[]>();
    private static int[][] _idx = Array.Empty<int[]>();       // 格子在环路中的序号（-1 = 不在环路上）
    private static List<(int R, int C)> _order = new();       // 环路经过的格子顺序
    private static int _cycleLen;
    private static (int R, int C) _hole = (-1, -1);           // 奇数边长时留下的小洞
    private static List<(int R, int C)> _snake = new();
    private static (int R, int C) _food = (-1, -1);
    private static bool _over, _full;
    private static long _moves;
    private static readonly Random _rng = new();

    public static void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;
        _len = Ask("输入地图大小(5-20),默认为15 >>> ", 5, 20, 15);
        _tickMs = Ask("输入每步间隔毫秒(默认25, 0=最快) >>> ", 0, 2000, 25);

        BuildCycle();
        Reset();
        Console.Clear();
        Draw();

        var clock = Stopwatch.StartNew();
        while (!_over && !_full)
        {
            int d = ChooseDir();
            if (d < 0) { _over = true; break; }
            Step(d);
            _moves++;
            Draw();
            if (_tickMs > 0) Thread.Sleep(_tickMs);
        }
        clock.Stop();

        Console.SetCursorPosition(0, _len + 3);
        if (_full)
        {
            Console.WriteLine($"恭喜你，你赢了！{_len * _len} 格全部填满。");
            Console.WriteLine($"步数={_moves}  最终长度={_snake.Count}  用时={clock.Elapsed.TotalSeconds:0.0} 秒");
        }
        else
        {
            Console.WriteLine("游戏结束（意外死亡）！");
            Console.WriteLine($"死亡时：步数={_moves}  长度={_snake.Count}  用时={clock.Elapsed.TotalSeconds:0.0} 秒");
        }
        Console.WriteLine("按任意键退出...");
        Console.ReadKey(true);
    }

    private static int Ask(string prompt, int min, int max, int def)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return def;
            if (int.TryParse(input.Trim(), out int v) && v >= min && v <= max) return v;
            Console.WriteLine($"请输入 {min}~{max} 之间的整数（直接回车用默认 {def}）。");
        }
    }

    // ---------------- 环路构造 ----------------

    private static void BuildCycle()
    {
        _hole = (-1, -1);
        _order = _len % 2 == 0 ? BuildEvenCycle(_len) : BuildOddCycle(_len);
        _cycleLen = _order.Count;

        _idx = new int[_len + 2][];
        for (int r = 0; r < _len + 2; r++)
        {
            _idx[r] = new int[_len + 2];
            for (int c = 0; c < _len + 2; c++) _idx[r][c] = -1;
        }
        for (int i = 0; i < _order.Count; i++) _idx[_order[i].R][_order[i].C] = i;
    }

    /// <summary>偶数边长：第 1 行从左到右，其余各行蛇形，最后沿第 1 列折返收口。</summary>
    private static List<(int R, int C)> BuildEvenCycle(int n)
    {
        var order = new List<(int R, int C)>();
        for (int c = 1; c <= n; c++) order.Add((1, c));
        for (int r = 2; r <= n; r++)
        {
            if (r % 2 == 0) for (int c = n; c >= 2; c--) order.Add((r, c));
            else for (int c = 2; c <= n; c++) order.Add((r, c));
        }
        for (int r = n; r >= 2; r--) order.Add((r, 1));
        return order;
    }

    /// <summary>奇数边长：一层层边框各自成环，从最内圈开始逐层交叉合并，中心格留作小洞。</summary>
    private static List<(int R, int C)> BuildOddCycle(int n)
    {
        int rings = (n - 1) / 2;                 // 层数：n=5 时是 5×5 和 3×3 两层
        _hole = (rings + 1, rings + 1);          // 正中心那格

        var ringList = new List<List<(int R, int C)>>();
        for (int k = 0; k < rings; k++) ringList.Add(Ring(n, k));

        var cur = ringList[^1];                  // 最内圈先当作环
        for (int k = ringList.Count - 2; k >= 0; k--)
            cur = Merge(cur, ringList[k], k + 1);
        return cur;
    }

    private static List<(int R, int C)> Ring(int n, int k)
    {
        int lo = 1 + k, hi = n - k;
        var ring = new List<(int R, int C)>();
        for (int c = lo; c <= hi; c++) ring.Add((lo, c));
        for (int r = lo + 1; r <= hi; r++) ring.Add((r, hi));
        for (int c = hi - 1; c >= lo; c--) ring.Add((hi, c));
        for (int r = hi - 1; r >= lo + 1; r--) ring.Add((r, lo));
        return ring;
    }

    /// <summary>把外圈（左上角在第 lo 行）切开交叉接进已经合并好的内环里，两个环变成一个大环。</summary>
    private static List<(int R, int C)> Merge(List<(int R, int C)> inner, List<(int R, int C)> ring, int lo)
    {
        // 外圈上边相邻两格 a1 -> a2；内圈上边相邻两格 b1 -> b2
        // 依赖的相邻关系：a1~b1、a2~b2，于是把两条环各切一刀再交叉接上就并成一条大环
        var a1 = (R: lo, C: lo + 1);
        var b1 = (R: lo + 1, C: lo + 1);

        int ia = ring.IndexOf(a1);
        int ib = inner.IndexOf(b1);

        var merged = new List<(int R, int C)>(ring.Count + inner.Count);
        for (int i = 0; i < ring.Count; i++) merged.Add(ring[(ia + 1 + i) % ring.Count]);          // a2 ... a1
        for (int i = 0; i < inner.Count; i++) merged.Add(inner[((ib - i) % inner.Count + inner.Count) % inner.Count]); // b1 ... b2
        return merged;
    }

    // ---------------- 棋盘与规则 ----------------

    private static bool IsWall(int r, int c) => r <= 0 || c <= 0 || r >= _len + 1 || c >= _len + 1;

    private static void Reset()
    {
        _map = new int[_len + 2][];
        for (int r = 0; r < _len + 2; r++)
        {
            _map[r] = new int[_len + 2];
            for (int c = 0; c < _len + 2; c++) _map[r][c] = IsWall(r, c) ? Wall : Empty;
        }

        // 初始三节必须和环路方向一致：直接沿环路取连续三格（最后一个是蛇头）
        int start = NearCenterIndex();
        _snake = new List<(int R, int C)>
        {
            _order[start],
            _order[(start - 1 + _cycleLen) % _cycleLen],
            _order[(start - 2 + _cycleLen) % _cycleLen],
        };
        _over = false;
        _full = false;
        _moves = 0;
        _food = (-1, -1);

        Base();
        DrawSnake();
        PickFood();
        DrawAll();
    }

    private static int NearCenterIndex()
    {
        int mid = (_len + 1) / 2;
        int best = 0, bestD = int.MaxValue;
        for (int i = 0; i < _cycleLen; i++)
        {
            int d = Math.Abs(_order[i].R - mid) + Math.Abs(_order[i].C - mid);
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    private static void Base()
    {
        for (int r = 0; r < _len + 2; r++)
            for (int c = 0; c < _len + 2; c++)
                _map[r][c] = IsWall(r, c) ? Wall : Empty;
    }

    private static void DrawSnake()
    {
        _map[_snake[0].R][_snake[0].C] = Head;
        for (int i = 1; i < _snake.Count; i++) _map[_snake[i].R][_snake[i].C] = Body;
    }

    private static void DrawAll()
    {
        Base();
        DrawSnake();
        if (_food.R >= 1) _map[_food.R][_food.C] = Food;
    }

    private static void PickFood()
    {
        var empty = new List<(int R, int C)>();
        for (int r = 1; r <= _len; r++)
            for (int c = 1; c <= _len; c++)
                if (_map[r][c] == Empty) empty.Add((r, c));

        if (empty.Count == 0)
        {
            _food = (-1, -1);
            _full = true;
            return;
        }

        // 奇数边长的场地上，正中心那格不在环路上：把它留到最后一口，
        // 平时食物不刷在洞里，这样蛇始终待在环路上，不会因为吃洞而离开环路把自己绕死。
        if (_hole.R >= 1 && empty.Count > 1)
        {
            var outside = new List<(int R, int C)>();
            foreach (var p in empty)
                if (p != _hole)
                    outside.Add(p);
            if (outside.Count > 0)
            {
                _food = outside[_rng.Next(outside.Count)];
                return;
            }
        }

        _food = empty[_rng.Next(empty.Count)];
    }

    /// <summary>走一步，规则和手玩版完全一致。</summary>
    private static void Step(int dir)
    {
        var h = _snake[0];
        int nr = h.R + DR[dir], nc = h.C + DC[dir];
        var tail = _snake[^1];
        bool intoTail = nr == tail.R && nc == tail.C;
        int v = _map[nr][nc];

        if (v == Wall || (v == Body && !intoTail))
        {
            _over = true;
            return;
        }

        _snake.Insert(0, (nr, nc));
        if (v == Food)
        {
            Base();
            DrawSnake();
            _food = (-1, -1);
            PickFood();
            if (!_full) _map[_food.R][_food.C] = Food;
        }
        else
        {
            _snake.RemoveAt(_snake.Count - 1);
            DrawAll();
        }
    }

    // ---------------- 决策 ----------------

    private static int DirTo((int R, int C) from, (int R, int C) to)
    {
        for (int d = 0; d < 4; d++)
            if (from.R + DR[d] == to.R && from.C + DC[d] == to.C)
                return d;
        return -1;
    }

    private static int ChooseDir()
    {
        var head = _snake[0];

        // 奇数边长：食物落在中心小洞时，走过去吃掉（洞旁边的环路格一定还有空位可以让蛇头出来）
        if (_hole.R >= 1 && _food.R == _hole.R && _food.C == _hole.C)
        {
            int dHole = DirTo(head, _hole);
            if (dHole >= 0) return dHole;
            int hIdx0 = _idx[head.R][head.C];
            return DirTo(head, _order[(hIdx0 + 1) % _cycleLen]);
        }

        int hIdx = _idx[head.R][head.C];
        int tIdx = _idx[_snake[^1].R][_snake[^1].C];
        int windowLen = ((tIdx - hIdx) % _cycleLen + _cycleLen) % _cycleLen;

        if (_food.R >= 1 && _idx[_food.R][_food.C] >= 0)
        {
            int fOff = ((_idx[_food.R][_food.C] - hIdx) % _cycleLen + _cycleLen) % _cycleLen;
            if (fOff > 0 && fOff < windowLen)
            {
                var prev = new int[_len + 2, _len + 2];
                for (int r = 0; r < _len + 2; r++)
                    for (int c = 0; c < _len + 2; c++)
                        prev[r, c] = -1;

                var q = new Queue<(int R, int C)>();
                prev[head.R, head.C] = 5;
                q.Enqueue(head);

                while (q.Count > 0)
                {
                    var (r, c) = q.Dequeue();
                    for (int d = 0; d < 4; d++)
                    {
                        int nr = r + DR[d], nc = c + DC[d];
                        if (nr <= 0 || nc <= 0 || nr > _len || nc > _len) continue;
                        if (prev[nr, nc] != -1) continue;
                        if (_idx[nr][nc] < 0) continue;                       // 不在环路上的格子（中心洞）不走
                        int off = ((_idx[nr][nc] - hIdx) % _cycleLen + _cycleLen) % _cycleLen;
                        if (off <= 0 || off > fOff) continue;                 // 朝食物单调前进、不越过食物
                        prev[nr, nc] = d;
                        if (nr == _food.R && nc == _food.C)
                        {
                            int cr = nr, cc = nc;
                            while (true)
                            {
                                int pd = prev[cr, cc];
                                int pr = cr - DR[pd], pc = cc - DC[pd];
                                if (pr == head.R && pc == head.C) return pd;
                                cr = pr;
                                cc = pc;
                            }
                        }
                        q.Enqueue((nr, nc));
                    }
                }
            }
        }

        // 抄不了近路：沿环路走下一格
        return DirTo(head, _order[(hIdx + 1) % _cycleLen]);
    }

    private static void Draw()
    {
        int rows = _len + 2;
        var sb = new StringBuilder(rows * rows * 8);
        sb.Append("\u001b[H");
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < rows; c++)
            {
                int v = _map[r][c];
                sb.Append(v is Head or Body ? "\u001b[96m" : "\u001b[97m");
                sb.Append(v switch
                {
                    Head => "头",
                    Body => "蛇",
                    Food => "食",
                    Wall => "墙",
                    _ => "  ",
                });
            }
            sb.Append('\n');
        }
        sb.Append("\u001b[0m").Append($"步数={_moves}  长度={_snake.Count}   ").Append('\n');
        Console.Write(sb.ToString());
    }
}
