using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Simplex.Solvers;

public sealed class MaximizationSolver : LinearSolverBase
{
    private readonly IZeroRowEliminator? _zeroRowEliminator;
    private readonly bool _useZeroRowElimination;

    public MaximizationSolver(
        IBasicSolutionFinder basicFinder,
        IOptimalSolution optimalFinder,
        ISimplexProtocol? protocol = null,
        IZeroRowEliminator? zeroRowEliminator = null,
        bool useZeroRowElimination = true)
        : base(basicFinder, optimalFinder, OptimizationMode.Maximization, protocol)
    {
        _zeroRowEliminator = zeroRowEliminator;
        _useZeroRowElimination = useZeroRowElimination;
    }

    protected override void BeforeBasicStage(SimplexTableau tableau)
    {
        if (_useZeroRowElimination)
            _zeroRowEliminator?.Eliminate(tableau);
    }
}
