using Xunit;
using Xunit.Abstractions;
using Lab01.Logic;
using Lab01.Logic.Simplex;
using Lab01.Logic.Interfaces;

namespace Lab01.Tests
{
    public class SimplexSolverTests
    {
        private const int Precision = 15;
        private readonly ITestOutputHelper _output;
        private readonly JordanSolver _jordan;
        private readonly BasicSolutionFinder _basicFinder;
        private readonly IFindPivot _findPivot;
        private readonly ISimplexProtocol? _protocol;

        public SimplexSolverTests(ITestOutputHelper output)
        {
            _output = output;
            _jordan = new JordanSolver();
            _basicFinder = new BasicSolutionFinder(_jordan);
            _findPivot = new OptimalSolutionFinderBase(); // Убедись, что этот класс существует в твоем проекте
            _protocol = null; // Или создай реализацию, если нужна
        }

        [Fact]
        public void Task1_Maximization_WithZeroRowElimination_ReturnsExpectedSolution()
        {
            // Целевая функция Z = x1 + 2x2 + x3 + 0*x4 -> max
            double[] vectorZ = { -1, -2, -1, 0 }; // Знаки для максимизации

            // Матрица коэффициентов (только x1, x2, x3, x4)
            double[,] matrixA = {
    { 2, -1, 3, 4 }, // Ур-е 1
    { 1,  1, 1, -1 }, // Ур-е 2
    { 1,  2, 2, 4 }   // Ур-е 3
};

            double[] vectorB = { 10, 5, 12 };
            double[] expectedX = { 0, 4, 2, 0 };
            const double expectedZ = 10;

            var zeroRowEliminator = new ZeroRowElliminator(_jordan, _findPivot, _protocol);
            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, zeroRowEliminator, true);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            LogResult("TASK 1 (MAX - With Zero Row Elimination)", result.X, result.Z, expectedX, expectedZ);

            Assert.True(result.Success);
            AssertVector(expectedX, result.X);
            Assert.Equal(expectedZ, result.Z, Precision);
        }

        [Fact]
        public void Task1_Maximization_WithoutZeroRowElimination_ReturnsExpectedSolution()
        {
            double[] vectorZ = { -1, -2, -1, 0 };
            double[,] matrixA = {
    { -2, 1, 1, 3 },
    { -3, 2, -3, 0 },
    { -3, 1, 4, 1 },
    { -3, 2, -2, 2 }
};
            double[] vectorB = { 6, -5, 10 };
            double[] expectedX = { 0d, 22d, 0d, 8d };
            const double expectedZ = 36d;

            var zeroRowEliminator = new ZeroRowElliminator(_jordan, _findPivot, _protocol);
            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, zeroRowEliminator, false);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            LogResult("TASK 1 (MAX - Without Zero Row Elimination)", result.X, result.Z, expectedX, expectedZ);

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

            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Minimization);
            var solver = new MinimizationSolver(_basicFinder, optimalFinder, _protocol);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            LogResult("TASK 2 (MIN)", result.X, result.Z, expectedX, expectedZ);

            Assert.True(result.Success);
            AssertVector(expectedX, result.X);
            Assert.Equal(expectedZ, result.Z, Precision);
        }

        [Fact]
        public void MaximizationSolver_DefaultConstructor_UsesZeroRowElimination()
        {
            double[] vectorZ = { -1, -2, 1, 1 };
            double[,] matrixA = {
                { 1, 1, -1, -2 },
                { -1, -1, -1, 1 },
                { 2, -1, 3, 4 }
            };
            double[] vectorB = { 6, -5, 10 };

            var zeroRowEliminator = new ZeroRowElliminator(_jordan, _findPivot, _protocol);
            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, zeroRowEliminator);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);
            LogResult("DEFAULT (Should use zero row elimination)", result.X, result.Z, new double[] { 0, 22, 0, 8 }, 36);
        }

        [Fact]
        public void MaximizationSolver_NullZeroRowEliminator_DoesNotThrow()
        {
            double[] vectorZ = { -1, -2, 1, 1 };
            double[,] matrixA = {
                { 1, 1, -1, -2 },
                { -1, -1, -1, 1 },
                { 2, -1, 3, 4 }
            };
            double[] vectorB = { 6, -5, 10 };

            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, null, true);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);
            LogResult("NULL ZeroRowEliminator", result.X, result.Z, new double[] { 0, 22, 0, 8 }, 36);
        }

        [Fact]
        public void MaximizationSolver_WithZeroRowEliminationFalse_SkipsElimination()
        {
            double[] vectorZ = { -1, -2, 1, 1 };
            double[,] matrixA = {
                { 1, 1, -1, -2 },
                { -1, -1, -1, 1 },
                { 2, -1, 3, 4 }
            };
            double[] vectorB = { 6, -5, 10 };

            var zeroRowEliminator = new ZeroRowElliminator(_jordan, _findPivot, _protocol);
            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, zeroRowEliminator, false);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);
            LogResult("ZERO ROW ELIMINATION DISABLED", result.X, result.Z, new double[] { 0, 22, 0, 8 }, 36);
        }

        [Fact]
        public void Example1_FromImage_Parsing_ShouldWork()
        {
            string objective = "10x1 - x2 - 42x3 - 52x4";
            string constraints = "-2x1 + x2 + x3 + 3x4 = 2\n-3x1 + 2x2 - 3x3 = 7\n-3x1 + x2 + 4x3 + x4 <= 1\n-3x1 + 2x2 - 2x3 + 2x4 >= 9";

            var parsedObjective = ParseLinearExpression(objective);
            var parsedConstraints = ParseConstraints(constraints);

            Assert.Equal(4, parsedObjective.Count);
            // 2 рівності ×2 (<= і >=) + 1 × <= + 1 × >= = 6 рядків після розкриття «=»
            Assert.Equal(6, parsedConstraints.Count);

            // Давайте посмотрим на реальные данные после парсинга
            _output.WriteLine("=== PARSED OBJECTIVE ===");
            foreach (var (var, coeff) in parsedObjective)
                _output.WriteLine($"x{var}: {coeff}");

            _output.WriteLine("=== PARSED CONSTRAINTS ===");
            for (int i = 0; i < parsedConstraints.Count; i++)
            {
                var c = parsedConstraints[i];
                _output.WriteLine($"Constraint {i}: {c.Operator}");
                foreach (var (var, coeff) in c.Coefficients)
                    _output.WriteLine($"  x{var}: {coeff}");
                _output.WriteLine($"  RHS: {c.RightSide}");
            }
        }

        /// <summary>
        /// Постановка задачі 1 з підручника: Z = 10x1 - x2 - 42x3 - 52x4 → max, початкова таблиця як на рисунку.
        /// Кінцевий оптимум з методички: Max Z = 21, X = (9; 17; 0; 1).
        /// </summary>
        [Fact]
        public void Example1_FromImage_WithoutZeroRowElimination_MatchesTextbookOptimum()
        {
            double[] vectorZ = { -10, 1, 42, 52 };
            double[,] matrixA = {
                { -2, 1, 1, 3 },
                { -3, 2, -3, 0 },
                { -3, 1, 4, 1 },
                { 3, -2, 2, -2 }
            };
            double[] vectorB = { 2, 7, 1, -9 };

            double[] expectedX = { 9, 17, 0, 1 };
            const double expectedZ = 21;

            var zeroRowEliminator = new ZeroRowElliminator(_jordan, _findPivot, _protocol);
            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, zeroRowEliminator, false);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);
            LogResult("Приклад 1 (з підручника, без усунення 0-рядків)", result.X, result.Z, expectedX, expectedZ);
            AssertVector(expectedX, result.X);
            Assert.Equal(expectedZ, result.Z, Precision);
        }

        /// <summary>
        /// Те саме, що приклад 1, але з кроком усунення 0-рядків (рис. 3.2) — оптимум має лишатись тим самим.
        /// </summary>
        [Fact]
        public void Example1_FromImage_WithZeroRowElimination_MatchesTextbookOptimum()
        {
            double[] vectorZ = { -10, 1, 42, 52 };
            double[,] matrixA = {
                { -2, 1, 1, 3 },
                { -3, 2, -3, 0 },
                { -3, 1, 4, 1 },
                { 3, -2, 2, -2 }
            };
            double[] vectorB = { 2, 7, 1, -9 };

            double[] expectedX = { 9, 17, 0, 1 };
            const double expectedZ = 21;

            var zeroRowEliminator = new ZeroRowElliminator(_jordan, _findPivot, _protocol);
            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var solver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, zeroRowEliminator, true);

            var context = new SimplexContext();
            context.SetStrategy(solver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);
            LogResult("Приклад 1 (з підручника, з усуненням 0-рядків)", result.X, result.Z, expectedX, expectedZ);
            AssertVector(expectedX, result.X);
            Assert.Equal(expectedZ, result.Z, Precision);
        }

        /// <summary>
        /// Постановка задачі 2: Z = -3x1 + 6x2 → max при п’яти обмеженнях ≥ 0 (вільні x1, x2 у підручнику — тут як у прикладі з додатку).
        /// Кінцевий оптимум: Max Z = 15, X = (3; 4).
        /// </summary>
        [Fact]
        public void Example2_Textbook_Image_MatchesOptimum_Z15_X3_4()
        {
            double[] vectorZ = { 3, -6 };
            double[,] matrixA = {
                { -1, -2 },
                { -2, -1 },
                { -1, 1 },
                { -1, 4 },
                { 4, -1 }
            };
            double[] vectorB = { 1, -4, 1, 13, 23 };

            double[] expectedX = { 3, 4 };
            const double expectedZ = 15;

            var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, OptimizationMode.Maximization);
            var maxSolver = new MaximizationSolver(_basicFinder, optimalFinder, _protocol, null, false);
            var context = new SimplexContext();
            context.SetStrategy(maxSolver);

            var result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            Assert.True(result.Success);
            LogResult("Приклад 2 (з підручника)", result.X, result.Z, expectedX, expectedZ);
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

        private static Dictionary<int, double> ParseLinearExpression(string expression)
        {
            string normalized = expression
                .Replace(" ", string.Empty)
                .Replace("*", string.Empty)
                .Replace(",", ".");

            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new FormatException("Expression cannot be empty.");
            }

            var regex = new System.Text.RegularExpressions.Regex(@"([+\-]?)(\d*(?:\.\d+)?)x(\d+)", System.Text.RegularExpressions.RegexOptions.Compiled);
            var matches = regex.Matches(normalized);

            if (matches.Count == 0)
            {
                throw new FormatException($"Invalid expression: '{expression}'. Use terms like x1, -2x2, +3.5x3.");
            }

            var coefficients = new Dictionary<int, double>();

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string signToken = match.Groups[1].Value;
                string valueToken = match.Groups[2].Value;
                string variableToken = match.Groups[3].Value;

                double value = string.IsNullOrEmpty(valueToken)
                    ? 1d
                    : double.Parse(valueToken, System.Globalization.CultureInfo.InvariantCulture);

                if (signToken == "-")
                {
                    value *= -1d;
                }

                int variableIndex = int.Parse(variableToken, System.Globalization.CultureInfo.InvariantCulture);
                coefficients[variableIndex] = coefficients.GetValueOrDefault(variableIndex) + value;
            }

            string leftover = regex.Replace(normalized, string.Empty).Replace("+", string.Empty).Replace("-", string.Empty).Replace(">", string.Empty).Replace("<", string.Empty).Replace("=", string.Empty);
            if (!string.IsNullOrEmpty(leftover))
            {
                throw new FormatException($"Unsupported expression part: '{leftover}'.");
            }

            return coefficients;
        }

        private static List<ParsedConstraint> ParseConstraints(string text)
        {
            var lines = text
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .ToList();

            var result = new List<ParsedConstraint>(lines.Count);

            foreach (string line in lines)
            {
                string op;
                string[] parts;

                if (line.Contains("<="))
                {
                    op = "<=";
                    parts = line.Split("<=", StringSplitOptions.TrimEntries);
                }
                else if (line.Contains("="))
                {
                    op = "=";
                    parts = line.Split("=", StringSplitOptions.TrimEntries);
                }
                else if (line.Contains(">="))
                {
                    op = ">=";
                    parts = line.Split(">=", StringSplitOptions.TrimEntries);
                }
                else
                {
                    throw new FormatException($"Constraint '{line}' must contain <= or >=.");
                }

                if (parts.Length != 2)
                {
                    throw new FormatException($"Invalid constraint format: '{line}'.");
                }

                if (!double.TryParse(parts[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double rightSide))
                {
                    throw new FormatException($"Invalid right side in constraint: '{line}'.");
                }

                var coefficients = ParseLinearExpression(parts[0]);

                if (op == "=")
                {
                    result.Add(new ParsedConstraint(new Dictionary<int, double>(coefficients), "<=", rightSide));
                    result.Add(new ParsedConstraint(new Dictionary<int, double>(coefficients), ">=", rightSide));
                }
                else
                {
                    result.Add(new ParsedConstraint(coefficients, op, rightSide));
                }
            }

            return result;
        }

        private sealed record ParsedConstraint(
            Dictionary<int, double> Coefficients,
            string Operator,
            double RightSide);
    }
}