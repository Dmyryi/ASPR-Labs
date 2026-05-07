using Lab01.Logic.Interfaces;
using Lab01.Logic.Simplex.Protocols;
using Lab01.Logic.Simplex.Solvers;
using Lab01.Logic.Simplex.Stages;

namespace Lab01.Logic.Simplex;

public sealed class SimplexSolverFactory : ISimplexSolverFactory
{
    private readonly IJordan _jordan;
    private readonly IPivotSelector _pivotSelector;

    public SimplexSolverFactory(IJordan jordan, IPivotSelector pivotSelector)
    {
        _jordan = jordan;
        _pivotSelector = pivotSelector;
    }

    public SimplexSolverHandle Create(OptimizationMode mode, SimplexOptions options)
    {
        var protocol = new SimplexProtocol();
        var basicFinder = new BasicSolutionFinder(_jordan, _pivotSelector, options, protocol);
        var optimalFinder = new OptimalSolutionFinder(_jordan, _pivotSelector, mode, options, protocol);

        ILinearSolver solver = mode == OptimizationMode.Maximization
            ? new MaximizationSolver(
                basicFinder,
                optimalFinder,
                protocol,
                new ZeroRowEliminator(_jordan, options, protocol),
                options.UseZeroRowElimination)
            : new MinimizationSolver(basicFinder, optimalFinder, protocol);

        return new SimplexSolverHandle(solver, protocol);
    }
}
