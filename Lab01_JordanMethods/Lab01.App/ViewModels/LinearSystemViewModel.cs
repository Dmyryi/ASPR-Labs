using System.Globalization;
using System.Windows.Input;
using Lab01.Logic;
using Lab01.Logic.BasicLogic;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public sealed class LinearSystemViewModel : ViewModelBase
{
    private readonly IJordan _jordan;
    private readonly IProtocolSaver _protocolSaver;

    private string _matrixText = "5 -3 7\n-1 4 3\n6 -2 5";
    private string _vectorText = "13 13 12";
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private string? _lastProtocol;

    public LinearSystemViewModel(IJordan jordan, IProtocolSaver protocolSaver)
    {
        _jordan = jordan;
        _protocolSaver = protocolSaver;
        ComputeCommand = new RelayCommand(Compute);
        SaveProtocolCommand = new RelayCommand(SaveProtocol);
    }

    public ICommand ComputeCommand { get; }
    public ICommand SaveProtocolCommand { get; }

    public string MatrixText
    {
        get => _matrixText;
        set { _matrixText = value; OnPropertyChanged(); }
    }

    public string VectorText
    {
        get => _vectorText;
        set { _vectorText = value; OnPropertyChanged(); }
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
            var matrix = MatrixTextParser.Parse(MatrixText);
            var vector = MatrixTextParser.ParseVector(VectorText);

            if (matrix is null || vector is null)
            {
                Status = "Невірний формат. Матриця: рядки переходом, числа пробілом. Вектор: числа пробілом.";
                return;
            }

            if (matrix.GetLength(0) != vector.Length)
            {
                Status = "Кількість рядків матриці має дорівнювати довжині вектора.";
                return;
            }

            var logger = new CalculationLogger();
            var inverter = new MatrixInverter(_jordan, logger);
            var solver = new InverseSolveStrategy(inverter, logger);
            var result = solver.Solve(matrix, vector);
            _lastProtocol = logger.GetText();

            ResultText = string.Join(", ", result.Select(x => x.ToString("F2", CultureInfo.InvariantCulture)));
            Status = "Готово.";
            CommandManager.InvalidateRequerySuggested();
        }
        catch (Exception ex)
        {
            Status = "Помилка: " + ex.Message;
            ResultText = string.Empty;
        }
    }

    private void SaveProtocol()
    {
        var content = string.IsNullOrEmpty(_lastProtocol)
            ? "=== СЛАР (метод оберненої матриці) ===\r\n\r\nНемає протоколу. Натисніть «Run»."
            : "=== СЛАР (метод оберненої матриці) ===\r\n\r\n" + _lastProtocol;
        _protocolSaver.Save(content);
        Status = "Протокол збережено у protocol.txt";
    }
}
