namespace Lab01.Logic.Transportation;

public sealed class TransportationProblem
{
    public required double[,] Costs { get; init; }
    public required double[] Supply { get; init; }
    public required double[] Demand { get; init; }
    public bool AddedDummySupply { get; init; }
    public bool AddedDummyDemand { get; init; }
    public int OriginalSupplyCount { get; init; }
    public int OriginalDemandCount { get; init; }

    public int Rows => Supply.Length;
    public int Cols => Demand.Length;
}
