using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace cli_life
{
    public class Config
    {
        public int width { get; set; }
        public int height { get; set; }
        public double liveDensity { get; set; }
    }

    public class Cell
    {
        public bool State;
        private bool _futureState;
        public readonly List<Cell> Adjacent = new();

        public void Evaluate()
        {
            int active = Adjacent.Count(c => c.State);
            _futureState = State ? active == 2 || active == 3 : active == 3;
        }

        public void Update() => State = _futureState;
    }

    public class Board
    {
        public readonly Cell[,] Grid;
        public readonly int CellSize;

        private readonly Random _random = new();
        private static readonly Dictionary<string, List<(int dx, int dy)>> Templates = new()
        {
            { "Block", new() { (0, 0), (0, 1), (1, 0), (1, 1) } },
            { "Beehive", new() { (0, 1), (1, 0), (1, 2), (2, 0), (2, 2), (3, 1)} },
            { "Loaf", new() { (0, 1), (1, 0), (1, 2), (2, 0), (2, 3), (3, 1), (3, 2)} },
            { "Boat", new() { (0, 0), (0, 1), (1, 0), (1, 2), (2, 1)} },
            { "Tub", new() { (0, 1), (1, 0), (1, 2), (2, 1) } }
        };

        public int Columns => Grid.GetLength(0);
        public int Rows => Grid.GetLength(1);

        public Board(int width, int height, int cellSize, double density = 0.1)
        {
            CellSize = cellSize;
            Grid = new Cell[width / cellSize, height / cellSize];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    Grid[x, y] = new Cell();

            LinkNeighbors();
            Populate(density);
        }

        public void Populate(double density)
        {
            foreach (var cell in Grid)
                cell.State = _random.NextDouble() < density;
        }

        public void NextGeneration()
        {
            foreach (var cell in Grid)
                cell.Evaluate();
            foreach (var cell in Grid)
                cell.Update();
        }

        private void LinkNeighbors()
        {
            for (int x = 0; x < Columns; x++)
            {
                for (int y = 0; y < Rows; y++)
                {
                    int left = (x == 0) ? Columns - 1 : x - 1;
                    int right = (x == Columns - 1) ? 0 : x + 1;
                    int up = (y == 0) ? Rows - 1 : y - 1;
                    int down = (y == Rows - 1) ? 0 : y + 1;

                    Cell current = Grid[x, y];
                    current.Adjacent.AddRange(new[]
                    {
                        Grid[left, up], Grid[x, up], Grid[right, up],
                        Grid[left, y],              Grid[right, y],
                        Grid[left, down], Grid[x, down], Grid[right, down]
                    });
                }
            }
        }

        public int CountAlive()
        {
            return Grid.Cast<Cell>().Count(c => c.State);
        }

        public int CountGroups()
        {
            var seen = new HashSet<Cell>();
            int groups = 0;

            foreach (var cell in Grid)
            {
                if (cell.State && !seen.Contains(cell))
                {
                    groups++;
                    var stack = new Stack<Cell>();
                    stack.Push(cell);

                    while (stack.Count > 0)
                    {
                        var current = stack.Pop();
                        if (!seen.Add(current)) continue;
                        foreach (var neighbor in current.Adjacent)
                        {
                            if (neighbor.State && !seen.Contains(neighbor))
                                stack.Push(neighbor);
                        }
                    }
                }
            }
            return groups;
        }

        public Dictionary<string, int> CountTemplates()
        {
            var result = Templates.Keys.ToDictionary(k => k, k => 0);
            foreach (var kvp in Templates)
            {
                string name = kvp.Key;
                var shape = kvp.Value;
                for (int x = 0; x < Columns; x++)
                    for (int y = 0; y < Rows; y++)
                        if (shape.All(offset =>
                        {
                            int nx = (x + offset.dx + Columns) % Columns;
                            int ny = (y + offset.dy + Rows) % Rows;
                            return Grid[nx, ny].State;
                        }))
                            result[name]++;
            }
            return result;
        }
    }

    class Program
    {
        static Board gameBoard;
        static Config settings;

        static void InitFromFile(string path)
        {
            var lines = File.ReadAllLines(path);
            int height = lines.Length;
            int width = lines[0].Length;
            gameBoard = new Board(width, height, 1, 0);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    gameBoard.Grid[x, y].State = lines[y][x] == '*';
        }

        static void SaveToFile(string path)
        {
            using var writer = new StreamWriter(path);
            for (int y = 0; y < gameBoard.Rows; y++)
            {
                for (int x = 0; x < gameBoard.Columns; x++)
                    writer.Write(gameBoard.Grid[x, y].State ? '*' : '-');
                writer.WriteLine();
            }
        }

        static void PerformStudy(int maxSteps = 500, int stableLength = 10)
        {
            using var log = new StreamWriter("../../../data.txt");
            log.WriteLine("Density StableStep");

            foreach (double density in Enumerable.Range(0, 11).Select(i => i / 10.0))
            {
                gameBoard = new Board(100, 100, 10, density);
                var recent = new Queue<int>();
                int stableAt = -1;

                for (int step = 0; step < maxSteps; step++)
                {
                    int current = gameBoard.CountAlive();
                    recent.Enqueue(current);
                    if (recent.Count > stableLength) recent.Dequeue();
                    if (recent.Count == stableLength && recent.All(v => v == recent.Peek()))
                    {
                        stableAt = step;
                        break;
                    }
                    gameBoard.NextGeneration();
                }
                log.WriteLine($"{density}    {(stableAt >= 0 ? stableAt.ToString() : "None")}");
            }
        }

        static void Setup()
        {
            var configData = File.ReadAllText("../../../config.json");
            settings = JsonSerializer.Deserialize<Config>(configData);
            gameBoard = new Board(settings.width, settings.height, 1, settings.liveDensity);
        }

        static void Display()
        {
            for (int y = 0; y < gameBoard.Rows; y++)
            {
                for (int x = 0; x < gameBoard.Columns; x++)
                    Console.Write(gameBoard.Grid[x, y].State ? '*' : ' ');
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("R - Random start");
            Console.WriteLine("L - Load from file");

            while (true)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.R) { Setup(); break; }
                if (key == ConsoleKey.L)
                {
                    if (File.Exists("../../../Board.txt")) InitFromFile("../../../Board.txt");
                    else Setup();
                    break;
                }
            }

            while (true)
            {
                Console.Clear();
                Display();
                gameBoard.NextGeneration();

                Console.WriteLine("[S]ave  [E]xperiment  [P]atterns");

                if (Console.KeyAvailable)
                {
                    var input = Console.ReadKey(true).Key;
                    if (input == ConsoleKey.S) { SaveToFile("../../../Board.txt"); Console.WriteLine("Saved."); }
                    else if (input == ConsoleKey.E) { PerformStudy(); Console.WriteLine("Experiment complete."); break; }
                    else if (input == ConsoleKey.P)
                    {
                        Console.WriteLine($"Alive: {gameBoard.CountAlive()}");
                        Console.WriteLine($"Groups: {gameBoard.CountGroups()}");

                        foreach (var kv in gameBoard.CountTemplates())
                            Console.WriteLine($"{kv.Key}: {kv.Value}");

                        Console.WriteLine("C - Continue   E - Exit");
                        while (true)
                        {
                            var next = Console.ReadKey(true).Key;
                            if (next == ConsoleKey.C) break;
                            if (next == ConsoleKey.E) return;
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}

