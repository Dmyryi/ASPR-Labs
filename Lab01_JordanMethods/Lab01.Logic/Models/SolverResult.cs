namespace Lab01.Logic.Models;

public sealed class SolverResult
{
    public required double[] X { get; init; }

    public double[] Y { get; init; } = Array.Empty<double>();

    public double[] U { get; init; } = Array.Empty<double>();

    public double Z { get; init; }

    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;
}
