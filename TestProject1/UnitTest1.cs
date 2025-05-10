using System.Linq;
using Xunit;
using cli_life;

namespace cli_life_tests
{
    public class BoardTests
    {
        [Fact]
        public void Populate_ShouldRespectDensity_Low()
        {
            var board = new Board(10, 10, 1, 0.1);
            int alive = board.CountAlive();
            Assert.InRange(alive, 0, 20);
        }

        [Fact]
        public void Populate_ShouldRespectDensity_High()
        {
            var board = new Board(10, 10, 1, 0.9);
            int alive = board.CountAlive();
            Assert.InRange(alive, 80, 100);
        }

        [Fact]
        public void CountAlive_ShouldBeZero_WhenNoCellsAlive()
        {
            var board = new Board(10, 10, 1, 0);
            Assert.Equal(0, board.CountAlive());
        }

        [Fact]
        public void CountAlive_ShouldBeMax_WhenAllAlive()
        {
            var board = new Board(10, 10, 1, 1);
            Assert.Equal(100, board.CountAlive());
        }

        [Fact]
        public void CountGroups_ShouldReturnZero_WhenEmpty()
        {
            var board = new Board(10, 10, 1, 0);
            Assert.Equal(0, board.CountGroups());
        }

        [Fact]
        public void CountGroups_ShouldDetectOneGroup()
        {
            var board = new Board(5, 5, 1, 0);
            board.Grid[2, 2].State = true;
            board.Grid[2, 3].State = true;
            Assert.Equal(1, board.CountGroups());
        }

        [Fact]
        public void CountGroups_ShouldDetectMultipleGroups()
        {
            var board = new Board(10, 10, 1, 0);
            board.Grid[1, 1].State = true;
            board.Grid[8, 8].State = true;
            Assert.Equal(2, board.CountGroups());
        }

        [Fact]
        public void NextGeneration_ShouldMaintainStableBlock()
        {
            var board = new Board(5, 5, 1, 0);
            board.Grid[1, 1].State = true;
            board.Grid[1, 2].State = true;
            board.Grid[2, 1].State = true;
            board.Grid[2, 2].State = true;

            board.NextGeneration();

            Assert.True(board.Grid[1, 1].State);
            Assert.True(board.Grid[1, 2].State);
            Assert.True(board.Grid[2, 1].State);
            Assert.True(board.Grid[2, 2].State);
        }

        [Fact]
        public void CountTemplates_ShouldDetectBlock()
        {
            var board = new Board(5, 5, 1, 0);
            board.Grid[1, 1].State = true;
            board.Grid[1, 2].State = true;
            board.Grid[2, 1].State = true;
            board.Grid[2, 2].State = true;

            var templates = board.CountTemplates();
            Assert.True(templates["Block"] >= 1);
        }

        [Fact]
        public void CountTemplates_ShouldDetectBeehive()
        {
            var board = new Board(6, 6, 1, 0);
            foreach (var (dx, dy) in new[] { (0, 1), (1, 0), (1, 2), (2, 0), (2, 2), (3, 1) })
                board.Grid[dx, dy].State = true;

            var templates = board.CountTemplates();
            Assert.True(templates["Beehive"] >= 1);
        }

        [Fact]
        public void CountTemplates_ShouldReturnZeroForUnknownPattern()
        {
            var board = new Board(5, 5, 1, 0);
            var templates = board.CountTemplates();
            Assert.All(templates.Values, count => Assert.Equal(0, count));
        }

        [Fact]
        public void Cell_ShouldSurvive_WithTwoOrThreeNeighbors()
        {
            var cell = new Cell { State = true };
            cell.Adjacent.AddRange(new[] {
                new Cell { State = true },
                new Cell { State = true },
                new Cell { State = false }
            });

            cell.Evaluate();
            cell.Update();
            Assert.True(cell.State);
        }

        [Fact]
        public void Cell_ShouldDie_WithFewerThanTwoNeighbors()
        {
            var cell = new Cell { State = true };
            cell.Adjacent.Add(new Cell { State = true });

            cell.Evaluate();
            cell.Update();
            Assert.False(cell.State);
        }

        [Fact]
        public void Cell_ShouldBeBorn_WithExactlyThreeNeighbors()
        {
            var cell = new Cell { State = false };
            cell.Adjacent.AddRange(new[] {
                new Cell { State = true },
                new Cell { State = true },
                new Cell { State = true }
            });

            cell.Evaluate();
            cell.Update();
            Assert.True(cell.State);
        }

        [Fact]
        public void Cell_ShouldStayDead_WithNotExactlyThreeNeighbors()
        {
            var cell = new Cell { State = false };
            cell.Adjacent.AddRange(new[] {
                new Cell { State = true },
                new Cell { State = false },
                new Cell { State = false }
            });

            cell.Evaluate();
            cell.Update();
            Assert.False(cell.State);
        }

        [Fact]
        public void CountTemplates_ShouldDetectLoaf()
        {
            var board = new Board(6, 6, 1, 0);
            foreach (var (dx, dy) in new[] { (0, 1), (1, 0), (1, 2), (2, 0), (2, 3), (3, 1), (3, 2) })
                board.Grid[dx, dy].State = true;

            var templates = board.CountTemplates();
            Assert.True(templates["Loaf"] >= 1);
        }
    }
}
