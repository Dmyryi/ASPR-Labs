using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Simplex.Solvers;

public abstract class LinearSolverBase : ILinearSolver
{
    protected IBasicSolutionFinder BasicFinder { get; }
    protected IOptimalSolution OptimalFinder { get; }
    protected ISimplexProtocol? Protocol { get; }
    protected OptimizationMode OptimizationMode { get; }

    protected LinearSolverBase(
        IBasicSolutionFinder basicFinder,
        IOptimalSolution optimalFinder,
        OptimizationMode optimizationMode,
        ISimplexProtocol? protocol)
    {
        BasicFinder = basicFinder;
        OptimalFinder = optimalFinder;
        Protocol = protocol;
        OptimizationMode = optimizationMode;
    }

    public SolverResult Solve(double[] vectorZ, double[,] matrixA, double[] vectorB)
    {
        double[] preparedZ = PrepareObjective(vectorZ);
        var tableau = new SimplexTableau(matrixA, vectorB, preparedZ);

        BeforeBasicStage(tableau);
        BasicFinder.Find(tableau);

        Protocol?.LogBasicSolution(tableau);
        OptimalFinder.Find(tableau);

        SolverResult result = BuildResult(tableau);
        if (!IsFeasibleStandardForm(matrixA, vectorB, result.X) &&
            matrixA.GetLength(1) <= 6 &&
            matrixA.GetLength(0) <= 12)
        {
            SolverResult fallback = SmallLpSolver.Solve(vectorZ, matrixA, vectorB, OptimizationMode);
            if (fallback.Success && IsFeasibleStandardForm(matrixA, vectorB, fallback.X))
                return fallback;
        }

        return result;
    }

    private static bool IsFeasibleStandardForm(double[,] matrixA, double[] vectorB, double[] x)
    {
        const double eps = 1e-6;
        int m = matrixA.GetLength(0);
        int n = matrixA.GetLength(1);
        for (int i = 0; i < m; i++)
        {
            double lhs = 0;
            for (int j = 0; j < n; j++)
                lhs += matrixA[i, j] * x[j];
            if (lhs > vectorB[i] + eps)
                return false;
        }

        for (int j = 0; j < n; j++)
        {
            if (x[j] < -eps)
                return false;
        }

        return true;
    }

    protected virtual void BeforeBasicStage(SimplexTableau tableau) { }

    protected virtual double[] PrepareObjective(double[] vectorZ) => vectorZ;

    protected virtual SolverResult BuildResult(SimplexTableau tableau)
    {
        double[] x = ExtractDecisionVariables(tableau);
        double[] y = ExtractSlackValues(tableau);
        double z = tableau.Data[tableau.RowsCount, tableau.ColsCount];
        double[] u = DualMultiplierExtractor.FromFinalTableau(tableau, OptimizationMode);

        return new SolverResult
        {
            X = x,
            Y = y,
            Z = z,
            U = u,
            Success = true
        };
    }

    protected static double[] ExtractDecisionVariables(SimplexTableau tableau)
    {
        var x = new double[tableau.ProblemVariableCount];
        for (int row = 0; row < tableau.RowsCount; row++)
        {
            int varIndex = tableau.BasisVariables[row];
            if (varIndex >= 0 && varIndex < tableau.ProblemVariableCount)
                x[varIndex] = tableau.GetB(row);
        }
        return x;
    }

    protected static double[] ExtractSlackValues(SimplexTableau tableau)
    {
        int slackCount = tableau.RowsCount;
        var y = new double[slackCount];

        for (int row = 0; row < tableau.RowsCount; row++)
        {
            int varIndex = tableau.BasisVariables[row];
            int slackIndex = varIndex - tableau.ProblemVariableCount;
            if (slackIndex >= 0 && slackIndex < slackCount)
                y[slackIndex] = tableau.GetB(row);
        }

        return y;
    }
}
