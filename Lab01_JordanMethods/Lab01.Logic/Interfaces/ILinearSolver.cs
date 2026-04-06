using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab01.Logic.Models;

namespace Lab01.Logic.Interfaces
{
    public interface ILinearSolver
    {
        SolverResult Solve(double[] vectorZ, double[,] matrixA, double[] vectorB);
    }
}