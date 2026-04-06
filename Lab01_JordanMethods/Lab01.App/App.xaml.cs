using System.Windows;
using Lab01.Logic;
using Lab01.Logic.BasicLogic;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Interfaces.IBasicLogic;
using Microsoft.Extensions.DependencyInjection;

namespace Lab01.App;

public partial class App : Application
{
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IJordan, JordanSolver>();
        services.AddSingleton<IProtocolSaver, ProtocolSaver>();
        services.AddTransient<IMatrixInverter>(sp => new MatrixInverter(sp.GetRequiredService<IJordan>(), null));
        services.AddTransient<IRankCalculator>(sp => new RankCalculator(sp.GetRequiredService<IJordan>()));

        services.AddTransient<ViewModels.InverseMatrixViewModel>();
        services.AddTransient<ViewModels.RankViewModel>();
        services.AddTransient<ViewModels.LinearSystemViewModel>();
        services.AddTransient<ViewModels.MainViewModel>();


        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var services = ConfigureServices();
        var mainWindow = new MainWindow
        {
            DataContext = services.GetRequiredService<ViewModels.MainViewModel>()
        };
        mainWindow.Show();
    }
}
