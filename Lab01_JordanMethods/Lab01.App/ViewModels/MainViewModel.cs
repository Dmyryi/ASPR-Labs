using System.Windows.Input;

namespace Lab01.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase _currentViewModel = null!;
    private int _selectedIndex;

    public MainViewModel()
    {
        _currentViewModel = new InverseMatrixViewModel();
        _selectedIndex = 0;
        SelectInverseCommand = new RelayCommand(() => { CurrentViewModel = new InverseMatrixViewModel(); SelectedIndex = 0; });
        SelectRankCommand = new RelayCommand(() => { CurrentViewModel = new RankViewModel(); SelectedIndex = 1; });
        SelectLinearSystemCommand = new RelayCommand(() => { CurrentViewModel = new LinearSystemViewModel(); SelectedIndex = 2; });
    }

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
