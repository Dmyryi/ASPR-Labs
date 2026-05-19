namespace Lab01.Logic.Transportation;

internal static class TransportationBasisTracker
{
    private static readonly HashSet<(int i, int j)> Forced = new();

    public static void Clear() => Forced.Clear();

    public static void Add(int i, int j) => Forced.Add((i, j));

    public static bool Contains(int i, int j) => Forced.Contains((i, j));
}
