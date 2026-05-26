using Lab01.Logic;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Parsing;
using Lab01.Logic.Simplex.Stages;
using Xunit;
using Xunit.Abstractions;

namespace Lab01.Tests;

public class DualVariant10Tests
{
    private const string PrimalObjective = "x1 + 2x2 + x3";
    private const string PrimalConstraints =
        "2x1 - x2 + 3x3 + 4x4 <= 10\n" +
        "x1 + x2 + x3 - x4 <= 5\n" +
        "x1 + 2x2 + 2x3 + 4x4 <= 12";

    private const string DualObjective = "10x1 + 5x2 + 12x3";
    private const string DualConstraints =
        "2x1 + x2 + x3 >= 1\n" +
        "-x1 + x2 + 2x3 >= 2\n" +
        "3x1 + x2 + 2x3 >= 1\n" +
        "4x1 - x2 + 4x3 >= 0";

    private readonly ITestOutputHelper _output;

    public DualVariant10Tests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Primal_Max_Variant10()
    {
        var result = Solve(PrimalObjective, PrimalConstraints, OptimizationMode.Maximization, useZeroRowElim: true);
        _output.WriteLine($"X={Fmt(result.X)} Z={result.Z}");
        Assert.Equal(32.0 / 3.0, result.Z, 2);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Dual_Min_Variant10(bool useZeroRowElim)
    {
        var result = Solve(DualObjective, DualConstraints, OptimizationMode.Minimization, useZeroRowElim);
        _output.WriteLine($"zeroRow={useZeroRowElim} X={Fmt(result.X)} W={result.Z}");
        AssertFeasibleDual(result.X);
        Assert.Equal(32.0 / 3.0, result.Z, 2);
    }

    [Fact]
    public void SmallLp_Dual_Min_Variant10()
    {
        var parser = new LinearProgramParser();
        var program = parser.Parse(DualObjective, DualConstraints);
        var result = SmallLpSolver.Solve(
            program.ObjectiveCoefficients.ToArray(),
            program.ConstraintMatrix,
            program.RightHandSide,
            OptimizationMode.Minimization);
        _output.WriteLine($"SmallLp X={Fmt(result.X)} W={result.Z} ok={result.Success}");
        Assert.True(result.Success);
        AssertFeasibleDual(result.X);
        Assert.Equal(32.0 / 3.0, result.Z, 2);
    }

    [Fact]
    public void UserPoint_0_2_0_IsNotFeasibleForDual()
    {
        double[] y = { 0, 2, 0 };
        Assert.False(IsFeasibleDual(y));
    }

    private static SolverResult Solve(
        string objective,
        string constraints,
        OptimizationMode mode,
        bool useZeroRowElim)
    {
        var parser = new LinearProgramParser();
        var program = parser.Parse(objective, constraints);
        var jordan = new JordanSolver();
        var factory = new SimplexSolverFactory(jordan, new PivotSelector());
        var handle = factory.Create(mode, new SimplexOptions { UseZeroRowElimination = useZeroRowElim });
        double[] z = mode == OptimizationMode.Maximization
            ? program.ObjectiveCoefficients.Select(c => -c).ToArray()
            : program.ObjectiveCoefficients.ToArray();
        return handle.Solver.Solve(z, program.ConstraintMatrix, program.RightHandSide);
    }

    private static void AssertFeasibleDual(double[] y)
    {
        Assert.True(IsFeasibleDual(y), $"y={Fmt(y)} порушує обмеження двоїстої");
    }

    private static bool IsFeasibleDual(double[] y)
    {
        return 2 * y[0] + y[1] + y[2] >= 1 - 1e-6
            && -y[0] + y[1] + 2 * y[2] >= 2 - 1e-6
            && 3 * y[0] + y[1] + 2 * y[2] >= 1 - 1e-6
            && 4 * y[0] - y[1] + 4 * y[2] >= -1e-6;
    }

    private static string Fmt(double[] x) =>
        string.Join("; ", x.Select(v => v.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture)));
}
