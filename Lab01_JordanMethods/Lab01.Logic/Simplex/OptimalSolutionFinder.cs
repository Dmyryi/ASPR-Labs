using System;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex
{
    public class OptimalSolutionFinder : IOptimalSolution
    {
        private readonly IJordan _jordan;
        private OptimizationMode _mode;
        private readonly ISimplexProtocol? _protocol;

        public OptimalSolutionFinder(IJordan jordan, OptimizationMode mode = OptimizationMode.Maximization, ISimplexProtocol? protocol = null)
        {
            _jordan = jordan;
            _mode = mode;
            _protocol = protocol;
        }

        public void Find(SimplexTableau tableau)
        {
            int step = 1;

            _protocol?.LogSection("Пошук оптимального розв’язку:");

            while (true)
            {
                int col = FindPivotColumn(tableau);
                if (col == -1)
                {
                    break;
                }

                int row = FindPivotRow(tableau, col);
                if (row == -1)
                {
                    string errorMsg = _mode == OptimizationMode.Maximization
                        ? "Функція не обмежена зверху"
                        : "Функція не обмежена знизу";
                    throw new Exception(errorMsg);
                }

                _protocol?.LogPivot(step, tableau, row, col);
                tableau.SetBasisColumn(row, col);
                var nextData = _jordan.ModifiedJordanMethod(tableau.Data, row, col);
                tableau.Update(nextData);
                _protocol?.LogTableau(tableau);

                step++;
            }
        }

        private int FindPivotColumn(SimplexTableau tableau)
        {
            int bestCol = -1;
            double bestValue = 0;

            for (int j = 0; j < tableau.ColsCount; j++)
            {
                double zValue = tableau.GetZ(j);

                // Для максимизации ищем самый отрицательный элемент (правило Бланда или просто минимальный)
                if (_mode == OptimizationMode.Maximization)
                {
                    if (zValue < bestValue)
                    {
                        bestValue = zValue;
                        bestCol = j;
                    }
                }
                else // Для минимизации ищем самый положительный
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

        private int FindPivotRow(SimplexTableau tableau, int pivotCol)
        {
            int pivotRow = -1;
            double minRatio = double.MaxValue;

            Console.WriteLine($"  Calculating ratios for column {pivotCol}:");
            for (int i = 0; i < tableau.RowsCount; i++)
            {
                double val = tableau.GetValue(i, pivotCol);
                if (val > 0) // В основной фазе ищем только положительные элементы
                {
                    double ratio = tableau.GetB(i) / val;
                    Console.WriteLine($"    Row {i}: Ratio = {ratio:F4} ({tableau.GetB(i):F2} / {val:F2})");

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