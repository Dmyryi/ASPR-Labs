using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using System.Linq;

namespace Lab01.Logic.Simplex
{
    public class MinimizationSolver : ILinearSolver
    {
        private readonly IBasicSolutionFinder _basicFinder;
        private readonly IOptimalSolution _optimalFinder;

        public MinimizationSolver(IBasicSolutionFinder b, IOptimalSolution o)
        {
            _basicFinder = b;
            _optimalFinder = o;
        }

        public SolverResult Solve(double[] vectorZ, double[,] matrixA, double[] vectorB)
        {
            
            double[] invertedZ = vectorZ.Select(val => -val).ToArray();

            var tableau = new SimplexTableau(matrixA, vectorB, invertedZ);

            _basicFinder.Find(tableau);

            _optimalFinder.Find(tableau);

            double[] fullResults = GetResults(tableau);
            double maxZPrime = fullResults.Last();

            double finalMinZ = -maxZPrime;

            return new SolverResult
            {
                X = fullResults.Take(tableau.ColsCount).ToArray(),
                Z = finalMinZ,
                Success = true
            };
        }

        private double[] GetResults(SimplexTableau tableau)
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
}