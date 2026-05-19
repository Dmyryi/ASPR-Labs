namespace Lab01.Logic.NetworkPlanning;

public sealed class NetworkSchedule
{
    private readonly Dictionary<int, int> _starts = new();

    public NetworkSchedule(CriticalPathSolveResult result)
    {
        Result = result;
        foreach (NetworkTask task in result.Tasks)
            _starts[task.Id] = task.EarlyStart;
    }

    public CriticalPathSolveResult Result { get; }

    public int GetStart(int taskId) => _starts[taskId];

    public int GetFinish(int taskId) => _starts[taskId] + Result.Tasks.First(t => t.Id == taskId).Duration;

    public void ResetToEarly()
    {
        foreach (NetworkTask task in Result.Tasks)
            _starts[task.Id] = task.EarlyStart;
    }

    public bool TryShift(int taskId, int delta)
    {
        NetworkTask task = Result.Tasks.First(t => t.Id == taskId);
        int lower = GetLowerBound(task);
        int upper = task.LateStart;
        int next = _starts[taskId] + delta;
        if (next < lower || next > upper)
            return false;
        _starts[taskId] = next;
        return true;
    }

    public void SetStart(int taskId, int start)
    {
        NetworkTask task = Result.Tasks.First(t => t.Id == taskId);
        int lower = GetLowerBound(task);
        int upper = task.LateStart;
        _starts[taskId] = Math.Clamp(start, lower, upper);
    }

    public int GetLowerBound(NetworkTask task)
    {
        int bound = task.EarlyStart;
        foreach (int predId in task.Predecessors)
        {
            int predFinish = GetFinish(predId);
            if (predFinish > bound)
                bound = predFinish;
        }

        return bound;
    }

    public void Optimize()
    {
        var ordered = Result.Tasks
            .Where(t => t.Reserve > 0)
            .OrderBy(t => t.EarlyStart)
            .ToList();

        for (int pass = 0; pass < 3; pass++)
        {
            foreach (NetworkTask task in ordered)
            {
                int lower = GetLowerBound(task);
                int upper = task.LateStart;
                int bestStart = _starts[task.Id];
                int bestPeak = int.MaxValue;

                for (int start = lower; start <= upper; start++)
                {
                    _starts[task.Id] = start;
                    int peak = ResourceLoadCalculator.ComputePeak(this);
                    if (peak < bestPeak)
                    {
                        bestPeak = peak;
                        bestStart = start;
                    }
                }

                _starts[task.Id] = bestStart;
            }
        }
    }
}
