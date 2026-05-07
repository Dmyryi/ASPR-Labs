using System;
using System.Linq;

namespace Lab01.Logic.Models
{
    public class SimplexTableau
    {
        public double[,] Data { get; private set; }
        public int RowsCount { get; }
        public int ColsCount { get; private set; }

        /// <summary>Скільки змінних у початковій постановці (довжина вектора X у відповіді після операцій із таблицею).</summary>
        public int ProblemVariableCount { get; }

        public int[] BasisVariables { get; }

        public int[] ColumnVariables { get; private set; }

        public SimplexTableau(double[,] matrixA, double[] vectorB, double[] vectorZ)
        {
            RowsCount = matrixA.GetLength(0);
            ColsCount = matrixA.GetLength(1);
            ProblemVariableCount = ColsCount;

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


            NormalizeBasisRows();

        }
        private void NormalizeBasisRows()
        {
            for (int i = 0; i < RowsCount; i++)
            {
                if (Math.Abs(GetB(i)) < 1e-9)
                {
                    // Ищем столбец, где стоит единица (базис)
                    for (int j = 0; j < ColsCount; j++)
                    {
                        if (Math.Abs(Data[i, j]) > 1e-9) // Нашли базисный элемент
                        {
                            if (Data[i, j] < 0) // Если он отрицательный, множим ряд на -1
                            {
                                for (int k = 0; k <= ColsCount; k++) Data[i, k] *= -1;
                            }
                            break;
                        }
                    }
                }
            }
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

        public void RemoveConstraintColumn(int colIndex)
        {
            if ((uint)colIndex >= (uint)ColsCount)
                throw new ArgumentOutOfRangeException(nameof(colIndex));

            int newColsCount = ColsCount - 1;
            double[,] next = new double[RowsCount + 1, newColsCount + 1];

            for (int i = 0; i <= RowsCount; i++)
            {
                int dst = 0;
                for (int j = 0; j < ColsCount; j++)
                {
                    if (j == colIndex) continue;
                    next[i, dst++] = Data[i, j];
                }

                next[i, newColsCount] = Data[i, ColsCount];
            }

            Data = next;
            ColsCount = newColsCount;

            var newColumnVars = new int[ColsCount];
            for (int j = 0, k = 0; j < ColumnVariables.Length; j++)
            {
                if (j != colIndex) newColumnVars[k++] = ColumnVariables[j];
            }

            ColumnVariables = newColumnVars;
        }

    };
}