using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Lab01.Logic.Interfaces;
using Lab01.Logic.NetworkPlanning;

namespace Lab01.App.ViewModels;

public sealed class NetworkPlanningViewModel : ViewModelBase
{
    private readonly IProtocolSaver _protocolSaver;

    private string _criticalPathDisplay = string.Empty;
    private string _durationDisplay = string.Empty;
    private string _savedProtocolPath = string.Empty;
    private string _scheduleHint = string.Empty;
    private NetworkTaskOptionViewModel? _selectedTask;
    private NetworkSchedule? _schedule;

    public NetworkPlanningViewModel(IProtocolSaver protocolSaver)
    {
        _protocolSaver = protocolSaver;
        Tasks = new ObservableCollection<NetworkTaskRowViewModel>();
        TaskOptions = new ObservableCollection<NetworkTaskOptionViewModel>();

        SolveCommand = new RelayCommand(Solve);
        GenerateProtocolCommand = new RelayCommand(GenerateProtocol);
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
        LoadVariant10Command = new RelayCommand(LoadVariant10);
        SelectPreviousTaskCommand = new RelayCommand(SelectPreviousTask);
        SelectNextTaskCommand = new RelayCommand(SelectNextTask);
        ShiftEarlierCommand = new RelayCommand(() => ShiftSelected(-1), () => CanShiftSelected);
        ShiftLaterCommand = new RelayCommand(() => ShiftSelected(1), () => CanShiftSelected);

        LoadVariant10();
    }

    public ObservableCollection<NetworkTaskRowViewModel> Tasks { get; }
    public ObservableCollection<NetworkTaskOptionViewModel> TaskOptions { get; }

    public NetworkTaskOptionViewModel? SelectedTask
    {
        get => _selectedTask;
        set
        {
            _selectedTask = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanShiftSelected));
            UpdateScheduleHint();
            NotifyChartChanged();
        }
    }

    public string ScheduleHint
    {
        get => _scheduleHint;
        private set { _scheduleHint = value; OnPropertyChanged(); }
    }

    public string CriticalPathDisplay
    {
        get => _criticalPathDisplay;
        private set { _criticalPathDisplay = value; OnPropertyChanged(); }
    }

    public string DurationDisplay
    {
        get => _durationDisplay;
        private set { _durationDisplay = value; OnPropertyChanged(); }
    }

    public string SavedProtocolPath
    {
        get => _savedProtocolPath;
        private set { _savedProtocolPath = value; OnPropertyChanged(); }
    }

    public CriticalPathSolveResult? LastResult { get; private set; }
    public NetworkSchedule? Schedule => _schedule;

    public bool CanShiftSelected =>
        _schedule is not null &&
        SelectedTask is not null &&
        _schedule.Result.Tasks.First(t => t.Id == SelectedTask.TaskId).Reserve > 0;

    public ICommand SolveCommand { get; }
    public ICommand GenerateProtocolCommand { get; }
    public ICommand LoadExample1Command { get; }
    public ICommand LoadExample2Command { get; }
    public ICommand LoadVariant10Command { get; }
    public ICommand SelectPreviousTaskCommand { get; }
    public ICommand SelectNextTaskCommand { get; }
    public ICommand ShiftEarlierCommand { get; }
    public ICommand ShiftLaterCommand { get; }

    public event EventHandler? ChartChanged;

    private void Solve()
    {
        try
        {
            var inputs = Tasks.Select(ToInput).ToList();
            LastResult = CriticalPathSolver.Solve(inputs);
            _schedule = new NetworkSchedule(LastResult);
            ApplyResult(LastResult);
            RebuildTaskOptions();
            OnPropertyChanged(nameof(LastResult));
            OnPropertyChanged(nameof(Schedule));
            OnPropertyChanged(nameof(CanShiftSelected));
            NotifyChartChanged();
        }
        catch (Exception ex)
        {
            LastResult = null;
            _schedule = null;
            TaskOptions.Clear();
            SelectedTask = null;
            OnPropertyChanged(nameof(LastResult));
            OnPropertyChanged(nameof(Schedule));
            CriticalPathDisplay = string.Empty;
            DurationDisplay = string.Empty;
            ScheduleHint = string.Empty;
            MessageBox.Show(Application.Current?.MainWindow, ex.Message, "Сіткове планування", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RebuildTaskOptions()
    {
        TaskOptions.Clear();
        if (LastResult is null) return;

        foreach (NetworkTask task in LastResult.Tasks.OrderBy(t => t.Id))
        {
            TaskOptions.Add(new NetworkTaskOptionViewModel
            {
                TaskId = task.Id,
                Title = $"Робота {task.Id}"
            });
        }

        SelectedTask = TaskOptions.FirstOrDefault();
    }

    private void SelectPreviousTask()
    {
        if (SelectedTask is null || TaskOptions.Count == 0) return;
        int idx = TaskOptions.IndexOf(SelectedTask);
        SelectedTask = TaskOptions[idx <= 0 ? TaskOptions.Count - 1 : idx - 1];
    }

    private void SelectNextTask()
    {
        if (SelectedTask is null || TaskOptions.Count == 0) return;
        int idx = TaskOptions.IndexOf(SelectedTask);
        SelectedTask = TaskOptions[(idx + 1) % TaskOptions.Count];
    }

    private void ShiftSelected(int delta)
    {
        if (_schedule is null || SelectedTask is null) return;
        if (_schedule.TryShift(SelectedTask.TaskId, delta))
        {
            UpdateScheduleHint();
            NotifyChartChanged();
        }
    }

    private void UpdateScheduleHint()
    {
        if (_schedule is null || SelectedTask is null)
        {
            ScheduleHint = string.Empty;
            return;
        }

        NetworkTask task = _schedule.Result.Tasks.First(t => t.Id == SelectedTask.TaskId);
        int start = _schedule.GetStart(task.Id);
        int lower = _schedule.GetLowerBound(task);
        ScheduleHint = task.Reserve == 0
            ? $"Робота {task.Id} — критична, зсув неможливий."
            : $"Робота {task.Id}: старт {start} (допустимо {lower}…{task.LateStart}), резерв {task.Reserve}";
    }

    private void NotifyChartChanged()
    {
        OnPropertyChanged(nameof(CanShiftSelected));
        ChartChanged?.Invoke(this, EventArgs.Empty);
        CommandManager.InvalidateRequerySuggested();
    }

    private void GenerateProtocol()
    {
        try
        {
            if (LastResult is null)
                Solve();

            if (LastResult is null)
                throw new InvalidOperationException("Спочатку виконайте розрахунок.");

            string text = CriticalPathProtocolFormatter.Build(LastResult);
            string dir = ProtocolSavePaths.ResolveLab01AppProjectDirectory();
            string path = Path.GetFullPath(Path.Combine(dir, $"protokol_krytychnyi_shliakh_{DateTime.Now:yyyyMMdd_HHmmss}.txt"));
            _protocolSaver.Save(text, path);
            SavedProtocolPath = path;

            MessageBox.Show(
                Application.Current?.MainWindow,
                "Протокол збережено у файл:\r\n" + path,
                "Протокол",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            SavedProtocolPath = string.Empty;
            MessageBox.Show(Application.Current?.MainWindow, ex.Message, "Протокол", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ApplyResult(CriticalPathSolveResult r)
    {
        var uk = CultureInfo.GetCultureInfo("uk-UA");
        CriticalPathDisplay = string.Join("-", r.CriticalPath);
        DurationDisplay = r.ProjectDuration.ToString(uk);
    }

    private void LoadExample1()
    {
        SetTasks(
            Row(1, "-", 5, 2),
            Row(2, "1", 8, 3),
            Row(3, "1", 3, 2),
            Row(4, "1", 6, 2),
            Row(5, "2", 7, 3),
            Row(6, "2,3", 6, 2),
            Row(7, "4,5,6", 4, 2));
        Solve();
    }

    private void LoadExample2()
    {
        SetTasks(
            Row(1, "-", 3, 2),
            Row(2, "1", 4, 3),
            Row(3, "1", 2, 4),
            Row(4, "2", 5, 3),
            Row(5, "3", 1, 2),
            Row(6, "3", 2, 3),
            Row(7, "4,5", 4, 2),
            Row(8, "6,7", 3, 2));
        Solve();
    }

    private void LoadVariant10()
    {
        SetTasks(
            Row(1, "-", 10, 3),
            Row(2, "-", 12, 4),
            Row(3, "-", 7, 2),
            Row(4, "3", 10, 3),
            Row(5, "3", 15, 6),
            Row(6, "1,2,4", 5, 1),
            Row(7, "2,4", 13, 3),
            Row(8, "1,2,4", 12, 4),
            Row(9, "3", 11, 5),
            Row(10, "5,6,7", 10, 6),
            Row(11, "9", 8, 4));
        Solve();
    }

    private void SetTasks(params NetworkTaskRowViewModel[] rows)
    {
        Tasks.Clear();
        foreach (NetworkTaskRowViewModel row in rows)
            Tasks.Add(row);
    }

    private static NetworkTaskRowViewModel Row(int id, string preds, int duration, int people) =>
        new()
        {
            Id = id,
            PredecessorsText = preds,
            Duration = duration,
            People = people
        };

    private static NetworkTaskInput ToInput(NetworkTaskRowViewModel row) =>
        new()
        {
            Id = row.Id,
            Predecessors = ParsePreds(row.PredecessorsText),
            Duration = row.Duration,
            People = row.People
        };

    private static IReadOnlyList<int> ParsePreds(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim() == "-")
            return Array.Empty<int>();

        return text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(int.Parse)
            .ToArray();
    }
}
