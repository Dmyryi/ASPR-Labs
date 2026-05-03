using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex
{
    public class BasicSolutionFinder : IBasicSolutionFinder
    {
        private readonly IJordan _jordan;
        private readonly ISimplexProtocol? _protocol;

        public BasicSolutionFinder(IJordan jordan, ISimplexProtocol? protocol = null)
        {
            _jordan = jordan;
            _protocol = protocol;
        }

        public void Find(SimplexTableau tableau)
        {
       
            int step = 1;
            const int maxSteps = 500;
            var seenStates = new HashSet<string>(StringComparer.Ordinal);

            _protocol?.LogInitialTableau(tableau);
            _protocol?.LogSection("Пошук опорного розв’язку:");

            while (step <= maxSteps)
            {
                

                int row = FindNegativeB(tableau);
                if (row == -1)
                {
                    
                    break;
                }

                

                int col = FindNegativeInRow(tableau, row);
                if (col == -1)
                {
                    throw new Exception("Infeasible");
                }

                int actualPivotRow = FindPivotRow(tableau, col, row);

                string state = BuildStateKey(tableau, row, col, actualPivotRow);
                if (!seenStates.Add(state))
                {
                    throw new InvalidOperationException("Зациклення під час пошуку опорного розв’язку.");
                }

                _protocol?.LogPivot(step, tableau, actualPivotRow, col);
                tableau.SetBasisColumn(actualPivotRow, col);
                var nextData = _jordan.ModifiedJordanMethod(tableau.Data, actualPivotRow, col);
                tableau.Update(nextData);
                _protocol?.LogTableau(tableau);

                step++;
            }

            if (step > maxSteps)
            {
                throw new InvalidOperationException("Перевищено ліміт ітерацій під час пошуку опорного розв’язку.");
            }
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

        private int FindNegativeB(SimplexTableau tableau)
        {
            for (int i = 0; i < tableau.RowsCount; i++)
            {
                if (tableau.GetB(i) < -1e-9) return i;
            }
            return -1;
        }

        private int FindNegativeInRow(SimplexTableau tableau, int pivotRow)
        {
            for (int j = 0; j < tableau.ColsCount; j++)
            {
                if (tableau.GetValue(pivotRow, j) < -1e-9) return j;
            }
            return -1;
        }

        private int FindPivotRow(SimplexTableau tableau, int pivotCol, int initialRow)
        {
            int actualPivot = -1;
            double minRatio = double.MaxValue;

            for (int r = 0; r < tableau.RowsCount; r++)
            {
                double a = tableau.GetValue(r, pivotCol);
                if (a < -1e-9)
                {
                    double ratio = tableau.GetB(r) / a;
                    if (ratio >= 0 && ratio < minRatio)
                    {
                        minRatio = ratio;
                        actualPivot = r;
                    }
                }
            }

            return actualPivot == -1 ? initialRow : actualPivot;
        }

      
    }
}