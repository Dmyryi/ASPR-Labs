using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab01.Logic.Interfaces;

namespace Lab01.Logic.Simplex
{
   public class BasicSolutionFinder:IBasicSolutionFinder
    {

        private readonly IJordan _jordan;
        public BasicSolutionFinder(IJordan jordan) { 
        _jordan = jordan;
        
        }

        public void Find(SimplexTableau tableau)
        {
         
            while (true)
            {
                int row = FindNegativeB(tableau);
                if (row == -1) break;
                int col = FindNegativeInRow(tableau, row);
                if (col == -1) throw new Exception("Infeasible");

                int actualPivot = FindPivotRow(tableau, col, row);
                var nextData = _jordan.ModifiedJordanMethod(tableau.Data, actualPivot, col);
                tableau.Update(nextData);
            }
        }

        private int FindNegativeB(SimplexTableau tableau)
        {
            for (int i = 0; i < tableau.RowsCount; i++)
            {
                if (tableau.GetB(i) < 0) return i;
            }
            return -1;
        }

        private int FindNegativeInRow(SimplexTableau tableau, int pivotRow)
        {
            for (int j = 0; j < tableau.ColsCount; j++)
            {
                if (tableau.GetValue(pivotRow, j) < 0) return j;
            }
            return -1;
        }

        private int FindPivotRow(SimplexTableau tableau, int pivotCol, int initialRow)
        {
            int actualPivot = -1;
            double minRatio = double.MaxValue;

            for (int r = 0; r < tableau.RowsCount; r++)
            {
                double valInCol = tableau.GetValue(r, pivotCol);

                if (Math.Abs(valInCol) > 0)
                {
                    double ratio = tableau.GetB(r) / valInCol;

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
