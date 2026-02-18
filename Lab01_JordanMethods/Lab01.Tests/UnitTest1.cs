using Lab01.Logic;
using Lab01.Logic.Interfaces;
using Xunit.Abstractions;

namespace Lab01.Tests;

/// <summary>
/// Demo tests that print intermediate results (Jordan steps, matrix output, etc.).
/// Main data-driven tests are in InverseMatrixTests, RankCalculatorTests, LinearSystemSolverTests.
/// </summary>
public class UnitTest1
{
    private readonly ITestOutputHelper _output;
    private readonly IJordan _jordan;
    private readonly IMatrixInverter _matrix;
    private readonly IRankCalculator _rankCalculator;
    private readonly ILinearSystemSolver _linearSystemSolver;

    private static readonly double[,] DemoMatrix = { { 5, -3, 7 }, { -1, 4, 3 }, { 6, -2, 5 } };

    public UnitTest1(ITestOutputHelper output)
    {
        _output = output;
        _jordan = new JordanSolver();
        _matrix = new MatrixInverter(_jordan);
        _rankCalculator = new RankCalculator(_jordan);
        _linearSystemSolver = new InverseSolveStrategy(_matrix);
    }

    [Fact]
    public void TestJordanSteps()
    {
        var result = _jordan.JordanMethod(DemoMatrix, 1, 1);
        foreach (var step in result)
            _output.WriteLine(step.ToString());
    }

    [Fact]
    public void TestInvertMatrix_Output()
    {
        var result = _matrix.Invert(DemoMatrix);
         foreach (var step in result){
            _output.WriteLine(step.ToString("F2"));
         }
    }

    [Fact]
    public void TestRankCalculator_Output()
    {
        var matrix = new double[,] { { 1, 2, 3, 4 }, { 2, 4, 6, 8 } };
        var result = _rankCalculator.Calculate(matrix);
        _output.WriteLine(result.ToString());
    }

    [Fact]
    public void TestLinearSystemSolver_Output()
    {
        var B = new double[] { 13, 13, 12 };
        var result = _linearSystemSolver.Solve(DemoMatrix, B);
        foreach (var x in result)
            _output.WriteLine(x.ToString("F2"));
    }
}
