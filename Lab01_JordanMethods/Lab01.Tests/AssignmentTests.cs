using Lab01.Logic.Assignment;
using Xunit;

namespace Lab01.Tests;

public class AssignmentTests
{
    [Fact]
    public void MethodicalExample1_Cost8()
    {
        double[,] c =
        {
            { 2, 4, 1, 3, 3 },
            { 1, 5, 4, 1, 2 },
            { 3, 5, 2, 2, 4 },
            { 1, 4, 3, 1, 4 },
            { 3, 2, 5, 3, 5 }
        };

        AssignmentSolveResult r = HungarianMethod.Solve(c);

        Assert.Equal(8, r.TotalCost, 3);
        Assert.Equal(1, r.AssignmentMatrix[0, 2]);
        Assert.Equal(1, r.AssignmentMatrix[1, 4]);
        Assert.Equal(1, r.AssignmentMatrix[2, 3]);
        Assert.Equal(1, r.AssignmentMatrix[3, 0]);
        Assert.Equal(1, r.AssignmentMatrix[4, 1]);
    }

    [Fact]
    public void MethodicalExample2_Cost28()
    {
        double[,] c =
        {
            { 2, 10, 9, 7 },
            { 15, 4, 14, 8 },
            { 13, 14, 16, 11 },
            { 4, 15, 13, 19 }
        };

        AssignmentSolveResult r = HungarianMethod.Solve(c);

        Assert.Equal(28, r.TotalCost, 3);
        Assert.Equal(1, r.AssignmentMatrix[0, 2]);
        Assert.Equal(1, r.AssignmentMatrix[1, 1]);
        Assert.Equal(1, r.AssignmentMatrix[2, 3]);
        Assert.Equal(1, r.AssignmentMatrix[3, 0]);
    }

    [Fact]
    public void Variant10_Solves()
    {
        double[,] c =
        {
            { 45, 17, 33, 10 },
            { 35, 15, 38, 8 },
            { 40, 16, 31, 9 },
            { 37, 22, 35, 15 }
        };

        AssignmentSolveResult r = HungarianMethod.Solve(c);
        Assert.True(r.TotalCost > 0);
        int assigned = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
                assigned += r.AssignmentMatrix[i, j];
        }

        Assert.Equal(4, assigned);
    }

    [Fact]
    public void Example2_Simplex_Cost28()
    {
        double[,] c =
        {
            { 2, 10, 9, 7 },
            { 15, 4, 14, 8 },
            { 13, 14, 16, 11 },
            { 4, 15, 13, 19 }
        };

        var trace = new AssignmentTrace();
        AssignmentSimplexRunner.Run(c, 4, trace);
        Assert.Equal(28, trace.SimplexMinCost, 3);
    }
}
