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

        return BuildResult(tableau);
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
