using Lab01.Logic.NetworkPlanning;
using Xunit;

namespace Lab01.Tests;

public class CriticalPathTests
{
    [Fact]
    public void MethodicalExample1_Path_1_2_5_7_Duration24()
    {
        var inputs = new[]
        {
            Task(1, null, 5, 2),
            Task(2, "1", 8, 3),
            Task(3, "1", 3, 2),
            Task(4, "1", 6, 2),
            Task(5, "2", 7, 3),
            Task(6, "2,3", 6, 2),
            Task(7, "4,5,6", 4, 2)
        };

        CriticalPathSolveResult r = CriticalPathSolver.Solve(inputs);

        Assert.Equal(24, r.ProjectDuration);
        Assert.Equal(new[] { 1, 2, 5, 7 }, r.CriticalPath);
    }

    [Fact]
    public void MethodicalExample2_Path_1_2_4_7_8_Duration19()
    {
        var inputs = new[]
        {
            Task(1, null, 3, 2),
            Task(2, "1", 4, 3),
            Task(3, "1", 2, 4),
            Task(4, "2", 5, 3),
            Task(5, "3", 1, 2),
            Task(6, "3", 2, 3),
            Task(7, "4,5", 4, 2),
            Task(8, "6,7", 3, 2)
        };

        CriticalPathSolveResult r = CriticalPathSolver.Solve(inputs);

        Assert.Equal(19, r.ProjectDuration);
        Assert.Equal(new[] { 1, 2, 4, 7, 8 }, r.CriticalPath);
    }

    [Fact]
    public void Variant10_Path_3_4_7_10_Duration40()
    {
        var inputs = new[]
        {
            Task(1, null, 10, 3),
            Task(2, null, 12, 4),
            Task(3, null, 7, 2),
            Task(4, "3", 10, 3),
            Task(5, "3", 15, 6),
            Task(6, "1,2,4", 5, 1),
            Task(7, "2,4", 13, 3),
            Task(8, "1,2,4", 12, 4),
            Task(9, "3", 11, 5),
            Task(10, "5,6,7", 10, 6),
            Task(11, "9", 8, 4)
        };

        CriticalPathSolveResult r = CriticalPathSolver.Solve(inputs);

        Assert.Equal(40, r.ProjectDuration);
        Assert.Equal(new[] { 3, 4, 7, 10 }, r.CriticalPath);
    }

    private static NetworkTaskInput Task(int id, string? preds, int duration, int people) =>
        new()
        {
            Id = id,
            Predecessors = ParsePreds(preds),
            Duration = duration,
            People = people
        };

    private static IReadOnlyList<int> ParsePreds(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim() == "-")
            return Array.Empty<int>();

        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToArray();
    }
}
