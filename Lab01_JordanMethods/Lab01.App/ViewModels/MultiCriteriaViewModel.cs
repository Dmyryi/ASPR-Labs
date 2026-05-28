using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lab01.Logic.Interfaces;
using Lab01.Logic.MultiCriteria;

namespace Lab01.App.ViewModels;

public sealed class MultiCriteriaViewModel : ViewModelBase
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    private readonly MultiCriteriaSolver _solver;
    private readonly IProtocolSaver _protocolSaver;

    private string _objectivesText =
        "2x1 + 2x2 + x3 + x4 + x5 max\r\n" +
        "x1 - 3x2 + 5x3 - x4 - 2x5 min\r\n" +
        "x1 - 4x2 + 5x3 + 9x4 - 2x5 max";

    private string _constraintsText =
        "x1 + 4x2 + 3x3 + 2x4 + x5 = 9\r\n" +
        "-x1 + 2x2 - x3 + 2x4 + x5 = 6\r\n" +
        "x1 + 2x2 + 2x4 - x5 = 2";

    private string _coefficientsDisplay = string.Empty;
    private string _optimalVectorsDisplay = string.Empty;
    private string _suboptimalityDisplay = string.Empty;
    private string _gameMatrixDisplay = string.Empty;
    private string _weightsDisplay = string.Empty;
    private string _compromiseDisplay = string.Empty;
    private string _savedProtocolPath = string.Empty;

    public MultiCriteriaViewModel(MultiCriteriaSolver solver, IProtocolSaver protocolSaver)
    {
        _solver = solver;
        _protocolSaver = protocolSaver;
        SolveCommand = new RelayCommand(Solve);
        GenerateProtocolCommand = new RelayCommand(GenerateProtocol);
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
        LoadVariant10Command = new RelayCommand(LoadVariant10);
    }

    public string ObjectivesText
    {
        get => _objectivesText;
        set { _objectivesText = value; OnPropertyChanged(); }
    }

    public string ConstraintsText
    {
        get => _constraintsText;
        set { _constraintsText = value; OnPropertyChanged(); }
    }

    public string CoefficientsDisplay
    {
        get => _coefficientsDisplay;
        private set { _coefficientsDisplay = value; OnPropertyChanged(); }
    }

    public string OptimalVectorsDisplay
    {
        get => _optimalVectorsDisplay;
        private set { _optimalVectorsDisplay = value; OnPropertyChanged(); }
    }

    public string SuboptimalityDisplay
    {
        get => _suboptimalityDisplay;
        private set { _suboptimalityDisplay = value; OnPropertyChanged(); }
    }

    public string GameMatrixDisplay
    {
        get => _gameMatrixDisplay;
        private set { _gameMatrixDisplay = value; OnPropertyChanged(); }
    }

    public string WeightsDisplay
    {
        get => _weightsDisplay;
        private set { _weightsDisplay = value; OnPropertyChanged(); }
    }

    public string CompromiseDisplay
    {
        get => _compromiseDisplay;
        private set { _compromiseDisplay = value; OnPropertyChanged(); }
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

    private MultiCriteriaSolveResult? _lastResult;

    private void Solve()
    {
        try
        {
            _lastResult = _solver.Solve(ObjectivesText, ConstraintsText);
            ApplyResult(_lastResult);
        }
        catch (Exception ex)
        {
            _lastResult = null;
            ClearOutputs();
            MessageBox.Show(Application.Current?.MainWindow, ex.Message, "Багатокритеріальна оптимізація", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            string text = MultiCriteriaProtocolFormatter.Build(_lastResult);
            string dir = ProtocolSavePaths.ResolveLab01AppProjectDirectory();
            string path = Path.GetFullPath(Path.Combine(dir, $"protokol_bagatokryterialna_{DateTime.Now:yyyyMMdd_HHmmss}.txt"));
            _protocolSaver.Save(text, path);
            SavedProtocolPath = path;
            MessageBox.Show(Application.Current?.MainWindow, $"Протокол збережено:\n{path}", "Багатокритеріальна оптимізація", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Application.Current?.MainWindow, ex.Message, "Багатокритеріальна оптимізація", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void ApplyResult(MultiCriteriaSolveResult r)
    {
        int k = r.Objectives.Count;
        int n = r.CompromiseSolution.Length;

        CoefficientsDisplay = FormatMatrixRows(
            r.Objectives.Select(o => Pad(o.Coefficients, n)).ToArray());

        OptimalVectorsDisplay = FormatMatrixRows(
            r.PerObjectiveSolutions.Select(s => s.X).ToArray());

        SuboptimalityDisplay = FormatSquareMatrix(r.SuboptimalityMatrix, k);
        GameMatrixDisplay = FormatSquareMatrix(r.GameMatrix, k);
        WeightsDisplay = string.Join("; ", r.Weights.Select(v => Fmt(v)));
        CompromiseDisplay = string.Join("; ", r.CompromiseSolution.Select(v => Fmt(v)));
    }

    private void ClearOutputs()
    {
        CoefficientsDisplay = string.Empty;
        OptimalVectorsDisplay = string.Empty;
        SuboptimalityDisplay = string.Empty;
        GameMatrixDisplay = string.Empty;
        WeightsDisplay = string.Empty;
        CompromiseDisplay = string.Empty;
    }

    private void LoadExample1()
    {
        ObjectivesText =
            "2x1 + 2x2 + x3 + x4 + x5 max\r\n" +
            "x1 - 3x2 + 5x3 - x4 - 2x5 min\r\n" +
            "x1 - 4x2 + 5x3 + 9x4 - 2x5 max";
        ConstraintsText =
            "x1 + 4x2 + 3x3 + 2x4 + x5 = 9\r\n" +
            "-x1 + 2x2 - x3 + 2x4 + x5 = 6\r\n" +
            "x1 + 2x2 + 2x4 - x5 = 2";
    }

    private void LoadExample2()
    {
        ObjectivesText =
            "x1 - 8x2 + x3 + 4x4 max\r\n" +
            "-x1 + 3x2 + 5x3 + x4 min\r\n" +
            "3x1 + x2 + x3 - x4 max";
        ConstraintsText =
            "x1 - x2 + x3 + x4 <= 2\r\n" +
            "x1 + x2 + x3 - x4 <= 2\r\n" +
            "-x1 + x2 + x3 + x4 <= 2\r\n" +
            "x1 + x2 - x3 + x4 <= 2";
    }

    private void LoadVariant10()
    {
        ObjectivesText =
            "x1 + x2 + x3 + x4 + x5 max\r\n" +
            "x1 - 2x2 + x3 max\r\n" +
            "x2 - x3 min";
        ConstraintsText =
            "x1 + x2 + 2x3 = 4\r\n" +
            "2x2 + 2x3 - x4 + x5 = 6\r\n" +
            "x1 - x2 + 6x3 + x4 + x5 = 12";
    }

    private static string FormatMatrixRows(IReadOnlyList<double[]> rows)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < rows.Count; i++)
        {
            if (i > 0)
                sb.AppendLine();
            sb.Append(string.Join("  ", rows[i].Select(Fmt)));
        }

        return sb.ToString();
    }

    private static string FormatSquareMatrix(double[,] m, int size)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < size; i++)
        {
            if (i > 0)
                sb.AppendLine();
            for (int j = 0; j < size; j++)
            {
                if (j > 0)
                    sb.Append("  ");
                sb.Append(Fmt(m[i, j]));
            }
        }

        return sb.ToString();
    }

    private static double[] Pad(double[] v, int n)
    {
        if (v.Length >= n)
            return v.Take(n).ToArray();
        var p = new double[n];
        Array.Copy(v, p, v.Length);
        return p;
    }

    private static string Fmt(double v) => v.ToString("0.##", Uk);
}
