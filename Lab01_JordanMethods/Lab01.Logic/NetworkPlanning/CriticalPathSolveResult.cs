namespace Lab01.Logic.NetworkPlanning;

public sealed class CriticalPathSolveResult
{
    public required IReadOnlyList<NetworkTask> Tasks { get; init; }
    public required int ProjectDuration { get; init; }
    public required IReadOnlyList<int> CriticalPath { get; init; }
}
