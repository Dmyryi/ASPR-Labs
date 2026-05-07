using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using System;
using System.Linq;

public class MaximizationSolver : ILinearSolver
{
    private readonly IBasicSolutionFinder _basicFinder;
    private readonly IOptimalSolution _optimalFinder;
    private readonly ISimplexProtocol? _protocol;
    private readonly IZeroRowElliminator? _zeroRowElliminator;
    private readonly bool _useZeroRowElimination;

    public MaximizationSolver(IBasicSolutionFinder b, IOptimalSolution o, ISimplexProtocol? protocol = null, IZeroRowElliminator? zeroRowElliminator = null, bool useZeroRowElimination = true)
    {
        _basicFinder = b;
        _optimalFinder = o;
        _protocol = protocol;
        _zeroRowElliminator = zeroRowElliminator;
        _useZeroRowElimination = useZeroRowElimination;
    }

    public SolverResult Solve(double[] vectorZ, double[,] matrixA, double[] vectorB)
    {
        var tableau = new SimplexTableau(matrixA, vectorB, vectorZ);

        if (_useZeroRowElimination)
            _zeroRowElliminator?.Elliminate(tableau);

        _basicFinder.Find(tableau);

        _protocol?.LogBasicSolution(tableau);
        _optimalFinder.Find(tableau);

        double[] fullResults = GetResults(tableau);
        return new SolverResult
        {
            X = fullResults.Take(tableau.ProblemVariableCount).ToArray(),
            Y = ExtractSlackValues(tableau),
            Z = fullResults.Last(),
            Success = true
        };
    }

    private static double[] ExtractSlackValues(SimplexTableau tableau)
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

    public double[] GetResults(SimplexTableau tableau)
    {
        double[] results = new double[tableau.ProblemVariableCount + 1];

        for (int row = 0; row < tableau.RowsCount; row++)
        {
            int varIndex = tableau.BasisVariables[row];

            if (varIndex >= 0 && varIndex < tableau.ProblemVariableCount)
            {
                results[varIndex] = tableau.GetB(row);
            }
        }

        results[tableau.ProblemVariableCount] = tableau.Data[tableau.RowsCount, tableau.ColsCount];

        return results;
    }
}