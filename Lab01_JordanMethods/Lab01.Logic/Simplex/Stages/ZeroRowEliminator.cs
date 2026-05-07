using Lab01.Logic.Exceptions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex.Stages;

public sealed class ZeroRowEliminator : IZeroRowEliminator
{
    private readonly IJordan _jordan;
    private readonly SimplexOptions _options;
    private readonly ISimplexProtocol? _protocol;

    public ZeroRowEliminator(
        IJordan jordan,
        SimplexOptions? options = null,
        ISimplexProtocol? protocol = null)
    {
        _jordan = jordan;
        _options = options ?? SimplexOptions.Default;
        _protocol = protocol;
    }

    public void Eliminate(SimplexTableau tableau)
    {
        _protocol?.LogSection("Усунення нуль-рядків (алгоритм рис. 3.2):");
        int step = 1;

        while (true)
        {
            int zeroRow = FindZeroRow(tableau);
            if (zeroRow == -1)
            {
                _protocol?.LogSection("Усі нуль-рядки видалено.");
                return;
            }

            int pivotCol = FindPositiveInRow(tableau, zeroRow);
            if (pivotCol == -1) throw new InfeasibleProblemException();

            _protocol?.LogPivot(step, tableau, zeroRow, pivotCol);

            tableau.SetBasisColumn(zeroRow, pivotCol);
            var nextData = _jordan.ModifiedJordanMethod(tableau.Data, zeroRow, pivotCol);
            tableau.Update(nextData);
            _protocol?.LogTableau(tableau);

            _protocol?.LogSection($"Викреслення нуль-стовпця: j = {pivotCol}");
            tableau.RemoveConstraintColumn(pivotCol);
            _protocol?.LogTableau(tableau);

            step++;
        }
    }

    private int FindZeroRow(SimplexTableau tableau)
    {
        for (int i = 0; i < tableau.RowsCount; i++)
        {
            if (Math.Abs(tableau.GetB(i)) < _options.Epsilon) return i;
        }
        return -1;
    }

    private int FindPositiveInRow(SimplexTableau tableau, int row)
    {
        for (int j = 0; j < tableau.ColsCount; j++)
        {
            if (tableau.GetValue(row, j) > _options.Epsilon) return j;
        }
        return -1;
    }
}
