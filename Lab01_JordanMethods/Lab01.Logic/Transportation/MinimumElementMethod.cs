namespace Lab01.Logic.Transportation;

public static class MinimumElementMethod
{
    private const double Tol = 1e-9;

    public static TransportationPlan Solve(TransportationProblem problem)
    {
        int n = problem.Rows;
        int m = problem.Cols;
        var plan = new double[n, m];
        var supply = (double[])problem.Supply.Clone();
        var demand = (double[])problem.Demand.Clone();
        var rowActive = Enumerable.Repeat(true, n).ToArray();
        var colActive = Enumerable.Repeat(true, m).ToArray();

        while (HasUnfilledCells(rowActive, colActive))
        {
            if (!TryFindMinimumCell(problem, rowActive, colActive, out int i, out int j))
                break;

            double amount = Math.Min(supply[i], demand[j]);
            plan[i, j] = amount;
            supply[i] -= amount;
            demand[j] -= amount;

            bool rowDone = supply[i] <= Tol;
            bool colDone = demand[j] <= Tol;

            if (rowDone && colDone)
                PlaceDegenerateZero(plan, problem.Rows, problem.Cols, i, j);

            if (rowDone)
                rowActive[i] = false;

            if (colDone)
                colActive[j] = false;
        }

        TransportationPlanHelper.AddDegenerateCellsIfNeeded(problem, plan);
        return new TransportationPlan
        {
            Allocations = plan,
            TotalCost = TransportationPlanHelper.ComputeCost(problem, plan),
            MethodName = "метод мінімального елемента"
        };
    }

    private static bool HasUnfilledCells(bool[] rowActive, bool[] colActive)
    {
        for (int i = 0; i < rowActive.Length; i++)
        {
            if (!rowActive[i]) continue;
            for (int j = 0; j < colActive.Length; j++)
            {
                if (colActive[j])
                    return true;
            }
        }

        return false;
    }

    private static bool TryFindMinimumCell(
        TransportationProblem problem,
        bool[] rowActive,
        bool[] colActive,
        out int bestI,
        out int bestJ)
    {
        bestI = -1;
        bestJ = -1;
        double bestCost = double.PositiveInfinity;

        for (int i = 0; i < problem.Rows; i++)
        {
            if (!rowActive[i]) continue;
            for (int j = 0; j < problem.Cols; j++)
            {
                if (!colActive[j]) continue;
                double c = problem.Costs[i, j];
                if (c < bestCost)
                {
                    bestCost = c;
                    bestI = i;
                    bestJ = j;
                }
            }
        }

        return bestI >= 0;
    }

    private static void PlaceDegenerateZero(double[,] plan, int rows, int cols, int i, int j)
    {
        if (j + 1 < cols && plan[i, j + 1] <= Tol)
        {
            plan[i, j + 1] = 0;
            TransportationBasisTracker.Add(i, j + 1);
        }
        else if (i + 1 < rows && plan[i + 1, j] <= Tol)
        {
            plan[i + 1, j] = 0;
            TransportationBasisTracker.Add(i + 1, j);
        }
    }
}
