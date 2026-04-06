using Xunit;
using Xunit.Abstractions;
using Lab01.Logic;
using Lab01.Logic.Simplex;
using Lab01.Logic.Models;

namespace Lab01.Tests
{
    public class SimplexSolverTests
    {
        private readonly ITestOutputHelper _output;
        private readonly JordanSolver _jordan;
        private readonly BasicSolutionFinder _basicFinder;

        public SimplexSolverTests(ITestOutputHelper output)
        {
            _output = output;
            _jordan = new JordanSolver();
            _basicFinder = new BasicSolutionFinder(_jordan);
        }

        [Fact]
        public void Task1_Maximization_Output()
        {
            double[] vectorZ = { -1, -2, 1, 1 };
            double[,] matrixA = {
                { 1, 1, -1, -2 },
                { -1, -1, -1, 1 },
                { 2, -1, 3, 4 }
            };
            double[] vectorB = { 6, -5, 10 };

            var optimalFinder = new OptimalSolutionFinder(_jordan, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder);

            var result = solver.Solve(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);

            _output.WriteLine("=== TASK 1 (MAX) ===");
            _output.WriteLine($"Z = {result.Z}");
            for (int i = 0; i < result.X.Length; i++) _output.WriteLine($"X{i + 1} = {result.X[i]}");
        }

        [Fact]
        public void Task2_Minimization_Output()
        {
            double[] vectorZ = { -2, 3, 0, -3 };
            double[,] matrixA = {
                { 1, 1, -1, -2 },
                { -1, -1, -1, 1 },
                { 2, -1, 3, 4 }
            };
            double[] vectorB = { 6, -5, 10 };

            var optimalFinder = new OptimalSolutionFinder(_jordan, OptimizationMode.Minimization);
            var solver = new MinimizationSolver(_basicFinder, optimalFinder);

            var result = solver.Solve(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);

            _output.WriteLine("=== TASK 2 (MIN) ===");
            _output.WriteLine($"Z = {result.Z}");
            for (int i = 0; i < result.X.Length; i++) _output.WriteLine($"X{i + 1} = {result.X[i]}");
        }
    }
}