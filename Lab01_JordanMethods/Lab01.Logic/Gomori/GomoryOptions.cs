namespace Lab01.Logic.Gomori;

/// <summary>
/// Налаштування методу Гоморі: ліміт відсічень і поріг для визнання значення цілим.
/// </summary>
public sealed class GomoryOptions
{
    public int MaxCuts { get; init; } = 30;

    public double IntegerEpsilon { get; init; } = 1e-7;

    public static GomoryOptions Default { get; } = new();
}
