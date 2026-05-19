namespace Lab01.Logic.NetworkPlanning;

public sealed class NetworkTask
{
    public required int Id { get; init; }
    public required IReadOnlyList<int> Predecessors { get; init; }
    public required int Duration { get; init; }
    public required int People { get; init; }

    public int EarlyStart { get; set; }
    public int EarlyFinish { get; set; }
    public int LateStart { get; set; }
    public int LateFinish { get; set; }
    public int Reserve { get; set; }
    public bool IsCritical { get; set; }
    public IReadOnlyList<int> Successors { get; set; } = Array.Empty<int>();
}
