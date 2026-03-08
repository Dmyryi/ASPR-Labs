using System.Globalization;
using System.Windows.Input;
using Lab01.Logic;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public class InverseMatrixViewModel : ViewModelBase
{
    private readonly IJordan _jordan;
    private readonly IProtocolSaver _protocolSaver;
    private string _inputText = "5 -3 7\n-1 4 3\n6 -2 5";
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private string? _lastProtocol;

    public InverseMatrixViewModel(IJordan jordan, IProtocolSaver protocolSaver)
    {
        _jordan = jordan;
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
                Status = "Invalid matrix format. Use rows separated by newlines, numbers by space.";
                _lastProtocol = null;
                return;
            }
            var logger = new CalculationLogger();
            var inverter = new MatrixInverter(_jordan, logger);
            var result = inverter.Invert(matrix);
            _lastProtocol = logger.GetText();
            ResultText = FormatMatrix(result);
            Status = "Done.";
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Status = "Error: " + ex.Message;
            ResultText = string.Empty;
            _lastProtocol = null;
        }
    }

    private void SaveProtocol()
    {
        var content = string.IsNullOrEmpty(_lastProtocol)
            ? "=== Обернена матриця ===\r\n\r\nНемає протоколу. Натисніть «Run Computation»."
            : "=== Обернена матриця ===\r\n\r\n" + _lastProtocol;
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

    private static string FormatMatrix(double[,] m)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m.GetLength(1); j++)
                sb.Append(m[i, j].ToString("F2", CultureInfo.InvariantCulture)).Append("  ");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
