using Lab01.Logic.Transportation;
using Xunit;

namespace Lab01.Tests;

public class TransportationProtocolTests
{
    [Fact]
    public void Example1_Protocol_ContainsNorthwestChainAndMinCost225()
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
        string text = TransportationProtocolFormatter.Build(r);

        Assert.Contains("(x11 = 10)", text);
        Assert.Contains("S = 10 * 6 + 20 * 3", text);
        Assert.Contains("= 265", text);
        Assert.Contains("Умова оптимальності", text);
        Assert.Contains("Постановка задачі:", text);
        Assert.Contains("Min (Z) = 225,00", text);
        Assert.Contains("λ = 10", text);
        Assert.Contains("λ = 10", text);
    }
}
