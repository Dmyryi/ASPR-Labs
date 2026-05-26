using Lab01.Logic.Simplex.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace Lab01.Tests;

public class DualVariant10MatrixTests
{
    private readonly ITestOutputHelper _output;

    public DualVariant10MatrixTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void PrintParsedDualMatrix()
    {
        const string objective = "10x1 + 5x2 + 12x3";
        const string constraints =
            "2x1 + x2 + x3 >= 1\n" +
            "-x1 + x2 + 2x3 >= 2\n" +
            "3x1 + x2 + 2x3 >= 1\n" +
            "4x1 - x2 + 4x3 >= 0";

        var program = new LinearProgramParser().Parse(objective, constraints);
        _output.WriteLine($"rows={program.ConstraintCount} cols={program.VariableCount}");
        for (int i = 0; i < program.ConstraintCount; i++)
        {
            var row = string.Join(" ", Enumerable.Range(0, program.VariableCount)
                .Select(j => program.ConstraintMatrix[i, j].ToString("0.##")));
            _output.WriteLine($"  {row} <= {program.RightHandSide[i]:0.##}");
        }
    }
}
