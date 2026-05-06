using System.Globalization;
using System.Text;
using System.Windows.Input;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Interfaces.IBasicLogic;

namespace Lab01.App.ViewModels;

public sealed class RankViewModel : ViewModelBase
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

    public ICommand ComputeCommand { get; }
    public ICommand SaveProtocolCommand { get; }

    public string InputText
    {
        get => _inputText;
        set { _inputText = value; OnPropertyChanged(); }
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

    private void Compute()
    {
        try
        {
            var matrix = MatrixTextParser.Parse(InputText);
            if (matrix is null)
            {
                Status = "Невірний формат матриці. Рядки розділяйте новим рядком, числа — пробілом.";
                _lastProtocol = null;
                return;
            }

            var rank = _rankCalculator.Calculate(matrix);
            ResultText = "Ранг = " + rank;
            _lastProtocol = BuildProtocol(matrix, rank);
            Status = "Готово.";
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Status = "Помилка: " + ex.Message;
            ResultText = string.Empty;
            _lastProtocol = null;
        }
    }

    private void SaveProtocol()
    {
        var content = string.IsNullOrEmpty(_lastProtocol)
            ? "=== Ранг матриці ===\r\n\r\nНемає протоколу. Натисніть «Run»."
            : "=== Ранг матриці ===\r\n\r\n" + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Протокол збережено у protocol.txt";
    }

    private static string BuildProtocol(double[,] matrix, int rank)
    {
        var culture = CultureInfo.GetCultureInfo("uk-UA");
        var sb = new StringBuilder();
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
}
