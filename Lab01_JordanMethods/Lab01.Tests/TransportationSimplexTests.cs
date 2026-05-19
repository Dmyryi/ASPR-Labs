using Lab01.Logic.Transportation;
using Xunit;

namespace Lab01.Tests;

public class TransportationSimplexTests
{
    [Fact]
    public void Example1_Simplex_MinCost225()
    {
        double[,] sp =
        {
            { 6, 3, 2 },
            { 2, 1, 5 },
            { 3, 4, 1 }
        };
        var problem = TransportationBalancer.Balance(sp, new[] { 30.0, 20, 50 }, new[] { 10.0, 65, 25 });
        var trace = new TransportationTrace();
        TransportationSimplexRunner.Run(problem, trace);

        Assert.NotNull(trace.SimplexSolution);
        double cost = 0;
        int m = problem.Cols;
        for (int i = 0; i < problem.Rows; i++)
        {
            for (int j = 0; j < problem.Cols; j++)
            {
                int k = TransportationLpBuilder.VariableIndex(i, j, m);
                cost += trace.SimplexSolution[k] * problem.Costs[i, j];
            }
        }

        Assert.Equal(225, trace.SimplexMinCost, 3);
        Assert.Equal(225, cost, 3);
    }
}
