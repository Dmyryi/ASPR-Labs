namespace Lab01.Logic.Transportation;

public sealed class TransportationSolveResult
{
    public required TransportationProblem Problem { get; init; }
    public required bool WasOpen { get; init; }
    public string? BalanceNote { get; init; }
    public required TransportationPlan NorthwestCornerPlan { get; init; }
    public required TransportationPlan MinimumElementPlan { get; init; }
    public required TransportationPlan OptimalPlan { get; init; }
    public TransportationTrace? Trace { get; init; }
}
