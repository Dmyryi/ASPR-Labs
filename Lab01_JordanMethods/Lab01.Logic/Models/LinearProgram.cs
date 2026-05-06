namespace Lab01.Logic.Models;

/// <summary>
/// Канонізована задача лінійного програмування: усі обмеження приведені до вигляду
/// A·x ≤ b (кожне ≥ помножене на -1, рівність розкладена на дві нерівності).
/// </summary>
public sealed class LinearProgram
{
    public required double[] ObjectiveCoefficients { get; init; }

    public required double[,] ConstraintMatrix { get; init; }

    public required double[] RightHandSide { get; init; }

    public int VariableCount => ObjectiveCoefficients.Length;

    public int ConstraintCount => RightHandSide.Length;
}
