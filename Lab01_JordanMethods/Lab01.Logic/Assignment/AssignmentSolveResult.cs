namespace Lab01.Logic.Assignment;

public sealed class AssignmentSolveResult
{
    public required int Size { get; init; }
    public required double[,] OriginalCosts { get; init; }
    public required double[,] ReducedCosts { get; init; }
    public required int[,] AssignmentMatrix { get; init; }
    public required int[] AssignedColumns { get; init; }
    public required double TotalCost { get; init; }
}
