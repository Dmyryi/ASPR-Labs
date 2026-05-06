using Lab01.Logic.Simplex;

namespace Lab01.Logic.Interfaces;

public interface ISimplexSolverFactory
{
    SimplexSolverHandle Create(OptimizationMode mode, SimplexOptions options);
}

public sealed record SimplexSolverHandle(ILinearSolver Solver, ISimplexProtocol Protocol);
