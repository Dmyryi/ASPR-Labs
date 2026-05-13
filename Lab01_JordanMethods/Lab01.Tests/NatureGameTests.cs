using Lab01.Logic.GameTheory;
using Xunit;

namespace Lab01.Tests;

public class NatureGameTests
{
    private static void AssertSetEqual(IReadOnlyList<int> expected, IReadOnlyList<int> actual)
    {
        Assert.Equal(
            expected.OrderBy(x => x).ToArray(),
            actual.OrderBy(x => x).ToArray());
    }

    [Fact]
    public void MethodicalExample1_AllCriteria()
    {
        double[,] u =
        {
            { -1, 1, 1, 4 },
            { -1, -2, 2, 3 },
            { 3, -1, 3, 2 }
        };
        double[] p = { 0.2, 0.4, 0.1, 0.3 };

        NatureGameSolveResult r = NatureGameSolver.Solve(u, 0.3, p);

        AssertSetEqual(new[] { 0, 2 }, r.WaldRows);
        AssertSetEqual(new[] { 0 }, r.MaximaxRows);
        AssertSetEqual(new[] { 0 }, r.HurwiczRows);
        AssertSetEqual(new[] { 2 }, r.SavageRows);
        AssertSetEqual(new[] { 0 }, r.BayesRows);
        AssertSetEqual(new[] { 2 }, r.LaplaceRows);
        AssertSetEqual(new[] { 0 }, r.MostFrequentRows);
    }

    [Fact]
    public void MethodicalExample2_AllCriteria()
    {
        double[,] u =
        {
            { 2, -1, 3, 4 },
            { -1, 2, 3, 7 },
            { 5, 4, 6, 2 }
        };
        double[] p = { 0.4, 0.1, 0.2, 0.3 };

        NatureGameSolveResult r = NatureGameSolver.Solve(u, 0.4, p);

        AssertSetEqual(new[] { 2 }, r.WaldRows);
        AssertSetEqual(new[] { 1 }, r.MaximaxRows);
        AssertSetEqual(new[] { 2 }, r.HurwiczRows);
        AssertSetEqual(new[] { 0, 2 }, r.SavageRows);
        AssertSetEqual(new[] { 2 }, r.BayesRows);
        AssertSetEqual(new[] { 2 }, r.LaplaceRows);
        AssertSetEqual(new[] { 2 }, r.MostFrequentRows);
    }

    [Fact]
    public void SavageRegretMatrix_Example1()
    {
        double[,] u =
        {
            { -1, 1, 1, 4 },
            { -1, -2, 2, 3 },
            { 3, -1, 3, 2 }
        };
        NatureGameSolveResult r = NatureGameSolver.Solve(u, 0.5, new[] { 0.25, 0.25, 0.25, 0.25 });

        Assert.Equal(4, r.SavageRegretMatrix[0, 0], 10);
        Assert.Equal(0, r.SavageRegretMatrix[2, 0], 10);
        Assert.Equal(2, r.SavageRegretMatrix[2, 3], 10);
    }

    [Fact]
    public void Probabilities_NormalizedWhenSumNotExactlyOne()
    {
        double[,] u =
        {
            { 1, 0 },
            { 0, 1 }
        };
        NatureGameSolveResult r = NatureGameSolver.Solve(u, 0.5, new[] { 1.0, 1.0 });
        AssertSetEqual(new[] { 0, 1 }, r.BayesRows);
    }
}
