namespace Lab01.Logic.Models;


public sealed class LinearProgram
{
    public required double[] ObjectiveCoefficients { get; init; }

    public required double[,] ConstraintMatrix { get; init; }

    public required double[] RightHandSide { get; init; }

    public int VariableCount => ObjectiveCoefficients.Length;

    public int ConstraintCount => RightHandSide.Length;
}
