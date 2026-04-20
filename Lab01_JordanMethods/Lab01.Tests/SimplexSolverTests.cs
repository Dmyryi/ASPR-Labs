using Xunit;
using Xunit.Abstractions;
using Lab01.Logic;
using Lab01.Logic.Simplex;

namespace Lab01.Tests
{
    public class SimplexSolverTests
    {
        private const int Precision = 15;
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
        public void Task1_Maximization_ReturnsExpectedSolution()
        {
            double[] vectorZ = { -1, -2, 1, 1 };
            double[,] matrixA = {
                { 1, 1, -1, -2 },
                { -1, -1, -1, 1 },
                { 2, -1, 3, 4 }
            };
            double[] vectorB = { 6, -5, 10 };
            double[] expectedX = { 0d, 22d, 0d, 8d };
            const double expectedZ = 36d;

            var optimalFinder = new OptimalSolutionFinder(_jordan, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder);
            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            LogResult("TASK 1 (MAX)", result.X, result.Z, expectedX, expectedZ);

            Assert.True(result.Success);
            AssertVector(expectedX, result.X);
            Assert.Equal(expectedZ, result.Z, Precision);
        }

        [Fact]
        public void Task2_Minimization_ReturnsExpectedSolution()
        {
            double[] vectorZ = { -2, 3, 0, -3 };
            double[,] matrixA = {
                { 1, 1, -1, -2 },
                { -1, -1, -1, 1 },
                { 2, -1, 3, 4 }
            };
            double[] vectorB = { 6, -5, 10 };
            double[] expectedX = { 5d, 0d, 0d, 0d };
            const double expectedZ = -10d;

            var optimalFinder = new OptimalSolutionFinder(_jordan, OptimizationMode.Minimization);
            var solver = new MinimizationSolver(_basicFinder, optimalFinder);
            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            LogResult("TASK 2 (MIN)", result.X, result.Z, expectedX, expectedZ);

            Assert.True(result.Success);
            AssertVector(expectedX, result.X);
            Assert.Equal(expectedZ, result.Z, Precision);
        }

        private void LogResult(string title, double[] actualX, double actualZ,
            double[] expectedX, double expectedZ)
        {
            _output.WriteLine($"=== {title} ===");
            _output.WriteLine($"Actual Z:   {actualZ}");
            _output.WriteLine($"Expected Z: {expectedZ}");
            _output.WriteLine($"Actual X:   ({string.Join(", ", actualX)})");
            _output.WriteLine($"Expected X: ({string.Join(", ", expectedX)})");
        }

        private static void AssertVector(double[] expected, double[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
                Assert.Equal(expected[i], actual[i], Precision);
        }
    }
}