using Lab01.Logic.Interfaces;
using Lab01.Logic.Interfaces.IBasicLogic;

namespace Lab01.Logic.BasicLogic;

public sealed class RankCalculator : IRankCalculator
{
    private const double Epsilon = 1e-10;

    private readonly IJordan _jordan;

    public RankCalculator(IJordan jordan) => _jordan = jordan;

    public int Calculate(double[,] matrixA)
    {
        int rows = matrixA.GetLength(0);
        int cols = matrixA.GetLength(1);
        int rank = 0;
        double[,] result = matrixA;
        int limit = Math.Min(rows, cols);

        for (int i = 0; i < limit; i++)
        {
            if (Math.Abs(result[i, i]) > Epsilon)
            {
                result = _jordan.JordanMethod(result, i, i);
                rank++;
            }
        }

        return rank;
    }
}
