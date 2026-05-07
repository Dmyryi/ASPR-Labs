using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex;

public sealed class SimplexContext
{
    private ILinearSolver? _solver;

    public SimplexContext() { }

    public SimplexContext(ILinearSolver solver) => _solver = solver;

    public void SetStrategy(ILinearSolver solver) => _solver = solver;

    public SolverResult ExecuteStrategy(double[] z, double[,] a, double[] b)
    {
        if (_solver is null)
            throw new InvalidOperationException("Стратегію солвера не встановлено.");

        return _solver.Solve(z, a, b);
    }
}
