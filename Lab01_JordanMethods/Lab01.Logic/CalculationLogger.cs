using System.Globalization;
using System.Text;

namespace Lab01.Logic;

public sealed class CalculationLogger
{
    private readonly StringBuilder _sb = new();
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("uk-UA");

    public void LogTitle(string title) => _sb.AppendLine(title);

    public void LogSection(string section) => _sb.AppendLine(section);

    public void LogStep(int stepNumber, int row, int col, double pivotValue)
    {
        _sb.AppendLine($"Крок #{stepNumber}:");
        _sb.AppendLine($"Розв'язувальний елемент: A[{row}, {col}] = {Format(pivotValue)}");
        _sb.AppendLine("Матриця після виконання ЗЖВ:");
    }

    public void LogMatrix(string label, double[,] matrix)
    {
        if (!string.IsNullOrEmpty(label))
            _sb.AppendLine(label);

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            var row = new List<string>();
            for (int j = 0; j < matrix.GetLength(1); j++)
                row.Add(Format(matrix[i, j]));
            _sb.AppendLine(string.Join("  ", row));
        }
        _sb.AppendLine();
    }

    public void LogVector(string label, double[] vector)
    {
        _sb.AppendLine(label);
        foreach (var v in vector)
            _sb.AppendLine(Format(v));
        _sb.AppendLine();
    }

    public void LogFinalCalculation(double[] x, double[,] invA, double[] b)
    {
        _sb.AppendLine("Обчислення розв'язків:");
        int n = x.Length;
        for (int i = 0; i < n; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < n; j++)
            {
                string bStr = b[j] < 0 ? $"({Format(b[j])})" : Format(b[j]);
                string aStr = invA[i, j] < 0 ? $"({Format(invA[i, j])})" : Format(invA[i, j]);
                terms.Add($"{bStr} * {aStr}");
            }
            _sb.AppendLine($"X[{i + 1}] = {string.Join(" + ", terms)} = {Format(x[i])}");
        }
    }

    public void Save(string path = "protocol.txt") => File.WriteAllText(path, _sb.ToString());

    public string GetText() => _sb.ToString();

    private string Format(double value) => value.ToString("F2", _culture);
}
