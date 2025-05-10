using cli_life;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Text.Json;
using Xunit;

namespace Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var board = new Board(50, 20, 5, 0.5);
            Assert.Equal(10, board.Columns);
            Assert.Equal(4, board.Rows);
            Assert.Equal(5, board.CellSize);
        }
        [Fact]
        public void Test2()
        {
            var board = new Board(72, 34, 1, 1);
            int AliveCells = board.CountAliveCells();
            Assert.Equal(board.Rows * board.Columns, AliveCells);
        }
        [Fact]
        public void Test3()
        {
            var board = new Board(72, 34, 1, 1);
            board.Advance();
            int AliveCells = board.CountAliveCells();
            Assert.Equal(0, AliveCells);
        }
        [Fact]
        public void Test4()
        {
            var board = new Board(4, 4, 1, 0.0);
            board.Cells[0, 1].IsAlive = true;
            board.Cells[1, 0].IsAlive = true;
            board.Cells[1, 2].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;
            var patterns = board.CountPatterns();
            Assert.True(patterns["Tub"] == 1);
        }
        [Fact]
        public void Test5()
        {
            var board = new Board(10, 10, 1, 0.0);
            board.Cells[0, 1].IsAlive = true;
            board.Cells[1, 0].IsAlive = true;
            board.Cells[1, 2].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;

            board.Cells[3, 3].IsAlive = true;
            board.Cells[3, 4].IsAlive = true;
            board.Cells[4, 3].IsAlive = true;
            board.Cells[4, 4].IsAlive = true;

            var patterns = board.CountPatterns();
            int count = patterns.Values.Sum();
            Assert.Equal(2, count);

        }
        [Fact]
        public void Test6()
        {
            var board = new Board(4, 4, 1, 0.0);
            board.Cells[0, 1].IsAlive = true;
            board.Cells[1, 0].IsAlive = true;
            board.Cells[1, 2].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;
            board.Advance();
            Assert.True(board.Cells[0, 1].IsAlive);
            Assert.True(board.Cells[1, 0].IsAlive);
            Assert.True(board.Cells[1, 2].IsAlive);
            Assert.True(board.Cells[2, 1].IsAlive);
        }
        [Fact]
        public void Test7()
        {
            var jsonFile = "{\"width\": 50,\"height\": 20,\"liveDensity\": 0.5}";
            var config = JsonSerializer.Deserialize<Config>(jsonFile);
            Assert.Equal(50, config.width);
            Assert.Equal(20, config.height);
            Assert.Equal(0.5, config.liveDensity);
        }
        [Fact]
        public void Test8()
        {
            var board = new Board(100, 100, 1, 0.5);
            board.Advance();
            int AliveCells = board.CountAliveCells();
            Assert.True(AliveCells >= 0);
        }
        [Fact]
        public void Test9()
        {
            var board = new Board(5, 5, 1, 0);
            board.Cells[1, 2].IsAlive = true;
            board.Cells[2, 2].IsAlive = true;
            board.Cells[3, 2].IsAlive = true;
            board.Advance();
            Assert.True(board.Cells[2, 1].IsAlive);
            Assert.True(board.Cells[2, 2].IsAlive);
            Assert.True(board.Cells[2, 3].IsAlive);
        }
        [Fact]
        public void Test10()
        {
            var board = new Board(5, 5, 1, 0);
            board.Cells[1, 1].IsAlive = true;
            board.Cells[2, 3].IsAlive = true;
            board.Cells[3, 1].IsAlive = true;
            board.Advance();
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (i == 2 && j == 2)
                    {
                        Assert.True(board.Cells[i, j].IsAlive);
                    }
                    else
                    {
                        Assert.False(board.Cells[i, j].IsAlive);
                    }
                }
            }
        }
        [Fact]
        public void Test11()
        {
            var board = new Board(10, 5, 1);
            board.Randomize(0);
            int count = board.CountAliveCells();
            Assert.Equal(0, count);

            board.Randomize(1);
            count = board.CountAliveCells();
            Assert.Equal(board.Columns * board.Rows, count);
        }
        [Fact]
        public void Test12()
        {
            var board = new Board(10, 10, 1, 0);
            int firstCount = board.CountAliveCells();
            board.Randomize(0.7);
            int secondCount = board.CountAliveCells();
            Assert.NotEqual(firstCount, secondCount);
        }
        [Fact]
        public void Test13()
        {
            var board = new Board(100, 100, 10);
            foreach (var cell in board.Cells)
            {
                Assert.Equal(8, cell.neighbors.Count);
            }
        }
        [Fact]
        public void Test14()
        {
            var cell = new Cell { IsAlive = true };
            for (int i = 0; i < 1; i++) cell.neighbors.Add(new Cell { IsAlive = true });
            for (int i = 0; i < 7; i++) cell.neighbors.Add(new Cell { IsAlive = false });

            cell.DetermineNextLiveState();
            cell.Advance();
            Assert.False(cell.IsAlive);
        }
        [Fact]
        public void Test15()
        {
            var board = new Board(10, 10, 1, 0.0);
            var patterns = board.CountPatterns();
            int count = patterns.Values.Sum();
            Assert.Equal(0, count);
        }
        [Fact]
        public void Test16()
        {
            var board = new Board(30, 30, 1, 0);
            board.Cells[5, 7].IsAlive = true;
            board.Cells[2, 1].IsAlive = true;
            board.Cells[11, 4].IsAlive = true;

            int count = board.CountAliveCells();
            Assert.Equal(3, count);
        }
    }
}
