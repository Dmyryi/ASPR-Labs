namespace Lab01.Logic.Assignment;

public sealed class AssignmentTrace
{
    public List<RowReductionStep> RowReductions { get; } = new();
    public List<ColumnReductionStep> ColumnReductions { get; } = new();
    public List<CoverIterationStep> CoverIterations { get; } = new();
    public List<AssignmentFillStep> AssignmentSteps { get; } = new();
    public string? SimplexProtocolText { get; set; }
    public double[]? SimplexSolution { get; set; }
    public double SimplexMinCost { get; set; }
}

public sealed class RowReductionStep
{
    public required int Row { get; init; }
    public required double Minimum { get; init; }
}

public sealed class ColumnReductionStep
{
    public required int Column { get; init; }
    public required double Minimum { get; init; }
}

public sealed class CoverIterationStep
{
    public required double[,] MatrixBefore { get; init; }
    public required bool[] CoverRows { get; init; }
    public required bool[] CoverCols { get; init; }
    public required int LineCount { get; init; }
    public required int MatchingCount { get; init; }
    public required bool IsOptimal { get; init; }
    public double AdjustmentMin { get; init; }
    public required double[,] MatrixAfter { get; init; }
}

public sealed class AssignmentFillStep
{
    public required string Description { get; init; }
    public required int Row { get; init; }
    public required int Column { get; init; }
    public required int AssignmentIndex { get; init; }
}
