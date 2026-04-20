using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using System.Linq;

namespace Lab01.Logic.Simplex
{
    public class MinimizationSolver : ILinearSolver
    {
        private readonly IBasicSolutionFinder _basicFinder;
        private readonly IOptimalSolution _optimalFinder;
        private readonly ISimplexProtocol? _protocol;

        public MinimizationSolver(IBasicSolutionFinder b, IOptimalSolution o, ISimplexProtocol? protocol = null)
        {
            _basicFinder = b;
            _optimalFinder = o;
            _protocol = protocol;
        }

        public SolverResult Solve(double[] vectorZ, double[,] matrixA, double[] vectorB)
        {
            
            double[] invertedZ = vectorZ.Select(val => -val).ToArray();

            var tableau = new SimplexTableau(matrixA, vectorB, invertedZ);

            _basicFinder.Find(tableau);

            _protocol?.LogBasicSolution(tableau);
            _optimalFinder.Find(tableau);

            double[] fullResults = GetResults(tableau);
            double maxZPrime = fullResults.Last();

            return new SolverResult
            {
                X = fullResults.Take(tableau.ColsCount).ToArray(),
                Z = maxZPrime,
                Success = true
            };
        }

        private double[] GetResults(SimplexTableau tableau)
        {
            double[] results = new double[tableau.ColsCount + 1];

            for (int row = 0; row < tableau.RowsCount; row++)
            {
                int basisColumn = tableau.BasisVariables[row];

                if (basisColumn >= 0 && basisColumn < tableau.ColsCount)
                    results[basisColumn] = tableau.GetB(row);
            }

            results[tableau.ColsCount] = tableau.Data[tableau.RowsCount, tableau.ColsCount];
            return results;
        }
    }
}