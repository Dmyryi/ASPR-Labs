using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Lab01.App.ViewModels;

namespace Lab01.App.Views;

public partial class NetworkPlanningView
{
    public NetworkPlanningView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => HookViewModel();
    }

    private void NetworkPlanningView_OnLoaded(object sender, RoutedEventArgs e) => RefreshCharts();

    private void HookViewModel()
    {
        if (DataContext is NetworkPlanningViewModel vm)
        {
            vm.PropertyChanged -= ViewModel_OnPropertyChanged;
            vm.ChartChanged -= ViewModel_OnChartChanged;
            vm.PropertyChanged += ViewModel_OnPropertyChanged;
            vm.ChartChanged += ViewModel_OnChartChanged;
            RefreshCharts();
        }
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(NetworkPlanningViewModel.LastResult) or nameof(NetworkPlanningViewModel.Schedule))
            RefreshCharts();
    }

    private void ViewModel_OnChartChanged(object? sender, EventArgs e) => RefreshCharts();

    private void RefreshCharts()
    {
        if (DataContext is not NetworkPlanningViewModel vm)
            return;

        Charts.Render(vm.Schedule, vm.SelectedTask?.TaskId);
        Dispatcher.BeginInvoke(() =>
        {
            Charts.Render(vm.Schedule, vm.SelectedTask?.TaskId);
            CommandManager.InvalidateRequerySuggested();
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }
}
