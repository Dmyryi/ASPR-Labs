using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex.Stages;

public sealed class PivotSelector : IPivotSelector
{
    private readonly double _epsilon;

    public PivotSelector() : this(SimplexOptions.Default) { }

    public PivotSelector(SimplexOptions options)
    {
        _epsilon = options.Epsilon;
    }

    public int FindOptimalPivotRow(SimplexTableau tableau, int pivotCol)
    {
        int pivotRow = -1;
        double minRatio = double.MaxValue;

        for (int i = 0; i < tableau.RowsCount; i++)
        {
            double a = tableau.GetValue(i, pivotCol);
            if (a > _epsilon)
            {
                double ratio = tableau.GetB(i) / a;
                if (ratio < minRatio)
                {
                    minRatio = ratio;
                    pivotRow = i;
                }
            }
        }

        return pivotRow;
    }

    public int FindBasicPivotRow(SimplexTableau tableau, int pivotCol, int fallbackRow)
    {
        int bestRow = -1;
        double minRatio = double.MaxValue;

        for (int r = 0; r < tableau.RowsCount; r++)
        {
            double a = tableau.GetValue(r, pivotCol);
            if (a < -_epsilon)
            {
                double ratio = tableau.GetB(r) / a;
                if (ratio >= 0 && ratio < minRatio)
                {
                    minRatio = ratio;
                    bestRow = r;
                }
            }
        }

        return bestRow == -1 ? fallbackRow : bestRow;
    }
}
