namespace Lab01.Logic.GameTheory;

public static class MatrixGameRectangularSolver
{
    private const double Tol = 1e-9;

    public static bool TrySolveTwoByN(double[,] payoff, out double value, out double row0Prob, out double[] columnStrategy)
    {
        columnStrategy = Array.Empty<double>();
        int n = payoff.GetLength(1);
        if (payoff.GetLength(0) != 2 || n < 2)
        {
            value = double.NaN;
            row0Prob = double.NaN;
            return false;
        }

        var candidates = new List<double> { 0, 1 };
        for (int j = 0; j < n; j++)
        {
            for (int k = j + 1; k < n; k++)
            {
                if (TryIntersectLines(
                        payoff[0, j], payoff[1, j],
                        payoff[0, k], payoff[1, k],
                        out double p) && p > Tol && p < 1 - Tol)
                    candidates.Add(p);
            }
        }

        double best = double.NegativeInfinity;
        double bestP = 0;
        foreach (double p in UniqueSorted(candidates))
        {
            double f = MinLineTwoByN(payoff, p);
            if (f > best + Tol)
            {
                best = f;
                bestP = p;
            }
        }

        value = best;
        row0Prob = bestP;

        var active = new List<int>();
        for (int j = 0; j < n; j++)
        {
            double y = LineTwoByN(payoff, j, bestP);
            if (Math.Abs(y - best) <= 1e-6 * (1 + Math.Abs(best)))
                active.Add(j);
        }

        if (active.Count == 0)
            return false;

        if (active.Count == 1)
        {
            columnStrategy = new double[n];
            columnStrategy[active[0]] = 1;
            return true;
        }

        for (int a = 0; a < active.Count; a++)
        {
            for (int b = a + 1; b < active.Count; b++)
            {
                int j0 = active[a];
                int j1 = active[b];
                if (!MatrixGameTwoByTwoSolver.TrySolve(
                        payoff[0, j0], payoff[0, j1],
                        payoff[1, j0], payoff[1, j1],
                        out double v2, out _, out double q0))
                    continue;

                if (Math.Abs(v2 - value) > 1e-5 * (1 + Math.Abs(value)))
                    continue;

                columnStrategy = new double[n];
                columnStrategy[j0] = q0;
                columnStrategy[j1] = 1 - q0;
                return true;
            }
        }

        return false;
    }

    public static bool TrySolveMByTwo(double[,] payoff, out double value, out double[] rowStrategy, out double col0Prob)
    {
        rowStrategy = Array.Empty<double>();
        int m = payoff.GetLength(0);
        if (payoff.GetLength(1) != 2 || m < 2)
        {
            value = double.NaN;
            col0Prob = double.NaN;
            return false;
        }

        var candidates = new List<double> { 0, 1 };
        for (int i = 0; i < m; i++)
        {
            for (int k = i + 1; k < m; k++)
            {
                if (TryIntersectLines(
                        payoff[i, 0], payoff[i, 1],
                        payoff[k, 0], payoff[k, 1],
                        out double q) && q > Tol && q < 1 - Tol)
                    candidates.Add(q);
            }
        }

        double best = double.PositiveInfinity;
        double bestQ = 0;
        foreach (double q in UniqueSorted(candidates))
        {
            double g = MaxLineMByTwo(payoff, q);
            if (g < best - Tol)
            {
                best = g;
                bestQ = q;
            }
        }

        value = best;
        col0Prob = bestQ;

        var active = new List<int>();
        for (int i = 0; i < m; i++)
        {
            double y = LineMByTwo(payoff, i, bestQ);
            if (Math.Abs(y - best) <= 1e-6 * (1 + Math.Abs(best)))
                active.Add(i);
        }

        if (active.Count == 0)
            return false;

        if (active.Count == 1)
        {
            rowStrategy = new double[m];
            rowStrategy[active[0]] = 1;
            return true;
        }

        for (int a = 0; a < active.Count; a++)
        {
            for (int b = a + 1; b < active.Count; b++)
            {
                int i0 = active[a];
                int i1 = active[b];
                if (!MatrixGameTwoByTwoSolver.TrySolve(
                        payoff[i0, 0], payoff[i0, 1],
                        payoff[i1, 0], payoff[i1, 1],
                        out double v2, out double p0, out _))
                    continue;

                if (Math.Abs(v2 - value) > 1e-5 * (1 + Math.Abs(value)))
                    continue;

                rowStrategy = new double[m];
                rowStrategy[i0] = p0;
                rowStrategy[i1] = 1 - p0;
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<double> UniqueSorted(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        if (sorted.Count == 0) yield break;
        double prev = sorted[0];
        yield return prev;
        for (int i = 1; i < sorted.Count; i++)
        {
            if (sorted[i] - prev <= 1e-12) continue;
            prev = sorted[i];
            yield return prev;
        }
    }

    private static double LineTwoByN(double[,] payoff, int j, double p) =>
        payoff[0, j] * p + payoff[1, j] * (1 - p);

    private static double MinLineTwoByN(double[,] payoff, double p)
    {
        int n = payoff.GetLength(1);
        double min = double.PositiveInfinity;
        for (int j = 0; j < n; j++)
        {
            double y = LineTwoByN(payoff, j, p);
            if (y < min) min = y;
        }

        return min;
    }

    private static double LineMByTwo(double[,] payoff, int i, double q) =>
        payoff[i, 0] * q + payoff[i, 1] * (1 - q);

    private static double MaxLineMByTwo(double[,] payoff, double q)
    {
        int m = payoff.GetLength(0);
        double max = double.NegativeInfinity;
        for (int i = 0; i < m; i++)
        {
            double y = LineMByTwo(payoff, i, q);
            if (y > max) max = y;
        }

        return max;
    }

    private static bool TryIntersectLines(double a0, double a1, double b0, double b1, out double p)
    {
        double s0 = a0 - a1;
        double s1 = b0 - b1;
        double den = s0 - s1;
        if (Math.Abs(den) < Tol)
        {
            p = double.NaN;
            return false;
        }

        p = (b1 - a1) / den;
        return true;
    }
}
