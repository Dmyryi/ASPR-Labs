using Lab01.Logic.Interfaces;

namespace Lab01.Logic.GameTheory;

/// <summary>
/// Алгоритм рис. 6.1: після вилучення домінованих стратегій — сідло; якщо немає — 2×2, 2×n, m×2 або пара двоїстих ЗЛП (симплекс).
/// </summary>
public sealed class MatrixGameSolver
{
    private readonly MatrixGameMixedLpSolver _lp;

    public MatrixGameSolver(ISimplexSolverFactory simplexSolverFactory)
    {
        _lp = new MatrixGameMixedLpSolver(simplexSolverFactory);
    }

    public GameTheorySolveResult Solve(double[,] payoff)
    {
        int m0 = payoff.GetLength(0);
        int n0 = payoff.GetLength(1);
        if (m0 < 1 || n0 < 1)
            throw new ArgumentException("Матриця порожня.", nameof(payoff));

        double[,] original = Clone(payoff);
        DominatedStrategyReducer.Reduction red = DominatedStrategyReducer.Reduce(payoff);
        bool dom = red.Log.Count > 0;
        string? logText = red.Log.Count == 0 ? null : string.Join(Environment.NewLine, red.Log);

        double[,] a = red.Matrix;
        int m = a.GetLength(0);
        int n = a.GetLength(1);
        IReadOnlyList<int> rowOrig = red.OriginalRowIndices;
        IReadOnlyList<int> colOrig = red.OriginalColumnIndices;

        if (m == 1)
            return SolveOneRowExpanded(a, original, rowOrig, colOrig, m0, n0, dom, logText);

        if (n == 1)
            return SolveOneColumnExpanded(a, original, rowOrig, colOrig, m0, n0, dom, logText);

        if (SaddlePointFinder.TryFind(a, out int sr, out int sc, out double saddleValue))
        {
            int origR = rowOrig[sr];
            int origC = colOrig[sc];
            return PackSaddle(original, origR, origC, saddleValue, m0, n0, dom, logText);
        }

        if (m == 2 && n == 2)
            return PackTwoByTwoExpanded(a, original, rowOrig, colOrig, m0, n0, dom, logText);

        if (m == 2 && n > 2 && MatrixGameRectangularSolver.TrySolveTwoByN(
                a, out double v2n, out double p0, out double[] colMix))
        {
            var rowR = new[] { p0, 1 - p0 };
            return PackMixed(
                original,
                ExpandRow(rowR, rowOrig, m0),
                ExpandCol(colMix, colOrig, n0),
                v2n,
                WithDominancePrefix("2 × n (нижня огортна, активні стратегії → 2 × 2)", dom),
                dom,
                logText);
        }

        if (n == 2 && m > 2 && MatrixGameRectangularSolver.TrySolveMByTwo(
                a, out double vm2, out double[] rowMix, out double q0))
        {
            var colR = new[] { q0, 1 - q0 };
            return PackMixed(
                original,
                ExpandRow(rowMix, rowOrig, m0),
                ExpandCol(colR, colOrig, n0),
                vm2,
                WithDominancePrefix("m × 2 (верхня огортна, активні стратегії → 2 × 2)", dom),
                dom,
                logText);
        }

        GameTheorySolveResult lp = _lp.Solve(a);
        return new GameTheorySolveResult
        {
            PayoffMatrix = Clone(original),
            HasSaddlePoint = false,
            SaddleRow = null,
            SaddleColumn = null,
            GameValue = lp.GameValue,
            RowPlayerStrategy = ExpandRow(lp.RowPlayerStrategy, rowOrig, m0),
            ColumnPlayerStrategy = ExpandCol(lp.ColumnPlayerStrategy, colOrig, n0),
            SolutionKind = WithDominancePrefix(lp.SolutionKind, dom),
            LpShift = lp.LpShift,
            LpObjectiveMaxSumX = lp.LpObjectiveMaxSumX,
            EliminatedDominatedStrategies = dom,
            DominanceReductionLog = logText
        };
    }

    private static GameTheorySolveResult PackSaddle(
        double[,] original,
        int origRow,
        int origCol,
        double value,
        int origM,
        int origN,
        bool dom,
        string? log)
    {
        var row = new double[origM];
        row[origRow] = 1;
        var col = new double[origN];
        col[origCol] = 1;
        return new GameTheorySolveResult
        {
            PayoffMatrix = Clone(original),
            HasSaddlePoint = true,
            SaddleRow = origRow,
            SaddleColumn = origCol,
            GameValue = value,
            RowPlayerStrategy = row,
            ColumnPlayerStrategy = col,
            SolutionKind = WithDominancePrefix("Чисті стратегії (сідлова точка)", dom),
            LpShift = null,
            LpObjectiveMaxSumX = null,
            EliminatedDominatedStrategies = dom,
            DominanceReductionLog = log
        };
    }

    private static GameTheorySolveResult PackTwoByTwoExpanded(
        double[,] a,
        double[,] original,
        IReadOnlyList<int> rowOrig,
        IReadOnlyList<int> colOrig,
        int origM,
        int origN,
        bool dom,
        string? log)
    {
        if (!MatrixGameTwoByTwoSolver.TrySolve(
                a[0, 0], a[0, 1],
                a[1, 0], a[1, 1],
                out double v, out double p0, out double q0))
            throw new InvalidOperationException("Не вдалося розв’язати гру 2 × 2.");

        var rowR = new[] { p0, 1 - p0 };
        var colR = new[] { q0, 1 - q0 };
        return PackMixed(
            original,
            ExpandRow(rowR, rowOrig, origM),
            ExpandCol(colR, colOrig, origN),
            v,
            WithDominancePrefix("2 × 2 (аналітичні формули)", dom),
            dom,
            log);
    }

    private static GameTheorySolveResult PackMixed(
        double[,] original,
        double[] rowFull,
        double[] colFull,
        double value,
        string kind,
        bool dom,
        string? log) =>
        new()
        {
            PayoffMatrix = Clone(original),
            HasSaddlePoint = false,
            SaddleRow = null,
            SaddleColumn = null,
            GameValue = value,
            RowPlayerStrategy = rowFull,
            ColumnPlayerStrategy = colFull,
            SolutionKind = kind,
            LpShift = null,
            LpObjectiveMaxSumX = null,
            EliminatedDominatedStrategies = dom,
            DominanceReductionLog = log
        };

    private static GameTheorySolveResult SolveOneRowExpanded(
        double[,] a,
        double[,] original,
        IReadOnlyList<int> rowOrig,
        IReadOnlyList<int> colOrig,
        int origM,
        int origN,
        bool dom,
        string? log)
    {
        int n = a.GetLength(1);
        int bestJ = 0;
        double best = a[0, 0];
        for (int j = 1; j < n; j++)
        {
            if (a[0, j] < best)
            {
                best = a[0, j];
                bestJ = j;
            }
        }

        int origR = rowOrig[0];
        int origC = colOrig[bestJ];
        var row = new double[origM];
        row[origR] = 1;
        var col = new double[origN];
        col[origC] = 1;
        return new GameTheorySolveResult
        {
            PayoffMatrix = Clone(original),
            HasSaddlePoint = true,
            SaddleRow = origR,
            SaddleColumn = origC,
            GameValue = best,
            RowPlayerStrategy = row,
            ColumnPlayerStrategy = col,
            SolutionKind = WithDominancePrefix("Чисті стратегії (1 × n)", dom),
            LpShift = null,
            LpObjectiveMaxSumX = null,
            EliminatedDominatedStrategies = dom,
            DominanceReductionLog = log
        };
    }

    private static GameTheorySolveResult SolveOneColumnExpanded(
        double[,] a,
        double[,] original,
        IReadOnlyList<int> rowOrig,
        IReadOnlyList<int> colOrig,
        int origM,
        int origN,
        bool dom,
        string? log)
    {
        int m = a.GetLength(0);
        int bestI = 0;
        double best = a[0, 0];
        for (int i = 1; i < m; i++)
        {
            if (a[i, 0] > best)
            {
                best = a[i, 0];
                bestI = i;
            }
        }

        int origR = rowOrig[bestI];
        int origC = colOrig[0];
        var row = new double[origM];
        row[origR] = 1;
        var col = new double[origN];
        col[origC] = 1;
        return new GameTheorySolveResult
        {
            PayoffMatrix = Clone(original),
            HasSaddlePoint = true,
            SaddleRow = origR,
            SaddleColumn = origC,
            GameValue = best,
            RowPlayerStrategy = row,
            ColumnPlayerStrategy = col,
            SolutionKind = WithDominancePrefix("Чисті стратегії (m × 1)", dom),
            LpShift = null,
            LpObjectiveMaxSumX = null,
            EliminatedDominatedStrategies = dom,
            DominanceReductionLog = log
        };
    }

    private static double[] ExpandRow(double[] reduced, IReadOnlyList<int> rowOrig, int origM)
    {
        var v = new double[origM];
        for (int i = 0; i < reduced.Length; i++)
            v[rowOrig[i]] = reduced[i];
        return v;
    }

    private static double[] ExpandCol(double[] reduced, IReadOnlyList<int> colOrig, int origN)
    {
        var v = new double[origN];
        for (int j = 0; j < reduced.Length; j++)
            v[colOrig[j]] = reduced[j];
        return v;
    }

    private static string WithDominancePrefix(string kind, bool dom) =>
        dom ? "Вилучення домінованих стратегій → " + kind : kind;

    private static double[,] Clone(double[,] a)
    {
        int m = a.GetLength(0);
        int n = a.GetLength(1);
        var c = new double[m, n];
        Array.Copy(a, c, a.Length);
        return c;
    }
}
