namespace Lab01.Logic.Transportation;

public static class TransportationSolver
{
    public static TransportationSolveResult Solve(double[,] costs, double[] supply, double[] demand)
    {
        TransportationBasisTracker.Clear();
        double sumA = supply.Sum();
        double sumB = demand.Sum();
        bool wasOpen = Math.Abs(sumA - sumB) > 1e-9;

        TransportationProblem problem = TransportationBalancer.Balance(costs, supply, demand);
        string? balanceNote = null;
        if (wasOpen)
        {
            if (problem.AddedDummySupply)
                balanceNote = $"Відкрита задача: додано фіктивний пункт відправлення (запас {problem.Supply[^1]:0.##}).";
            else if (problem.AddedDummyDemand)
                balanceNote = $"Відкрита задача: додано фіктивний пункт призначення (заявка {problem.Demand[^1]:0.##}).";
        }

        var trace = new TransportationTrace();
        TransportationPlan nw = NorthwestCornerMethod.Solve(problem, trace);
        TransportationPlan minEl = MinimumElementMethod.Solve(problem);
        TransportationPlan optimal = PotentialMethod.Solve(problem, minEl.Allocations);

        var nwForPotentials = TransportationPlanHelper.ClonePlan(nw.Allocations);
        TransportationBasisTracker.Clear();
        PotentialMethod.Solve(problem, nwForPotentials, trace);

        TransportationBasisTracker.Clear();
        TransportationSimplexRunner.Run(problem, trace);
        TransportationBasisTracker.Clear();

        return new TransportationSolveResult
        {
            Problem = problem,
            WasOpen = wasOpen,
            BalanceNote = balanceNote,
            NorthwestCornerPlan = nw,
            MinimumElementPlan = minEl,
            OptimalPlan = optimal,
            Trace = trace
        };
    }
}
