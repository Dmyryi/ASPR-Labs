using System;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex
{
    public class SimplexContext
    {
        private ILinearSolver? solver;

        public SimplexContext() { }

        public SimplexContext(ILinearSolver solver) { this.solver = solver; }

        public void SetStrategy(ILinearSolver solver)
        {
            this.solver = solver;
        }

        public SolverResult ExecuteStrategy(double[] z, double[,] a, double[] b)
        {
            if (solver == null)
            {
                throw new InvalidOperationException(
                    "Солвер не встановлено. Спершу оберіть стратегію (Min або Max).");
            }

            return solver.Solve(z, a, b);
        }
    }
}
