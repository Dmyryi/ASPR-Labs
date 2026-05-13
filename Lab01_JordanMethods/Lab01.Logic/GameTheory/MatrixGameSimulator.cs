namespace Lab01.Logic.GameTheory;

public sealed class MatrixGameProtocolRow
{
    public int Round { get; init; }

    public double RandomRowPlayer { get; init; }

    public int RowStrategyIndex { get; init; }

    public double RandomColumnPlayer { get; init; }

    public int ColumnStrategyIndex { get; init; }

    public double PayoffRowPlayer { get; init; }

    public double AccumulatedPayoffRowPlayer { get; init; }

    public double AveragePayoffRowPlayer { get; init; }
}

public sealed class MatrixGameSimulationResult
{
    public required double TheoreticalGameValue { get; init; }

    public double AveragePayoff { get; init; }

    public required double[] EmpiricalRowFrequencies { get; init; }

    public required double[] EmpiricalColumnFrequencies { get; init; }

    public required IReadOnlyList<MatrixGameProtocolRow> Protocol { get; init; }
}

public static class MatrixGameSimulator
{
    public static MatrixGameSimulationResult Simulate(
        double[,] payoff,
        IReadOnlyList<double> rowProbabilities,
        IReadOnlyList<double> columnProbabilities,
        int rounds,
        double theoreticalValue,
        int? seed = null,
        int maxProtocolRows = 200)
    {
        int m = payoff.GetLength(0);
        int n = payoff.GetLength(1);
        if (rowProbabilities.Count != m || columnProbabilities.Count != n)
            throw new ArgumentException("Розмірності ймовірностей не збігаються з матрицею.");

        var rnd = seed.HasValue ? new Random(seed.Value) : new Random();
        var rowCum = BuildCumulative(rowProbabilities);
        var colCum = BuildCumulative(columnProbabilities);

        var rowCounts = new int[m];
        var colCounts = new int[n];
        var protocol = new List<MatrixGameProtocolRow>();
        double acc = 0;

        for (int t = 1; t <= rounds; t++)
        {
            double uR = rnd.NextDouble();
            double uC = rnd.NextDouble();
            int ir = PickIndex(rowCum, uR);
            int jc = PickIndex(colCum, uC);
            double pay = payoff[ir, jc];
            acc += pay;
            rowCounts[ir]++;
            colCounts[jc]++;

            if (protocol.Count < maxProtocolRows)
            {
                protocol.Add(new MatrixGameProtocolRow
                {
                    Round = t,
                    RandomRowPlayer = uR,
                    RowStrategyIndex = ir,
                    RandomColumnPlayer = uC,
                    ColumnStrategyIndex = jc,
                    PayoffRowPlayer = pay,
                    AccumulatedPayoffRowPlayer = acc,
                    AveragePayoffRowPlayer = acc / t
                });
            }
        }

        var rowFreq = new double[m];
        var colFreq = new double[n];
        for (int i = 0; i < m; i++)
            rowFreq[i] = rowCounts[i] / (double)rounds;
        for (int j = 0; j < n; j++)
            colFreq[j] = colCounts[j] / (double)rounds;

        return new MatrixGameSimulationResult
        {
            TheoreticalGameValue = theoreticalValue,
            AveragePayoff = acc / rounds,
            EmpiricalRowFrequencies = rowFreq,
            EmpiricalColumnFrequencies = colFreq,
            Protocol = protocol
        };
    }

    private static double[] BuildCumulative(IReadOnlyList<double> p)
    {
        var c = new double[p.Count];
        double s = 0;
        for (int i = 0; i < p.Count; i++)
        {
            s += Math.Max(0, p[i]);
            c[i] = s;
        }

        if (s <= 0)
            throw new ArgumentException("Ймовірності гравця дорівнюють нулю.");

        for (int i = 0; i < c.Length; i++)
            c[i] /= s;

        return c;
    }

    private static int PickIndex(double[] cumulative, double u)
    {
        for (int i = 0; i < cumulative.Length; i++)
        {
            if (u < cumulative[i])
                return i;
        }

        return cumulative.Length - 1;
    }
}
