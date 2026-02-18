using System.Globalization;
using System.Windows.Input;
using Lab01.Logic;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public class RankViewModel : ViewModelBase
{
    private readonly IRankCalculator _rankCalculator;
    private string _inputText = "1 2 3 4\n2 4 6 8";
    private string _resultText = string.Empty;
    private string _status = string.Empty;

    public RankViewModel()
    {
        var jordan = new JordanSolver();
        _rankCalculator = new RankCalculator(jordan);
        ComputeCommand = new RelayCommand(Compute);
    }

    public string InputText
    {
        get => _inputText;
        set
        {
            _inputText = value;
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

    public ICommand ComputeCommand { get; }

    private void Compute()
    {
        try
        {
            var matrix = ParseMatrix(InputText);
            if (matrix == null)
            {
                Status = "Invalid matrix format. Use rows separated by newlines, numbers by space.";
                return;
            }
            var rank = _rankCalculator.Calculate(matrix);
            ResultText = "Rank = " + rank;
            Status = "Done.";
        }
        catch (Exception ex)
        {
            Status = "Error: " + ex.Message;
            ResultText = string.Empty;
        }
    }

    private static double[,]? ParseMatrix(string text)
    {
        var rows = text.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (rows.Length == 0) return null;
        var cols = rows[0].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (cols.Length == 0) return null;
        var matrix = new double[rows.Length, cols.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            var vals = rows[i].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (vals.Length != cols.Length) return null;
            for (int j = 0; j < vals.Length; j++)
            {
                if (!double.TryParse(vals[j], NumberStyles.Any, CultureInfo.InvariantCulture, out matrix[i, j]))
                    return null;
            }
        }
        return matrix;
    }
}
