using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Gomori;

public interface IGomorySolver
{
    SolverResult Solve(
        double[] vectorZ,
        double[,] matrixA,
        double[] vectorB,
        OptimizationMode mode,
        GomoryOptions? options = null,
        ISimplexProtocol? protocol = null);
}
