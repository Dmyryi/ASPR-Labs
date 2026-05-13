using System.Collections.ObjectModel;
using System.Globalization;
using Lab01.Logic.GameTheory;

namespace Lab01.App.ViewModels;

public sealed class MatrixGameSimulationViewModel
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public MatrixGameSimulationViewModel(
        ObservableCollection<MatrixGameProtocolRowDisplay> rows,
        string theoreticalRowStrategies,
        string theoreticalColumnStrategies,
        string theoreticalGameValue,
        string empiricalRowFrequencies,
        string empiricalColumnFrequencies,
        string averagePayoffAfterSimulation,
        int roundsTotal,
        string averagePayoffCaption)
    {
        Rows = rows;
        TheoreticalRowStrategies = theoreticalRowStrategies;
        TheoreticalColumnStrategies = theoreticalColumnStrategies;
        TheoreticalGameValue = theoreticalGameValue;
        EmpiricalRowFrequencies = empiricalRowFrequencies;
        EmpiricalColumnFrequencies = empiricalColumnFrequencies;
        AveragePayoffAfterSimulation = averagePayoffAfterSimulation;
        RoundsTotal = roundsTotal;
        AveragePayoffCaption = averagePayoffCaption;
    }

    public ObservableCollection<MatrixGameProtocolRowDisplay> Rows { get; }

    public string TheoreticalRowStrategies { get; }

    public string TheoreticalColumnStrategies { get; }

    public string TheoreticalGameValue { get; }

    public string EmpiricalRowFrequencies { get; }

    public string EmpiricalColumnFrequencies { get; }

    public string AveragePayoffAfterSimulation { get; }

    public int RoundsTotal { get; }

    public string AveragePayoffCaption { get; }

    public static MatrixGameSimulationViewModel Create(
        MatrixGameSimulationResult sim,
        GameTheorySolveResult solve,
        int roundsTotal)
    {
        var rows = new ObservableCollection<MatrixGameProtocolRowDisplay>();

        foreach (MatrixGameProtocolRow r in sim.Protocol)
        {
            rows.Add(new MatrixGameProtocolRowDisplay
            {
                Round = r.Round,
                RandomRowPlayer = r.RandomRowPlayer.ToString("N3", Uk),
                StrategyRowPlayer = "X" + (r.RowStrategyIndex + 1),
                RandomColumnPlayer = r.RandomColumnPlayer.ToString("N3", Uk),
                StrategyColumnPlayer = "Y" + (r.ColumnStrategyIndex + 1),
                PayoffRowPlayer = r.PayoffRowPlayer.ToString("N2", Uk),
                AccumulatedPayoffRowPlayer = r.AccumulatedPayoffRowPlayer.ToString("N2", Uk),
                AveragePayoffRowPlayer = r.AveragePayoffRowPlayer.ToString("N4", Uk)
            });
        }

        string theoryRow = FormatProbs(solve.RowPlayerStrategy);
        string theoryCol = FormatProbs(solve.ColumnPlayerStrategy);
        string empRow = FormatProbs(sim.EmpiricalRowFrequencies);
        string empCol = FormatProbs(sim.EmpiricalColumnFrequencies);
        string caption = $"Середній виграш А після {roundsTotal} партій";

        return new MatrixGameSimulationViewModel(
            rows,
            theoryRow,
            theoryCol,
            solve.GameValue.ToString("N2", Uk),
            empRow,
            empCol,
            sim.AveragePayoff.ToString("N4", Uk),
            roundsTotal,
            caption);
    }

    private static string FormatProbs(IReadOnlyList<double> v)
    {
        var parts = new string[v.Count];
        for (int i = 0; i < v.Count; i++)
            parts[i] = v[i].ToString("N2", Uk);
        return string.Join("; ", parts);
    }
}
