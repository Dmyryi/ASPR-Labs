using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lab01.Logic.GameTheory;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public sealed class NatureGameViewModel : ViewModelBase
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    private readonly IProtocolSaver _protocolSaver;

    private string _matrixText =
        "-1  1  1  4\r\n" +
        "-1 -2  2  3\r\n" +
        " 3 -1  3  2";

    private string _gammaText = "0,3";
    private string _probabilitiesText = "0.2 0.4 0.1 0.3";

    private string _waldDisplay = string.Empty;
    private string _maximaxDisplay = string.Empty;
    private string _hurwiczDisplay = string.Empty;
    private string _savageDisplay = string.Empty;
    private string _bayesDisplay = string.Empty;
    private string _laplaceDisplay = string.Empty;
    private string _frequentDisplay = string.Empty;
    private string _regretMatrixDisplay = string.Empty;
    private string _savedProtocolPath = string.Empty;

    public NatureGameViewModel(IProtocolSaver protocolSaver)
    {
        _protocolSaver = protocolSaver;
        SolveCommand = new RelayCommand(Solve);
        GenerateProtocolCommand = new RelayCommand(GenerateProtocol);
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
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

    public string GammaText
    {
        get => _gammaText;
        set
        {
            _gammaText = value;
            OnPropertyChanged();
        }
    }

    public string ProbabilitiesText
    {
        get => _probabilitiesText;
        set
        {
            _probabilitiesText = value;
            OnPropertyChanged();
        }
    }

    public string WaldDisplay
    {
        get => _waldDisplay;
        private set
        {
            _waldDisplay = value;
            OnPropertyChanged();
        }
    }

    public string MaximaxDisplay
    {
        get => _maximaxDisplay;
        private set
        {
            _maximaxDisplay = value;
            OnPropertyChanged();
        }
    }

    public string HurwiczDisplay
    {
        get => _hurwiczDisplay;
        private set
        {
            _hurwiczDisplay = value;
            OnPropertyChanged();
        }
    }

    public string SavageDisplay
    {
        get => _savageDisplay;
        private set
        {
            _savageDisplay = value;
            OnPropertyChanged();
        }
    }

    public string BayesDisplay
    {
        get => _bayesDisplay;
        private set
        {
            _bayesDisplay = value;
            OnPropertyChanged();
        }
    }

    public string LaplaceDisplay
    {
        get => _laplaceDisplay;
        private set
        {
            _laplaceDisplay = value;
            OnPropertyChanged();
        }
    }

    public string FrequentDisplay
    {
        get => _frequentDisplay;
        private set
        {
            _frequentDisplay = value;
            OnPropertyChanged();
        }
    }

    public string RegretMatrixDisplay
    {
        get => _regretMatrixDisplay;
        private set
        {
            _regretMatrixDisplay = value;
            OnPropertyChanged();
        }
    }

    public string SavedProtocolPath
    {
        get => _savedProtocolPath;
        private set
        {
            _savedProtocolPath = value;
            OnPropertyChanged();
        }
    }

    public ICommand SolveCommand { get; }
    public ICommand GenerateProtocolCommand { get; }
    public ICommand LoadExample1Command { get; }
    public ICommand LoadExample2Command { get; }

    private void GenerateProtocol()
    {
        try
        {
            double[,] u = ParseMatrix(MatrixText);
            int cols = u.GetLength(1);

            if (!TryParseGamma(GammaText.Trim(), out double gamma))
                throw new InvalidOperationException("Введіть коефіцієнт γ для критерію Гурвіца (число від 0 до 1).");

            double[] p = ParseProbabilities(ProbabilitiesText, cols);
            string text = NatureGameProtocolFormatter.Build(u, gamma, p);

            string dir = ResolveLab01AppProjectDirectory();
            string name = $"protokol_hra_z_prirodoyu.txt";
            string fullPath = Path.GetFullPath(Path.Combine(dir, name));

            _protocolSaver.Save(text, fullPath);
            SavedProtocolPath = fullPath;

            MessageBox.Show(
                Application.Current?.MainWindow,
                "Протокол збережено у файл:\r\n" + fullPath,
                "Протокол",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            SavedProtocolPath = string.Empty;
            MessageBox.Show(
                Application.Current?.MainWindow,
                ex.Message,
                "Протокол",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private static string ResolveLab01AppProjectDirectory()
    {
        string start = AppContext.BaseDirectory;
        for (var di = new DirectoryInfo(start); di != null; di = di.Parent)
        {
            if (File.Exists(Path.Combine(di.FullName, "Lab01.App.csproj")))
                return di.FullName;
        }

        return start;
    }

    private void Solve()
    {
        try
        {
            double[,] u = ParseMatrix(MatrixText);
            int cols = u.GetLength(1);

            if (!TryParseGamma(GammaText.Trim(), out double gamma))
                throw new InvalidOperationException("Введіть коефіцієнт γ для критерію Гурвіца (число від 0 до 1).");

            double[] p = ParseProbabilities(ProbabilitiesText, cols);

            NatureGameSolveResult r = NatureGameSolver.Solve(u, gamma, p);

            WaldDisplay = FormatRows(r.WaldRows);
            MaximaxDisplay = FormatRows(r.MaximaxRows);
            HurwiczDisplay = FormatRows(r.HurwiczRows);
            SavageDisplay = FormatRows(r.SavageRows);
            BayesDisplay = FormatRows(r.BayesRows);
            LaplaceDisplay = FormatRows(r.LaplaceRows);
            FrequentDisplay = FormatRows(r.MostFrequentRows);
            RegretMatrixDisplay = FormatMatrix(r.SavageRegretMatrix);
        }
        catch (Exception ex)
        {
            ClearAllDisplays();
            MessageBox.Show(
                Application.Current?.MainWindow,
                ex.Message,
                "Ігри з природою",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void ClearAllDisplays()
    {
        WaldDisplay = string.Empty;
        MaximaxDisplay = string.Empty;
        HurwiczDisplay = string.Empty;
        SavageDisplay = string.Empty;
        BayesDisplay = string.Empty;
        LaplaceDisplay = string.Empty;
        FrequentDisplay = string.Empty;
        RegretMatrixDisplay = string.Empty;
    }

    private void LoadExample1()
    {
        MatrixText =
            "-1  1  1  4\r\n" +
            "-1 -2  2  3\r\n" +
            " 3 -1  3  2";
        GammaText = "0,3";
        ProbabilitiesText = "0.2 0.4 0.1 0.3";
        Solve();
    }

    private void LoadExample2()
    {
        MatrixText =
            " 2 -1  3  4\r\n" +
            "-1  2  3  7\r\n" +
            " 5  4  6  2";
        GammaText = "0,4";
        ProbabilitiesText = "0.4 0.1 0.2 0.3";
        Solve();
    }

    private static string FormatRows(IReadOnlyList<int> rowsZeroBased)
    {
        if (rowsZeroBased.Count == 0)
            return "—";
        var labels = rowsZeroBased.Distinct().OrderBy(i => i).Select(i => $"A{i + 1}").ToArray();
        return string.Join(" або ", labels);
    }

    private static string FormatMatrix(double[,] m)
    {
        int r = m.GetLength(0);
        int c = m.GetLength(1);
        var sb = new StringBuilder();
        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                if (j > 0) sb.Append('\t');
                sb.Append(m[i, j].ToString("G6", CultureInfo.InvariantCulture));
            }

            if (i < r - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static bool TryParseGamma(string s, out double gamma)
    {
        gamma = 0;
        if (!TryParseDouble(s, out gamma))
            return false;
        return gamma is >= 0 and <= 1;
    }

    private static double[] ParseProbabilities(string text, int expectedCount)
    {
        string[] parts = text.Split(new[] { ' ', '\t', ';' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != expectedCount)
            throw new InvalidOperationException(
                $"Очікується {expectedCount} ймовірностей (по одній на кожен стовпець), отримано {parts.Length}.");

        var p = new double[expectedCount];
        for (int j = 0; j < expectedCount; j++)
        {
            if (!TryParseDouble(parts[j], out p[j]))
                throw new InvalidOperationException($"Не вдалося розпізнати ймовірність: «{parts[j]}».");
        }

        return p;
    }

    private static bool TryParseDouble(string s, out double v) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v)
        || double.TryParse(s, NumberStyles.Float, Uk, out v);

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
                if (!TryParseDouble(parts[j], out row[j]))
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
}
