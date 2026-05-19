namespace Lab01.Logic.Transportation;

public sealed class TransportationTrace
{
    public List<NorthwestAllocationStep> NorthwestSteps { get; } = new();
    public List<PotentialIterationStep> PotentialIterations { get; } = new();
    public string? SimplexProtocolText { get; set; }
    public double[]? SimplexSolution { get; set; }
    public double SimplexMinCost { get; set; }
}

public sealed class NorthwestAllocationStep
{
    public required int Row { get; init; }
    public required int Col { get; init; }
    public required double Amount { get; init; }
}

public sealed class PotentialIterationStep
{
    public required double[] SupplyPotentials { get; init; }
    public required double[] DemandPotentials { get; init; }
    public required double[,] IndirectCosts { get; init; }
    public required bool IsOptimal { get; init; }
    public List<(int Row, int Col)> ProblematicCells { get; init; } = new();
    public int? EnterRow { get; init; }
    public int? EnterCol { get; init; }
    public double MaxDifference { get; init; }
    public List<(int Row, int Col)> Cycle { get; init; } = new();
    public double Theta { get; init; }
    public required double[,] PlanBefore { get; init; }
    public required double[,] PlanAfter { get; init; }
}
