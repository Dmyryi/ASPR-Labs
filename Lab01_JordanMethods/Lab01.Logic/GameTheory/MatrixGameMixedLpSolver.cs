using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.GameTheory;

public sealed class MatrixGameMixedLpSolver
{
    private const double Tol = 1e-10;
    private readonly ISimplexSolverFactory _solverFactory;

    public MatrixGameMixedLpSolver(ISimplexSolverFactory solverFactory)
    {
        _solverFactory = solverFactory;
    }

    public GameTheorySolveResult Solve(double[,] payoff)
    {
        int m = payoff.GetLength(0);
        int n = payoff.GetLength(1);
        if (m < 1 || n < 1)
            throw new ArgumentException("Матриця порожня.", nameof(payoff));

        double minEntry = Min(payoff);
        double shift = minEntry > Tol ? 0 : Tol - minEntry;
        var b = new double[m, n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
                b[i, j] = payoff[i, j] + shift;
        }

        var vectorZ = new double[n];
        for (int j = 0; j < n; j++)
            vectorZ[j] = -1;

        var matrixA = new double[m, n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
                matrixA[i, j] = b[i, j];
        }

        var vectorB = new double[m];
        for (int i = 0; i < m; i++)
            vectorB[i] = 1;

        SimplexSolverHandle handle = _solverFactory.Create(OptimizationMode.Maximization, SimplexOptions.Default);
        SolverResult result = handle.Solver.Solve(vectorZ, matrixA, vectorB);

        if (!result.Success || result.Z <= Tol)
            throw new InvalidOperationException("Симплекс не дав допустимого розв’язку для ЗЛП гри.");

        double sumX = result.Z;
        double v = 1 / sumX - shift;

        var col = new double[n];
        for (int j = 0; j < n; j++)
            col[j] = result.X[j] / sumX;

        var row = new double[m];
        double posSum = 0;
        for (int i = 0; i < m; i++)
        {
            double ui = Math.Max(0, i < result.U.Length ? result.U[i] : 0);
            row[i] = ui;
            posSum += ui;
        }

        if (posSum > Tol)
        {
            for (int i = 0; i < m; i++)
                row[i] /= posSum;
        }
        else
        {
            for (int i = 0; i < m; i++)
                row[i] = 1.0 / m;
        }

        Normalize(col);
        Normalize(row);

        return new GameTheorySolveResult
        {
            PayoffMatrix = Clone(payoff),
            HasSaddlePoint = false,
            SaddleRow = null,
            SaddleColumn = null,
            GameValue = v,
            RowPlayerStrategy = row,
            ColumnPlayerStrategy = col,
            SolutionKind = "Змішані стратегії (ЗЛП / симплекс)",
            LpShift = shift,
            LpObjectiveMaxSumX = sumX,
            EliminatedDominatedStrategies = false,
            DominanceReductionLog = null
        };
    }

    private static void Normalize(double[] v)
    {
        double s = 0;
        for (int i = 0; i < v.Length; i++)
            s += v[i];
        if (s <= Tol) return;
        for (int i = 0; i < v.Length; i++)
            v[i] /= s;
    }

    private static double Min(double[,] a)
    {
        double min = a[0, 0];
        int m = a.GetLength(0);
        int n = a.GetLength(1);
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (a[i, j] < min) min = a[i, j];
            }
        }

        return min;
    }

    private static double[,] Clone(double[,] a)
    {
        int m = a.GetLength(0);
        int n = a.GetLength(1);
        var c = new double[m, n];
        Array.Copy(a, c, a.Length);
        return c;
    }
}
