using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Lab01.Logic;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.App.ViewModels;

public class SimplexViewModel : ViewModelBase
{
    private readonly IJordan _jordan;
    private readonly IProtocolSaver _protocolSaver;
    private string _objectiveText = "x1 + 2x2 - x3 - x4";
    private string _constraintsText = "x1 + x2 - x3 - 2x4 <= 6\nx1 + x2 + x3 - x4 >= 5\n2x1 - x2 + 3x3 + 4x4 <= 10";
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private string? _lastProtocol;
    private bool _useZeroRowElimination = true;
    private const string Example1Objective = "10x1 - x2 - 42x3 - 52x4";
    private const string Example1Constraints =
        "-2x1 + x2 + x3 + 3x4 = 2\n" +
        "-3x1 + 2x2 - 3x3 = 7\n" +
        "-3x1 + x2 + 4x3 + x4 <= 1\n" +
        "-3x1 + 2x2 - 2x3 + 2x4 >= 9";
    private const string Example2Objective = "-3x1 + 6x2";
    private const string Example2Constraints =
        "x1 + 2x2 + 1 >= 0\n" +
        "2x1 + x2 - 4 >= 0\n" +
        "x1 - x2 + 1 >= 0\n" +
        "x1 - 4x2 + 13 >= 0\n" +
        "-4x1 + x2 + 23 >= 0";
    private readonly IFindPivot _findPivot;

    public SimplexViewModel(IJordan jordan, IProtocolSaver protocolSaver)
    {
        _jordan = jordan;
        _protocolSaver = protocolSaver;
        SolveMaxCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Maximization));
        SolveMinCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Minimization));
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
        SaveProtocolCommand = new RelayCommand(SaveProtocol);
        _findPivot = new OptimalSolutionFinderBase();
    }

    public ICommand SolveMaxCommand { get; }
    public ICommand SolveMinCommand { get; }
    public ICommand LoadExample1Command { get; }
    public ICommand LoadExample2Command { get; }
    public ICommand SaveProtocolCommand { get; }

    public bool UseZeroRowElimination
    {
        get => _useZeroRowElimination;
        set
        {
            _useZeroRowElimination = value;
            OnPropertyChanged();
        }
    }

    public string ObjectiveText
    {
        get => _objectiveText;
        set
        {
            _objectiveText = value;
            OnPropertyChanged();
        }
    }

    public string ConstraintsText
    {
        get => _constraintsText;
        set
        {
            _constraintsText = value;
            OnPropertyChanged();
        }
    }

    public string ResultText
    {
        get => _resultText;
        set
        {
            _resultText = value;
            OnPropertyChanged();
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    private async void SolveAsync(OptimizationMode mode)
    {
        static void ApplyUi(SimplexViewModel vm, SolverResult result, SimplexProtocol protocol, OptimizationMode m)
        {
            if (!result.Success)
            {
                vm.Status = "Помилка.";
                vm.ResultText = string.IsNullOrWhiteSpace(result.Message)
                    ? "Помилка: обчислення не вдалося."
                    : "Помилка:\n\n" + result.Message;
                protocol.LogResult(result);
                vm._lastProtocol = protocol.GetText();
                return;
            }

            vm.ResultText = FormatResult(result);
            protocol.LogResult(result);
            vm._lastProtocol = protocol.GetText();
            vm.Status = m == OptimizationMode.Maximization
                ? "Maximization solved."
                : "Minimization solved.";
        }

        void ApplyError(Exception ex)
        {
            string detail = FormatExceptionDetail(ex);
            Status = "Помилка.";
            ResultText = "Помилка обчислення:\n\n" + detail;
            _lastProtocol = null;
        }

        try
        {
            Status = "Обчислення… (інтерфейс не блокується)";
            Dispatcher? ui = Application.Current?.Dispatcher;

            // Знімаємо знімок полів із UI перед фоном, щоб не змішувати з середнім редагуванням тексту користувачем.
            string objectiveSnapshot = ObjectiveText;
            string constraintsSnapshot = ConstraintsText;
            bool zeroElimSnapshot = _useZeroRowElimination;

            // Важкий симплекс офф-потоку інакше вікно WPF «вішається» під час довгої фази 0-рядків або оптимуму.
            OptimizationMode modeSnap = mode;
            SimplexSolvePayload payload = await Task.Run(() =>
            {
                var objective = ParseLinearExpression(objectiveSnapshot);
                var parsedConstraints = ParseConstraints(constraintsSnapshot);
                if (parsedConstraints.Count == 0)
                    throw new InvalidOperationException("Add at least one constraint.");

                int variablesCount = Math.Max(
                    objective.Keys.DefaultIfEmpty(0).Max(),
                    parsedConstraints.SelectMany(c => c.Coefficients.Keys).DefaultIfEmpty(0).Max());

                if (variablesCount <= 0)
                    throw new InvalidOperationException("Cannot detect variables. Use x1, x2, ... format.");

                double[] vectorZ = BuildObjectiveVector(objective, variablesCount, modeSnap);
                double[,] matrixA = new double[parsedConstraints.Count, variablesCount];
                double[] vectorB = new double[parsedConstraints.Count];

                for (int i = 0; i < parsedConstraints.Count; i++)
                {
                    var constraint = parsedConstraints[i];
                    int sign = constraint.Operator == ">=" ? -1 : 1;

                    foreach (var (index, value) in constraint.Coefficients)
                    {
                        matrixA[i, index - 1] = sign * value;
                    }

                    vectorB[i] = sign * constraint.RightSide;
                }

                var protocol = new SimplexProtocol();
                protocol.Start(modeSnap, objectiveSnapshot, constraintsSnapshot);

                var basicFinder = new BasicSolutionFinder(_jordan, protocol);
                var optimalFinder = new OptimalSolutionFinder(_jordan, _findPivot, modeSnap, protocol);
                var zeroRowEliminator = new ZeroRowElliminator(_jordan, _findPivot, protocol);
                ILinearSolver solver = modeSnap == OptimizationMode.Maximization
                    ? new MaximizationSolver(basicFinder, optimalFinder, protocol, zeroRowEliminator,
                        zeroElimSnapshot)
                    : new MinimizationSolver(basicFinder, optimalFinder, protocol);

                var context = new SimplexContext();
                context.SetStrategy(solver);
                SolverResult r = context.ExecuteStrategy(vectorZ, matrixA, vectorB);
                return new SimplexSolvePayload(r, protocol);
            }).ConfigureAwait(false);

            if (ui is not null)
            {
                await ui.InvokeAsync(() => ApplyUi(this, payload.Result, payload.Protocol, modeSnap));
            }
            else
            {
                ApplyUi(this, payload.Result, payload.Protocol, modeSnap);
            }
        }
        catch (Exception ex)
        {
            Dispatcher? ui = Application.Current?.Dispatcher;
            if (ui is not null)
                await ui.InvokeAsync(() => ApplyError(ex));
            else
                ApplyError(ex);
        }
    }

    private static string FormatExceptionDetail(Exception ex)
    {
        var sb = new StringBuilder();
        int depth = 0;
        for (Exception? e = ex; e != null; e = e.InnerException)
        {
            if (depth > 0)
                sb.AppendLine().AppendLine("— — —").AppendLine();
            sb.Append(e.Message);
            depth++;
        }

        return sb.Length > 0 ? sb.ToString() : ex.ToString();
    }

    private sealed record SimplexSolvePayload(SolverResult Result, SimplexProtocol Protocol);

    private void SaveProtocol()
    {
        var content = string.IsNullOrEmpty(_lastProtocol)
            ? "=== Симплекс-метод ===\r\n\r\nНемає протоколу. Натисніть «Run»."
            : "=== Симплекс-метод ===\r\n\r\n" + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Protocol saved to protocol.txt";
    }

    private void LoadExample1()
    {
        ObjectiveText = Example1Objective;
        ConstraintsText = Example1Constraints;
        ResultText = string.Empty;
        Status = "Loaded test example 1.";
        _lastProtocol = null;
    }

    private void LoadExample2()
    {
        ObjectiveText = Example2Objective;
        ConstraintsText = Example2Constraints;
        ResultText = string.Empty;
        Status = "Loaded test example 2.";
        _lastProtocol = null;
    }

    private static string FormatResult(SolverResult result)
    {
        var xRows = result.X
            .Select((x, i) => $"x{i + 1} = {x.ToString("F4", CultureInfo.InvariantCulture)}");
        var yRows = (result.Y ?? Array.Empty<double>())
            .Select((y, i) => $"y{i + 1} = {y.ToString("F4", CultureInfo.InvariantCulture)}");
        var zRow = $"Z = {result.Z.ToString("F4", CultureInfo.InvariantCulture)}";
        return string.Join(Environment.NewLine, xRows.Concat(yRows).Append(zRow));
    }

    private static double[] BuildObjectiveVector(
        Dictionary<int, double> objective,
        int variablesCount,
        OptimizationMode mode)
    {
        double[] vector = new double[variablesCount];
        int sign = mode == OptimizationMode.Maximization ? -1 : 1;

        foreach (var (index, value) in objective)
        {
            vector[index - 1] = sign * value;
        }

        return vector;
    }

    private static List<ParsedConstraint> ParseConstraints(string text)
    {
        var lines = text
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToList();

        var result = new List<ParsedConstraint>(lines.Count);

        foreach (string line in lines)
        {
            string op;
            string[] parts;

            if (line.Contains("<=")) { op = "<="; parts = line.Split("<=", StringSplitOptions.TrimEntries); }
            else if (line.Contains(">=")) { op = ">="; parts = line.Split(">=", StringSplitOptions.TrimEntries); }
            else if (line.Contains("=")) { op = "="; parts = line.Split("=", StringSplitOptions.TrimEntries); }
            else throw new FormatException($"Обмеження '{line}' повинно містити <=, >= або =.");

            if (parts.Length != 2) throw new FormatException($"Невірний формат: '{line}'.");

            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double rightSide))
                throw new FormatException($"Невірне число справа: '{line}'.");

            // Викликаємо оновлений парсер, який повертає коефіцієнти ТА вільний член
            var (coefficients, constant) = ParseLinearExpressionWithConstant(parts[0]);

            // Переносимо константу вправо: ax + b >= c  => ax >= c - b
            double adjustedRightSide = rightSide - constant;

            if (op == "=")
            {
                result.Add(new ParsedConstraint(new Dictionary<int, double>(coefficients), "<=", adjustedRightSide));
                result.Add(new ParsedConstraint(new Dictionary<int, double>(coefficients), ">=", adjustedRightSide));
            }
            else
            {
                result.Add(new ParsedConstraint(coefficients, op, adjustedRightSide));
            }
        }

        return result;
    }
    private static (Dictionary<int, double> coefficients, double constant) ParseLinearExpressionWithConstant(string expression)
    {
        string normalized = expression
            .Replace(" ", string.Empty)
            .Replace("*", string.Empty)
            .Replace(",", ".");

        var coefficients = new Dictionary<int, double>();
        double constant = 0;

        // 1. Шукаємо всі змінні (напр. -2x1, x2, +3.5x3)
        var varRegex = new Regex(@"([+\-]?)(\d*(?:\.\d+)?)x(\d+)", RegexOptions.Compiled);
        var matches = varRegex.Matches(normalized);

        foreach (Match match in matches)
        {
            string signToken = match.Groups[1].Value;
            string valueToken = match.Groups[2].Value;
            int variableIndex = int.Parse(match.Groups[3].Value);

            double val = string.IsNullOrEmpty(valueToken) ? 1.0 : double.Parse(valueToken, CultureInfo.InvariantCulture);
            if (signToken == "-") val *= -1.0;

            coefficients[variableIndex] = coefficients.GetValueOrDefault(variableIndex) + val;
        }

        string leftover = varRegex.Replace(normalized, "|");


        var constRegex = new Regex(@"([+\-]?\d+(?:\.\d+)?)", RegexOptions.Compiled);
        var constMatches = constRegex.Matches(leftover);

        foreach (Match match in constMatches)
        {
            constant += double.Parse(match.Value, CultureInfo.InvariantCulture);
        }

        return (coefficients, constant);
    }
    private static Dictionary<int, double> ParseLinearExpression(string expression)
    {
        string normalized = expression
            .Replace(" ", string.Empty)
            .Replace("*", string.Empty)
            .Replace(",", ".");

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new FormatException("Expression cannot be empty.");
        }

        var regex = new Regex(@"([+\-]?)(\d*(?:\.\d+)?)x(\d+)", RegexOptions.Compiled);
        var matches = regex.Matches(normalized);

        if (matches.Count == 0)
        {
            throw new FormatException($"Invalid expression: '{expression}'. Use terms like x1, -2x2, +3.5x3.");
        }

        var coefficients = new Dictionary<int, double>();

        foreach (Match match in matches)
        {
            string signToken = match.Groups[1].Value;
            string valueToken = match.Groups[2].Value;
            string variableToken = match.Groups[3].Value;

            double value = string.IsNullOrEmpty(valueToken)
                ? 1d
                : double.Parse(valueToken, CultureInfo.InvariantCulture);

            if (signToken == "-")
            {
                value *= -1d;
            }

            int variableIndex = int.Parse(variableToken, CultureInfo.InvariantCulture);
            coefficients[variableIndex] = coefficients.GetValueOrDefault(variableIndex) + value;
        }

        string leftover = regex.Replace(normalized, string.Empty).Replace("+", string.Empty).Replace("-", string.Empty).Replace(">", string.Empty).Replace("<", string.Empty).Replace("=", string.Empty);
        if (!string.IsNullOrEmpty(leftover))
        {
            throw new FormatException($"Unsupported expression part: '{leftover}'.");
        }

        return coefficients;
    }

    private sealed record ParsedConstraint(
        Dictionary<int, double> Coefficients,
        string Operator,
        double RightSide);
}
