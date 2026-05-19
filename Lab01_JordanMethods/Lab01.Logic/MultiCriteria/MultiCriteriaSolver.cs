using Lab01.Logic.GameTheory;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.MultiCriteria;

public sealed class MultiCriteriaSolver
{
    private const double Epsilon = 1e-9;

    private readonly MultiCriteriaProblemParser _parser;
    private readonly ISimplexSolverFactory _simplexFactory;
    private readonly MatrixGameSolver _gameSolver;

    public MultiCriteriaSolver(
        ILinearProgramParser linearParser,
        ISimplexSolverFactory simplexFactory,
        MatrixGameSolver gameSolver)
    {
        _parser = new MultiCriteriaProblemParser(linearParser);
        _simplexFactory = simplexFactory;
        _gameSolver = gameSolver;
    }

    public MultiCriteriaSolveResult Solve(string objectivesText, string constraintsText)
    {
        IReadOnlyList<MultiCriteriaObjective> objectives = _parser.ParseObjectives(objectivesText);
        int k = objectives.Count;
        int varHint = objectives.Max(o => o.Coefficients.Length);
        MultiCriteriaConstraintSet constraints = _parser.ParseConstraints(constraintsText, varHint);
        int variableCount = constraints.VariableCount;

        var solutions = new SolverResult[k];
        for (int i = 0; i < k; i++)
        {
            solutions[i] = EqualityReducedLpSolver.Solve(
                PadCoefficients(objectives[i].Coefficients, variableCount),
                constraints.Equalities,
                constraints.Inequalities,
                _simplexFactory,
                objectives[i].Mode);
            if (!solutions[i].Success)
                throw new InvalidOperationException($"Симплекс не знайшов розв'язок для Z{i + 1}.");
        }

        double[,] f = new double[k, k];
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < k; j++)
                f[i, j] = Evaluate(objectives[j], solutions[i].X);
        }

        double[,] q = BuildSuboptimalityMatrix(f, objectives);
        double[,] g = BuildGameMatrix(q);
        GameTheorySolveResult game = _gameSolver.Solve(g);

        double[] weights = game.RowPlayerStrategy;
        double[] xKomp = new double[variableCount];
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < variableCount; j++)
                xKomp[j] += weights[i] * solutions[i].X[j];
        }

        return new MultiCriteriaSolveResult
        {
            Objectives = objectives,
            ConstraintsText = constraintsText,
            PerObjectiveSolutions = solutions,
            ObjectiveValues = f,
            SuboptimalityMatrix = q,
            GameMatrix = g,
            Weights = weights,
            CompromiseSolution = xKomp,
            GameResult = game
        };
    }

    internal static double[,] BuildSuboptimalityMatrix(
        double[,] objectiveValues,
        IReadOnlyList<MultiCriteriaObjective> objectives)
    {
        int k = objectives.Count;
        var q = new double[k, k];
        for (int j = 0; j < k; j++)
        {
            if (objectives[j].Mode == OptimizationMode.Maximization)
            {
                double maxCol = MaxColumn(objectiveValues, j);
                if (maxCol < Epsilon)
                    maxCol = 1.0;

                for (int i = 0; i < k; i++)
                {
                    if (i == j)
                    {
                        q[i, j] = 0;
                        continue;
                    }

                    double delta = maxCol - objectiveValues[i, j];
                    q[i, j] = delta <= Epsilon ? 0 : delta / maxCol;
                }
            }
            else
            {
                double minCol = MinColumn(objectiveValues, j);
                double denom = Math.Abs(objectiveValues[j, j]);
                if (denom < Epsilon)
                    denom = MaxColumn(objectiveValues, j) - minCol;
                if (denom < Epsilon)
                    denom = 1.0;

                for (int i = 0; i < k; i++)
                {
                    if (i == j)
                    {
                        q[i, j] = 0;
                        continue;
                    }

                    double delta = objectiveValues[i, j] - minCol;
                    q[i, j] = delta <= Epsilon ? 0 : delta / denom;
                }
            }
        }

        return q;
    }

    internal static double[,] BuildGameMatrix(double[,] suboptimality)
    {
        int k = suboptimality.GetLength(0);
        double maxQ = 0;
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < k; j++)
                maxQ = Math.Max(maxQ, suboptimality[i, j]);
        }

        var g = new double[k, k];
        for (int i = 0; i < k; i++)
        {
            for (int j = 0; j < k; j++)
                g[i, j] = maxQ - suboptimality[i, j];
        }

        return g;
    }

    private static double MinColumn(double[,] f, int j)
    {
        double m = f[0, j];
        for (int i = 1; i < f.GetLength(0); i++)
            m = Math.Min(m, f[i, j]);
        return m;
    }

    private static double MaxColumn(double[,] f, int j)
    {
        double m = f[0, j];
        for (int i = 1; i < f.GetLength(0); i++)
            m = Math.Max(m, f[i, j]);
        return m;
    }

    private static double[] PadCoefficients(double[] c, int n)
    {
        if (c.Length >= n)
            return c.Length == n ? c : c.Take(n).ToArray();

        var padded = new double[n];
        Array.Copy(c, padded, c.Length);
        return padded;
    }

    private static double Evaluate(MultiCriteriaObjective objective, double[] x)
    {
        double sum = 0;
        for (int i = 0; i < x.Length && i < objective.Coefficients.Length; i++)
            sum += objective.Coefficients[i] * x[i];
        return sum;
    }
}
