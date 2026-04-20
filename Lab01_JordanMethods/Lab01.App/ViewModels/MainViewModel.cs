using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Lab01.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel = null!;
    private int _selectedIndex;
    private readonly IServiceProvider _services;

    public MainViewModel(IServiceProvider services)
    {
        _services = services;
        SimplexViewModel = _services.GetRequiredService<SimplexViewModel>();
        _currentViewModel = _services.GetRequiredService<InverseMatrixViewModel>();
        _selectedIndex = 0;
        SelectInverseCommand = new RelayCommand(() => { CurrentViewModel = _services.GetRequiredService<InverseMatrixViewModel>(); SelectedIndex = 0; });
        SelectRankCommand = new RelayCommand(() => { CurrentViewModel = _services.GetRequiredService<RankViewModel>(); SelectedIndex = 1; });
        SelectLinearSystemCommand = new RelayCommand(() => { CurrentViewModel = _services.GetRequiredService<LinearSystemViewModel>(); SelectedIndex = 2; });
    }

    public SimplexViewModel SimplexViewModel { get; }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            OnPropertyChanged();
        }
    }

    public ViewModelBase CurrentViewModel
    {
        get => _currentViewModel;
        set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public ICommand SelectInverseCommand { get; }
    public ICommand SelectRankCommand { get; }
    public ICommand SelectLinearSystemCommand { get; }
}
