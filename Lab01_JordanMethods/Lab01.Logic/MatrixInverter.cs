using Lab01.Logic.Interfaces;

namespace Lab01.Logic
{
    public class MatrixInverter:IMatrixInverter
    {
        private readonly IJordan _jordan;

        public MatrixInverter(IJordan jordan)
        {
           _jordan = jordan;
        }

        public double[,] Invert(double[,] matrixA)
        {
            int n = matrixA.GetLength(0);
            double[,] result = matrixA;
            for (int i = 0; i < n; i++)
            {
                result = _jordan.JordanMethod(result, i, i);
            }
            return result;
        }
    }
}