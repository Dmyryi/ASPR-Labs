using System.Linq;
using Lab01.Logic;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Parsing;
using Lab01.Logic.Simplex.Protocols;
using Lab01.Logic.Simplex.Solvers;
using Lab01.Logic.Simplex.Stages;
using Xunit;

namespace Lab01.Tests;

/// <summary>
/// Задача призначення 4×4 як ЛП (суми по рядках/стовпцях = 1, x ≥ 0).
/// Перевірка: симплекс завершується без CyclingDetectedException і збігається з перебором перестановок.
/// </summary>
public sealed class AssignmentSimplexTests
{
    private const string Objective =
        "45x1 + 17x2 + 33x3 + 10x4 + 35x5 + 15x6 + 38x7 + 8x8 + "
        + "40x9 + 16x10 + 31x11 + 9x12 + 37x13 + 22x14 + 35x15 + 15x16";

    private const string Constraints =
        "x1 + x2 + x3 + x4 = 1\n"
        + "x5 + x6 + x7 + x8 = 1\n"
        + "x9 + x10 + x11 + x12 = 1\n"
        + "x13 + x14 + x15 + x16 = 1\n"
        + "x1 + x5 + x9 + x13 = 1\n"
        + "x2 + x6 + x10 + x14 = 1\n"
        + "x3 + x7 + x11 + x15 = 1\n"
        + "x4 + x8 + x12 + x16 = 1";

    private static readonly double[,] Cost =
    {
        { 45, 17, 33, 10 },
        { 35, 15, 38, 8 },
        { 40, 16, 31, 9 },
        { 37, 22, 35, 15 }
    };

    [Fact]
    public void Assignment4x4_Minimization_NoCycling_MatchesEnumeration()
    {
        double brute = MinCostOverPermutations();
        var parser = new LinearProgramParser();
        LinearProgram program = parser.Parse(Objective, Constraints);

        var jordan = new JordanSolver();
        var pivotSelector = new PivotSelector();
        var protocol = new SimplexProtocol();
        var basicFinder = new BasicSolutionFinder(
            jordan, pivotSelector, SimplexOptions.Default, protocol);
        var optimalFinder = new OptimalSolutionFinder(
            jordan, pivotSelector, OptimizationMode.Minimization,
            SimplexOptions.Default, protocol, logPivotStepNumbers: false);
        var solver = new MinimizationSolver(basicFinder, optimalFinder, protocol);

        double[] vectorZ = (double[])program.ObjectiveCoefficients.Clone();
        SolverResult result = solver.Solve(vectorZ, program.ConstraintMatrix, program.RightHandSide);

        Assert.True(result.Success, result.Message ?? "solve failed");

        double objectiveFromX = Enumerable.Range(0, 16).Sum(k => Cost[k / 4, k % 4] * result.X[k]);
        Assert.Equal(brute, objectiveFromX, 5);
        Assert.Equal(brute, result.Z, 5);

        foreach (double v in result.X)
            Assert.InRange(v, -1e-5, 1 + 1e-5);

        RowSumsApproxOne(result.X, row: 0, 4);
        RowSumsApproxOne(result.X, row: 1, 4);
        RowSumsApproxOne(result.X, row: 2, 4);
        RowSumsApproxOne(result.X, row: 3, 4);
        ColSumApproxOne(result.X, col: 0);
        ColSumApproxOne(result.X, col: 1);
        ColSumApproxOne(result.X, col: 2);
        ColSumApproxOne(result.X, col: 3);
    }

    private static void RowSumsApproxOne(double[] x, int row, int cols)
    {
        double sum = 0;
        for (int j = 0; j < cols; j++)
            sum += x[row * cols + j];
        Assert.Equal(1.0, sum, 5);
    }

    private static void ColSumApproxOne(double[] x, int col)
    {
        const int cols = 4;
        double sum = x[col] + x[cols + col] + x[2 * cols + col] + x[3 * cols + col];
        Assert.Equal(1.0, sum, 5);
    }

    private static double MinCostOverPermutations()
    {
        var cols = new[] { 0, 1, 2, 3 };
        double best = double.PositiveInfinity;
        PermuteRecursive(cols, 0, ref best);
        return best;
    }

    private static void PermuteRecursive(int[] a, int start, ref double best)
    {
        if (start >= a.Length)
        {
            double sum = Cost[0, a[0]] + Cost[1, a[1]] + Cost[2, a[2]] + Cost[3, a[3]];
            if (sum < best)
                best = sum;
            return;
        }

        for (int i = start; i < a.Length; i++)
        {
            (a[start], a[i]) = (a[i], a[start]);
            PermuteRecursive(a, start + 1, ref best);
            (a[start], a[i]) = (a[i], a[start]);
        }
    }
}
