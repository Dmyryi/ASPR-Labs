using System.Windows.Input;

namespace Lab01.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly Func<InverseMatrixViewModel> _inverseFactory;
    private readonly Func<RankViewModel> _rankFactory;
    private readonly Func<LinearSystemViewModel> _linearSystemFactory;

    private ViewModelBase _currentViewModel;
    private int _selectedIndex;

    public MainViewModel(
        Func<InverseMatrixViewModel> inverseFactory,
        Func<RankViewModel> rankFactory,
        Func<LinearSystemViewModel> linearSystemFactory,
        SimplexViewModel simplexViewModel,
        GomoryViewModel gomoryViewModel,
        MatrixGameViewModel matrixGameViewModel,
        NatureGameViewModel natureGameViewModel)
    {
        _inverseFactory = inverseFactory;
        _rankFactory = rankFactory;
        _linearSystemFactory = linearSystemFactory;
        SimplexViewModel = simplexViewModel;
        GomoryViewModel = gomoryViewModel;
        MatrixGameViewModel = matrixGameViewModel;
        NatureGameViewModel = natureGameViewModel;

        _currentViewModel = _inverseFactory();
        _selectedIndex = 0;

        SelectInverseCommand = new RelayCommand(() => Navigate(_inverseFactory(), 0));
        SelectRankCommand = new RelayCommand(() => Navigate(_rankFactory(), 1));
        SelectLinearSystemCommand = new RelayCommand(() => Navigate(_linearSystemFactory(), 2));
    }

    public SimplexViewModel SimplexViewModel { get; }
    public GomoryViewModel GomoryViewModel { get; }
    public MatrixGameViewModel MatrixGameViewModel { get; }
    public NatureGameViewModel NatureGameViewModel { get; }

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
        private set
        {
            _currentViewModel = value;
            OnPropertyChanged();
        }
    }

    public ICommand SelectInverseCommand { get; }
    public ICommand SelectRankCommand { get; }
    public ICommand SelectLinearSystemCommand { get; }

    private void Navigate(ViewModelBase viewModel, int index)
    {
        CurrentViewModel = viewModel;
        SelectedIndex = index;
    }
}
