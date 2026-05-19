using Lab01.Logic;
using Lab01.Logic.GameTheory;
using Lab01.Logic.Interfaces;
using Lab01.Logic.MultiCriteria;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Parsing;
using Lab01.Logic.Simplex.Stages;

namespace Lab01.Tests;

public class MultiCriteriaTests
{
    private static MultiCriteriaSolver CreateSolver()
    {
        var jordan = new JordanSolver();
        var factory = new SimplexSolverFactory(jordan, new PivotSelector());
        var game = new MatrixGameSolver(factory);
        return new MultiCriteriaSolver(new LinearProgramParser(), factory, game);
    }

    [Fact]
    public void MethodicalExample1_CompromiseSolution()
    {
        string objectives =
            "2x1 + 2x2 + x3 + x4 + x5 max\r\n" +
            "x1 - 3x2 + 5x3 - x4 - 2x5 min\r\n" +
            "x1 - 4x2 + 5x3 + 9x4 - 2x5 max";

        string constraints =
            "x1 + 4x2 + 3x3 + 2x4 + x5 = 9\r\n" +
            "-x1 + 2x2 - x3 + 2x4 + x5 = 6\r\n" +
            "x1 + 2x2 + 2x4 - x5 = 2";

        MultiCriteriaSolveResult r = CreateSolver().Solve(objectives, constraints);

        Assert.Equal(3, r.Weights.Length);
        Assert.Equal(0.8, r.Weights[0], 2);
        Assert.Equal(0.0, r.Weights[1], 2);
        Assert.Equal(0.2, r.Weights[2], 2);

        Assert.Equal(1.2, r.CompromiseSolution[0], 2);
        Assert.Equal(0.0, r.CompromiseSolution[1], 2);
        Assert.Equal(0.15, r.CompromiseSolution[2], 2);
        Assert.Equal(2.04, r.CompromiseSolution[3], 2);
        Assert.Equal(3.27, r.CompromiseSolution[4], 2);

        Assert.Equal(0, r.SuboptimalityMatrix[0, 0], 2);
        Assert.Equal(0.35, r.SuboptimalityMatrix[1, 0], 2);
        Assert.Equal(1.29, r.SuboptimalityMatrix[1, 2], 2);
        Assert.Equal(1.29, r.GameMatrix[0, 0], 2);
    }

    [Fact]
    public void MethodicalExample2_CompromiseSolution()
    {
        string objectives =
            "x1 - 8x2 + x3 + 4x4 max\r\n" +
            "-x1 + 3x2 + 5x3 + x4 min\r\n" +
            "3x1 + x2 + x3 - x4 max";

        string constraints =
            "x1 - x2 + x3 + x4 <= 2\r\n" +
            "x1 + x2 + x3 - x4 <= 2\r\n" +
            "-x1 + x2 + x3 + x4 <= 2\r\n" +
            "x1 + x2 - x3 + x4 <= 2";

        MultiCriteriaSolveResult r = CreateSolver().Solve(objectives, constraints);

        Assert.Equal(0.27, r.Weights[0], 2);
        Assert.Equal(0.73, r.Weights[1], 2);
        Assert.Equal(0.0, r.Weights[2], 2);

        Assert.Equal(1.45, r.CompromiseSolution[0], 2);
        Assert.Equal(0.0, r.CompromiseSolution[1], 2);
        Assert.Equal(0.0, r.CompromiseSolution[2], 2);
        Assert.Equal(0.55, r.CompromiseSolution[3], 2);
    }

    [Fact]
    public void Variant10_SolvesWithoutError()
    {
        string objectives =
            "x1 + x2 + x3 + x4 + x5 max\r\n" +
            "x1 - 2x2 + x3 max\r\n" +
            "x2 - x3 min";

        string constraints =
            "x1 + x2 + 2x3 = 4\r\n" +
            "2x2 + 2x3 - x4 + x5 = 6\r\n" +
            "x1 - x2 + 6x3 + x4 + x5 = 12";

        MultiCriteriaSolveResult r = CreateSolver().Solve(objectives, constraints);

        Assert.Equal(3, r.Weights.Length);
        Assert.True(r.Weights.Sum() is > 0.99 and < 1.01);
        Assert.Equal(5, r.CompromiseSolution.Length);
        Assert.All(r.CompromiseSolution, v => Assert.True(v >= -1e-6));
    }
}
