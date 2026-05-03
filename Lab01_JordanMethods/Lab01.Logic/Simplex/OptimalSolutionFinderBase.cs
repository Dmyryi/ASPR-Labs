using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex
{
    public class OptimalSolutionFinderBase:IFindPivot
    {

        public int FindPivotRow(SimplexTableau tableau, int pivotCol)
        {
            int pivotRow = -1;
            double minRatio = double.MaxValue;

            Console.WriteLine($"  Calculating ratios for column {pivotCol}:");
            for (int i = 0; i < tableau.RowsCount; i++)
            {
                double val = tableau.GetValue(i, pivotCol);
                if (val > 0)
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

        public int FindPivotInitialRow(SimplexTableau tableau, int pivotCol, int initialRow)
        {
            int bestRow = -1;
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
                        bestRow = r;
                    }
                }
            }

            return bestRow == -1 ? initialRow : bestRow;
        }


    }
}