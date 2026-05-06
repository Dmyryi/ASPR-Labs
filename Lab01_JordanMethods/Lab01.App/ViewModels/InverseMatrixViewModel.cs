using System.Globalization;
using System.Text;
using System.Windows.Input;
using Lab01.Logic;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public sealed class InverseMatrixViewModel : ViewModelBase
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

            var logger = new CalculationLogger();
            var inverter = new Logic.BasicLogic.MatrixInverter(_jordan, logger);
            var result = inverter.Invert(matrix);
            _lastProtocol = logger.GetText();

            ResultText = MatrixTextFormatter.Format(result);
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
            ? "=== Обернена матриця ===\r\n\r\nНемає протоколу. Натисніть «Run»."
            : "=== Обернена матриця ===\r\n\r\n" + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Протокол збережено у protocol.txt";
    }
}
