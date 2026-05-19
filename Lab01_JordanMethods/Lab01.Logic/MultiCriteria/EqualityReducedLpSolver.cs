using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.MultiCriteria;

public static class EqualityReducedLpSolver
{
    private const double Epsilon = 1e-9;

    public static SolverResult Solve(
        double[] objective,
        IReadOnlyList<(double[] Coefficients, double RightSide)> equalities,
        IReadOnlyList<(double[] Coefficients, double RightSide)> inequalities,
        ISimplexSolverFactory simplexFactory,
        OptimizationMode mode)
    {
        int n = objective.Length;
        if (equalities.Count == 0)
        {
            var rows = WithNonNegativity(inequalities, n);
            return n <= 5
                ? SmallLpSolver.Solve(objective, rows, mode)
                : SolveDirect(objective, rows, simplexFactory, mode);
        }

        double[,] aEq = new double[equalities.Count, n];
        double[] bEq = new double[equalities.Count];
        for (int i = 0; i < equalities.Count; i++)
        {
            var (coeff, rhs) = equalities[i];
            for (int j = 0; j < n; j++)
                aEq[i, j] = j < coeff.Length ? coeff[j] : 0;
            bEq[i] = rhs;
        }

        if (!TryBuildParametricForm(aEq, bEq, n, out int[] freeCols, out int[] basicCols, out double[,] basicInv))
            return SolveDirect(objective, Concatenate(equalities, inequalities), simplexFactory, mode);

        int f = freeCols.Length;
        var reducedIneq = new List<(double[] Coefficients, double RightSide)>();

        void AddInequality(double[] aRow, double rhs)
        {
            var row = new double[f];
            double bound = rhs;
            for (int j = 0; j < n; j++)
            {
                double a = j < aRow.Length ? aRow[j] : 0;
                if (Math.Abs(a) < Epsilon)
                    continue;

                double c0 = ConstantTerm(basicCols, basicInv, freeCols, j);
                bound -= a * c0;
                for (int k = 0; k < f; k++)
                {
                    double ck = FreeCoeff(basicCols, basicInv, freeCols, j, k);
                    row[k] += a * ck;
                }
            }

            reducedIneq.Add((row, bound));
        }

        foreach (var (coeff, rhs) in inequalities)
            AddInequality(coeff, rhs);

        for (int j = 0; j < n; j++)
        {
            var row = new double[n];
            row[j] = -1;
            AddInequality(row, 0);
        }

        var reducedObj = new double[f];
        for (int k = 0; k < f; k++)
        {
            for (int j = 0; j < n; j++)
                reducedObj[k] += objective[j] * FreeCoeff(basicCols, basicInv, freeCols, j, k);
        }

        SolverResult reduced = reducedObj.Length <= 5
            ? SmallLpSolver.Solve(reducedObj, reducedIneq, mode)
            : SolveDirect(reducedObj, reducedIneq, simplexFactory, mode);
        if (!reduced.Success)
            return reduced;

        var x = Expand(basicCols, basicInv, freeCols, reduced.X, n);
        return new SolverResult
        {
            X = x,
            Success = true,
            Z = reduced.Z
        };
    }

    private static double[] Expand(int[] basicCols, double[,] basicInv, int[] freeCols, double[] t, int n)
    {
        var x = new double[n];
        for (int k = 0; k < freeCols.Length; k++)
            x[freeCols[k]] = t[k];

        for (int bi = 0; bi < basicCols.Length; bi++)
        {
            double val = basicInv[bi, freeCols.Length];
            for (int k = 0; k < freeCols.Length; k++)
                val += basicInv[bi, k] * t[k];
            x[basicCols[bi]] = val;
        }

        return x;
    }

    private static double ConstantTerm(int[] basicCols, double[,] basicInv, int[] freeCols, int varIndex)
    {
        int bi = Array.IndexOf(basicCols, varIndex);
        return bi < 0 ? 0 : basicInv[bi, freeCols.Length];
    }

    private static double FreeCoeff(int[] basicCols, double[,] basicInv, int[] freeCols, int varIndex, int freeSlot)
    {
        int fi = Array.IndexOf(freeCols, varIndex);
        if (fi >= 0)
            return fi == freeSlot ? 1.0 : 0.0;

        int bi = Array.IndexOf(basicCols, varIndex);
        return bi < 0 ? 0 : basicInv[bi, freeSlot];
    }

    private static SolverResult SolveDirect(
        double[] objective,
        IReadOnlyList<(double[] Coefficients, double RightSide)> rows,
        ISimplexSolverFactory simplexFactory,
        OptimizationMode mode)
    {
        int n = objective.Length;
        if (rows.Count == 0)
        {
            var handleOnly = simplexFactory.Create(mode, new SimplexOptions { UseZeroRowElimination = false });
            return handleOnly.Solver.Solve(objective, new double[0, n], Array.Empty<double>());
        }

        double[,] a = new double[rows.Count, n];
        double[] b = new double[rows.Count];
        for (int i = 0; i < rows.Count; i++)
        {
            for (int j = 0; j < n; j++)
                a[i, j] = j < rows[i].Coefficients.Length ? rows[i].Coefficients[j] : 0;
            b[i] = rows[i].RightSide;
        }

        var handle = simplexFactory.Create(mode, new SimplexOptions { UseZeroRowElimination = false });
        return handle.Solver.Solve(objective, a, b);
    }

    private static IReadOnlyList<(double[] Coefficients, double RightSide)> WithNonNegativity(
        IReadOnlyList<(double[] Coefficients, double RightSide)> rows,
        int n)
    {
        var list = rows.ToList();
        for (int j = 0; j < n; j++)
        {
            var row = new double[n];
            row[j] = -1;
            list.Add((row, 0));
        }

        return list;
    }

    private static IReadOnlyList<(double[] Coefficients, double RightSide)> Concatenate(
        IReadOnlyList<(double[] Coefficients, double RightSide)> eq,
        IReadOnlyList<(double[] Coefficients, double RightSide)> ineq) =>
        eq.Concat(ineq).ToList();

    private static bool TryBuildParametricForm(
        double[,] a,
        double[] b,
        int n,
        out int[] freeCols,
        out int[] basicCols,
        out double[,] basicInv)
    {
        int m = a.GetLength(0);
        var aug = new double[m, n + 1];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
                aug[i, j] = a[i, j];
            aug[i, n] = b[i];
        }

        var pivotCols = new List<int>();
        int row = 0;
        for (int col = 0; col < n && row < m; col++)
        {
            int pivotRow = row;
            for (int r = row + 1; r < m; r++)
            {
                if (Math.Abs(aug[r, col]) > Math.Abs(aug[pivotRow, col]))
                    pivotRow = r;
            }

            if (Math.Abs(aug[pivotRow, col]) < Epsilon)
                continue;

            SwapRows(aug, row, pivotRow);
            double pivot = aug[row, col];
            for (int c = col; c <= n; c++)
                aug[row, c] /= pivot;

            for (int r = 0; r < m; r++)
            {
                if (r == row)
                    continue;
                double factor = aug[r, col];
                if (Math.Abs(factor) < Epsilon)
                    continue;
                for (int c = col; c <= n; c++)
                    aug[r, c] -= factor * aug[row, c];
            }

            pivotCols.Add(col);
            row++;
        }

        if (pivotCols.Count < m)
        {
            freeCols = Array.Empty<int>();
            basicCols = Array.Empty<int>();
            basicInv = new double[0, 0];
            return false;
        }

        basicCols = pivotCols.ToArray();
        freeCols = Enumerable.Range(0, n).Except(basicCols).ToArray();
        int f = freeCols.Length;
        basicInv = new double[m, f + 1];

        for (int bi = 0; bi < m; bi++)
        {
            for (int k = 0; k < f; k++)
                basicInv[bi, k] = -aug[bi, freeCols[k]];
            basicInv[bi, f] = aug[bi, n];
        }

        return true;
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
