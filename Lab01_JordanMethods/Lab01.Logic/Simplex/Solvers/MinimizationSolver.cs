using Lab01.Logic.Interfaces;

namespace Lab01.Logic.Simplex.Solvers;

public sealed class MinimizationSolver : LinearSolverBase
{
    public MinimizationSolver(
        IBasicSolutionFinder basicFinder,
        IOptimalSolution optimalFinder,
        ISimplexProtocol? protocol = null)
        : base(basicFinder, optimalFinder, protocol)
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
