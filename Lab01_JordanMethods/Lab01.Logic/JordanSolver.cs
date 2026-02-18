using System.Runtime.InteropServices;
using Lab01.Logic.Interfaces;

namespace Lab01.Logic
{
    public class JordanSolver : IJordan
    {
        public JordanSolver()
        {
          
        }
        
        public double[,] JordanMethod(double[,] matrixA, int r, int s)
        {
            int rowsCount = matrixA.GetLength(0);
            int columnsCount = matrixA.GetLength(1);
            double[,] nextMatrix = new double[rowsCount, columnsCount];
            double oldPivot = matrixA[r, s];

           
            nextMatrix[r, s] = 1 / oldPivot;

            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < columnsCount; j++)
                {
                    
                    if (i == r && j != s)
                    {
                        nextMatrix[r, j] = -matrixA[r, j] / oldPivot;
                    }
                   
                    else if (i != r && j == s)
                    {
                        nextMatrix[i, s] = matrixA[i, s] / oldPivot;
                    }
                    
                    else if (i != r && j != s)
                    {
                        nextMatrix[i, j] = (matrixA[i, j] * oldPivot - matrixA[i, s] * matrixA[r, j]) / oldPivot;
                    }
                }
            }

            return nextMatrix;
        }
    }
}
