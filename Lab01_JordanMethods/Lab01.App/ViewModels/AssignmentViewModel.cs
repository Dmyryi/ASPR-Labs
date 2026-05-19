using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lab01.Logic.Assignment;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public sealed class AssignmentViewModel : ViewModelBase
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    private readonly IProtocolSaver _protocolSaver;

    private string _costsText =
        "2 10 9 7\r\n" +
        "15 4 14 8\r\n" +
        "13 14 16 11\r\n" +
        "4 15 13 19";

    private string _assignmentDisplay = string.Empty;
    private string _costDisplay = string.Empty;
    private string _savedProtocolPath = string.Empty;

    public AssignmentViewModel(IProtocolSaver protocolSaver)
    {
        _protocolSaver = protocolSaver;
        SolveCommand = new RelayCommand(Solve);
        GenerateProtocolCommand = new RelayCommand(GenerateProtocol);
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
        LoadVariant10Command = new RelayCommand(LoadVariant10);
    }

    public string CostsText
    {
        get => _costsText;
        set { _costsText = value; OnPropertyChanged(); }
    }

    public string AssignmentDisplay
    {
        get => _assignmentDisplay;
        private set { _assignmentDisplay = value; OnPropertyChanged(); }
    }

    public string CostDisplay
    {
        get => _costDisplay;
        private set { _costDisplay = value; OnPropertyChanged(); }
    }

    public string SavedProtocolPath
    {
        get => _savedProtocolPath;
        private set { _savedProtocolPath = value; OnPropertyChanged(); }
    }

    public ICommand SolveCommand { get; }
    public ICommand GenerateProtocolCommand { get; }
    public ICommand LoadExample1Command { get; }
    public ICommand LoadExample2Command { get; }
    public ICommand LoadVariant10Command { get; }

    private AssignmentSolveResultWithTrace? _lastResult;

    private void Solve()
    {
        try
        {
            double[,] costs = ParseMatrix(CostsText);
            _lastResult = AssignmentSolver.Solve(costs);
            ApplyResult(_lastResult);
        }
        catch (Exception ex)
        {
            _lastResult = null;
            AssignmentDisplay = string.Empty;
            CostDisplay = string.Empty;
            MessageBox.Show(Application.Current?.MainWindow, ex.Message, "Задача про призначення", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void GenerateProtocol()
    {
        try
        {
            if (_lastResult is null)
                Solve();

            if (_lastResult is null)
                throw new InvalidOperationException("Спочатку виконайте розрахунок.");

            string text = AssignmentProtocolFormatter.Build(_lastResult);
            string dir = ProtocolSavePaths.ResolveLab01AppProjectDirectory();
            string path = Path.GetFullPath(Path.Combine(dir, $"protokol_pryznachennia_{DateTime.Now:yyyyMMdd_HHmmss}.txt"));
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

    private void ApplyResult(AssignmentSolveResultWithTrace r)
    {
        int n = r.Hungarian.Size;
        AssignmentDisplay = FormatAssignmentMatrix(r.Hungarian.AssignmentMatrix, n);
        CostDisplay = $"S = {r.Hungarian.TotalCost.ToString("0.##", Uk)}";
    }

    private static string FormatAssignmentMatrix(int[,] assignment, int n)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(assignment[i, j].ToString(Uk));
            }

            if (i < n - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private void LoadExample1()
    {
        CostsText =
            "2 4 1 3 3\r\n" +
            "1 5 4 1 2\r\n" +
            "3 5 2 2 4\r\n" +
            "1 4 3 1 4\r\n" +
            "3 2 5 3 5";
        Solve();
    }

    private void LoadExample2()
    {
        CostsText =
            "2 10 9 7\r\n" +
            "15 4 14 8\r\n" +
            "13 14 16 11\r\n" +
            "4 15 13 19";
        Solve();
    }

    private void LoadVariant10()
    {
        CostsText =
            "45 17 33 10\r\n" +
            "35 15 38 8\r\n" +
            "40 16 31 9\r\n" +
            "37 22 35 15";
        Solve();
    }

    private static double[,] ParseMatrix(string text)
    {
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            throw new InvalidOperationException("Введіть матрицю вартостей.");

        var rows = new List<double[]>();
        int? nCols = null;
        foreach (string line in lines)
        {
            string[] parts = line.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            if (nCols is null) nCols = parts.Length;
            else if (parts.Length != nCols)
                throw new InvalidOperationException("Матриця має бути квадратною.");

            var row = new double[parts.Length];
            for (int j = 0; j < parts.Length; j++)
            {
                if (!TryParseDouble(parts[j], out row[j]))
                    throw new InvalidOperationException($"Не вдалося розпізнати число: «{parts[j]}».");
            }

            rows.Add(row);
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("Немає даних матриці.");

        if (rows.Count != rows[0].Length)
            throw new InvalidOperationException("Матриця має бути квадратною.");

        int n = rows.Count;
        var a = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                a[i, j] = rows[i][j];
        }

        return a;
    }

    private static bool TryParseDouble(string s, out double v) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v)
        || double.TryParse(s, NumberStyles.Float, Uk, out v);
}
