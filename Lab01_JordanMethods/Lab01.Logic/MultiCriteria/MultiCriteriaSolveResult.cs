using Lab01.Logic.Models;

namespace Lab01.Logic.MultiCriteria;

public sealed class MultiCriteriaSolveResult
{
    public required IReadOnlyList<MultiCriteriaObjective> Objectives { get; init; }

    public required string ConstraintsText { get; init; }

    public required IReadOnlyList<SolverResult> PerObjectiveSolutions { get; init; }

    public required double[,] ObjectiveValues { get; init; }

    public required double[,] SuboptimalityMatrix { get; init; }

    public required double[,] GameMatrix { get; init; }

    public required double[] Weights { get; init; }

    public required double[] CompromiseSolution { get; init; }

    public required GameTheory.GameTheorySolveResult GameResult { get; init; }
}
