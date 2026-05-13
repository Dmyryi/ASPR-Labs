using System.Windows;
using Lab01.App.ViewModels;
using Lab01.Logic;
using Lab01.Logic.BasicLogic;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Interfaces.IBasicLogic;
using Lab01.Logic.GameTheory;
using Lab01.Logic.Gomori;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Parsing;
using Lab01.Logic.Simplex.Stages;
using Microsoft.Extensions.DependencyInjection;

namespace Lab01.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = ConfigureServices();
        var mainWindow = new MainWindow
        {
            DataContext = services.GetRequiredService<MainViewModel>()
        };
        mainWindow.Show();
    }

    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IJordan, JordanSolver>();
        services.AddSingleton<IPivotSelector, PivotSelector>();
        services.AddSingleton<IProtocolSaver, ProtocolSaver>();
        services.AddSingleton<ILinearProgramParser, LinearProgramParser>();
        services.AddSingleton<ISimplexSolverFactory, SimplexSolverFactory>();
        services.AddSingleton<IGomorySolver, GomorySolver>();
        services.AddSingleton<MatrixGameSolver>(sp =>
            new MatrixGameSolver(sp.GetRequiredService<ISimplexSolverFactory>()));

        services.AddTransient<IMatrixInverter>(sp => new MatrixInverter(sp.GetRequiredService<IJordan>()));
        services.AddTransient<IRankCalculator>(sp => new RankCalculator(sp.GetRequiredService<IJordan>()));
        services.AddTransient<ILinearSystemSolver>(sp =>
            new InverseSolveStrategy(sp.GetRequiredService<IMatrixInverter>()));

        services.AddTransient<InverseMatrixViewModel>();
        services.AddTransient<RankViewModel>();
        services.AddTransient<LinearSystemViewModel>();
        services.AddTransient<SimplexViewModel>(sp =>
            new SimplexViewModel(
                sp.GetRequiredService<ILinearProgramParser>(),
                sp.GetRequiredService<ISimplexSolverFactory>(),
                sp.GetRequiredService<IProtocolSaver>(),
                sp));
        services.AddTransient<GomoryViewModel>();
        services.AddTransient<MatrixGameViewModel>(sp =>
            new MatrixGameViewModel(sp.GetRequiredService<MatrixGameSolver>()));
        services.AddTransient<MainViewModel>();

        services.AddSingleton<Func<InverseMatrixViewModel>>(sp => sp.GetRequiredService<InverseMatrixViewModel>);
        services.AddSingleton<Func<RankViewModel>>(sp => sp.GetRequiredService<RankViewModel>);
        services.AddSingleton<Func<LinearSystemViewModel>>(sp => sp.GetRequiredService<LinearSystemViewModel>);

        return services.BuildServiceProvider();
    }
}
