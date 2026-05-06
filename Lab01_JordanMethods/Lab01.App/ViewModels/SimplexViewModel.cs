using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Lab01.Logic.Exceptions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.App.ViewModels;

public sealed class SimplexViewModel : ViewModelBase
{
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

    private readonly ILinearProgramParser _parser;
    private readonly ISimplexSolverFactory _solverFactory;
    private readonly IProtocolSaver _protocolSaver;

    private string _objectiveText = "x1 + 2x2 - x3 - x4";
    private string _constraintsText =
        "x1 + x2 - x3 - 2x4 <= 6\n" +
        "x1 + x2 + x3 - x4 >= 5\n" +
        "2x1 - x2 + 3x3 + 4x4 <= 10";
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private string? _lastProtocol;
    private bool _useZeroRowElimination = true;

    public SimplexViewModel(
        ILinearProgramParser parser,
        ISimplexSolverFactory solverFactory,
        IProtocolSaver protocolSaver)
    {
        _parser = parser;
        _solverFactory = solverFactory;
        _protocolSaver = protocolSaver;

        SolveMaxCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Maximization));
        SolveMinCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Minimization));
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
        SaveProtocolCommand = new RelayCommand(SaveProtocol);
    }

    public ICommand SolveMaxCommand { get; }
    public ICommand SolveMinCommand { get; }
    public ICommand LoadExample1Command { get; }
    public ICommand LoadExample2Command { get; }
    public ICommand SaveProtocolCommand { get; }

    public bool UseZeroRowElimination
    {
        get => _useZeroRowElimination;
        set { _useZeroRowElimination = value; OnPropertyChanged(); }
    }

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

    private async void SolveAsync(OptimizationMode mode)
    {
        Status = "Обчислення… (інтерфейс не блокується)";

        string objectiveSnapshot = ObjectiveText;
        string constraintsSnapshot = ConstraintsText;
        var options = new SimplexOptions { UseZeroRowElimination = _useZeroRowElimination };

        try
        {
            SolveOutput payload = await Task.Run(() =>
                Solve(objectiveSnapshot, constraintsSnapshot, mode, options)).ConfigureAwait(false);

            await DispatchAsync(() =>
            {
                ResultText = FormatResult(payload.Result);
                _lastProtocol = payload.ProtocolText;
                Status = mode == OptimizationMode.Maximization
                    ? "Максимізацію виконано."
                    : "Мінімізацію виконано.";
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

    private SolveOutput Solve(string objectiveText, string constraintsText, OptimizationMode mode, SimplexOptions options)
    {
        LinearProgram program = _parser.Parse(objectiveText, constraintsText);
        SimplexSolverHandle handle = _solverFactory.Create(mode, options);
        handle.Protocol.Start(mode, objectiveText, constraintsText);

        double[] vectorZ = BuildObjectiveVector(program.ObjectiveCoefficients, mode);
        SolverResult result = handle.Solver.Solve(vectorZ, program.ConstraintMatrix, program.RightHandSide);

        handle.Protocol.LogResult(result);
        return new SolveOutput(result, handle.Protocol.GetText());
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
            ? "=== Симплекс-метод ===\r\n\r\nНемає протоколу. Натисніть «Run»."
            : "=== Симплекс-метод ===\r\n\r\n" + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Протокол збережено у protocol.txt";
    }

    private void LoadExample1()
    {
        ObjectiveText = Example1Objective;
        ConstraintsText = Example1Constraints;
        ResultText = string.Empty;
        Status = "Завантажено приклад 1.";
        _lastProtocol = null;
    }

    private void LoadExample2()
    {
        ObjectiveText = Example2Objective;
        ConstraintsText = Example2Constraints;
        ResultText = string.Empty;
        Status = "Завантажено приклад 2.";
        _lastProtocol = null;
    }

    private static string FormatResult(SolverResult result)
    {
        var xRows = result.X.Select((x, i) => $"x{i + 1} = {x.ToString("F4", CultureInfo.InvariantCulture)}");
        var yRows = result.Y.Select((y, i) => $"y{i + 1} = {y.ToString("F4", CultureInfo.InvariantCulture)}");
        var zRow = $"Z = {result.Z.ToString("F4", CultureInfo.InvariantCulture)}";
        return string.Join(Environment.NewLine, xRows.Concat(yRows).Append(zRow));
    }

    private sealed record SolveOutput(SolverResult Result, string ProtocolText);
}
