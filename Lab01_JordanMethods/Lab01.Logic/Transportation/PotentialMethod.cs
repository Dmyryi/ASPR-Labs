namespace Lab01.Logic.Transportation;

public static class PotentialMethod
{
    private const double Tol = 1e-9;
    private const int MaxIterations = 500;

    public static TransportationPlan Solve(TransportationProblem problem, double[,] initialPlan, TransportationTrace? trace = null)
    {
        var plan = TransportationPlanHelper.ClonePlan(initialPlan);
        TransportationPlanHelper.AddDegenerateCellsIfNeeded(problem, plan);

        for (int iteration = 0; iteration < MaxIterations; iteration++)
        {
            if (!TryComputePotentials(problem, plan, out double[] u, out double[] v))
                break;

            var basics = TransportationPlanHelper.GetBasicCells(problem, plan);
            double[,] indirect = BuildIndirectMatrix(problem, plan, u, v, basics);
            var problematic = FindProblematicCells(problem, plan, u, v, basics);

            if (!TryFindEnteringCell(problem, plan, u, v, out int enterI, out int enterJ, out double maxDiff))
            {
                trace?.PotentialIterations.Add(new PotentialIterationStep
                {
                    SupplyPotentials = (double[])u.Clone(),
                    DemandPotentials = (double[])v.Clone(),
                    IndirectCosts = indirect,
                    IsOptimal = true,
                    PlanBefore = TransportationPlanHelper.ClonePlan(plan),
                    PlanAfter = TransportationPlanHelper.ClonePlan(plan)
                });
                break;
            }

            if (maxDiff <= Tol)
            {
                trace?.PotentialIterations.Add(new PotentialIterationStep
                {
                    SupplyPotentials = (double[])u.Clone(),
                    DemandPotentials = (double[])v.Clone(),
                    IndirectCosts = indirect,
                    IsOptimal = true,
                    PlanBefore = TransportationPlanHelper.ClonePlan(plan),
                    PlanAfter = TransportationPlanHelper.ClonePlan(plan)
                });
                break;
            }

            var planBefore = TransportationPlanHelper.ClonePlan(plan);
            if (!TryImprovePlan(problem, plan, enterI, enterJ, out var cycle, out double theta))
                break;

            trace?.PotentialIterations.Add(new PotentialIterationStep
            {
                SupplyPotentials = (double[])u.Clone(),
                DemandPotentials = (double[])v.Clone(),
                IndirectCosts = indirect,
                IsOptimal = false,
                ProblematicCells = problematic,
                EnterRow = enterI,
                EnterCol = enterJ,
                MaxDifference = maxDiff,
                Cycle = cycle,
                Theta = theta,
                PlanBefore = planBefore,
                PlanAfter = TransportationPlanHelper.ClonePlan(plan)
            });
        }

        return new TransportationPlan
        {
            Allocations = plan,
            TotalCost = TransportationPlanHelper.ComputeCost(problem, plan),
            MethodName = "метод потенціалів"
        };
    }

    private static double[,] BuildIndirectMatrix(
        TransportationProblem problem,
        double[,] plan,
        double[] u,
        double[] v,
        List<(int i, int j)> basics)
    {
        int n = problem.Rows;
        int m = problem.Cols;
        var indirect = new double[n, m];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (basics.Contains((i, j)))
                    indirect[i, j] = double.NaN;
                else
                    indirect[i, j] = u[i] + v[j];
            }
        }

        return indirect;
    }

    private static List<(int Row, int Col)> FindProblematicCells(
        TransportationProblem problem,
        double[,] plan,
        double[] u,
        double[] v,
        List<(int i, int j)> basics)
    {
        var list = new List<(int, int)>();
        for (int i = 0; i < problem.Rows; i++)
        {
            for (int j = 0; j < problem.Cols; j++)
            {
                if (basics.Contains((i, j)))
                    continue;
                if (u[i] + v[j] > problem.Costs[i, j] + Tol)
                    list.Add((i, j));
            }
        }

        return list;
    }

    private static bool TryComputePotentials(
        TransportationProblem problem,
        double[,] plan,
        out double[] u,
        out double[] v)
    {
        int n = problem.Rows;
        int m = problem.Cols;
        u = new double[n];
        v = new double[m];
        var uKnown = new bool[n];
        var vKnown = new bool[m];

        u[0] = 0;
        uKnown[0] = true;

        var basics = TransportationPlanHelper.GetBasicCells(problem, plan);
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach ((int i, int j) in basics)
            {
                if (uKnown[i] && !vKnown[j])
                {
                    v[j] = problem.Costs[i, j] - u[i];
                    vKnown[j] = true;
                    changed = true;
                }
                else if (!uKnown[i] && vKnown[j])
                {
                    u[i] = problem.Costs[i, j] - v[j];
                    uKnown[i] = true;
                    changed = true;
                }
            }
        }

        return uKnown.All(x => x) && vKnown.All(x => x);
    }

    private static bool TryFindEnteringCell(
        TransportationProblem problem,
        double[,] plan,
        double[] u,
        double[] v,
        out int enterI,
        out int enterJ,
        out double maxDiff)
    {
        enterI = -1;
        enterJ = -1;
        maxDiff = 0;
        var basics = TransportationPlanHelper.GetBasicCells(problem, plan);

        for (int i = 0; i < problem.Rows; i++)
        {
            for (int j = 0; j < problem.Cols; j++)
            {
                if (basics.Contains((i, j)))
                    continue;

                double indirect = u[i] + v[j];
                double diff = indirect - problem.Costs[i, j];
                if (diff > maxDiff + Tol)
                {
                    maxDiff = diff;
                    enterI = i;
                    enterJ = j;
                }
            }
        }

        return enterI >= 0;
    }

    private static bool TryImprovePlan(
        TransportationProblem problem,
        double[,] plan,
        int enterI,
        int enterJ,
        out List<(int i, int j)> cycle,
        out double theta)
    {
        cycle = new List<(int, int)>();
        theta = 0;
        var basics = TransportationPlanHelper.GetBasicCells(problem, plan);
        if (!TryBuildCycle(basics, enterI, enterJ, out cycle))
            return false;

        theta = double.PositiveInfinity;
        for (int k = 1; k < cycle.Count; k += 2)
        {
            (int i, int j) = cycle[k];
            if (plan[i, j] < theta)
                theta = plan[i, j];
        }

        if (double.IsPositiveInfinity(theta))
            return false;

        for (int k = 0; k < cycle.Count; k++)
        {
            (int i, int j) = cycle[k];
            if (k % 2 == 0)
                plan[i, j] += theta;
            else
                plan[i, j] -= theta;
        }

        TransportationPlanHelper.AddDegenerateCellsIfNeeded(problem, plan);
        return true;
    }

    private static bool TryBuildCycle(
        List<(int i, int j)> basics,
        int enterI,
        int enterJ,
        out List<(int i, int j)> cycle)
    {
        cycle = new List<(int, int)>();
        var start = (enterI, enterJ);
        var visitedPaths = new Dictionary<(int i, int j, bool sameRowNext), List<(int i, int j)>>();
        var queue = new Queue<(int i, int j, bool sameRowNext, List<(int i, int j)> path)>();
        queue.Enqueue((enterI, enterJ, true, new List<(int, int)> { start }));

        while (queue.Count > 0)
        {
            var (ci, cj, sameRowNext, path) = queue.Dequeue();

            if (sameRowNext && ci == enterI && path.Count >= 2)
            {
                cycle = path;
                return true;
            }

            if (!sameRowNext && cj == enterJ && path.Count >= 2)
            {
                cycle = path;
                return true;
            }

            var key = (ci, cj, sameRowNext);
            if (visitedPaths.ContainsKey(key))
                continue;
            visitedPaths[key] = path;

            if (sameRowNext)
            {
                foreach ((int i, int j) in basics)
                {
                    if (i != ci || j == cj)
                        continue;
                    var nextPath = new List<(int, int)>(path) { (i, j) };
                    queue.Enqueue((i, j, false, nextPath));
                }
            }
            else
            {
                foreach ((int i, int j) in basics)
                {
                    if (j != cj || i == ci)
                        continue;
                    var nextPath = new List<(int, int)>(path) { (i, j) };
                    queue.Enqueue((i, j, true, nextPath));
                }
            }
        }

        return false;
    }
}
