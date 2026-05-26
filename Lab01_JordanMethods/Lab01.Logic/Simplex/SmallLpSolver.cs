using Lab01.Logic.Models;
using Lab01.Logic.Simplex.Solvers;

namespace Lab01.Logic.Simplex;

public static class SmallLpSolver
{
    private const double Epsilon = 1e-7;

    public static SolverResult Solve(
        double[] objective,
        double[,] matrixA,
        double[] vectorB,
        OptimizationMode mode)
    {
        int n = objective.Length;
        int m = vectorB.Length;
        var inequalities = new List<(double[] Coefficients, double RightSide)>(m);
        for (int i = 0; i < m; i++)
        {
            var row = new double[n];
            for (int j = 0; j < n; j++)
                row[j] = matrixA[i, j];
            inequalities.Add((row, vectorB[i]));
        }

        return Solve(objective, inequalities, mode);
    }

    public static SolverResult Solve(
        double[] objective,
        IReadOnlyList<(double[] Coefficients, double RightSide)> inequalities,
        OptimizationMode mode)
    {
        int n = objective.Length;
        var allInequalities = new List<(double[] Coefficients, double RightSide)>(inequalities);
        for (int j = 0; j < n; j++)
        {
            var row = new double[n];
            row[j] = -1;
            allInequalities.Add((row, 0));
        }

        var candidates = new List<double[]>();
        candidates.Add(new double[n]);

        int m = allInequalities.Count;
        for (int mask = 1; mask < 1 << m; mask++)
        {
            if (BitCount(mask) != n)
                continue;

            if (TryIntersection(allInequalities, mask, n, out double[]? point))
                candidates.Add(point);
        }

        SolverResult? best = null;
        double bestValue = mode == OptimizationMode.Maximization
            ? double.NegativeInfinity
            : double.PositiveInfinity;

        foreach (double[] x in candidates)
        {
            if (!IsFeasible(x, inequalities))
                continue;

            double value = Dot(objective, x);
            bool better = mode == OptimizationMode.Maximization
                ? value > bestValue + Epsilon
                : value < bestValue - Epsilon;

            if (!better)
                continue;

            bestValue = value;
            best = new SolverResult
            {
                X = (double[])x.Clone(),
                Success = true,
                Z = mode == OptimizationMode.Maximization ? -value : value
            };
        }

        return best ?? new SolverResult
        {
            X = new double[n],
            Success = false,
            Message = "Допустимий розв'язок не знайдено."
        };
    }

    private static bool TryIntersection(
        IReadOnlyList<(double[] Coefficients, double RightSide)> inequalities,
        int mask,
        int n,
        out double[] point)
    {
        var a = new double[n, n];
        var b = new double[n];
        int row = 0;
        for (int i = 0; i < inequalities.Count; i++)
        {
            if ((mask & (1 << i)) == 0)
                continue;

            var (coeff, rhs) = inequalities[i];
            for (int j = 0; j < n; j++)
                a[row, j] = j < coeff.Length ? coeff[j] : 0;
            b[row] = rhs;
            row++;
        }

        point = new double[n];
        return GaussianEliminate(a, b, point);
    }

    private static bool GaussianEliminate(double[,] a, double[] b, double[] x)
    {
        int n = b.Length;
        var aug = new double[n, n + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                aug[i, j] = a[i, j];
            aug[i, n] = b[i];
        }

        for (int col = 0, row = 0; col < n && row < n; col++)
        {
            int pivot = row;
            for (int r = row + 1; r < n; r++)
            {
                if (Math.Abs(aug[r, col]) > Math.Abs(aug[pivot, col]))
                    pivot = r;
            }

            if (Math.Abs(aug[pivot, col]) < Epsilon)
                return false;

            SwapRows(aug, row, pivot);
            double div = aug[row, col];
            for (int c = col; c <= n; c++)
                aug[row, c] /= div;

            for (int r = 0; r < n; r++)
            {
                if (r == row)
                    continue;
                double factor = aug[r, col];
                if (Math.Abs(factor) < Epsilon)
                    continue;
                for (int c = col; c <= n; c++)
                    aug[r, c] -= factor * aug[row, c];
            }

            row++;
        }

        for (int i = 0; i < n; i++)
            x[i] = aug[i, n];
        return true;
    }

    private static bool IsFeasible(
        double[] x,
        IReadOnlyList<(double[] Coefficients, double RightSide)> inequalities)
    {
        for (int j = 0; j < x.Length; j++)
        {
            if (x[j] < -Epsilon)
                return false;
        }

        foreach (var (coeff, rhs) in inequalities)
        {
            double lhs = 0;
            for (int j = 0; j < x.Length; j++)
            {
                double a = j < coeff.Length ? coeff[j] : 0;
                lhs += a * x[j];
            }

            if (lhs > rhs + Epsilon)
                return false;
        }

        return true;
    }

    private static double Dot(double[] a, double[] b)
    {
        double s = 0;
        for (int i = 0; i < a.Length; i++)
            s += a[i] * b[i];
        return s;
    }

    private static int BitCount(int mask)
    {
        int c = 0;
        while (mask != 0)
        {
            c += mask & 1;
            mask >>= 1;
        }

        return c;
    }

    private static void SwapRows(double[,] m, int a, int b)
    {
        if (a == b)
            return;
        int cols = m.GetLength(1);
        for (int c = 0; c < cols; c++)
            (m[a, c], m[b, c]) = (m[b, c], m[a, c]);
    }
}
