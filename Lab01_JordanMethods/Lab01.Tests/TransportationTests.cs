using Lab01.Logic.Transportation;
using Xunit;

namespace Lab01.Tests;

public class TransportationTests
{
    [Fact]
    public void MethodicalExample1_NorthwestCorner_Cost265()
    {
        double[,] sp =
        {
            { 6, 3, 2 },
            { 2, 1, 5 },
            { 3, 4, 1 }
        };
        double[] po = { 30, 20, 50 };
        double[] pn = { 10, 65, 25 };

        TransportationSolveResult r = TransportationSolver.Solve(sp, po, pn);

        Assert.Equal(265, r.NorthwestCornerPlan.TotalCost, 3);
        Assert.Equal(10, r.NorthwestCornerPlan.Allocations[0, 0], 3);
        Assert.Equal(20, r.NorthwestCornerPlan.Allocations[0, 1], 3);
        Assert.Equal(20, r.NorthwestCornerPlan.Allocations[1, 1], 3);
        Assert.Equal(25, r.NorthwestCornerPlan.Allocations[2, 1], 3);
        Assert.Equal(25, r.NorthwestCornerPlan.Allocations[2, 2], 3);
    }

    [Fact]
    public void MethodicalExample1_MinimumElement_AndOptimal_Cost225()
    {
        double[,] sp =
        {
            { 6, 3, 2 },
            { 2, 1, 5 },
            { 3, 4, 1 }
        };
        double[] po = { 30, 20, 50 };
        double[] pn = { 10, 65, 25 };

        TransportationSolveResult r = TransportationSolver.Solve(sp, po, pn);

        Assert.Equal(225, r.MinimumElementPlan.TotalCost, 3);
        Assert.Equal(225, r.OptimalPlan.TotalCost, 3);
        Assert.Equal(30, r.MinimumElementPlan.Allocations[0, 1], 3);
        Assert.Equal(20, r.MinimumElementPlan.Allocations[1, 1], 3);
        Assert.Equal(10, r.MinimumElementPlan.Allocations[2, 0], 3);
        Assert.Equal(15, r.MinimumElementPlan.Allocations[2, 1], 3);
        Assert.Equal(25, r.MinimumElementPlan.Allocations[2, 2], 3);
    }

    [Fact]
    public void MethodicalExample2_NorthwestCorner_Cost2050()
    {
        double[,] sp =
        {
            { 7, 6, 4 },
            { 3, 8, 5 },
            { 2, 3, 7 }
        };
        double[] po = { 120, 100, 80 };
        double[] pn = { 90, 90, 120 };

        TransportationSolveResult r = TransportationSolver.Solve(sp, po, pn);

        Assert.Equal(2050, r.NorthwestCornerPlan.TotalCost, 3);
    }

    [Fact]
    public void MethodicalExample2_MinimumElement_Cost1390()
    {
        double[,] sp =
        {
            { 7, 6, 4 },
            { 3, 8, 5 },
            { 2, 3, 7 }
        };
        double[] po = { 120, 100, 80 };
        double[] pn = { 90, 90, 120 };

        TransportationSolveResult r = TransportationSolver.Solve(sp, po, pn);

        Assert.Equal(1390, r.MinimumElementPlan.TotalCost, 3);
    }

    [Fact]
    public void MethodicalExample2_Optimal_Cost1060()
    {
        double[,] sp =
        {
            { 7, 6, 4 },
            { 3, 8, 5 },
            { 2, 3, 7 }
        };
        double[] po = { 120, 100, 80 };
        double[] pn = { 90, 90, 120 };

        TransportationSolveResult r = TransportationSolver.Solve(sp, po, pn);

        Assert.Equal(1060, r.OptimalPlan.TotalCost, 3);
        Assert.Equal(10, r.OptimalPlan.Allocations[0, 1], 3);
        Assert.Equal(110, r.OptimalPlan.Allocations[0, 2], 3);
        Assert.Equal(90, r.OptimalPlan.Allocations[1, 0], 3);
        Assert.Equal(10, r.OptimalPlan.Allocations[1, 2], 3);
        Assert.Equal(80, r.OptimalPlan.Allocations[2, 1], 3);
    }

    [Fact]
    public void Variant10_Balanced()
    {
        double[,] sp =
        {
            { 10, 9, 7, 10 },
            { 5, 8, 6, 11 },
            { 11, 9, 7, 9 }
        };
        double[] po = { 40, 45, 25 };
        double[] pn = { 25, 10, 35, 40 };

        TransportationSolveResult r = TransportationSolver.Solve(sp, po, pn);

        Assert.False(r.WasOpen);
        Assert.True(r.OptimalPlan.TotalCost > 0);
    }
}
