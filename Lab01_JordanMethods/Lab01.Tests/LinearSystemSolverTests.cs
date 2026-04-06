using Lab01.Logic;
using Lab01.Logic.BasicLogic;
using Lab01.Logic.Interfaces.IBasicLogic;

namespace Lab01.Tests;

public class LinearSystemSolverTests
{
    private readonly ILinearSystemSolver _solver;

    public LinearSystemSolverTests()
    {
        var jordan = new JordanSolver();
        var inverter = new MatrixInverter(jordan);
        _solver = new InverseSolveStrategy(inverter);
    }

    public static List<object[]> GetLinearSystemTestCases()
    {
        var cases = new List<object[]>();

        cases.Add(new object[]
        {
            new double[,] { { 5, -3, 7 }, { -1, 4, 3 }, { 6, -2, 5 } },
            new double[] { 13, 13, 12 },
            new double[] { 1, 2, 2 }
        });

        cases.Add(new object[]
        {
            new double[,] { { 6, 2, 5 }, { -3, 4, -1 }, { 1, 4, 3 } },
            new double[] { 1, 6, 6 },
            new double[] { -1, 1, 1 }
        });

        cases.Add(new object[]
        {
            new double[,] { { -1, 1, 1 }, { -1, -2, 2 }, { 3, -1, 3 } },
            new double[] { 4, 3, 2 },
            new double[] { -1, 1, 2 }
        });

        return cases;
    }

    [Theory]
    [MemberData(nameof(GetLinearSystemTestCases))]
    public void Solve_ReturnsExpectedVector(double[,] A, double[] B, double[] expectedX)
    {
        double[] actual = _solver.Solve(A, B);
        Assert.Equal(expectedX.Length, actual.Length);
        for (int i = 0; i < expectedX.Length; i++)
        {
            Assert.Equal(expectedX[i], actual[i], precision: 2);
        }
    }

}
