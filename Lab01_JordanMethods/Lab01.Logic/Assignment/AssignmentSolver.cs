namespace Lab01.Logic.Assignment;

public static class AssignmentSolver
{
    public static AssignmentSolveResultWithTrace Solve(double[,] costs)
    {
        var trace = new AssignmentTrace();
        AssignmentSolveResult hungarian = HungarianMethod.Solve(costs, trace);
        int n = hungarian.Size;
        AssignmentSimplexRunner.Run(costs, n, trace);

        return new AssignmentSolveResultWithTrace
        {
            Hungarian = hungarian,
            Trace = trace
        };
    }
}

public sealed class AssignmentSolveResultWithTrace
{
    public required AssignmentSolveResult Hungarian { get; init; }
    public required AssignmentTrace Trace { get; init; }
}
