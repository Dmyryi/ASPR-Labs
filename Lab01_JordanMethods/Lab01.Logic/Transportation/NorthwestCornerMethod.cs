namespace Lab01.Logic.Transportation;

public static class NorthwestCornerMethod
{
    private const double Tol = 1e-9;

    public static TransportationPlan Solve(TransportationProblem problem, TransportationTrace? trace = null)
    {
        int n = problem.Rows;
        int m = problem.Cols;
        var plan = new double[n, m];
        var supply = (double[])problem.Supply.Clone();
        var demand = (double[])problem.Demand.Clone();

        int i = 0;
        int j = 0;

        while (true)
        {
            double amount = Math.Min(supply[i], demand[j]);
            plan[i, j] = amount;
            trace?.NorthwestSteps.Add(new NorthwestAllocationStep { Row = i, Col = j, Amount = amount });
            supply[i] -= amount;
            demand[j] -= amount;

            bool supplyHasStock = supply[i] > Tol;
            bool demandSatisfied = demand[j] <= Tol;

            if (supplyHasStock)
            {
                j++;
                if (j >= m)
                    break;
            }
            else if (!demandSatisfied)
            {
                i++;
                if (i >= n)
                    break;
            }
            else
            {
                i++;
                j++;
                if (j >= m && i >= n)
                    break;
            }
        }

        TransportationPlanHelper.AddDegenerateCellsIfNeeded(problem, plan);
        return new TransportationPlan
        {
            Allocations = plan,
            TotalCost = TransportationPlanHelper.ComputeCost(problem, plan),
            MethodName = "метод північно-західного кута"
        };
    }
}
