namespace Lab01.Logic.Assignment;

internal static class AssignmentMatrixBuilder
{
    private const double Tol = 1e-9;

    public static int[] Build(double[,] reducedMatrix, int n, AssignmentTrace? trace)
    {
        var crossed = new bool[n, n];
        var assignedCol = Enumerable.Repeat(-1, n).ToArray();
        int k = 0;

        while (k < n)
        {
            int rowOne = FindRowWithSingleZero(reducedMatrix, n, crossed, assignedCol);
            if (rowOne >= 0)
            {
                int col = FindZeroColumnInRow(reducedMatrix, n, rowOne, crossed);
                assignedCol[rowOne] = col;
                k++;
                trace?.AssignmentSteps.Add(new AssignmentFillStep
                {
                    Description = "рядок з одним «0»",
                    Row = rowOne,
                    Column = col,
                    AssignmentIndex = k
                });
                CrossColumnExcept(n, crossed, col, rowOne);
            }
            else
            {
                int rowMin = FindRowWithMinimumZeros(reducedMatrix, n, crossed, assignedCol);
                if (rowMin < 0)
                    break;

                int col = FindFirstZeroColumnInRow(reducedMatrix, n, rowMin, crossed);
                assignedCol[rowMin] = col;
                k++;
                trace?.AssignmentSteps.Add(new AssignmentFillStep
                {
                    Description = "рядок з мінімальною кількістю «0»",
                    Row = rowMin,
                    Column = col,
                    AssignmentIndex = k
                });
                CrossColumnExcept(n, crossed, col, rowMin);
                CrossRowExcept(n, crossed, rowMin, col);
            }
        }

        return assignedCol;
    }

    public static int[,] ToBinaryMatrix(int[] assignedCol, int n)
    {
        var result = new int[n, n];
        for (int i = 0; i < n; i++)
        {
            if (assignedCol[i] >= 0)
                result[i, assignedCol[i]] = 1;
        }

        return result;
    }

    private static int FindRowWithSingleZero(double[,] m, int n, bool[,] crossed, int[] assignedCol)
    {
        for (int i = 0; i < n; i++)
        {
            if (assignedCol[i] >= 0)
                continue;

            if (CountAvailableZeros(m, n, i, crossed) == 1)
                return i;
        }

        return -1;
    }

    private static int FindRowWithMinimumZeros(double[,] m, int n, bool[,] crossed, int[] assignedCol)
    {
        int bestRow = -1;
        int bestCount = int.MaxValue;
        for (int i = 0; i < n; i++)
        {
            if (assignedCol[i] >= 0)
                continue;

            int count = CountAvailableZeros(m, n, i, crossed);
            if (count > 0 && count < bestCount)
            {
                bestCount = count;
                bestRow = i;
            }
        }

        return bestRow;
    }

    private static int CountAvailableZeros(double[,] m, int n, int row, bool[,] crossed)
    {
        int count = 0;
        for (int j = 0; j < n; j++)
        {
            if (!crossed[row, j] && IsZero(m[row, j]))
                count++;
        }

        return count;
    }

    private static int FindZeroColumnInRow(double[,] m, int n, int row, bool[,] crossed)
    {
        for (int j = 0; j < n; j++)
        {
            if (!crossed[row, j] && IsZero(m[row, j]))
                return j;
        }

        return -1;
    }

    private static int FindFirstZeroColumnInRow(double[,] m, int n, int row, bool[,] crossed) =>
        FindZeroColumnInRow(m, n, row, crossed);

    private static void CrossColumnExcept(int n, bool[,] crossed, int col, int exceptRow)
    {
        for (int i = 0; i < n; i++)
        {
            if (i != exceptRow)
                crossed[i, col] = true;
        }
    }

    private static void CrossRowExcept(int n, bool[,] crossed, int row, int exceptCol)
    {
        for (int j = 0; j < n; j++)
        {
            if (j != exceptCol)
                crossed[row, j] = true;
        }
    }

    private static bool IsZero(double v) => Math.Abs(v) <= Tol;
}
