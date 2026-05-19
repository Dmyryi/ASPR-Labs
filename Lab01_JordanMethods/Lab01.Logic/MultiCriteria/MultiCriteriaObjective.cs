using Lab01.Logic.Simplex;

namespace Lab01.Logic.MultiCriteria;

public sealed class MultiCriteriaObjective
{
    public required double[] Coefficients { get; init; }

    public required OptimizationMode Mode { get; init; }

    public required string SourceText { get; init; }
}
