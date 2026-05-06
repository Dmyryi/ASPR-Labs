using Lab01.Logic;
using Lab01.Logic.Gomori;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Protocols;
using Lab01.Logic.Simplex.Stages;
using Xunit;
using Xunit.Abstractions;

namespace Lab01.Tests;

public class GomorySolverTests
{
    private const int Precision = 9;

    private readonly ITestOutputHelper _output;
    private readonly IGomorySolver _solver;

    public GomorySolverTests(ITestOutputHelper output)
    {
        _output = output;
        _solver = new GomorySolver(new JordanSolver(), new PivotSelector());
    }

    /// <summary>
    /// Тестовий приклад 1 з методички (Гоморі):
    /// Z = x1 + 4x2 -> max,  2x1 + x2 ≤ 6,  x1 + 3x2 ≤ 4,  x_j ≥ 0 цілі.
    /// LP-оптимум: X=(0; 4/3), Z=16/3 ≈ 5.33.
    /// Цілочисловий оптимум: X=(1; 1), Z=5.
    /// </summary>
    [Fact]
    public void TextbookExample_Maximization_X_1_1_Z_5()
    {
        double[] vectorZ = { -1, -4 };
        double[,] matrixA = { { 2, 1 }, { 1, 3 } };
        double[] vectorB = { 6, 4 };
        double[] expectedX = { 1, 1 };
        const double expectedZ = 5;

        var protocol = new SimplexProtocol();
        var result = _solver.Solve(vectorZ, matrixA, vectorB, OptimizationMode.Maximization, GomoryOptions.Default, protocol);

        DumpResult(result, protocol);
        AssertSolution(expectedX, expectedZ, result);
    }

    /// <summary>
    /// Якщо LP-оптимум уже цілий — Гоморі повертає його без жодного відсічення.
    /// </summary>
    [Fact]
    public void IntegerLpOptimum_ReturnsImmediately()
    {
        // Z = x1 + x2 -> max, x1 ≤ 3, x2 ≤ 2  (LP-оптимум одразу X=(3;2), Z=5)
        double[] vectorZ = { -1, -1 };
        double[,] matrixA = { { 1, 0 }, { 0, 1 } };
        double[] vectorB = { 3, 2 };

        var result = _solver.Solve(vectorZ, matrixA, vectorB, OptimizationMode.Maximization);

        Assert.Equal(3d, result.X[0], Precision);
        Assert.Equal(2d, result.X[1], Precision);
        Assert.Equal(5d, result.Z, Precision);
    }

    /// <summary>
    /// Задача з дробовим LP-оптимумом: Z = 8x1 + 5x2 -> max, x1+x2 ≤ 6, 9x1+5x2 ≤ 45.
    /// LP-оптимум: X=(15/4; 9/4), Z=41.25.  Цілочисловий оптимум: X=(5; 0), Z=40
    /// (перевіряється вручну: 9·5 = 45 ≤ 45, ціль Z=40, інші цілі точки дають менше).
    /// </summary>
    [Fact]
    public void DakinExample_Maximization_X_5_0_Z_40()
    {
        double[] vectorZ = { -8, -5 };
        double[,] matrixA = { { 1, 1 }, { 9, 5 } };
        double[] vectorB = { 6, 45 };
        double[] expectedX = { 5, 0 };
        const double expectedZ = 40;

        var protocol = new SimplexProtocol();
        var result = _solver.Solve(vectorZ, matrixA, vectorB, OptimizationMode.Maximization, GomoryOptions.Default, protocol);

        DumpResult(result, protocol);
        AssertSolution(expectedX, expectedZ, result);
    }

    private void AssertSolution(double[] expectedX, double expectedZ, SolverResult result)
    {
        Assert.True(result.Success);
        Assert.Equal(expectedZ, result.Z, Precision);
        Assert.Equal(expectedX.Length, result.X.Length);
        for (int i = 0; i < expectedX.Length; i++)
            Assert.Equal(expectedX[i], result.X[i], Precision);
    }

    private void DumpResult(SolverResult result, ISimplexProtocol protocol)
    {
        _output.WriteLine($"X = ({string.Join("; ", result.X)})");
        _output.WriteLine($"Z = {result.Z}");
        _output.WriteLine("--- protocol ---");
        _output.WriteLine(protocol.GetText());
    }
}
