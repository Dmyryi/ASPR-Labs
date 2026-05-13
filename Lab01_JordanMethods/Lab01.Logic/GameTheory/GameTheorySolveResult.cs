namespace Lab01.Logic.GameTheory;

public sealed class GameTheorySolveResult
{
    public required double[,] PayoffMatrix { get; init; }

    public bool HasSaddlePoint { get; init; }

    public int? SaddleRow { get; init; }

    public int? SaddleColumn { get; init; }

    public double GameValue { get; init; }

    public required double[] RowPlayerStrategy { get; init; }

    public required double[] ColumnPlayerStrategy { get; init; }

    public required string SolutionKind { get; init; }

    public double? LpShift { get; init; }

    public double? LpObjectiveMaxSumX { get; init; }

    /// <summary> Чи виконувалось ітеративне вилучення строго домінованих стратегій перед розв’язком (рис. 6.1). </summary>
    public bool EliminatedDominatedStrategies { get; init; }

    /// <summary> Журнал вилучень (для звіту за методичкою). </summary>
    public string? DominanceReductionLog { get; init; }
}
