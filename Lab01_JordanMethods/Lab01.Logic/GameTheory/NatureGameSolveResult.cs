namespace Lab01.Logic.GameTheory;

public sealed class NatureGameSolveResult
{
    public required IReadOnlyList<int> WaldRows { get; init; }
    public required IReadOnlyList<int> MaximaxRows { get; init; }
    public required IReadOnlyList<int> HurwiczRows { get; init; }
    public required IReadOnlyList<int> SavageRows { get; init; }
    public required IReadOnlyList<int> BayesRows { get; init; }
    public required IReadOnlyList<int> LaplaceRows { get; init; }

    public required double[,] SavageRegretMatrix { get; init; }

    public required IReadOnlyList<int> MostFrequentRows { get; init; }
}
