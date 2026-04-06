using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

public class MaximizationSolver : ILinearSolver
{
    private readonly IBasicSolutionFinder _basicFinder;
    private readonly IOptimalSolution _optimalFinder;

    public MaximizationSolver(IBasicSolutionFinder b, IOptimalSolution o)
    {
        _basicFinder = b; _optimalFinder = o;
    }

    public SolverResult Solve(double[] vectorZ, double[,] matrixA, double[] vectorB)
    {
        var tableau = new SimplexTableau(matrixA, vectorB, vectorZ);

        _basicFinder.Find(tableau);
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

        for (int i = 0; i < tableau.RowsCount; i++)
        {
            if (i < tableau.ColsCount)
                results[i] = tableau.GetB(i);
        }

        results[tableau.ColsCount] = tableau.Data[tableau.RowsCount, tableau.ColsCount];

        return results;
    }
}