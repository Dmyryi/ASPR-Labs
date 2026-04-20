using System;
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

            _protocol?.LogInitialTableau(tableau);
            _protocol?.LogSection("Пошук опорного розв’язку:");

            while (true)
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
                

                
                _protocol?.LogPivot(step, tableau, actualPivotRow, col);
                tableau.SetBasisColumn(actualPivotRow, col);
                var nextData = _jordan.ModifiedJordanMethod(tableau.Data, actualPivotRow, col);
                tableau.Update(nextData);
                _protocol?.LogTableau(tableau);

                step++;
            }
            
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
                double valInCol = tableau.GetValue(r, pivotCol);
                if (valInCol < 0)
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