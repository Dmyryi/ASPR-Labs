namespace Lab01.Logic.Transportation;

public sealed class TransportationPlan
{
    public required double[,] Allocations { get; init; }
    public required double TotalCost { get; init; }
    public string MethodName { get; init; } = string.Empty;
}
