namespace Lab01.Logic.NetworkPlanning;

public static class CriticalPathSolver
{
    public static CriticalPathSolveResult Solve(IReadOnlyList<NetworkTaskInput> inputs)
    {
        if (inputs.Count == 0)
            throw new InvalidOperationException("Потрібна хоча б одна робота.");

        var tasks = inputs
            .OrderBy(t => t.Id)
            .Select(t => new NetworkTask
            {
                Id = t.Id,
                Predecessors = t.Predecessors,
                Duration = t.Duration,
                People = t.People
            })
            .ToList();

        Validate(tasks);
        BuildSuccessors(tasks);
        ForwardPass(tasks);
        int projectDuration = tasks.Max(t => t.EarlyFinish);
        BackwardPass(tasks, projectDuration);
        MarkCritical(tasks);

        var criticalPath = BuildCriticalPath(tasks);
        return new CriticalPathSolveResult
        {
            Tasks = tasks,
            ProjectDuration = projectDuration,
            CriticalPath = criticalPath
        };
    }

    private static void Validate(List<NetworkTask> tasks)
    {
        var ids = new HashSet<int>(tasks.Select(t => t.Id));
        foreach (NetworkTask task in tasks)
        {
            if (task.Duration < 0)
                throw new InvalidOperationException($"Робота {task.Id}: тривалість не може бути від'ємною.");
            if (task.People < 0)
                throw new InvalidOperationException($"Робота {task.Id}: кількість людей не може бути від'ємною.");
            foreach (int p in task.Predecessors)
            {
                if (!ids.Contains(p))
                    throw new InvalidOperationException($"Робота {task.Id}: невідома попередня робота {p}.");
                if (p == task.Id)
                    throw new InvalidOperationException($"Робота {task.Id}: не може залежати від себе.");
            }
        }

        if (HasCycle(tasks))
            throw new InvalidOperationException("У графіку робіт виявлено цикл.");
    }

    private static bool HasCycle(List<NetworkTask> tasks)
    {
        var state = tasks.ToDictionary(t => t.Id, _ => 0);
        foreach (NetworkTask task in tasks)
        {
            if (DfsCycle(task.Id, tasks, state))
                return true;
        }

        return false;
    }

    private static bool DfsCycle(int id, List<NetworkTask> tasks, Dictionary<int, int> state)
    {
        if (state[id] == 1) return true;
        if (state[id] == 2) return false;
        state[id] = 1;
        NetworkTask task = tasks.First(t => t.Id == id);
        foreach (int succ in task.Successors)
        {
            if (DfsCycle(succ, tasks, state))
                return true;
        }

        state[id] = 2;
        return false;
    }

    private static void BuildSuccessors(List<NetworkTask> tasks)
    {
        var map = tasks.ToDictionary(t => t.Id);
        foreach (NetworkTask task in tasks)
        {
            var succ = new List<int>();
            foreach (NetworkTask other in tasks)
            {
                if (other.Predecessors.Contains(task.Id))
                    succ.Add(other.Id);
            }

            task.Successors = succ;
        }
    }

    private static void ForwardPass(List<NetworkTask> tasks)
    {
        var order = TopologicalSort(tasks);
        foreach (int id in order)
        {
            NetworkTask task = tasks.First(t => t.Id == id);
            if (task.Predecessors.Count == 0)
                task.EarlyStart = 0;
            else
            {
                task.EarlyStart = task.Predecessors
                    .Select(p => tasks.First(t => t.Id == p).EarlyFinish)
                    .Max();
            }

            task.EarlyFinish = task.EarlyStart + task.Duration;
        }
    }

    private static void BackwardPass(List<NetworkTask> tasks, int projectDuration)
    {
        var order = TopologicalSort(tasks);
        order.Reverse();
        foreach (int id in order)
        {
            NetworkTask task = tasks.First(t => t.Id == id);
            if (task.Successors.Count == 0)
                task.LateFinish = projectDuration;
            else
            {
                task.LateFinish = task.Successors
                    .Select(s => tasks.First(t => t.Id == s).LateStart)
                    .Min();
            }

            task.LateStart = task.LateFinish - task.Duration;
            task.Reserve = task.LateFinish - task.EarlyFinish;
        }
    }

    private static void MarkCritical(List<NetworkTask> tasks)
    {
        foreach (NetworkTask task in tasks)
            task.IsCritical = task.Reserve == 0;
    }

    private static List<int> BuildCriticalPath(List<NetworkTask> tasks)
    {
        var map = tasks.ToDictionary(t => t.Id);
        List<NetworkTask> starts = tasks
            .Where(t => t.IsCritical && (t.Predecessors.Count == 0 || t.Predecessors.All(p => !map[p].IsCritical)))
            .OrderBy(t => t.EarlyStart)
            .ToList();

        if (starts.Count == 0)
            starts = tasks.Where(t => t.IsCritical).OrderBy(t => t.EarlyStart).Take(1).ToList();

        var path = new List<int> { starts[0].Id };
        int current = starts[0].Id;
        while (true)
        {
            NetworkTask task = map[current];
            List<int> nextCandidates = task.Successors.Where(s => map[s].IsCritical).OrderBy(s => map[s].EarlyStart).ToList();
            if (nextCandidates.Count == 0)
                break;

            int next = nextCandidates[0];
            if (path.Contains(next))
                break;

            path.Add(next);
            current = next;
        }

        return path;
    }

    private static List<int> TopologicalSort(List<NetworkTask> tasks)
    {
        var indegree = tasks.ToDictionary(t => t.Id, t => t.Predecessors.Count);
        var queue = new Queue<int>(indegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var order = new List<int>();
        var succMap = tasks.ToDictionary(t => t.Id, t => t.Successors.ToList());

        while (queue.Count > 0)
        {
            int id = queue.Dequeue();
            order.Add(id);
            foreach (int succ in succMap[id])
            {
                indegree[succ]--;
                if (indegree[succ] == 0)
                    queue.Enqueue(succ);
            }
        }

        if (order.Count != tasks.Count)
            throw new InvalidOperationException("У графіку робіт виявлено цикл.");

        return order;
    }
}

public sealed class NetworkTaskInput
{
    public required int Id { get; init; }
    public required IReadOnlyList<int> Predecessors { get; init; }
    public required int Duration { get; init; }
    public required int People { get; init; }
}
