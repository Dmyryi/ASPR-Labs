using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
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

    public SimplexViewModel(IJordan jordan, IProtocolSaver protocolSaver)
    {
        _jordan = jordan;
        _protocolSaver = protocolSaver;
        SolveMaxCommand = new RelayCommand(() => Solve(OptimizationMode.Maximization));
        SolveMinCommand = new RelayCommand(() => Solve(OptimizationMode.Minimization));
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
        SaveProtocolCommand = new RelayCommand(SaveProtocol);
    }

    public ICommand SolveMaxCommand { get; }
    public ICommand SolveMinCommand { get; }
    public ICommand LoadExample1Command { get; }
    public ICommand LoadExample2Command { get; }
    public ICommand SaveProtocolCommand { get; }

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

    private void Solve(OptimizationMode mode)
    {
        try
        {
            var objective = ParseLinearExpression(ObjectiveText);
            var parsedConstraints = ParseConstraints(ConstraintsText);
            if (parsedConstraints.Count == 0)
            {
                throw new InvalidOperationException("Add at least one constraint.");
            }

            int variablesCount = Math.Max(
                objective.Keys.DefaultIfEmpty(0).Max(),
                parsedConstraints.SelectMany(c => c.Coefficients.Keys).DefaultIfEmpty(0).Max());

            if (variablesCount <= 0)
            {
                throw new InvalidOperationException("Cannot detect variables. Use x1, x2, ... format.");
            }

            double[] vectorZ = BuildObjectiveVector(objective, variablesCount, mode);
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
            protocol.Start(mode, ObjectiveText, ConstraintsText);

            var basicFinder = new BasicSolutionFinder(_jordan, protocol);
            var optimalFinder = new OptimalSolutionFinder(_jordan, mode, protocol);
            ILinearSolver solver = mode == OptimizationMode.Maximization
                ? new MaximizationSolver(basicFinder, optimalFinder, protocol)
                : new MinimizationSolver(basicFinder, optimalFinder, protocol);

            var context = new SimplexContext();
            context.SetStrategy(solver);
            SolverResult result = context.ExecuteStrategy(vectorZ, matrixA, vectorB);

            ResultText = FormatResult(result);
            protocol.LogResult(result);
            _lastProtocol = protocol.GetText();
            Status = mode == OptimizationMode.Maximization
                ? "Maximization solved."
                : "Minimization solved.";
        }
        catch (Exception ex)
        {
            Status = "Error: " + ex.Message;
            ResultText = string.Empty;
            _lastProtocol = null;
        }
    }

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
        var zRow = $"Z = {result.Z.ToString("F4", CultureInfo.InvariantCulture)}";
        return string.Join(Environment.NewLine, xRows.Append(zRow));
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

            if (line.Contains("<="))
            {
                op = "<=";
                parts = line.Split("<=", StringSplitOptions.TrimEntries);
            }
            else if (line.Contains("="))
            {
                op = "=";
                parts = line.Split("=", StringSplitOptions.TrimEntries);
            }
            else if (line.Contains(">="))
            {
                op = ">=";
                parts = line.Split(">=", StringSplitOptions.TrimEntries);
            }
            else
            {
                throw new FormatException($"Constraint '{line}' must contain <= or >=.");
            }

            if (parts.Length != 2)
            {
                throw new FormatException($"Invalid constraint format: '{line}'.");
            }

            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double rightSide))
            {
                throw new FormatException($"Invalid right side in constraint: '{line}'.");
            }

            var coefficients = ParseLinearExpression(parts[0]);

            if (op == "=")
            {
                result.Add(new ParsedConstraint(new Dictionary<int, double>(coefficients), "<=", rightSide));
                result.Add(new ParsedConstraint(new Dictionary<int, double>(coefficients), ">=", rightSide));
            }
            else
            {
                result.Add(new ParsedConstraint(coefficients, op, rightSide));
            }
        }

        return result;
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

        string leftover = regex.Replace(normalized, string.Empty).Replace("+", string.Empty).Replace("-", string.Empty);
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
