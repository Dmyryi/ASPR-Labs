using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex
{
    public class SimplexContext
    {
        private ILinearSolver solver;

        public SimplexContext() { }

        public SimplexContext(ILinearSolver solver) { this.solver = solver; }

        public void SetStrategy(ILinearSolver solver)
        {
            this.solver = solver;
        }

        public SolverResult ExecuteStrategy(double[] z, double[,] a, double[] b)
        {
            if (solver == null)
                throw new Exception("Солвер не встановлено! Виберіть стратегію (Min або Max).");

            return solver.Solve(z, a, b);
        }
    }
}
