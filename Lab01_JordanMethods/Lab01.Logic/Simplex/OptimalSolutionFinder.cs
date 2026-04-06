using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab01.Logic.Interfaces;


namespace Lab01.Logic.Simplex
{
    public class OptimalSolutionFinder:IOptimalSolution
    {
        private readonly IJordan _jordan;
        private OptimizationMode _mode;

        public OptimalSolutionFinder(IJordan jordan, OptimizationMode mode = OptimizationMode.Maximization)
        {
            _jordan = jordan;
            _mode = mode;
        }

        public void Find(SimplexTableau tableau)
        {
            while (true)
            {
                int col = FindPivotColumn(tableau);

                if (col == -1) break;

                int row = FindPivotRow(tableau, col);

                if (row == -1)
                {
                    string errorMsg = _mode == OptimizationMode.Maximization
                        ? "Функція не обмежена зверху"
                        : "Функція не обмежена знизу";
                    throw new Exception(errorMsg);
                }

                var nextData = _jordan.ModifiedJordanMethod(tableau.Data, row, col);
                tableau.Update(nextData);

            }
        }

       

        private int FindPivotColumn(SimplexTableau tableau)
        {
            for (int j = 0; j < tableau.ColsCount; j++)
            {
                double zValue = tableau.GetZ(j);

                if (_mode == OptimizationMode.Maximization)
                {
                    if (zValue < 0) return j;
                }
                else
                {
                    if (zValue > 0) return j;
                }
            }
            return -1;
        }
        private int FindPivotRow(SimplexTableau tableau, int pivotCol)
        {
            int pivotRow = -1;
            double minRatio = double.MaxValue;

            for (int i = 0; i < tableau.RowsCount; i++)
            {
                double val = tableau.GetValue(i, pivotCol);

                if (val > 0)
                {
                    double ratio = tableau.GetB(i) / val;
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }
            return pivotRow;
        }
    }
}
