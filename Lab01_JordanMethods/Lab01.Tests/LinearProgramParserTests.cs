using Lab01.Logic.Simplex.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace Lab01.Tests;

public class LinearProgramParserTests
{
    private readonly ITestOutputHelper _output;
    private readonly LinearProgramParser _parser = new();

    public LinearProgramParserTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Parse_ObjectiveAndConstraints_ReturnsCanonicalProgram()
    {
        const string objective = "10x1 - x2 - 42x3 - 52x4";
        const string constraints =
            "-2x1 + x2 + x3 + 3x4 = 2\n" +
            "-3x1 + 2x2 - 3x3 = 7\n" +
            "-3x1 + x2 + 4x3 + x4 <= 1\n" +
            "-3x1 + 2x2 - 2x3 + 2x4 >= 9";

        var program = _parser.Parse(objective, constraints);

        Assert.Equal(4, program.VariableCount);
        Assert.Equal(6, program.ConstraintCount);

        Assert.Equal(10, program.ObjectiveCoefficients[0]);
        Assert.Equal(-1, program.ObjectiveCoefficients[1]);
        Assert.Equal(-42, program.ObjectiveCoefficients[2]);
        Assert.Equal(-52, program.ObjectiveCoefficients[3]);
    }

    [Fact]
    public void Parse_GreaterOrEqualConstraint_IsInvertedToLessOrEqual()
    {
        const string objective = "x1";
        const string constraints = "2x1 + x2 >= 4";

        var program = _parser.Parse(objective, constraints);

        Assert.Equal(-2, program.ConstraintMatrix[0, 0]);
        Assert.Equal(-1, program.ConstraintMatrix[0, 1]);
        Assert.Equal(-4, program.RightHandSide[0]);
    }

    [Fact]
    public void Parse_EqualityConstraint_IsExpandedIntoTwoInequalities()
    {
        const string objective = "x1";
        const string constraints = "x1 + x2 = 5";

        var program = _parser.Parse(objective, constraints);

        Assert.Equal(2, program.ConstraintCount);
    }

    [Fact]
    public void Parse_EmptyConstraints_Throws()
    {
        Assert.Throws<FormatException>(() => _parser.Parse("x1", string.Empty));
    }
}
