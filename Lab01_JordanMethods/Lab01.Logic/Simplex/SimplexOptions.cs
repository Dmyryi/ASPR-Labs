namespace Lab01.Logic.Simplex;

public sealed class SimplexOptions
{
    public int MaxIterations { get; init; } = 500;

    public double Epsilon { get; init; } = 1e-9;

    public bool UseZeroRowElimination { get; init; } = true;

    public static SimplexOptions Default { get; } = new();
}
