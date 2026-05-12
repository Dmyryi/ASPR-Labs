using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Lab01.Logic.Exceptions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.App;
using Lab01.Logic.Simplex;
using Microsoft.Extensions.DependencyInjection;

namespace Lab01.App.ViewModels;

public sealed class SimplexViewModel : ViewModelBase
{
    private const string Example1Objective = "x1 + 2x2 - x3 - x4";
    private const string Example1Constraints =
        "x1 + x2 - x3 - 2x4 <= 6\n" +
        "x1 + x2 + x3 - x4 >= 5\n" +
        "2x1 - x2 + 3x3 + 4x4 <= 10";

    private const string Example2Objective = "10x1 - x2 - 42x3 - 52x4";
    private const string Example2Constraints =
        "-3x1 + x2 + 4x3 + x4 <= 1\n" +
        "3x1 - 2x2 + 2x3 - 2x4 <= -9\n" +
        "-2x1 + x2 + x3 + 3x4 = 2\n" +
        "-3x1 + 2x2 - 3x3 = 7";

    private const string Variant10Objective = "2x1 + x2";
    private const string Variant10Constraints =
        "x1 + 2x2 = 4\n" +
        "x1 + x2 <= 3";

    private readonly SimplexUiProfile _profile;
    private readonly ILinearProgramParser _parser;
    private readonly ISimplexSolverFactory _solverFactory;
    private readonly IProtocolSaver _protocolSaver;
    private readonly IServiceProvider? _services;

    private string _objectiveText;
    private string _constraintsText;
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private string? _lastProtocol;
    private bool _useZeroRowElimination = true;

    public SimplexViewModel(
        ILinearProgramParser parser,
        ISimplexSolverFactory solverFactory,
        IProtocolSaver protocolSaver,
        IServiceProvider? services = null,
        SimplexUiProfile? profile = null)
    {
        _profile = profile ?? SimplexUiProfile.Primal;
        _parser = parser;
        _solverFactory = solverFactory;
        _protocolSaver = protocolSaver;
        _services = services;

        _objectiveText = _profile.DefaultObjectiveText ?? "x1 + 2x2 - x3 - x4";
        _constraintsText = _profile.DefaultConstraintsText ??
            "x1 + x2 - x3 - 2x4 <= 6\n" +
            "x1 + x2 + x3 - x4 >= 5\n" +
            "2x1 - x2 + 3x3 + 4x4 <= 10";

        SolveMaxCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Maximization));
        SolveMinCommand = new RelayCommand(() => SolveAsync(OptimizationMode.Minimization));
        LoadExample1Command = new RelayCommand(LoadExample1);
        LoadExample2Command = new RelayCommand(LoadExample2);
        LoadVariant10Command = new RelayCommand(LoadVariant10);
        SaveProtocolCommand = new RelayCommand(SaveProtocol);
        OpenDualSimplexWindowCommand = new RelayCommand(OpenDualSimplexWindow, CanOpenDualSimplexWindow);
    }

    public string PageTitle => _profile.PageTitle;
    public string PageBadge => _profile.PageBadge;
    public string ObjectiveSectionTitle => _profile.ObjectiveSectionTitle;
    public string ObjectiveHint => _profile.ObjectiveHint;
    public string ConstraintsHint => _profile.ConstraintsHint;
    public string ResultSectionTitle => _profile.ResultSectionTitle;
    public Visibility PrimalExamplesVisibility => _profile.IsDual ? Visibility.Collapsed : Visibility.Visible;
    public Visibility OpenDualButtonVisibility =>
        _services is not null && !_profile.IsDual ? Visibility.Visible : Visibility.Collapsed;
    public Visibility ContextBannerVisibility =>
        string.IsNullOrEmpty(_profile.ContextBanner) ? Visibility.Collapsed : Visibility.Visible;

    public ICommand SolveMaxCommand { get; }
    public ICommand SolveMinCommand { get; }
    public ICommand LoadExample1Command { get; }
    public ICommand LoadExample2Command { get; }
    public ICommand LoadVariant10Command { get; }
    public ICommand SaveProtocolCommand { get; }
    public ICommand OpenDualSimplexWindowCommand { get; }

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

    private bool CanOpenDualSimplexWindow() => _services is not null && !_profile.IsDual;

    private void OpenDualSimplexWindow()
    {
        if (_services is null) return;

        var vm = new SimplexViewModel(
            _services.GetRequiredService<ILinearProgramParser>(),
            _services.GetRequiredService<ISimplexSolverFactory>(),
            _services.GetRequiredService<IProtocolSaver>(),
            services: null,
            profile: SimplexUiProfile.Dual);

        var win = new DualSimplexWindow { DataContext = vm };
        win.Owner = Application.Current?.MainWindow;
        win.Show();
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
                ResultText = FormatResult(payload.Result, mode);
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
        var protocolStyle = _profile.IsDual ? SimplexProtocolStyle.DualW : SimplexProtocolStyle.PrimalZ;
        handle.Protocol.Start(mode, objectiveText, constraintsText, program, protocolStyle);

        double[] vectorZ = BuildObjectiveVector(program.ObjectiveCoefficients, mode);
        SolverResult result = handle.Solver.Solve(vectorZ, program.ConstraintMatrix, program.RightHandSide);

        handle.Protocol.LogResult(result, _profile.ResultObjectiveSymbol);
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
        var preamble = _profile.ProtocolFilePreamble + "\r\n\r\n";
        var content = string.IsNullOrEmpty(_lastProtocol)
            ? preamble + "Немає протоколу. Натисніть «Run»."
            : preamble + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Протокол збережено у protocol.txt";
    }

    private void LoadExample1()
    {
        ObjectiveText = Example1Objective;
        ConstraintsText = Example1Constraints;
        ResultText = string.Empty;
        Status = "Завантажено «Задачу 1» з методички (Z = x1 + 2x2 - x3 - x4).";
        _lastProtocol = null;
    }

    private void LoadExample2()
    {
        ObjectiveText = Example2Objective;
        ConstraintsText = Example2Constraints;
        ResultText = string.Empty;
        Status = "Завантажено «Задачу 2» з методички (рівності + нерівності).";
        _lastProtocol = null;
    }

    private void LoadVariant10()
    {
        ObjectiveText = Variant10Objective;
        ConstraintsText = Variant10Constraints;
        ResultText = string.Empty;
        Status = "Завантажено варіант 10 (Z = 2x1 + x2; рівність + нерівність).";
        _lastProtocol = null;
    }

    private string FormatResult(SolverResult result, OptimizationMode mode)
    {
        var culture = CultureInfo.GetCultureInfo("uk-UA");
        string fmt(double v) => v.ToString("F2", culture);
        string label = _profile.ResultObjectiveSymbol;

        var lines = new List<string>
        {
            "Знайдено оптимальний розв’язок:",
            string.Empty,
        };

        if (!_profile.IsDual)
        {
            lines.Add("Розв’язки прямої задачі:");
            lines.Add($"X = ({string.Join("; ", result.X.Select(fmt))})");
            if (result.U.Length > 0)
            {
                lines.Add(string.Empty);
                lines.Add("Розв’язки двоїстої задачі:");
                lines.Add($"U = ({string.Join("; ", result.U.Select(fmt))})");
            }

            lines.Add(string.Empty);
            lines.Add($"Max (Z) = {fmt(result.Z)}");
            lines.Add($"Min (W) = {fmt(result.Z)}");
        }
        else
        {
            lines.Add("Розв’язки двоїстої задачі:");
            lines.Add($"X = ({string.Join("; ", result.X.Select(fmt))})");
            if (result.U.Length > 0)
            {
                lines.Add(string.Empty);
                lines.Add("Оцінки обмежень:");
                lines.Add($"U = ({string.Join("; ", result.U.Select(fmt))})");
            }

            lines.Add(string.Empty);
            if (mode == OptimizationMode.Maximization)
            {
                lines.Add($"Max ({label}) = {fmt(result.Z)}");
                lines.Add($"Min (Z) = {fmt(result.Z)}");
            }
            else
            {
                lines.Add($"Min ({label}) = {fmt(result.Z)}");
                lines.Add($"Max (Z) = {fmt(result.Z)}");
            }
        }

        bool anyY = result.Y.Any(y => Math.Abs(y) > 1e-9);
        if (anyY)
        {
            lines.Add(string.Empty);
            lines.Add("Додатково (y):");
            for (int i = 0; i < result.Y.Length; i++)
                lines.Add($"y{i + 1} = {result.Y[i].ToString("F2", culture)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private sealed record SolveOutput(SolverResult Result, string ProtocolText);
}
