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
                if (IsBetterMinRatioRow(ratio, minRatio, i, pivotRow))
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
            if (a >= -_epsilon)
                continue;

            double ratio = tableau.GetB(r) / a;
            if (ratio < -_epsilon)
                continue;

            if (IsBetterMinRatioRow(ratio, minRatio, r, bestRow))
            {
                minRatio = ratio;
                bestRow = r;
            }
        }

        return bestRow == -1 ? fallbackRow : bestRow;
    }

    /// <summary>
    /// При виродженості кілька рядків дають те саме відношення b/a; недетермінований вибір
    /// може повертати той самий базис. Беремо мінімальне ratio, а при нічиї — рядок з меншим індексом.
    /// </summary>
    private static bool IsBetterMinRatioRow(
        double ratio,
        double bestRatio,
        int rowIndex,
        int currentBestRow)
    {
        if (currentBestRow < 0)
            return true;

        double tol = RatioTolerance(bestRatio);
        if (ratio < bestRatio - tol)
            return true;

        if (ApproxEqualRatios(ratio, bestRatio, tol) && rowIndex < currentBestRow)
            return true;

        return false;
    }

    private static double RatioTolerance(double reference)
    {
        if (!double.IsFinite(reference) || reference is double.MaxValue or double.MinValue)
            return 1e-9;

        return Math.Max(1e-9, Math.Abs(reference) * 1e-9);
    }

    private static bool ApproxEqualRatios(double a, double b, double tol)
        => Math.Abs(a - b) <= tol;
}
