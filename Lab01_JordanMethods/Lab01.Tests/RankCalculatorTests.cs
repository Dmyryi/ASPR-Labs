using Lab01.Logic;
using Lab01.Logic.Interfaces;

namespace Lab01.Tests;

public class RankCalculatorTests
{
    private readonly IRankCalculator _rankCalculator;

    public RankCalculatorTests()
    {
        var jordan = new JordanSolver();
        _rankCalculator = new RankCalculator(jordan);
    }

    public static List<object[]> GetRankTestCases()
    {
        var cases = new List<object[]>();

        cases.Add(new object[] { new double[,] { { 1, 2, 3, 4 }, { 2, 4, 6, 8 } }, 1 });
        cases.Add(new object[] { new double[,] { { 1, 2 }, { 3, 6 }, { 5, 10 }, { 4, 8 } }, 1 });
        cases.Add(new object[] { new double[,] { { 6, 2, 5 }, { -3, 4, -1 }, { 1, 4, 3 } }, 3 });
        cases.Add(new object[] { new double[,] { { 1, 2, 3, 4 }, { -2, 5, -1, 3 }, { 2, 4, 6, 8 }, { -1, 9, 2, 7 } }, 3 });
        cases.Add(new object[] { new double[,] { { 2, 5, 4 }, { -3, 1, -2 }, { -1, 6, 2 } }, 2 });
        cases.Add(new object[] { new double[,] { { -1, 5, 4 }, { -2, 7, 5 }, { -3, 4, 1 } }, 2 });
        cases.Add(new object[] { new double[,] { { 1, 2, 3, 4 }, { -2, 5, -1, 3 }, { 2, 4, 6, 8 }, { -1, 7, 2, 7 } }, 2 });
        cases.Add(new object[] { new double[,] { { 1, 2, 3, 4 }, { -2, 5, -1, 3 }, { 2, 4, 7, 8 }, { -1, 9, 2, 7 } }, 4 });

        return cases;
    }

    [Theory]
    [MemberData(nameof(GetRankTestCases))]
    public void CalculateRank_ReturnsExpectedRank(double[,] A, int expectedRank)
    {
        int actual = _rankCalculator.Calculate(A);
        Assert.Equal(expectedRank, actual);
    }
}
