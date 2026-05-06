using Lab01.Logic.Exceptions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex.Stages;

/// <summary>
/// Пошук оптимального розв’язку від заданого опорного: на кожному кроці обирає
/// розв’язувальний стовпець за оцінками рядка Z і виконує модифіковане ЖВ.
/// </summary>
public sealed class OptimalSolutionFinder : IOptimalSolution
{
    private const string Stage = "пошук оптимального розв’язку";

    private readonly IJordan _jordan;
    private readonly IPivotSelector _pivotSelector;
    private readonly OptimizationMode _mode;
    private readonly SimplexOptions _options;
    private readonly ISimplexProtocol? _protocol;
    private readonly bool _logPivotStepNumbers;

    public OptimalSolutionFinder(
        IJordan jordan,
        IPivotSelector pivotSelector,
        OptimizationMode mode,
        SimplexOptions? options = null,
        ISimplexProtocol? protocol = null,
        bool logPivotStepNumbers = true)
    {
        _jordan = jordan;
        _pivotSelector = pivotSelector;
        _mode = mode;
        _options = options ?? SimplexOptions.Default;
        _protocol = protocol;
        _logPivotStepNumbers = logPivotStepNumbers;
    }

    public void Find(SimplexTableau tableau)
    {
        int step = 1;
        _protocol?.LogSection("Пошук оптимального розв’язку:");

        while (step <= _options.MaxIterations)
        {
            int col = FindPivotColumn(tableau);
            if (col == -1) return;

            int row = _pivotSelector.FindOptimalPivotRow(tableau, col);
            if (row == -1) throw new UnboundedProblemException(_mode);

            _protocol?.LogPivot(_logPivotStepNumbers ? step : null, tableau, row, col);
            tableau.SetBasisColumn(row, col);
            var nextData = _jordan.ModifiedJordanMethod(tableau.Data, row, col);
            tableau.Update(nextData);
            _protocol?.LogTableau(tableau);

            step++;
        }

        throw new IterationLimitExceededException(_options.MaxIterations, Stage);
    }

    private int FindPivotColumn(SimplexTableau tableau)
    {
        int bestCol = -1;
        double bestValue = 0;

        for (int j = 0; j < tableau.ColsCount; j++)
        {
            double zValue = tableau.GetZ(j);

            if (_mode == OptimizationMode.Maximization)
            {
                if (zValue < bestValue)
                {
                    bestValue = zValue;
                    bestCol = j;
                }
            }
            else
            {
                if (zValue > bestValue)
                {
                    bestValue = zValue;
                    bestCol = j;
                }
            }
        }

        return bestCol;
    }
}
