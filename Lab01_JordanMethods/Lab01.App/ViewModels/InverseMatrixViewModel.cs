using System.Globalization;
using System.Windows.Input;
using Lab01.Logic;
using Lab01.Logic.Interfaces;

namespace Lab01.App.ViewModels;

public class InverseMatrixViewModel : ViewModelBase
{
    private readonly IMatrixInverter _inverter;
    private string _inputText = "5 -3 7\n-1 4 3\n6 -2 5";
    private string _resultText = string.Empty;
    private string _status = string.Empty;

    public InverseMatrixViewModel()
    {
        var jordan = new JordanSolver();
        _inverter = new MatrixInverter(jordan);
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
            var result = _inverter.Invert(matrix);
            ResultText = FormatMatrix(result);
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
