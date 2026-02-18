using System.Globalization;
using System.Windows.Input;
using Lab01.Logic;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public class LinearSystemViewModel : ViewModelBase
{
    private ILinearSystemSolver _solver;
    private string _matrixText = "5 -3 7\n-1 4 3\n6 -2 5";
    private string _vectorText = "13 13 12";
    private string _resultText = string.Empty;
    private string _status = string.Empty;
    private CalculationLogger? _lastLogger;

    public LinearSystemViewModel()
    {
        var jordan = new JordanSolver();
        var inverter = new MatrixInverter(jordan);
        _solver = new InverseSolveStrategy(inverter);
        ComputeCommand = new RelayCommand(Compute);
        SaveProtocolCommand = new RelayCommand(SaveProtocol, () => _lastLogger != null);
    }

    public ICommand SaveProtocolCommand { get; }

    public string MatrixText
    {
        get => _matrixText;
        set
        {
            _matrixText = value;
            OnPropertyChanged();
        }
    }

    public string VectorText
    {
        get => _vectorText;
        set
        {
            _vectorText = value;
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
            var matrix = ParseMatrix(MatrixText);
            var vector = ParseVector(VectorText);
            if (matrix == null || vector == null)
            {
                Status = "Invalid format. Matrix: rows by newline, numbers by space. Vector: numbers by space.";
                return;
            }
            if (matrix.GetLength(0) != vector.Length)
            {
                Status = "Matrix rows must match vector length.";
                return;
            }
            var logger = new CalculationLogger();
            var jordan = new JordanSolver();
            var inverter = new MatrixInverter(jordan, logger);
            var solver = new InverseSolveStrategy(inverter, logger);
            var result = solver.Solve(matrix, vector);
            _lastLogger = logger;
            CommandManager.InvalidateRequerySuggested();
            ResultText = string.Join(", ", result.Select(x => x.ToString("F2", CultureInfo.InvariantCulture)));
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

    private static double[]? ParseVector(string text)
    {
        var vals = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (vals.Length == 0) return null;
        var v = new double[vals.Length];
        for (int i = 0; i < vals.Length; i++)
        {
            if (!double.TryParse(vals[i], NumberStyles.Any, CultureInfo.InvariantCulture, out v[i]))
                return null;
        }
        return v;
    }

    private void SaveProtocol()
    {
        _lastLogger?.Save("protocol.txt");
        Status = "Protocol saved to protocol.txt";
    }
}
