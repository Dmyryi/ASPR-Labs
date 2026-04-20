using System;
using System.Linq;

namespace Lab01.Logic.Models
{
    public class SimplexTableau
    {
        public double[,] Data { get; private set; }
        public int RowsCount { get; }
        public int ColsCount { get; }


        public int[] BasisVariables { get; }
 
        public int[] ColumnVariables { get; }

        public SimplexTableau(double[,] matrixA, double[] vectorB, double[] vectorZ)
        {
            RowsCount = matrixA.GetLength(0);
            ColsCount = matrixA.GetLength(1);

            Data = new double[RowsCount + 1, ColsCount + 1];

            ColumnVariables = Enumerable.Range(0, ColsCount).ToArray();

            BasisVariables = Enumerable.Range(ColsCount, RowsCount).ToArray();

            for (int i = 0; i < RowsCount; i++)
            {
                for (int j = 0; j < ColsCount; j++)
                    Data[i, j] = matrixA[i, j];

                Data[i, ColsCount] = vectorB[i];
            }

            for (int j = 0; j < ColsCount; j++)
                Data[RowsCount, j] = vectorZ[j];
        }

        public double GetB(int row) => Data[row, ColsCount];
        public double GetZ(int col) => Data[RowsCount, col];
        public double GetValue(int r, int c) => Data[r, c];


        public void SetBasisColumn(int row, int col)
        {
            int outgoingVariable = BasisVariables[row];
            int incomingVariable = ColumnVariables[col];

            BasisVariables[row] = incomingVariable;
            ColumnVariables[col] = outgoingVariable;
        }

        public void Update(double[,] newData) => Data = newData;
    }
}