using Lab01.Logic;
using Lab01.Logic.GameTheory;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Stages;
using Xunit;

namespace Lab01.Tests;

public class MatrixGameTests
{
    private static MatrixGameSolver CreateSolver()
    {
        var jordan = new JordanSolver();
        var pivot = new PivotSelector();
        var factory = new SimplexSolverFactory(jordan, pivot);
        return new MatrixGameSolver(factory);
    }

    [Fact]
    public void TextbookMatrix2x4_DominanceThenTwoByTwo_Value46over11()
    {
        var solver = CreateSolver();
        double[,] a =
        {
            { 3, 2, 6, 9 },
            { 10, 8, 1, 3 }
        };

        GameTheorySolveResult r = solver.Solve(a);
        Assert.True(r.EliminatedDominatedStrategies);
        Assert.Contains("2 × 2", r.SolutionKind);
        Assert.Equal(46.0 / 11.0, r.GameValue, 5);
        Assert.False(r.HasSaddlePoint);
    }

    [Fact]
    public void Variant2_A1_AfterDominance_NoSaddle_ValidMix()
    {
        var solver = CreateSolver();
        double[,] a =
        {
            { -2, -1, -2 },
            { 4, -2, 1 },
            { 1, 3, -5 }
        };

        GameTheorySolveResult r = solver.Solve(a);
        Assert.False(r.HasSaddlePoint);
        Assert.True(
            r.SolutionKind.Contains("ЗЛП", StringComparison.Ordinal) ||
            r.SolutionKind.Contains("m × 2", StringComparison.Ordinal) ||
            r.SolutionKind.Contains("2 × n", StringComparison.Ordinal) ||
            r.SolutionKind.Contains("2 × 2", StringComparison.Ordinal),
            "Очікувалась гілка після вилучення домінування: m×2 / 2×n / 2×2 або ЗЛП.");
        Assert.Equal(1, r.RowPlayerStrategy.Sum(), 5);
        Assert.Equal(1, r.ColumnPlayerStrategy.Sum(), 5);
    }

    [Fact]
    public void Textbook3x3_AllPositive_ValueAndStrategies()
    {
        var solver = CreateSolver();
        double[,] a =
        {
            { 5, 2, 7 },
            { 1, 4, 3 },
            { 6, 1, 5 }
        };

        GameTheorySolveResult r = solver.Solve(a);

        Assert.False(r.HasSaddlePoint);
        Assert.Equal(3, r.GameValue, 5);
        Assert.Equal(0.5, r.RowPlayerStrategy[0], 5);
        Assert.Equal(0.5, r.RowPlayerStrategy[1], 5);
        Assert.Equal(0, r.RowPlayerStrategy[2], 5);
        Assert.Equal(1.0 / 3, r.ColumnPlayerStrategy[0], 4);
        Assert.Equal(2.0 / 3, r.ColumnPlayerStrategy[1], 4);
        Assert.Equal(0, r.ColumnPlayerStrategy[2], 5);
    }

    [Fact]
    public void Variant1_A1_HasSaddle_Value1()
    {
        var solver = CreateSolver();
        double[,] a =
        {
            { 3, 1, 1 },
            { 2, -2, 1 },
            { -1, -3, -2 }
        };

        GameTheorySolveResult r = solver.Solve(a);
        Assert.True(r.HasSaddlePoint);
        Assert.Equal(1, r.GameValue);
    }

    [Fact]
    public void Variant1_A3_TwoByFour_RectangularSolver()
    {
        var solver = CreateSolver();
        double[,] a =
        {
            { 16, 20, 15, 19 },
            { 22, 18, 17, 11 }
        };

        GameTheorySolveResult r = solver.Solve(a);
        Assert.False(r.HasSaddlePoint);
        Assert.True(r.EliminatedDominatedStrategies);
        Assert.True(
            r.SolutionKind.Contains("2 × n", StringComparison.Ordinal) ||
            r.SolutionKind.Contains("2 × 2", StringComparison.Ordinal),
            "Після вилучення домінованих стовпців очікується гілка 2×n або зведення до 2×2.");
        Assert.InRange(r.GameValue, 15, 22);
    }

    [Fact]
    public void TwoByTwo_NoSaddle_Analytic()
    {
        var solver = CreateSolver();
        double[,] a = { { 2, 5 }, { 8, 1 } };
        GameTheorySolveResult r = solver.Solve(a);
        Assert.False(r.HasSaddlePoint);
        Assert.Equal(3.8, r.GameValue, 5);
    }

    [Fact]
    public void SaddlePoint_Pure2x2()
    {
        double[,] a =
        {
            { 10, 5 },
            { 15, 2 }
        };
        var solver = CreateSolver();
        GameTheorySolveResult r = solver.Solve(a);
        Assert.True(r.HasSaddlePoint);
        Assert.Equal(5, r.GameValue);
        Assert.Equal(0, r.SaddleRow);
        Assert.Equal(1, r.SaddleColumn);
    }

    [Fact]
    public void Simulator_DeterministicSeed_AveragesNearValue()
    {
        double[,] a =
        {
            { 5, 2, 7 },
            { 1, 4, 3 },
            { 6, 1, 5 }
        };
        var solver = CreateSolver();
        GameTheorySolveResult r = solver.Solve(a);

        MatrixGameSimulationResult sim = MatrixGameSimulator.Simulate(
            a,
            r.RowPlayerStrategy,
            r.ColumnPlayerStrategy,
            50_000,
            r.GameValue,
            seed: 42,
            maxProtocolRows: 15);

        Assert.Equal(15, sim.Protocol.Count);
        Assert.Equal(1, sim.Protocol[0].Round);
        Assert.InRange(sim.AveragePayoff, 2.7, 3.3);
    }
}
