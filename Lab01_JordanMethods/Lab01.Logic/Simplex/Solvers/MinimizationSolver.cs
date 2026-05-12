using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Simplex.Solvers;

public sealed class MinimizationSolver : LinearSolverBase
{
    public MinimizationSolver(
        IBasicSolutionFinder basicFinder,
        IOptimalSolution optimalFinder,
        ISimplexProtocol? protocol = null)
        : base(basicFinder, optimalFinder, OptimizationMode.Minimization, protocol)
    {
    }

    protected override double[] PrepareObjective(double[] vectorZ)
    {
        var inverted = new double[vectorZ.Length];
        for (int i = 0; i < vectorZ.Length; i++)
            inverted[i] = -vectorZ[i];
        return inverted;
    }
}
