using System.Globalization;
using System.Windows.Input;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Interfaces.IBasicLogic;

namespace Lab01.App.ViewModels;

public class RankViewModel : ViewModelBase
{
    private readonly IRankCalculator _rankCalculator;
    private readonly IProtocolSaver _protocolSaver;
    private string _inputText = "1 2 3 4\n2 4 6 8";
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private string? _lastProtocol;

    public RankViewModel(IRankCalculator rankCalculator, IProtocolSaver protocolSaver)
    {
        _rankCalculator = rankCalculator;
        _protocolSaver = protocolSaver;
        ComputeCommand = new RelayCommand(Compute);
        SaveProtocolCommand = new RelayCommand(SaveProtocol);
    }

    public ICommand SaveProtocolCommand { get; }

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
                const string msg = "Invalid matrix format. Use rows separated by newlines, numbers by space.";
                Status = "Помилка.";
                ResultText = "Помилка:\n\n" + msg;
                _lastProtocol = null;
                return;
            }
            var rank = _rankCalculator.Calculate(matrix);
            ResultText = "Rank = " + rank;
            _lastProtocol = BuildProtocol(matrix, rank);
            Status = "Done.";
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Status = "Помилка.";
            ResultText = "Помилка обчислення:\n\n" + ex.Message;
            _lastProtocol = null;
        }
    }

    private static string BuildProtocol(double[,] matrix, int rank)
    {
        var culture = CultureInfo.GetCultureInfo("uk-UA");
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Протокол обчислення рангу матриці:");
        sb.AppendLine("Вхідна матриця:");
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            var row = new List<string>();
            for (int j = 0; j < matrix.GetLength(1); j++)
                row.Add(matrix[i, j].ToString("F2", culture));
            sb.AppendLine(string.Join("  ", row));
        }
        sb.AppendLine();
        sb.AppendLine("Ранг = " + rank);
        return sb.ToString();
    }

    private void SaveProtocol()
    {
        var content = string.IsNullOrEmpty(_lastProtocol)
            ? "=== Ранг матриці ===\r\n\r\nНемає протоколу. Натисніть «Run Computation»."
            : "=== Ранг матриці ===\r\n\r\n" + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Protocol saved to protocol.txt";
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




