namespace Lab01.Logic.Transportation;

internal static class TransportationPlanHelper
{
    private const double Tol = 1e-9;

    public static double ComputeCost(TransportationProblem problem, double[,] plan)
    {
        double sum = 0;
        for (int i = 0; i < problem.Rows; i++)
        {
            for (int j = 0; j < problem.Cols; j++)
                sum += plan[i, j] * problem.Costs[i, j];
        }

        return sum;
    }

    public static double[,] ClonePlan(double[,] plan)
    {
        int r = plan.GetLength(0);
        int c = plan.GetLength(1);
        var copy = new double[r, c];
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
                copy[i, j] = plan[i, j];
        }

        return copy;
    }

    public static void AddDegenerateCellsIfNeeded(TransportationProblem problem, double[,] plan)
    {
        var basics = BuildBasicCells(problem, plan);
        int needed = problem.Rows + problem.Cols - 1;
        if (basics.Count >= needed)
            return;

        for (int i = 0; i < problem.Rows && basics.Count < needed; i++)
        {
            for (int j = 0; j < problem.Cols && basics.Count < needed; j++)
            {
                if (basics.Contains((i, j)))
                    continue;
                if (!MaintainsIndependence(basics, i, j))
                    continue;
                plan[i, j] = 0;
                basics.Add((i, j));
            }
        }
    }

    public static List<(int i, int j)> GetBasicCells(TransportationProblem problem, double[,] plan) =>
        BuildBasicCells(problem, plan);

    private static List<(int i, int j)> BuildBasicCells(TransportationProblem problem, double[,] plan)
    {
        var basics = new List<(int, int)>();
        for (int i = 0; i < problem.Rows; i++)
        {
            for (int j = 0; j < problem.Cols; j++)
            {
                if (plan[i, j] > Tol || TransportationBasisTracker.Contains(i, j))
                    basics.Add((i, j));
            }
        }

        int needed = problem.Rows + problem.Cols - 1;
        for (int i = 0; i < problem.Rows && basics.Count < needed; i++)
        {
            for (int j = 0; j < problem.Cols && basics.Count < needed; j++)
            {
                if (basics.Contains((i, j)))
                    continue;
                if (!MaintainsIndependence(basics, i, j))
                    continue;
                if (plan[i, j] <= Tol)
                    plan[i, j] = 0;
                basics.Add((i, j));
            }
        }

        return basics;
    }

    private static bool MaintainsIndependence(List<(int i, int j)> basics, int i, int j)
    {
        if (basics.Count == 0)
            return true;

        bool touchesRow = basics.Any(b => b.i == i);
        bool touchesCol = basics.Any(b => b.j == j);
        return !(touchesRow && touchesCol);
    }
}
