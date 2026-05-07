using Lab01.Logic;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Solvers;
using Lab01.Logic.Simplex.Stages;
using Xunit;
using Xunit.Abstractions;

namespace Lab01.Tests;

public class SimplexSolverTests
{
    private const int Precision = 9;

    private readonly ITestOutputHelper _output;
    private readonly IJordan _jordan;
    private readonly IPivotSelector _pivotSelector;
    private readonly BasicSolutionFinder _basicFinder;

    public SimplexSolverTests(ITestOutputHelper output)
    {
        _output = output;
        _jordan = new JordanSolver();
        _pivotSelector = new PivotSelector();
        _basicFinder = new BasicSolutionFinder(_jordan, _pivotSelector);
    }

    [Fact]
    public void TestExample1_Maximization_X_0_22_0_8_Z_36()
    {
        double[] vectorZ = { -1, -2, 1, 1 };
        double[,] matrixA =
        {
            { 1, 1, -1, -2 },
            { -1, -1, -1, 1 },
            { 2, -1, 3, 4 }
        };
        double[] vectorB = { 6, -5, 10 };
        double[] expectedX = { 0, 22, 0, 8 };
        const double expectedZ = 36;

        var result = SolveMax(vectorZ, matrixA, vectorB, useZeroRowElimination: true);

        AssertSolution(expectedX, expectedZ, result);
    }

    [Fact]
    public void TestExample1_Maximization_WithoutZeroRowElim_X_0_22_0_8_Z_36()
    {
        double[] vectorZ = { -1, -2, 1, 1 };
        double[,] matrixA =
        {
            { 1, 1, -1, -2 },
            { -1, -1, -1, 1 },
            { 2, -1, 3, 4 }
        };
        double[] vectorB = { 6, -5, 10 };
        double[] expectedX = { 0, 22, 0, 8 };
        const double expectedZ = 36;

        var result = SolveMax(vectorZ, matrixA, vectorB, useZeroRowElimination: false);

        AssertSolution(expectedX, expectedZ, result);
    }

    [Fact]
    public void TestExample2_Minimization_X_5_0_0_0_Z_neg10()
    {
        double[] vectorZ = { -2, 3, 0, -3 };
        double[,] matrixA =
        {
            { 1, 1, -1, -2 },
            { -1, -1, -1, 1 },
            { 2, -1, 3, 4 }
        };
        double[] vectorB = { 6, -5, 10 };
        double[] expectedX = { 5, 0, 0, 0 };
        const double expectedZ = -10;

        var optimalFinder = new OptimalSolutionFinder(_jordan, _pivotSelector, OptimizationMode.Minimization);
        var solver = new MinimizationSolver(_basicFinder, optimalFinder);

        var result = solver.Solve(vectorZ, matrixA, vectorB);

        AssertSolution(expectedX, expectedZ, result);
    }

    [Fact]
    public void TextbookExample2_Maximization_X_3_4_Z_15()
    {
        double[] vectorZ = { 3, -6 };
        double[,] matrixA =
        {
            { -1, -2 },
            { -2, -1 },
            { -1, 1 },
            { -1, 4 },
            { 4, -1 }
        };
        double[] vectorB = { 1, -4, 1, 13, 23 };
        double[] expectedX = { 3, 4 };
        const double expectedZ = 15;

        var result = SolveMax(vectorZ, matrixA, vectorB, useZeroRowElimination: false);

        AssertSolution(expectedX, expectedZ, result);
    }

    [Fact(Skip = "Потребує окремої обробки рівностей у домені (рис. 3.2 для b ≠ 0).")]
    public void TextbookExample1_Maximization_X_9_17_0_1_Z_21()
    {
        double[] vectorZ = { -10, 1, 42, 52 };
        double[,] matrixA =
        {
            { -2, 1, 1, 3 },
            { -3, 2, -3, 0 },
            { -3, 1, 4, 1 },
            { 3, -2, 2, -2 }
        };
        double[] vectorB = { 2, 7, 1, -9 };
        double[] expectedX = { 9, 17, 0, 1 };
        const double expectedZ = 21;

        var result = SolveMax(vectorZ, matrixA, vectorB, useZeroRowElimination: true);

        AssertSolution(expectedX, expectedZ, result);
    }

    [Fact]
    public void Maximization_WithNullZeroRowEliminator_DoesNotThrow()
    {
        double[] vectorZ = { -1, -2, 1, 1 };
        double[,] matrixA =
        {
            { 1, 1, -1, -2 },
            { -1, -1, -1, 1 },
            { 2, -1, 3, 4 }
        };
        double[] vectorB = { 6, -5, 10 };

        var optimalFinder = new OptimalSolutionFinder(_jordan, _pivotSelector, OptimizationMode.Maximization);
        var solver = new MaximizationSolver(
            _basicFinder, optimalFinder,
            zeroRowEliminator: null,
            useZeroRowElimination: true);

        var result = solver.Solve(vectorZ, matrixA, vectorB);

        Assert.True(result.Success);
    }

    private SolverResult SolveMax(double[] vectorZ, double[,] matrixA, double[] vectorB, bool useZeroRowElimination)
    {
        var zeroRowEliminator = new ZeroRowEliminator(_jordan);
        var optimalFinder = new OptimalSolutionFinder(_jordan, _pivotSelector, OptimizationMode.Maximization);
        var solver = new MaximizationSolver(
            _basicFinder, optimalFinder,
            zeroRowEliminator: zeroRowEliminator,
            useZeroRowElimination: useZeroRowElimination);
        return solver.Solve(vectorZ, matrixA, vectorB);
    }

    private void AssertSolution(double[] expectedX, double expectedZ, SolverResult result)
    {
        _output.WriteLine($"Actual Z:   {result.Z}");
        _output.WriteLine($"Expected Z: {expectedZ}");
        _output.WriteLine($"Actual X:   ({string.Join(", ", result.X)})");
        _output.WriteLine($"Expected X: ({string.Join(", ", expectedX)})");

        Assert.True(result.Success);
        Assert.Equal(expectedZ, result.Z, Precision);
        Assert.Equal(expectedX.Length, result.X.Length);
        for (int i = 0; i < expectedX.Length; i++)
            Assert.Equal(expectedX[i], result.X[i], Precision);
    }
}
