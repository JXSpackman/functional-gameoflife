using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace GameOfLife.Tests
{
    [TestFixture]
    public class GameOfLifeTests
    {
        [Test, TestCaseSource(nameof(IsNextToTests))]
        public void OnGridTest(Cell otherCell, IResolveConstraint expectation)
        {
            var cell = new Cell(3, 4, true);
            Assert.That(cell.IsNextTo(otherCell), expectation);
        }

        private static IEnumerable<TestCaseData> IsNextToTests()
        {
            yield return new TestCaseData(new Cell(1, 1, false), Is.False).SetName("IsNextTo_NonNeighbour_False");
            yield return new TestCaseData(new Cell(3, 4, false), Is.False).SetName("IsNextTo_Self_False");
            yield return new TestCaseData(new Cell(1, 4, false), Is.False).SetName("IsNextTo_SameLine_False");
            yield return new TestCaseData(new Cell(2, 2, false), Is.False).SetName("IsNextTo_KnightMoveAway_False");

            yield return new TestCaseData(new Cell(3, 3, false), Is.True).SetName("IsNextTo_Right_True");
            yield return new TestCaseData(new Cell(3, 5, false), Is.True).SetName("IsNextTo_Left_True");
            yield return new TestCaseData(new Cell(2, 4, false), Is.True).SetName("IsNextTo_Above_True");
            yield return new TestCaseData(new Cell(4, 4, false), Is.True).SetName("IsNextTo_Below_True");
            yield return new TestCaseData(new Cell(2, 3, false), Is.True).SetName("IsNextTo_Diagonal_True");
        }

        [Test]
        public void Run_IteratedGridResult_IterationResultPassedToPrint()
        {
            var grid = GameOfLife.GetGrid(new bool[2, 2]);
            var iteratedGrid = GameOfLife.GetGrid(new bool[2, 2]);
            int iterations = 6;
            int actualIterations = 0;
            IEnumerable<Cell> actualGrid = null;
            
            Func<bool, int, bool> apply = (state, neighbours) => { return false; };
            Action<IEnumerable<Cell>, int> print = (gridToPrint, iteration) => { actualGrid = gridToPrint; actualIterations++; };
            
            GameOfLife.Run(grid, iterations, apply, print);

            Assert.That(actualIterations, Is.EqualTo(iterations));
            Assert.That(actualGrid, Is.EqualTo(iteratedGrid));
            Assert.That(actualGrid, Is.Not.SameAs(grid)); 
        }

        [Test, TestCaseSource(nameof(ApplyConditionsTest))]
        public void Run_ApplyConditionsCalledForEachCellAndIteration(IEnumerable<Cell> grid)
        {
            int numberOfCellsInGrid = grid.Count();
            int iterations = 3;
            int expectedApplications = iterations * numberOfCellsInGrid;
            int applyConditionsCalledTimes = 0;

            Func<bool, int, bool> apply = (state, neighbours) => {
                applyConditionsCalledTimes++;
                return false;
            };

            GameOfLife.Run(grid, iterations, apply, (bools, i) => { });

            Assert.That(applyConditionsCalledTimes, Is.EqualTo(expectedApplications));
        }

        private static IEnumerable<TestCaseData> ApplyConditionsTest()
        {
            yield return new TestCaseData(GameOfLife.GetGrid(new bool[1, 1])).SetName("ApplyConditionsTest_1By1Grid");
            yield return new TestCaseData(GameOfLife.GetGrid(new bool[2, 2])).SetName("ApplyConditionsTest_2By2Grid");
            yield return new TestCaseData(GameOfLife.GetGrid(new bool[3, 4])).SetName("ApplyConditionsTest_3By4Grid");
        }

        [Test]
        public void Neighbours_GridCellWithNoNeighbours_EmptyCollectionReturned()
        {
            var grid = GameOfLife.GetGrid(new bool[4, 4]);
            IEnumerable<Cell> neighbours = grid.Neighbours(new Cell(2, 2, false));

            Assert.That(neighbours.Count, Is.EqualTo(0));
        }

        [Test]
        public void Neighbours_GridCellWith3Neighbours_CollectionWith3CellsReturned()
        {
            var array = new bool[4, 4];
            array[1, 1] = true;
            array[1, 2] = true;
            array[2, 1] = true;
            var grid = GameOfLife.GetGrid(array);
            List<Cell> neighbours = grid.Neighbours(new Cell(2, 2, false));

            Assert.That(neighbours.Count, Is.EqualTo(3));
        }
        
        [Test]
        public void Print_WithPopulatedGrid_OutputIsFormatted()
        {
            bool[,] array = new bool[2, 2];
            array[0, 0] = true;
            array[1, 1] = true;
            IEnumerable<Cell> grid = GameOfLife.GetGrid(array);
            List<string> outputLines = new List<string>(3);
            Action<string> writeLine = line => outputLines.Add(line);

            GameOfLife.Print(writeLine, grid, 1);

            Assert.That(outputLines[0], Is.EqualTo("Iteration 1"));
            Assert.That(outputLines[1], Is.EqualTo("x."));
            Assert.That(outputLines[2], Is.EqualTo(".x"));
        }

        [Test, TestCaseSource(nameof(ApplyConditionsFactoryTestsAllDie))]
        public void ApplyConditionsFactory_BehavesAsSpecifiedWhenAllDieCondition(int neighbours, bool startState, bool expectedState)
        {
            var applyFunction = GameOfLife.ApplyConditionsFactory(new List<int>(), new List<int>());
            var actualState = applyFunction(startState, neighbours);
            Assert.That(actualState, Is.EqualTo(expectedState));
        }

        private static IEnumerable<TestCaseData> ApplyConditionsFactoryTestsAllDie()
        {
            for (int neighbours = 0; neighbours <= 8; neighbours++)
            {
                yield return new TestCaseData(neighbours, false, false).SetName($"ApplyConditionsFactoryTests_AllDie_StaysDead_{neighbours}");
                yield return new TestCaseData(neighbours, true, false).SetName($"ApplyConditionsFactoryTests_AllDie_Dies_{neighbours}");
            }
        }

        [Test, TestCaseSource(nameof(ApplyConditionsFactoryTestsStandard))]
        public void ApplyConditionsFactory_BehavesAsSpecifiedWithStandardRules(int neighbours, bool startState, bool expectedState)
        {
            var applyFunction = GameOfLife.ApplyConditionsFactory(new List<int> { 3 }, new List<int> { 2, 3 });
            var actualState = applyFunction(startState, neighbours);
            Assert.That(actualState, Is.EqualTo(expectedState));
        }

        private static IEnumerable<TestCaseData> ApplyConditionsFactoryTestsStandard()
        {
            yield return new TestCaseData(1, false, false).SetName($"ApplyConditionsFactoryTestsStandard_Born_1");
            yield return new TestCaseData(2, false, false).SetName($"ApplyConditionsFactoryTestsStandard_Born_2");
            yield return new TestCaseData(3, false, true).SetName($"ApplyConditionsFactoryTestsStandard_Born_3");
            yield return new TestCaseData(4, false, false).SetName($"ApplyConditionsFactoryTestsStandard_Born_4");

            yield return new TestCaseData(1, true, false).SetName($"ApplyConditionsFactoryTestsStandard_Stay_1");
            yield return new TestCaseData(2, true, true).SetName($"ApplyConditionsFactoryTestsStandard_Stay_2");
            yield return new TestCaseData(3, true, true).SetName($"ApplyConditionsFactoryTestsStandard_Stay_3");
            yield return new TestCaseData(4, true, false).SetName($"ApplyConditionsFactoryTestsStandard_Stay_4");
        }

    }
}
