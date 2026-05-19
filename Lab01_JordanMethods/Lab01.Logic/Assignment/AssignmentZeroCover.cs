namespace Lab01.Logic.Assignment;

internal static class AssignmentZeroCover
{
    private const double Tol = 1e-9;

    public static int FindMaximumMatching(double[,] matrix, int n, out int[] matchRow, out int[] matchCol)
    {
        matchRow = Enumerable.Repeat(-1, n).ToArray();
        matchCol = Enumerable.Repeat(-1, n).ToArray();

        int count = 0;
        for (int i = 0; i < n; i++)
        {
            var seen = new bool[n];
            if (TryAugment(i, matrix, n, matchRow, matchCol, seen))
                count++;
        }

        return count;
    }

    public static void FindMinimumLineCover(
        double[,] matrix,
        int n,
        int[] matchRow,
        int[] matchCol,
        out bool[] coverRows,
        out bool[] coverCols,
        out int lineCount)
    {
        var rowInZ = new bool[n];
        var colInZ = new bool[n];
        var queue = new Queue<int>();

        for (int i = 0; i < n; i++)
        {
            if (matchRow[i] < 0)
            {
                rowInZ[i] = true;
                queue.Enqueue(i);
            }
        }

        while (queue.Count > 0)
        {
            int i = queue.Dequeue();
            for (int j = 0; j < n; j++)
            {
                if (!IsZero(matrix[i, j]) || colInZ[j])
                    continue;

                colInZ[j] = true;
                int matchedRow = matchCol[j];
                if (matchedRow >= 0 && !rowInZ[matchedRow])
                {
                    rowInZ[matchedRow] = true;
                    queue.Enqueue(matchedRow);
                }
            }
        }

        coverRows = new bool[n];
        coverCols = new bool[n];
        for (int i = 0; i < n; i++)
            coverRows[i] = !rowInZ[i];
        for (int j = 0; j < n; j++)
            coverCols[j] = colInZ[j];

        lineCount = coverRows.Count(b => b) + coverCols.Count(b => b);
    }

    private static bool TryAugment(
        int row,
        double[,] matrix,
        int n,
        int[] matchRow,
        int[] matchCol,
        bool[] seenCol)
    {
        for (int j = 0; j < n; j++)
        {
            if (!IsZero(matrix[row, j]) || seenCol[j])
                continue;

            seenCol[j] = true;
            if (matchCol[j] < 0 || TryAugment(matchCol[j], matrix, n, matchRow, matchCol, seenCol))
            {
                matchRow[row] = j;
                matchCol[j] = row;
                return true;
            }
        }

        return false;
    }

    private static bool IsZero(double v) => Math.Abs(v) <= Tol;
}
