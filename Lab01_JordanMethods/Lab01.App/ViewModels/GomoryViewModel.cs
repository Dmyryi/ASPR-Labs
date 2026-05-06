using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Lab01.Logic.Exceptions;
using Lab01.Logic.Gomori;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Protocols;

namespace Lab01.App.ViewModels;

public sealed class GomoryViewModel : ViewModelBase
{
    private const string ExampleObjective = "x1 + 4x2";
    private const string ExampleConstraints =
        "2x1 + x2 <= 6\n" +
        "x1 + 3x2 <= 4";

    private readonly ILinearProgramParser _parser;
    private readonly IGomorySolver _solver;
    private readonly IProtocolSaver _protocolSaver;

    private string _objectiveText = ExampleObjective;
    private string _constraintsText = ExampleConstraints;
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private string? _lastProtocol;
    private int _maxCuts = 30;

    public GomoryViewModel(
        ILinearProgramParser parser,
        IGomorySolver solver,
        IProtocolSaver protocolSaver)
    {
        _parser = parser;
        _solver = solver;
        _protocolSaver = protocolSaver;

        SolveMaxCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Maximization));
        SolveMinCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Minimization));
        LoadExampleCommand = new RelayCommand(LoadExample);
        SaveProtocolCommand = new RelayCommand(SaveProtocol);
    }

    public ICommand SolveMaxCommand { get; }
    public ICommand SolveMinCommand { get; }
    public ICommand LoadExampleCommand { get; }
    public ICommand SaveProtocolCommand { get; }

    public string ObjectiveText
    {
        get => _objectiveText;
        set { _objectiveText = value; OnPropertyChanged(); }
    }

    public string ConstraintsText
    {
        get => _constraintsText;
        set { _constraintsText = value; OnPropertyChanged(); }
    }

    public string ResultText
    {
        get => _resultText;
        set { _resultText = value; OnPropertyChanged(); }
    }

    public string Status
    {
        get => _status;
        set { _status = value; OnPropertyChanged(); }
    }

    public int MaxCuts
    {
        get => _maxCuts;
        set { _maxCuts = value; OnPropertyChanged(); }
    }

    private async void SolveAsync(OptimizationMode mode)
    {
        Status = "Обчислення… (інтерфейс не блокується)";

        string objectiveSnapshot = ObjectiveText;
        string constraintsSnapshot = ConstraintsText;
        var gomoryOptions = new GomoryOptions { MaxCuts = Math.Max(1, _maxCuts) };

        try
        {
            SolveOutput payload = await Task.Run(() =>
                Solve(objectiveSnapshot, constraintsSnapshot, mode, gomoryOptions))
                .ConfigureAwait(false);

            await DispatchAsync(() =>
            {
                ResultText = FormatResult(payload.Result);
                _lastProtocol = payload.ProtocolText;
                Status = mode == OptimizationMode.Maximization
                    ? "Цілочислову максимізацію виконано."
                    : "Цілочислову мінімізацію виконано.";
            });
        }
        catch (SimplexException ex)
        {
            await DispatchAsync(() => ApplyError(ex.Message));
        }
        catch (FormatException ex)
        {
            await DispatchAsync(() => ApplyError(ex.Message));
        }
        catch (Exception ex)
        {
            await DispatchAsync(() => ApplyError($"Невідома помилка: {ex.Message}"));
        }
    }

    private SolveOutput Solve(
        string objectiveText,
        string constraintsText,
        OptimizationMode mode,
        GomoryOptions gomoryOptions)
    {
        LinearProgram program = _parser.Parse(objectiveText, constraintsText);

        var protocol = new SimplexProtocol();
        protocol.StartGomory(mode, objectiveText, constraintsText, program);

        double[] vectorZ = BuildObjectiveVector(program.ObjectiveCoefficients, mode);
        SolverResult result = _solver.Solve(
            vectorZ, program.ConstraintMatrix, program.RightHandSide, mode, gomoryOptions, protocol);

        return new SolveOutput(result, protocol.GetText());
    }

    private static double[] BuildObjectiveVector(double[] objective, OptimizationMode mode)
    {
        int sign = mode == OptimizationMode.Maximization ? -1 : 1;
        var vector = new double[objective.Length];
        for (int i = 0; i < objective.Length; i++)
            vector[i] = sign * objective[i];
        return vector;
    }

    private void ApplyError(string message)
    {
        Status = "Помилка: " + message;
        ResultText = string.Empty;
        _lastProtocol = null;
    }

    private static Task DispatchAsync(Action action)
    {
        Dispatcher? ui = Application.Current?.Dispatcher;
        if (ui is null) { action(); return Task.CompletedTask; }
        return ui.InvokeAsync(action).Task;
    }

    private void SaveProtocol()
    {
        var content = string.IsNullOrEmpty(_lastProtocol)
            ? "=== Метод Гоморі ===\r\n\r\nНемає протоколу. Натисніть «Run»."
            : "=== Метод Гоморі ===\r\n\r\n" + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Протокол збережено у protocol.txt";
    }

    private void LoadExample()
    {
        ObjectiveText = ExampleObjective;
        ConstraintsText = ExampleConstraints;
        ResultText = string.Empty;
        Status = "Завантажено тестовий приклад.";
        _lastProtocol = null;
    }

    private static string FormatResult(SolverResult result)
    {
        var xRows = result.X.Select((x, i) => $"x{i + 1} = {x.ToString("F4", CultureInfo.InvariantCulture)}");
        var zRow = $"Z = {result.Z.ToString("F4", CultureInfo.InvariantCulture)}";
        return string.Join(Environment.NewLine, xRows.Append(zRow));
    }

    private sealed record SolveOutput(SolverResult Result, string ProtocolText);
}
