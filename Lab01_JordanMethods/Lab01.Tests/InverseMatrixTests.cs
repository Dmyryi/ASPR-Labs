using Lab01.Logic;
using Lab01.Logic.Interfaces;

namespace Lab01.Tests;

public class InverseMatrixTests
{
    private readonly IMatrixInverter _matrix;

    public InverseMatrixTests()
    {
        var jordan = new JordanSolver();
        _matrix = new MatrixInverter(jordan);
    }

    public static List<object[]> GetInverseMatrixTestCases()
    {
        var cases = new List<object[]>();

        cases.Add(new object[]
        {
            new double[,] { { 5, -3, 7 }, { -1, 4, 3 }, { 6, -2, 5 } },
            new double[,] { { -0.28, -0.011, 0.398 }, { -0.247, 0.183, 0.237 }, { 0.237, 0.086, -0.183 } }
        });

        cases.Add(new object[]
        {
            new double[,] { { 6, 2, 5 }, { -3, 4, -1 }, { 1, 4, 3 } },
            new double[,] { { 0.5, 0.437, -0.687 }, { 0.25, 0.406, -0.281 }, { -0.5, -0.687, 0.937 } }
        });

        cases.Add(new object[]
        {
            new double[,] { { 2, -1, 3 }, { -1, 2, 2 }, { 1, 1, 1 } },
            new double[,] { { 0, -0.333, 0.667 }, { -0.25, 0.083, 0.583 }, { 0.25, 0.25, -0.25 } }
        });

        return cases;
    }

    [Theory]
    [MemberData(nameof(GetInverseMatrixTestCases))]
    public void Invert_ReturnsExpectedMatrix(double[,] A, double[,] expectedInverse)
    {
        double[,] actual = _matrix.Invert(A);
        for (int i = 0; i < expectedInverse.GetLength(0); i++)
        {
            for (int j = 0; j < expectedInverse.GetLength(1); j++)
            {
                Assert.Equal(expectedInverse[i, j], actual[i, j], precision:2);
            }
        }
    }
}
