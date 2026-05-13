namespace Lab01.App.ViewModels;

public sealed class MatrixGameProtocolRowDisplay
{
    public int Round { get; init; }

    public string RandomRowPlayer { get; init; } = string.Empty;

    public string StrategyRowPlayer { get; init; } = string.Empty;

    public string RandomColumnPlayer { get; init; } = string.Empty;

    public string StrategyColumnPlayer { get; init; } = string.Empty;

    public string PayoffRowPlayer { get; init; } = string.Empty;

    public string AccumulatedPayoffRowPlayer { get; init; } = string.Empty;

    public string AveragePayoffRowPlayer { get; init; } = string.Empty;
}
