using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Gomori;

public interface IGomorySolver
{
    /// <summary>
    /// Розв'язує цілочислову задачу лінійного програмування методом Гоморі.
    /// Передбачається, що всі змінні задачі мають бути цілими ≥ 0.
    /// </summary>
    SolverResult Solve(
        double[] vectorZ,
        double[,] matrixA,
        double[] vectorB,
        OptimizationMode mode,
        GomoryOptions? options = null,
        ISimplexProtocol? protocol = null);
}
