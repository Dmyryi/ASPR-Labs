namespace Lab01.Logic.NetworkPlanning;

public static class ResourceLoadCalculator
{
    public static List<(int From, int To, int People)> Compute(IReadOnlyList<NetworkTask> tasks, int projectDuration) =>
        Compute(tasks, projectDuration, t => t.EarlyStart, t => t.EarlyFinish);

    public static List<(int From, int To, int People)> Compute(NetworkSchedule schedule) =>
        Compute(
            schedule.Result.Tasks,
            schedule.Result.ProjectDuration,
            t => schedule.GetStart(t.Id),
            t => schedule.GetFinish(t.Id));

    public static int ComputePeak(NetworkSchedule schedule)
    {
        var loads = Compute(schedule);
        return loads.Count == 0 ? 0 : loads.Max(l => l.People);
    }

    private static List<(int From, int To, int People)> Compute(
        IReadOnlyList<NetworkTask> tasks,
        int projectDuration,
        Func<NetworkTask, int> startSelector,
        Func<NetworkTask, int> finishSelector)
    {
        if (projectDuration <= 0)
            return new List<(int, int, int)> { (0, 1, 0) };

        var points = new SortedSet<int> { 0, projectDuration };
        foreach (NetworkTask task in tasks)
        {
            points.Add(startSelector(task));
            points.Add(finishSelector(task));
        }

        var list = points.ToList();
        var segments = new List<(int From, int To, int People)>();
        for (int i = 0; i < list.Count - 1; i++)
        {
            int from = list[i];
            int to = list[i + 1];
            if (from == to) continue;

            int people = tasks
                .Where(t => startSelector(t) <= from && finishSelector(t) > from)
                .Sum(t => t.People);

            if (segments.Count > 0 && segments[^1].People == people)
            {
                var last = segments[^1];
                segments[^1] = (last.From, to, people);
            }
            else
            {
                segments.Add((from, to, people));
            }
        }

        return segments;
    }
}
