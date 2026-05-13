namespace Lab01.Logic.GameTheory;

public static class SaddlePointFinder
{
    private const double Tol = 1e-9;

    public static bool TryFind(double[,] payoff, out int row, out int col, out double value)
    {
        int m = payoff.GetLength(0);
        int n = payoff.GetLength(1);

        var rowMins = new double[m];
        for (int i = 0; i < m; i++)
        {
            double min = payoff[i, 0];
            for (int j = 1; j < n; j++)
            {
                if (payoff[i, j] < min) min = payoff[i, j];
            }

            rowMins[i] = min;
        }

        double maximin = rowMins[0];
        int maximinRow = 0;
        for (int i = 1; i < m; i++)
        {
            if (rowMins[i] > maximin + Tol)
            {
                maximin = rowMins[i];
                maximinRow = i;
            }
        }

        var colMaxes = new double[n];
        for (int j = 0; j < n; j++)
        {
            double max = payoff[0, j];
            for (int i = 1; i < m; i++)
            {
                if (payoff[i, j] > max) max = payoff[i, j];
            }

            colMaxes[j] = max;
        }

        double minimax = colMaxes[0];
        int minimaxCol = 0;
        for (int j = 1; j < n; j++)
        {
            if (colMaxes[j] < minimax - Tol)
            {
                minimax = colMaxes[j];
                minimaxCol = j;
            }
        }

        if (Math.Abs(maximin - minimax) > Tol * (1 + Math.Abs(maximin)))
        {
            row = -1;
            col = -1;
            value = double.NaN;
            return false;
        }

        value = maximin;
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (Math.Abs(payoff[i, j] - value) > Tol) continue;
                if (Math.Abs(rowMins[i] - value) > Tol) continue;
                if (Math.Abs(colMaxes[j] - value) > Tol) continue;
                row = i;
                col = j;
                return true;
            }
        }

        row = maximinRow;
        col = minimaxCol;
        return Math.Abs(payoff[row, col] - value) <= Tol;
    }
}
