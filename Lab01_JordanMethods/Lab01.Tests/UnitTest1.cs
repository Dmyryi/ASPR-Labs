using Lab01.Logic;
using Lab01.Logic.Interfaces;
using Xunit.Abstractions;

namespace Lab01.Tests;

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

    [Fact]
    public void GenerateProtocol_AndSave()
    {
        var logger = new CalculationLogger();
        var jordan = new JordanSolver();
        var inverter = new MatrixInverter(jordan, logger);
        var solver = new InverseSolveStrategy(inverter, logger);

        double[,] A = { { 6, 2, 5 }, { -3, 4, -1 }, { 1, 4, 3 } };
        double[] B = { 1, 6, 6 };

        solver.Solve(A, B);
        logger.Save("protocol.txt");

        _output.WriteLine("Protocol saved to protocol.txt");
    }
}
