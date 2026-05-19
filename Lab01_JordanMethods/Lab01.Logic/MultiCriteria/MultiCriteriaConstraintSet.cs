namespace Lab01.Logic.MultiCriteria;

public sealed class MultiCriteriaConstraintSet
{
    public required int VariableCount { get; init; }

    public required IReadOnlyList<(double[] Coefficients, double RightSide)> Equalities { get; init; }

    public required IReadOnlyList<(double[] Coefficients, double RightSide)> Inequalities { get; init; }
}
