using System.Globalization;
using System.Text;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex;

public sealed class SimplexProtocol : ISimplexProtocol
{
    private readonly StringBuilder _sb = new();
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("uk-UA");

    private string F(double value) => value.ToString("F2", _culture);

    public void Start(OptimizationMode mode, string objective, string constraints)
    {
        _sb.Clear();
        _sb.AppendLine("Згенерований протокол обчислення:");
        _sb.AppendLine();
        _sb.AppendLine("Постановка задачі:");
        _sb.AppendLine();
        _sb.AppendLine($"Z = {objective}  ->  {(mode == OptimizationMode.Maximization ? "max" : "min")}");
        _sb.AppendLine();
        _sb.AppendLine("при обмеженнях:");
        foreach (var line in constraints.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {
            _sb.AppendLine(line.Trim());
        }
        _sb.AppendLine();
        _sb.AppendLine("Вхідна симплекс-таблиця:");
        _sb.AppendLine();
    }

    public void LogInitialTableau(SimplexTableau tableau) => WriteTableau(tableau);

    public void LogSection(string title)
    {
        _sb.AppendLine(title);
        _sb.AppendLine();
    }

    public void LogPivot(int step, SimplexTableau tableau, int pivotRow, int pivotCol)
    {
        _sb.AppendLine($"Крок #{step}:");
        _sb.AppendLine($"Розв’язувальний рядок:  {FormatRowLabel(tableau, pivotRow)}");
        _sb.AppendLine($"Розв’язувальний стовпець: {FormatColLabel(tableau, pivotCol)}");
        _sb.AppendLine($"Розв’язувальний елемент: {F(tableau.GetValue(pivotRow, pivotCol))}");
        _sb.AppendLine();
    }

    public void LogTableau(SimplexTableau tableau) => WriteTableau(tableau);

    public void LogBasicSolution(SimplexTableau tableau)
    {
        _sb.AppendLine(new string('=', 28));
        _sb.AppendLine("ЗНАЙДЕНО ОПОРНИЙ РОЗВ’ЯЗОК");
        _sb.AppendLine(new string('=', 28));
        _sb.AppendLine();

        double[] x = new double[tableau.ColsCount];
        for (int r = 0; r < tableau.RowsCount; r++)
        {
            int varId = tableau.BasisVariables[r];
            if (varId >= 0 && varId < tableau.ColsCount)
            {
                x[varId] = tableau.GetB(r);
            }
        }

        _sb.AppendLine("Знайдено опорний розв’язок:");
        _sb.AppendLine($"X = ({string.Join("; ", x.Select(F))})");
        _sb.AppendLine(new string('-', 60));
        _sb.AppendLine();
    }

    public void LogResult(SolverResult result)
    {
        _sb.AppendLine();
        _sb.AppendLine("Знайдено оптимальний розв’язок:");
        _sb.AppendLine($"X = ({string.Join("; ", result.X.Select(v => F(v)))})");
        _sb.AppendLine($"{(result.Success ? "" : "Неуспіх. ")}Z = {F(result.Z)}");
    }

    public string GetText() => _sb.ToString();

    private void WriteTableau(SimplexTableau tableau)
    {
        var colLabels = tableau.ColumnVariables.Select(id => FormatColumnVar(id, tableau.ColsCount)).ToArray();
        int maxLabelWidth = Math.Max(2, tableau.BasisVariables.Select(id => FormatRowVar(id, tableau.ColsCount)).Max(s => s.Length));

        _sb.Append(' ', maxLabelWidth).Append(" = ");
        foreach (var label in colLabels)
        {
            _sb.Append(label.PadLeft(8));
        }
        _sb.Append("   ").Append("1".PadLeft(8));
        _sb.AppendLine();
        _sb.AppendLine(new string('-', maxLabelWidth + 3 + (colLabels.Length + 1) * 8));

        for (int r = 0; r < tableau.RowsCount; r++)
        {
            _sb.Append(FormatRowVar(tableau.BasisVariables[r], tableau.ColsCount).PadLeft(maxLabelWidth));
            _sb.Append(" = ");
            for (int c = 0; c < tableau.ColsCount; c++)
            {
                _sb.Append(F(tableau.Data[r, c]).PadLeft(8));
            }
            _sb.Append(F(tableau.Data[r, tableau.ColsCount]).PadLeft(11));
            _sb.AppendLine();
        }

        _sb.Append("Z".PadLeft(maxLabelWidth)).Append(" = ");
        for (int c = 0; c < tableau.ColsCount; c++)
        {
            _sb.Append(F(tableau.Data[tableau.RowsCount, c]).PadLeft(8));
        }
        _sb.Append(F(tableau.Data[tableau.RowsCount, tableau.ColsCount]).PadLeft(11));
        _sb.AppendLine();
        _sb.AppendLine();
    }

    private string FormatRowLabel(SimplexTableau tableau, int row) =>
        FormatRowVar(tableau.BasisVariables[row], tableau.ColsCount);
    private string FormatColLabel(SimplexTableau tableau, int col) =>
        FormatColumnVar(tableau.ColumnVariables[col], tableau.ColsCount);

    private static string FormatRowVar(int id, int xCount)
    {
        if (id < xCount) return $"x{id + 1}";
        return $"y{id - xCount + 1}";
    }

    private static string FormatColumnVar(int id, int xCount)
    {
        if (id < xCount) return $"-x{id + 1}";
        return $"-y{id - xCount + 1}";
    }
}

