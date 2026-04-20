using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using System;
using System.Linq;

public class MaximizationSolver : ILinearSolver
{
    private readonly IBasicSolutionFinder _basicFinder;
    private readonly IOptimalSolution _optimalFinder;
    private readonly ISimplexProtocol? _protocol;

    public MaximizationSolver(IBasicSolutionFinder b, IOptimalSolution o, ISimplexProtocol? protocol = null)
    {
        _basicFinder = b;
        _optimalFinder = o;
        _protocol = protocol;
    }

    public SolverResult Solve(double[] vectorZ, double[,] matrixA, double[] vectorB)
    {
        var tableau = new SimplexTableau(matrixA, vectorB, vectorZ);

        _basicFinder.Find(tableau);
        _protocol?.LogBasicSolution(tableau);
        _optimalFinder.Find(tableau);

        double[] fullResults = GetResults(tableau);

        return new SolverResult
        {
            X = fullResults.Take(tableau.ColsCount).ToArray(),
            Z = fullResults.Last(),
            Success = true
        };
    }

    public double[] GetResults(SimplexTableau tableau)
    {
        double[] results = new double[tableau.ColsCount + 1];

        for (int row = 0; row < tableau.RowsCount; row++)
        {
            int varIndex = tableau.BasisVariables[row];

            if (varIndex >= 0 && varIndex < tableau.ColsCount)
            {
                results[varIndex] = tableau.GetB(row);
            }
        }

        results[tableau.ColsCount] = tableau.Data[tableau.RowsCount, tableau.ColsCount];

        return results;
    }
}