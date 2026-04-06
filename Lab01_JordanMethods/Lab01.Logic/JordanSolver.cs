using System.Runtime.InteropServices;
using Lab01.Logic.Interfaces;

namespace Lab01.Logic
{

    public enum JordanMode
    {
        Standart,
        Modified
    }

    public class JordanSolver : IJordan
    {
    
        public double[,] JordanMethod(double[,] matrixA, int r, int s)
        {
            return ExecuteJordan(matrixA, r, s, JordanMode.Standart);
        }

        public double[,] ModifiedJordanMethod(double[,] matrixA, int r, int s)
        {
            return ExecuteJordan(matrixA,r,s, JordanMode.Modified);
        }

        public double[,] ExecuteJordan(double[,] matrixA, int r, int s, JordanMode mode)
        {
            
            int rowsCount = matrixA.GetLength(0);
            int columnsCount = matrixA.GetLength(1);
            double[,] nextMatrix = new double[rowsCount, columnsCount];
            double oldPivot = matrixA[r, s];
            if (Math.Abs(oldPivot) < 1e-10)
                throw new InvalidOperationException("Розв'язувальний елемент не може бути рівним нулю.");

            nextMatrix[r, s] = 1 / oldPivot;

            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < columnsCount; j++)
                {
                    
                    if (i == r && j != s)
                    {
                        double val = matrixA[r, j] / oldPivot;
                        nextMatrix[r, j] = (mode == JordanMode.Modified) ? val : -val;
                    }
                   
                    else if (i != r && j == s)
                    {
                        double val = matrixA[i, s] / oldPivot;
                        nextMatrix[i, s] = (mode == JordanMode.Modified) ? -val : val;
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
