namespace Lab01.Logic.Models;


public sealed class SimplexTableau
{
    private const double Epsilon = 1e-9;

    public double[,] Data { get; private set; }

    public int RowsCount { get; private set; }

    public int ColsCount { get; private set; }


    public int ProblemVariableCount { get; }

    public int[] BasisVariables { get; private set; }

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

    public double GetB(int row) => Data[row, ColsCount];

    public double GetZ(int col) => Data[RowsCount, col];

    public double GetValue(int row, int col) => Data[row, col];

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
        var next = new double[RowsCount + 1, newColsCount + 1];

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

   
    public void AppendBasisRow(double[] rowCoefficients, double rhs, int newBasisVariableId)
    {
        if (rowCoefficients is null) throw new ArgumentNullException(nameof(rowCoefficients));
        if (rowCoefficients.Length != ColsCount)
            throw new ArgumentException(
                $"Очікувана довжина рядка {ColsCount}, отримано {rowCoefficients.Length}.",
                nameof(rowCoefficients));

        int newRowsCount = RowsCount + 1;
        var next = new double[newRowsCount + 1, ColsCount + 1];

        for (int i = 0; i < RowsCount; i++)
            for (int j = 0; j <= ColsCount; j++)
                next[i, j] = Data[i, j];

        for (int j = 0; j < ColsCount; j++)
            next[RowsCount, j] = rowCoefficients[j];
        next[RowsCount, ColsCount] = rhs;

        for (int j = 0; j <= ColsCount; j++)
            next[newRowsCount, j] = Data[RowsCount, j];

        var nextBasis = new int[newRowsCount];
        Array.Copy(BasisVariables, nextBasis, BasisVariables.Length);
        nextBasis[RowsCount] = newBasisVariableId;

        Data = next;
        BasisVariables = nextBasis;
        RowsCount = newRowsCount;
    }

    private void NormalizeBasisRows()
    {
        for (int i = 0; i < RowsCount; i++)
        {
            if (Math.Abs(GetB(i)) >= Epsilon) continue;

            for (int j = 0; j < ColsCount; j++)
            {
                if (Math.Abs(Data[i, j]) <= Epsilon) continue;

                if (Data[i, j] < 0)
                {
                    for (int k = 0; k <= ColsCount; k++) Data[i, k] *= -1;
                }
                break;
            }
        }
    }
}
