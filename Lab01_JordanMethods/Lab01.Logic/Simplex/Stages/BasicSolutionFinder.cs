using Lab01.Logic.Exceptions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex.Stages;

/// <summary>
/// Пошук опорного розв’язку: ітеративно усуває від’ємні b_i модифікованим
/// жордановим виключенням, поки всі вільні члени не стануть невід’ємними.
/// </summary>
public sealed class BasicSolutionFinder : IBasicSolutionFinder
{
    private const string Stage = "пошук опорного розв’язку";

    private readonly IJordan _jordan;
    private readonly IPivotSelector _pivotSelector;
    private readonly ISimplexProtocol? _protocol;
    private readonly SimplexOptions _options;
    private readonly bool _logInitialTableau;
    private readonly bool _logPivotStepNumbers;

    public BasicSolutionFinder(
        IJordan jordan,
        IPivotSelector pivotSelector,
        SimplexOptions? options = null,
        ISimplexProtocol? protocol = null,
        bool logInitialTableau = true,
        bool logPivotStepNumbers = true)
    {
        _jordan = jordan;
        _pivotSelector = pivotSelector;
        _options = options ?? SimplexOptions.Default;
        _protocol = protocol;
        _logInitialTableau = logInitialTableau;
        _logPivotStepNumbers = logPivotStepNumbers;
    }

    public void Find(SimplexTableau tableau)
    {
        int step = 1;
        var seenStates = new HashSet<string>(StringComparer.Ordinal);

        if (_logInitialTableau)
            _protocol?.LogInitialTableau(tableau);
        _protocol?.LogSection("Пошук опорного розв’язку:");

        while (step <= _options.MaxIterations)
        {
            int row = FindNegativeB(tableau);
            if (row == -1) return;

            int col = FindNegativeInRow(tableau, row);
            if (col == -1) throw new InfeasibleProblemException();

            int pivotRow = _pivotSelector.FindBasicPivotRow(tableau, col, row);

            string state = BuildStateKey(tableau, row, col, pivotRow);
            if (!seenStates.Add(state)) throw new CyclingDetectedException();

            _protocol?.LogPivot(_logPivotStepNumbers ? step : null, tableau, pivotRow, col);
            tableau.SetBasisColumn(pivotRow, col);
            var nextData = _jordan.ModifiedJordanMethod(tableau.Data, pivotRow, col);
            tableau.Update(nextData);
            _protocol?.LogTableau(tableau);

            step++;
        }

        throw new IterationLimitExceededException(_options.MaxIterations, Stage);
    }

    private int FindNegativeB(SimplexTableau tableau)
    {
        double threshold = -_options.Epsilon;
        for (int i = 0; i < tableau.RowsCount; i++)
        {
            if (tableau.GetB(i) < threshold) return i;
        }
        return -1;
    }

    private int FindNegativeInRow(SimplexTableau tableau, int pivotRow)
    {
        double threshold = -_options.Epsilon;
        for (int j = 0; j < tableau.ColsCount; j++)
        {
            if (tableau.GetValue(pivotRow, j) < threshold) return j;
        }
        return -1;
    }

    private static string BuildStateKey(SimplexTableau tableau, int negativeRow, int pivotCol, int pivotRow)
    {
        return string.Join(
            "|",
            negativeRow.ToString(System.Globalization.CultureInfo.InvariantCulture),
            pivotCol.ToString(System.Globalization.CultureInfo.InvariantCulture),
            pivotRow.ToString(System.Globalization.CultureInfo.InvariantCulture),
            string.Join(",", tableau.BasisVariables));
    }
}
