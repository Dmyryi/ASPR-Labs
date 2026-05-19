using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Transportation;

namespace Lab01.App.ViewModels;

public sealed class TransportationViewModel : ViewModelBase
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    private readonly IProtocolSaver _protocolSaver;

    private string _costsText =
        "6 3 2\r\n" +
        "2 1 5\r\n" +
        "3 4 1";

    private string _supplyText = "30 20 50";
    private string _demandText = "10 65 25";

    private string _balanceInfo = string.Empty;
    private string _northwestDisplay = string.Empty;
    private string _minimumElementDisplay = string.Empty;
    private string _optimalDisplay = string.Empty;
    private string _savedProtocolPath = string.Empty;

    public TransportationViewModel(IProtocolSaver protocolSaver)
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

    public string SupplyText
    {
        get => _supplyText;
        set { _supplyText = value; OnPropertyChanged(); }
    }

    public string DemandText
    {
        get => _demandText;
        set { _demandText = value; OnPropertyChanged(); }
    }

    public string BalanceInfo
    {
        get => _balanceInfo;
        private set { _balanceInfo = value; OnPropertyChanged(); }
    }

    public string NorthwestDisplay
    {
        get => _northwestDisplay;
        private set { _northwestDisplay = value; OnPropertyChanged(); }
    }

    public string MinimumElementDisplay
    {
        get => _minimumElementDisplay;
        private set { _minimumElementDisplay = value; OnPropertyChanged(); }
    }

    public string OptimalDisplay
    {
        get => _optimalDisplay;
        private set { _optimalDisplay = value; OnPropertyChanged(); }
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

    private TransportationSolveResult? _lastResult;

    private void Solve()
    {
        try
        {
            double[,] costs = ParseMatrix(CostsText);
            double[] supply = ParseVector(SupplyText, costs.GetLength(0), "запасів");
            double[] demand = ParseVector(DemandText, costs.GetLength(1), "заявок");

            _lastResult = TransportationSolver.Solve(costs, supply, demand);
            ApplyResult(_lastResult);
        }
        catch (Exception ex)
        {
            ClearOutputs();
            MessageBox.Show(Application.Current?.MainWindow, ex.Message, "Транспортна задача", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            string text = TransportationProtocolFormatter.Build(_lastResult);
            string dir = ProtocolSavePaths.ResolveLab01AppProjectDirectory();
            string path = Path.GetFullPath(Path.Combine(dir, $"protokol_transport_{DateTime.Now:yyyyMMdd_HHmmss}.txt"));
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

    private void ApplyResult(TransportationSolveResult r)
    {
        BalanceInfo = r.WasOpen
            ? (r.BalanceNote ?? "Відкрита задача — додано фіктивний пункт.")
            : "Закрита задача.";

        NorthwestDisplay = FormatPlanSummary(r.NorthwestCornerPlan, r.Problem);
        MinimumElementDisplay = FormatPlanSummary(r.MinimumElementPlan, r.Problem);
        OptimalDisplay = FormatPlanSummary(r.OptimalPlan, r.Problem);
    }

    private static string FormatPlanSummary(TransportationPlan plan, TransportationProblem problem)
    {
        var sb = new StringBuilder();
        sb.AppendLine(FormatPlanMatrix(plan.Allocations, problem.Rows, problem.Cols));
        sb.Append($"S = {plan.TotalCost.ToString("0.##", Uk)}");
        return sb.ToString();
    }

    private static string FormatPlanMatrix(double[,] plan, int rows, int cols)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (j > 0) sb.Append(' ');
                double v = plan[i, j];
                sb.Append(v > 1e-9 ? v.ToString("0.##", Uk) : ".");
            }

            if (i < rows - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private void ClearOutputs()
    {
        _lastResult = null;
        BalanceInfo = string.Empty;
        NorthwestDisplay = string.Empty;
        MinimumElementDisplay = string.Empty;
        OptimalDisplay = string.Empty;
    }

    private void LoadExample1()
    {
        CostsText = "6 3 2\r\n2 1 5\r\n3 4 1";
        SupplyText = "30 20 50";
        DemandText = "10 65 25";
        Solve();
    }

    private void LoadExample2()
    {
        CostsText = "7 6 4\r\n3 8 5\r\n2 3 7";
        SupplyText = "120 100 80";
        DemandText = "90 90 120";
        Solve();
    }

    private void LoadVariant10()
    {
        CostsText =
            "10 9 7 10\r\n" +
            "5 8 6 11\r\n" +
            "11 9 7 9";
        SupplyText = "40 45 25";
        DemandText = "25 10 35 40";
        Solve();
    }

    private static double[] ParseVector(string text, int expectedCount, string label)
    {
        string[] parts = text.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != expectedCount)
            throw new InvalidOperationException($"Очікується {expectedCount} значень для {label}, отримано {parts.Length}.");

        var v = new double[expectedCount];
        for (int i = 0; i < expectedCount; i++)
        {
            if (!TryParseDouble(parts[i], out v[i]))
                throw new InvalidOperationException($"Не вдалося розпізнати число: «{parts[i]}».");
        }

        return v;
    }

    private static double[,] ParseMatrix(string text)
    {
        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            throw new InvalidOperationException("Введіть матрицю SP.");

        var rows = new List<double[]>();
        int? nCols = null;
        foreach (string line in lines)
        {
            string[] parts = line.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;
            if (nCols is null) nCols = parts.Length;
            else if (parts.Length != nCols)
                throw new InvalidOperationException("Усі рядки SP повинні мати однакову кількість чисел.");

            var row = new double[parts.Length];
            for (int j = 0; j < parts.Length; j++)
            {
                if (!TryParseDouble(parts[j], out row[j]))
                    throw new InvalidOperationException($"Не вдалося розпізнати число: «{parts[j]}».");
            }

            rows.Add(row);
        }

        if (rows.Count == 0)
            throw new InvalidOperationException("Немає даних матриці SP.");

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

    private static bool TryParseDouble(string s, out double v) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v)
        || double.TryParse(s, NumberStyles.Float, Uk, out v);
}
