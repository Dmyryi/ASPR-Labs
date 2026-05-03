using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex
{
    public class OptimalSolutionFinder : OptimalSolutionFinderBase, IOptimalSolution
    {
        private readonly IJordan _jordan;
        private OptimizationMode _mode;
        private readonly ISimplexProtocol? _protocol;
        private readonly IFindPivot _findPivot;

        public OptimalSolutionFinder(IJordan jordan, IFindPivot findPivot, OptimizationMode mode = OptimizationMode.Maximization, ISimplexProtocol? protocol = null)
        {
            _jordan = jordan;
            _findPivot = findPivot;
            _mode = mode;
            _protocol = protocol;
        }

        public void Find(SimplexTableau tableau)
        {
            int step = 1;
            const int maxSteps = 500;

            _protocol?.LogSection("Пошук оптимального розв’язку:");

            while (step <= maxSteps)
            {
                int col = FindPivotColumn(tableau);
                if (col == -1)
                {
                    break;
                }

                int row = _findPivot.FindPivotRow(tableau, col);
                if (row == -1)
                {
                    string errorMsg = _mode == OptimizationMode.Maximization
                        ? "Функція не обмежена зверху"
                        : "Функція не обмежена знизу";

                    System.Diagnostics.Debug.WriteLine($"CRITICAL: PivotCol {col} is unbounded!");
                    for (int i = 0; i < tableau.RowsCount; i++)
                        System.Diagnostics.Debug.WriteLine($"Row {i}, Val: {tableau.GetValue(i, col)}");

                    throw new Exception(errorMsg);
                   
                }

                _protocol?.LogPivot(step, tableau, row, col);
                tableau.SetBasisColumn(row, col);
                var nextData = _jordan.ModifiedJordanMethod(tableau.Data, row, col);
                tableau.Update(nextData);
                _protocol?.LogTableau(tableau);

                step++;
            }

            if (step > maxSteps)
            {
                throw new InvalidOperationException("Перевищено ліміт ітерацій під час пошуку оптимального розв’язку.");
            }
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
}