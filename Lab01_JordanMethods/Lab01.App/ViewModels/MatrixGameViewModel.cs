using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lab01.App;
using Lab01.Logic.GameTheory;

namespace Lab01.App.ViewModels;

public sealed class MatrixGameViewModel : ViewModelBase
{
    private const int MaxSimulationRounds = 250_000;

    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    private readonly MatrixGameSolver _solver;

    private string _matrixText =
        "5 2 7\r\n" +
        "1 4 3\r\n" +
        "6 1 5";

    private string _roundsText = "50";

    private string _rowStrategyDisplay = string.Empty;
    private string _columnStrategyDisplay = string.Empty;
    private string _gameValueDisplay = string.Empty;
    private string _detailsText = string.Empty;

    private GameTheorySolveResult? _lastSolve;

    public MatrixGameViewModel(MatrixGameSolver solver)
    {
        _solver = solver;
        SolveCommand = new RelayCommand(Solve);
        SimulateCommand = new RelayCommand(Simulate);
        LoadExampleCommand = new RelayCommand(LoadTextbookExample);
        LoadVariant1Command = new RelayCommand(LoadVariant1);
    }

    public string MatrixText
    {
        get => _matrixText;
        set
        {
            _matrixText = value;
            OnPropertyChanged();
        }
    }

    public string RoundsText
    {
        get => _roundsText;
        set
        {
            _roundsText = value;
            OnPropertyChanged();
        }
    }

    public string RowStrategyDisplay
    {
        get => _rowStrategyDisplay;
        private set
        {
            _rowStrategyDisplay = value;
            OnPropertyChanged();
        }
    }

    public string ColumnStrategyDisplay
    {
        get => _columnStrategyDisplay;
        private set
        {
            _columnStrategyDisplay = value;
            OnPropertyChanged();
        }
    }

    public string GameValueDisplay
    {
        get => _gameValueDisplay;
        private set
        {
            _gameValueDisplay = value;
            OnPropertyChanged();
        }
    }

    public string DetailsText
    {
        get => _detailsText;
        private set
        {
            _detailsText = value;
            OnPropertyChanged();
        }
    }

    public ICommand SolveCommand { get; }
    public ICommand SimulateCommand { get; }
    public ICommand LoadExampleCommand { get; }
    public ICommand LoadVariant1Command { get; }

    private void ClearMainOutputs()
    {
        RowStrategyDisplay = string.Empty;
        ColumnStrategyDisplay = string.Empty;
        GameValueDisplay = string.Empty;
    }

    private void Solve()
    {
        try
        {
            double[,] a = ParseMatrix(MatrixText);
            _lastSolve = _solver.Solve(a);
            ApplySolveToDisplays(_lastSolve);
            DetailsText = FormatSolveDetails(_lastSolve);
        }
        catch (Exception ex)
        {
            _lastSolve = null;
            ClearMainOutputs();
            DetailsText = "Помилка: " + ex.Message;
        }
    }

    private void Simulate()
    {
        if (_lastSolve is null)
        {
            MessageBox.Show(
                Application.Current.MainWindow,
                "Спочатку натисніть «Знайти розв’язок гри».",
                "Моделювання",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        try
        {
            if (!int.TryParse(RoundsText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int rounds) ||
                rounds < 1)
            {
                MessageBox.Show(
                    Application.Current.MainWindow,
                    "Введіть коректну кількість партій (ціле число ≥ 1).",
                    "Моделювання",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (rounds > MaxSimulationRounds)
            {
                MessageBox.Show(
                    Application.Current.MainWindow,
                    $"Кількість партій не більше {MaxSimulationRounds:N0}.",
                    "Моделювання",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            double[,] a = ParseMatrix(MatrixText);
            MatrixGameSimulationResult sim = MatrixGameSimulator.Simulate(
                a,
                _lastSolve.RowPlayerStrategy,
                _lastSolve.ColumnPlayerStrategy,
                rounds,
                _lastSolve.GameValue,
                seed: null,
                maxProtocolRows: rounds);

            var simVm = MatrixGameSimulationViewModel.Create(sim, _lastSolve, rounds);
            var win = new MatrixGameSimulationWindow { DataContext = simVm };
            if (Application.Current.MainWindow != null)
                win.Owner = Application.Current.MainWindow;
            win.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                Application.Current.MainWindow,
                ex.Message,
                "Моделювання",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void ApplySolveToDisplays(GameTheorySolveResult r)
    {
        RowStrategyDisplay = FormatProbVectorSemicolon(r.RowPlayerStrategy);
        ColumnStrategyDisplay = FormatProbVectorSemicolon(r.ColumnPlayerStrategy);
        GameValueDisplay = r.GameValue.ToString("N2", Uk);
    }

    private static string FormatProbVectorSemicolon(IReadOnlyList<double> v)
    {
        var parts = new string[v.Count];
        for (int i = 0; i < v.Count; i++)
            parts[i] = v[i].ToString("N2", Uk);
        return string.Join("; ", parts);
    }

    private void LoadTextbookExample()
    {
        MatrixText =
            "5 2 7\r\n" +
            "1 4 3\r\n" +
            "6 1 5";
        Solve();
    }

    private void LoadVariant1()
    {
        MatrixText =
            "-1 3 -2\r\n" +
            "3 -1 3\r\n" +
            "1 2 -3";
        Solve();
    }

    private static double[,] ParseMatrix(string text)
    {
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            throw new InvalidOperationException("Введіть хоча б один рядок матриці.");

        var rows = new List<double[]>();
        int? nCols = null;
        foreach (string line in lines)
        {
            string[] parts = line.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            if (nCols is null) nCols = parts.Length;
            else if (parts.Length != nCols)
                throw new InvalidOperationException("Усі рядки матриці повинні мати однакову кількість чисел.");

            var row = new double[parts.Length];
            for (int j = 0; j < parts.Length; j++)
            {
                if (!double.TryParse(parts[j], NumberStyles.Float, CultureInfo.InvariantCulture, out row[j]))
                    throw new InvalidOperationException($"Не вдалося розпізнати число: «{parts[j]}».");
            }

            rows.Add(row);
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("Немає даних матриці.");

        int m = rows.Count;
        int n = rows[0].Length;
        var a = new double[m, n];
        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
                a[i, j] = rows[i][j];
        }

        return a;
    }

    private static string FormatSolveDetails(GameTheorySolveResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine(r.SolutionKind);
        if (r.EliminatedDominatedStrategies && !string.IsNullOrWhiteSpace(r.DominanceReductionLog))
        {
            sb.AppendLine();
            sb.AppendLine("Журнал вилучення домінованих стратегій:");
            sb.AppendLine(r.DominanceReductionLog);
        }

        if (r.HasSaddlePoint && r.SaddleRow is not null && r.SaddleColumn is not null)
        {
            sb.AppendLine();
            sb.AppendLine(
                $"Сідлова точка: рядок {r.SaddleRow.Value + 1}, стовпець {r.SaddleColumn.Value + 1} (індексація з 1).");
        }

        if (r.LpShift is not null)
        {
            sb.AppendLine();
            sb.AppendLine(
                $"Зсув для ЗЛП: {r.LpShift.Value.ToString("G6", CultureInfo.InvariantCulture)}; max Σx = {r.LpObjectiveMaxSumX?.ToString("G8", CultureInfo.InvariantCulture)}");
        }

        return sb.ToString().TrimEnd();
    }
}
