namespace Lab01.Logic.Assignment;

public static class HungarianMethod
{
    private const double Tol = 1e-9;

    public static AssignmentSolveResult Solve(double[,] costs, AssignmentTrace? trace = null)
    {
        int n = costs.GetLength(0);
        if (costs.GetLength(1) != n)
            throw new InvalidOperationException("Матриця вартостей має бути квадратною.");

        var reduced = Clone(costs);
        ReduceRows(reduced, n, trace);
        ReduceColumns(reduced, n, trace);

        while (true)
        {
            var matrixBefore = Clone(reduced);
            int matching = AssignmentZeroCover.FindMaximumMatching(reduced, n, out int[] matchRow, out int[] matchCol);
            AssignmentZeroCover.FindMinimumLineCover(reduced, n, matchRow, matchCol, out bool[] coverRows, out bool[] coverCols, out int lineCount);
            bool isOptimal = lineCount == n;

            if (isOptimal)
            {
                trace?.CoverIterations.Add(new CoverIterationStep
                {
                    MatrixBefore = matrixBefore,
                    CoverRows = coverRows,
                    CoverCols = coverCols,
                    LineCount = lineCount,
                    MatchingCount = matching,
                    IsOptimal = true,
                    AdjustmentMin = 0,
                    MatrixAfter = Clone(reduced)
                });
                break;
            }

            double minUncovered = FindMinimumUncovered(reduced, n, coverRows, coverCols);
            AdjustMatrix(reduced, n, coverRows, coverCols, minUncovered);

            trace?.CoverIterations.Add(new CoverIterationStep
            {
                MatrixBefore = matrixBefore,
                CoverRows = coverRows,
                CoverCols = coverCols,
                LineCount = lineCount,
                MatchingCount = matching,
                IsOptimal = false,
                AdjustmentMin = minUncovered,
                MatrixAfter = Clone(reduced)
            });
        }

        int[] assignment = AssignmentMatrixBuilder.Build(reduced, n, trace);
        int[,] binary = AssignmentMatrixBuilder.ToBinaryMatrix(assignment, n);
        double totalCost = ComputeCost(costs, assignment, n);

        return new AssignmentSolveResult
        {
            Size = n,
            OriginalCosts = Clone(costs),
            ReducedCosts = reduced,
            AssignmentMatrix = binary,
            AssignedColumns = assignment,
            TotalCost = totalCost
        };
    }

    private static void ReduceRows(double[,] matrix, int n, AssignmentTrace? trace)
    {
        for (int i = 0; i < n; i++)
        {
            double min = matrix[i, 0];
            for (int j = 1; j < n; j++)
            {
                if (matrix[i, j] < min)
                    min = matrix[i, j];
            }

            trace?.RowReductions.Add(new RowReductionStep { Row = i, Minimum = min });
            for (int j = 0; j < n; j++)
                matrix[i, j] -= min;
        }
    }

    private static void ReduceColumns(double[,] matrix, int n, AssignmentTrace? trace)
    {
        for (int j = 0; j < n; j++)
        {
            double min = matrix[0, j];
            for (int i = 1; i < n; i++)
            {
                if (matrix[i, j] < min)
                    min = matrix[i, j];
            }

            trace?.ColumnReductions.Add(new ColumnReductionStep { Column = j, Minimum = min });
            for (int i = 0; i < n; i++)
                matrix[i, j] -= min;
        }
    }

    private static double FindMinimumUncovered(double[,] matrix, int n, bool[] coverRows, bool[] coverCols)
    {
        double min = double.PositiveInfinity;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (coverRows[i] || coverCols[j])
                    continue;
                if (matrix[i, j] < min)
                    min = matrix[i, j];
            }
        }

        if (double.IsPositiveInfinity(min))
            throw new InvalidOperationException("Не знайдено невикреслених елементів.");

        return min;
    }

    private static void AdjustMatrix(double[,] matrix, int n, bool[] coverRows, bool[] coverCols, double min)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                bool uncovered = !coverRows[i] && !coverCols[j];
                bool doubleCovered = coverRows[i] && coverCols[j];
                if (uncovered)
                    matrix[i, j] -= min;
                else if (doubleCovered)
                    matrix[i, j] += min;
            }
        }
    }

    private static double ComputeCost(double[,] costs, int[] assignedCol, int n)
    {
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            if (assignedCol[i] < 0)
                throw new InvalidOperationException("Не вдалося побудувати повне призначення.");
            sum += costs[i, assignedCol[i]];
        }

        return sum;
    }

    private static double[,] Clone(double[,] source)
    {
        int n = source.GetLength(0);
        int m = source.GetLength(1);
        var copy = new double[n, m];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
                copy[i, j] = source[i, j];
        }

        return copy;
    }
}
