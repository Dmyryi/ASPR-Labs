namespace Lab01.Logic.Transportation;

public static class TransportationBalancer
{
    private const double Tol = 1e-9;

    public static TransportationProblem Balance(double[,] costs, double[] supply, double[] demand)
    {
        double totalSupply = supply.Sum();
        double totalDemand = demand.Sum();
        int n = supply.Length;
        int m = demand.Length;

        if (Math.Abs(totalSupply - totalDemand) <= Tol)
        {
            return new TransportationProblem
            {
                Costs = CloneMatrix(costs),
                Supply = (double[])supply.Clone(),
                Demand = (double[])demand.Clone(),
                AddedDummySupply = false,
                AddedDummyDemand = false,
                OriginalSupplyCount = n,
                OriginalDemandCount = m
            };
        }

        if (totalSupply < totalDemand)
        {
            double diff = totalDemand - totalSupply;
            var newSupply = new double[n + 1];
            Array.Copy(supply, newSupply, n);
            newSupply[n] = diff;

            var newCosts = new double[n + 1, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                    newCosts[i, j] = costs[i, j];
            }

            for (int j = 0; j < m; j++)
                newCosts[n, j] = 0;

            return new TransportationProblem
            {
                Costs = newCosts,
                Supply = newSupply,
                Demand = (double[])demand.Clone(),
                AddedDummySupply = true,
                AddedDummyDemand = false,
                OriginalSupplyCount = n,
                OriginalDemandCount = m
            };
        }

        double diffDemand = totalSupply - totalDemand;
        var newDemand = new double[m + 1];
        Array.Copy(demand, newDemand, m);
        newDemand[m] = diffDemand;

        var costsExtended = new double[n, m + 1];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
                costsExtended[i, j] = costs[i, j];
            costsExtended[i, m] = 0;
        }

        return new TransportationProblem
        {
            Costs = costsExtended,
            Supply = (double[])supply.Clone(),
            Demand = newDemand,
            AddedDummySupply = false,
            AddedDummyDemand = true,
            OriginalSupplyCount = n,
            OriginalDemandCount = m
        };
    }

    private static double[,] CloneMatrix(double[,] a)
    {
        int r = a.GetLength(0);
        int c = a.GetLength(1);
        var copy = new double[r, c];
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
                copy[i, j] = a[i, j];
        }

        return copy;
    }
}
