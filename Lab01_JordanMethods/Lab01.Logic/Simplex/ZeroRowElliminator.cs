using System;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex
{
  
    public sealed class ZeroRowElliminator : IZeroRowElliminator
    {
        private const double Eps = 1e-10;

        private readonly IJordan _jordan;
        private readonly ISimplexProtocol? _protocol;

        public ZeroRowElliminator(IJordan jordan, IFindPivot _ = null!, ISimplexProtocol? protocol = null)
        {
            _jordan = jordan;
            _protocol = protocol;
        }

        public void Elliminate(SimplexTableau tableau)
        {
            _protocol?.LogSection("Усунення нуль-рядків:");
            int step = 1;

            while (true)
            {
                int zeroRow = FindZeroRow(tableau);
                if (zeroRow == -1)
                {
                    _protocol?.LogSection("Всі 0-рядки видалено.");
                    return;
                }

                int pivotCol = FindPositiveInRow(tableau, zeroRow);
                if (pivotCol == -1)
                    throw new InvalidOperationException("Система обмежень є суперечливою");

                _protocol?.LogPivot(step, tableau, zeroRow, pivotCol);

                tableau.SetBasisColumn(zeroRow, pivotCol);
                var nextData = _jordan.ModifiedJordanMethod(tableau.Data, zeroRow, pivotCol);
                tableau.Update(nextData);
                _protocol?.LogTableau(tableau);

                _protocol?.LogSection($"Викреслення 0-стовпця: j = {pivotCol}");
                tableau.RemoveConstraintColumn(pivotCol);
                _protocol?.LogTableau(tableau);

                step++;
            }
        }

        private static int FindZeroRow(SimplexTableau t)
        {
            for (int i = 0; i < t.RowsCount; i++)
            {
                if (Math.Abs(t.GetB(i)) < Eps) return i;
            }
            return -1;
        }

        private static int FindPositiveInRow(SimplexTableau t, int row)
        {
            for (int j = 0; j < t.ColsCount; j++)
            {
                if (t.GetValue(row, j) > Eps) return j;
            }
            return -1;
        }
    }
}
