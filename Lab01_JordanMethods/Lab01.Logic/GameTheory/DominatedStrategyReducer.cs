namespace Lab01.Logic.GameTheory;

public static class DominatedStrategyReducer
{
    private const double Tol = 1e-10;

    public sealed record Reduction(
        double[,] Matrix,
        IReadOnlyList<int> OriginalRowIndices,
        IReadOnlyList<int> OriginalColumnIndices,
        IReadOnlyList<string> Log);

    public static Reduction Reduce(double[,] payoff)
    {
        int m = payoff.GetLength(0);
        int n = payoff.GetLength(1);
        var activeRows = new List<int>(Enumerable.Range(0, m));
        var activeCols = new List<int>(Enumerable.Range(0, n));
        var log = new List<string>();

        bool changed;
        do
        {
            changed = false;
            if (TryRemoveOneDominatedColumn(activeRows, activeCols, payoff, log))
                changed = true;
            else if (TryRemoveOneDominatedRow(activeRows, activeCols, payoff, log))
                changed = true;
        } while (changed);

        double[,] matrix = ExtractSubmatrix(payoff, activeRows, activeCols);
        return new Reduction(matrix, activeRows, activeCols, log);
    }

    private static bool TryRemoveOneDominatedColumn(
        List<int> activeRows,
        List<int> activeCols,
        double[,] full,
        List<string> log)
    {
        foreach (int k in activeCols.ToList())
        {
            foreach (int j in activeCols)
            {
                if (j == k) continue;
                if (!IsColumnDominatedBy(full, activeRows, k, j)) continue;
                activeCols.Remove(k);
                log.Add(
                    $"Вилучено стовпець {k + 1} (домінується стовпцем {j + 1}; індексація з 1).");
                return true;
            }
        }

        return false;
    }

    private static bool TryRemoveOneDominatedRow(
        List<int> activeRows,
        List<int> activeCols,
        double[,] full,
        List<string> log)
    {
        foreach (int k in activeRows.ToList())
        {
            foreach (int i in activeRows)
            {
                if (i == k) continue;
                if (!IsRowDominatedBy(full, activeCols, k, i)) continue;
                activeRows.Remove(k);
                log.Add(
                    $"Вилучено рядок {k + 1} (домінується рядком {i + 1}; індексація з 1).");
                return true;
            }
        }

        return false;
    }

    private static bool IsColumnDominatedBy(double[,] full, List<int> rows, int colK, int colJ)
    {
        bool strict = false;
        foreach (int i in rows)
        {
            double vj = full[i, colJ];
            double vk = full[i, colK];
            if (vj > vk + Tol)
                return false;
            if (vj < vk - Tol)
                strict = true;
        }

        return strict;
    }

    private static bool IsRowDominatedBy(double[,] full, List<int> cols, int rowK, int rowI)
    {
        bool strict = false;
        foreach (int j in cols)
        {
            double vi = full[rowI, j];
            double vk = full[rowK, j];
            if (vi < vk - Tol)
                return false;
            if (vi > vk + Tol)
                strict = true;
        }

        return strict;
    }

    private static double[,] ExtractSubmatrix(double[,] full, List<int> rows, List<int> cols)
    {
        int m = rows.Count;
        int n = cols.Count;
        var a = new double[m, n];
        for (int ii = 0; ii < m; ii++)
        {
            for (int jj = 0; jj < n; jj++)
                a[ii, jj] = full[rows[ii], cols[jj]];
        }

        return a;
    }
}
