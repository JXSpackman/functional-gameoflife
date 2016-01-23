using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameOfLife
{
    public static class Program
    {
        static void Main(string[] args)
        {
            // A selection from https://en.wikipedia.org/wiki/Life-like_cellular_automaton
            Console.WriteLine("Choose conditions:");
            Console.WriteLine("A: Standard");
            Console.WriteLine("B: High Life");
            Console.WriteLine("C: Day and Night");
            Console.WriteLine("D: Life without Death");
            Console.WriteLine("E: Seeds");

            var ch = Console.ReadKey().KeyChar;
            var conditions = GetConditionsFromUserChoice(ch);
            var grid = GetRandomStartGrid();

            Console.Clear();
            GameOfLife.Print(Console.WriteLine, grid, 0);
            Console.WriteLine("Press Enter To Begin");
            Console.ReadLine();

            GameOfLife.Run(
                grid: grid,
                iterations: 1000,
                applyConditions: conditions,
                print: (gridToPrint, iteration) => GameOfLife.Print(Console.WriteLine, gridToPrint, iteration, clear: Console.Clear),
                postIteration: () => Task.Delay(250).Wait());

            Console.ReadLine();
        }

        private static IEnumerable<Cell> GetRandomStartGrid()
        {
            bool[,] array = new bool[6, 12];
            var rnd = new Random();
            var numberOfCells = RandomRangeInclusive(rnd, 10, 25);
            for (int i = 0; i < numberOfCells; i++)
            {
                var x = RandomRangeInclusive(rnd, 0, 5);
                var y = RandomRangeInclusive(rnd, 0, 11);
                array[x, y] = true;
            }

            return GameOfLife.GetGrid(array);
        }

        private static int RandomRangeInclusive(Random rnd, int start, int finish)
        {
            return start + rnd.Next(finish - start + 1);
        }

        private static Func<bool, int, bool> GetConditionsFromUserChoice(char ch)
        {
            var conditions = GameOfLife.DefaultApplyConditions();
            switch (ch)
            {
                case 'b':
                case 'B':
                    conditions = GameOfLife.ApplyConditionsFactory(new List<int> { 3, 6 }, new List<int> { 2, 3 });
                    break;
                case 'c':
                case 'C':
                    conditions = GameOfLife.ApplyConditionsFactory(new List<int> { 3, 6, 7, 8 }, new List<int> { 3, 4, 6, 7, 8 });
                    break;
                case 'd':
                case 'D':
                    conditions = GameOfLife.ApplyConditionsFactory(new List<int> { 3 }, new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 });
                    break;
                case 'e':
                case 'E':
                    conditions = GameOfLife.ApplyConditionsFactory(new List<int> { 2 }, new List<int>());
                    break;
            }

            return conditions;
        }
    }
}
