using System.Globalization;
using System.Text;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Simplex.Protocols;

public sealed class SimplexProtocol : ISimplexProtocol
{
    private readonly StringBuilder _sb = new();
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("uk-UA");

    private bool _gomoryMode;
    private int _problemVariableCount;
    private int _originalConstraintCount;
    private OptimizationMode _optimizationMode;
    private SimplexProtocolStyle _protocolStyle;

    public void Start(OptimizationMode mode, string objective, string constraints, LinearProgram? canonicalProgram = null, SimplexProtocolStyle style = SimplexProtocolStyle.PrimalZ)
    {
        _gomoryMode = false;
        _optimizationMode = mode;
        _protocolStyle = style;

        _sb.Clear();
        _sb.AppendLine("Згенерований протокол обчислення:");
        _sb.AppendLine();

        if (style == SimplexProtocolStyle.DualW)
        {
            _sb.AppendLine("Постановка двоїстої задачі:");
            _sb.AppendLine();
            _sb.AppendLine($"W = {objective}  ->  {(mode == OptimizationMode.Maximization ? "max" : "min")}");
        }
        else
        {
            _sb.AppendLine("Постановка задачі:");
            _sb.AppendLine();
            _sb.AppendLine($"Z = {objective}  ->  {(mode == OptimizationMode.Maximization ? "max" : "min")}");
        }

        _sb.AppendLine();
        _sb.AppendLine("при обмеженнях:");
        foreach (var line in constraints.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            _sb.AppendLine(line.Trim());

        if (canonicalProgram is not null && style == SimplexProtocolStyle.PrimalZ)
        {
            _sb.AppendLine();
            _sb.AppendLine("Перепишемо систему обмежень прямої задачі:");
            _sb.AppendLine();
            WriteCanonicalInequalities(canonicalProgram.ConstraintMatrix, canonicalProgram.RightHandSide);
        }

        _sb.AppendLine();
        if (style == SimplexProtocolStyle.DualW)
            _sb.AppendLine("Вхідна симплекс-таблиця для пари взаємно двоїстих задач:");
        else
            _sb.AppendLine("Вхідна симплекс-таблиця:");
        _sb.AppendLine();
    }

    public void StartGomory(OptimizationMode mode, string objective, string constraints, LinearProgram program)
    {
        ArgumentNullException.ThrowIfNull(program);

        _gomoryMode = true;
        _protocolStyle = SimplexProtocolStyle.PrimalZ;
        _optimizationMode = mode;
        _problemVariableCount = program.VariableCount;
        _originalConstraintCount = program.ConstraintCount;

        _sb.Clear();
        _sb.AppendLine("Згенерований протокол обчислення:");
        _sb.AppendLine();
        _sb.AppendLine("Постановка задачі:");
        _sb.AppendLine();
        _sb.AppendLine($"Z = {objective}  ->  {(mode == OptimizationMode.Maximization ? "max" : "min")}");
        _sb.AppendLine();
        _sb.AppendLine("при обмеженнях:");
        foreach (var line in constraints.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            _sb.AppendLine(line.Trim());

        _sb.AppendLine();
        _sb.AppendLine("Цілі числа:");
        var names = Enumerable.Range(1, _problemVariableCount).Select(i => $"x{i}");
        _sb.AppendLine(string.Join(", ", names));
        _sb.AppendLine();

        _sb.AppendLine("Перепишемо систему обмежень:");
        _sb.AppendLine();
        WriteCanonicalInequalities(program.ConstraintMatrix, program.RightHandSide);
        _sb.AppendLine();

        _sb.AppendLine("Вхідна симплекс-таблиця:");
        _sb.AppendLine();
    }

    private void WriteCanonicalInequalities(double[,] a, double[] b)
    {
        int rows = a.GetLength(0);
        int cols = a.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            var parts = new List<string>(cols + 1);
            for (int j = 0; j < cols; j++)
            {
                double coef = -a[i, j];
                parts.Add($"({Format(coef)}) * X[{j + 1}]");
            }
            parts.Add(Format(b[i]));
            _sb.AppendLine(string.Join(" + ", parts) + " ≥ 0");
        }
    }

    public void LogInitialTableau(SimplexTableau tableau) => WriteTableau(tableau);

    public void LogSection(string title)
    {
        _sb.AppendLine(title);
        _sb.AppendLine();
    }

    public void LogText(string text) => _sb.AppendLine(text);

    public void LogPivot(int? step, SimplexTableau tableau, int pivotRow, int pivotCol)
    {
        if (step.HasValue)
            _sb.AppendLine($"Крок #{step}:");

        _sb.AppendLine($"Розв’язувальний рядок:    {FormatRowLabel(tableau, pivotRow)}");
        _sb.AppendLine($"Розв’язувальний стовпець: {FormatColLabel(tableau, pivotCol)}");
        _sb.AppendLine();
    }

    public void LogTableau(SimplexTableau tableau) => WriteTableau(tableau);

    public void LogTableau(string caption, SimplexTableau tableau)
    {
        _sb.AppendLine(caption);
        _sb.AppendLine();
        WriteTableau(tableau);
    }

    public void LogBasicSolution(SimplexTableau tableau)
    {
        _sb.AppendLine("Знайдено опорний розв’язок:");
        _sb.AppendLine();

        double[] u = DualMultiplierExtractor.FromFinalTableau(tableau, _optimizationMode);

        if (_protocolStyle == SimplexProtocolStyle.PrimalZ)
        {
            _sb.AppendLine("Розв’язки прямої задачі:");
            _sb.AppendLine($"X = ({FormatDecisionVector(tableau)})");
            if (u.Length > 0)
            {
                _sb.AppendLine();
                _sb.AppendLine("Розв’язки двоїстої задачі:");
                _sb.AppendLine($"U = ({string.Join("; ", u.Select(Format))})");
            }
        }
        else
        {
            _sb.AppendLine("Розв’язки двоїстої задачі:");
            _sb.AppendLine($"X = ({FormatDecisionVector(tableau)})");
            if (u.Length > 0)
            {
                _sb.AppendLine();
                _sb.AppendLine("Оцінки обмежень:");
                _sb.AppendLine($"U = ({string.Join("; ", u.Select(Format))})");
            }
        }

        _sb.AppendLine();
    }

    public void LogContinuousOptimalSolution(SimplexTableau tableau)
    {
        double z = tableau.Data[tableau.RowsCount, tableau.ColsCount];
        double[] u = DualMultiplierExtractor.FromFinalTableau(tableau, _optimizationMode);

        _sb.AppendLine("Знайдено оптимальний розв’язок:");
        _sb.AppendLine();
        _sb.AppendLine("Розв’язки прямої задачі:");
        _sb.AppendLine($"X = ({FormatDecisionVector(tableau)})");
        if (u.Length > 0)
        {
            _sb.AppendLine();
            _sb.AppendLine("Розв’язки двоїстої задачі:");
            _sb.AppendLine($"U = ({string.Join("; ", u.Select(Format))})");
        }

        _sb.AppendLine();
        if (_optimizationMode == OptimizationMode.Maximization)
        {
            _sb.AppendLine($"Max (Z) = {Format(z)}");
            _sb.AppendLine($"Min (W) = {Format(z)}");
        }
        else
        {
            _sb.AppendLine($"Min (Z) = {Format(z)}");
            _sb.AppendLine($"Max (W) = {Format(z)}");
        }

        _sb.AppendLine();
    }

    public void LogGomoryFractionalSolution(int decisionVariableIndex0Based, double basisValue, double fractionalPart)
    {
        _ = fractionalPart;
        _sb.AppendLine(
            "Знайдено розв’язок, у якому змінні мають дробову частину, максимальна дробова частина у змінної: " +
            $"x{decisionVariableIndex0Based + 1} = {Format(basisValue)}");
        _sb.AppendLine();
    }

    public void LogGomoryCutEquation(int cutIndex1Based, SimplexTableau tableau, double[] cutRowCoefficients, double cutRhs)
    {
        _sb.AppendLine("Складено додаткове обмеження:");
        var parts = new List<string>();
        for (int j = 0; j < tableau.ColsCount; j++)
        {
            double beta = -cutRowCoefficients[j];
            int colVarId = tableau.ColumnVariables[j];
            string name = FormatStructuralName(colVarId, tableau);
            parts.Add($"{Format(beta)} * {name}");
        }

        parts.Add($"({Format(cutRhs)})");

        string lhs = string.Join(" + ", parts);
        _sb.AppendLine($"s{cutIndex1Based} = {lhs} ≥ 0");
        _sb.AppendLine();
    }

    public void LogResult(SolverResult result, string objectiveSymbol = "Z")
    {
        _sb.AppendLine();
        _sb.AppendLine("Знайдено оптимальний розв’язок:");
        _sb.AppendLine();

        if (_protocolStyle == SimplexProtocolStyle.PrimalZ)
        {
            _sb.AppendLine("Розв’язки прямої задачі:");
            _sb.AppendLine($"X = ({string.Join("; ", result.X.Select(Format))})");
            if (result.U.Length > 0)
            {
                _sb.AppendLine();
                _sb.AppendLine("Розв’язки двоїстої задачі:");
                _sb.AppendLine($"U = ({string.Join("; ", result.U.Select(Format))})");
            }

            _sb.AppendLine();
            _sb.AppendLine($"Max (Z) = {Format(result.Z)}");
            _sb.AppendLine($"Min (W) = {Format(result.Z)}");
        }
        else
        {
            _sb.AppendLine("Розв’язки двоїстої задачі:");
            _sb.AppendLine($"X = ({string.Join("; ", result.X.Select(Format))})");
            if (result.U.Length > 0)
            {
                _sb.AppendLine();
                _sb.AppendLine("Оцінки обмежень:");
                _sb.AppendLine($"U = ({string.Join("; ", result.U.Select(Format))})");
            }

            _sb.AppendLine();
            if (_optimizationMode == OptimizationMode.Maximization)
            {
                _sb.AppendLine($"Max ({objectiveSymbol}) = {Format(result.Z)}");
                _sb.AppendLine($"Min (Z) = {Format(result.Z)}");
            }
            else
            {
                _sb.AppendLine($"Min ({objectiveSymbol}) = {Format(result.Z)}");
                _sb.AppendLine($"Max (Z) = {Format(result.Z)}");
            }
        }

        if (!result.Success)
            _sb.AppendLine("Неуспіх.");
    }

    public string GetText() => _sb.ToString();

    private string OptimizationPrefix() =>
        _optimizationMode == OptimizationMode.Maximization ? "Max" : "Min";

    private string Format(double value) => value.ToString("F2", _culture);

    private string FormatDecisionVector(SimplexTableau tableau)
    {
        var x = new double[tableau.ProblemVariableCount];
        for (int row = 0; row < tableau.RowsCount; row++)
        {
            int id = tableau.BasisVariables[row];
            if (id >= 0 && id < tableau.ProblemVariableCount)
                x[id] = tableau.GetB(row);
        }
        return string.Join("; ", x.Select(Format));
    }

    private void WriteTableau(SimplexTableau tableau)
    {
        var colLabels = Enumerable.Range(0, tableau.ColsCount)
            .Select(c => FormatColLabel(tableau, c)).ToArray();

        int maxLabelWidth = Math.Max(
            2,
            Enumerable.Range(0, tableau.RowsCount)
                .Select(r => FormatRowLabel(tableau, r).Length)
                .DefaultIfEmpty(0).Max());

        maxLabelWidth = Math.Max(maxLabelWidth, "Z".Length);

        _sb.Append(' ', maxLabelWidth).Append(" = ");
        foreach (var label in colLabels)
            _sb.Append(label.PadLeft(8));
        _sb.Append("   ").Append("1".PadLeft(8));
        _sb.AppendLine();
        _sb.AppendLine(new string('-', maxLabelWidth + 3 + (colLabels.Length + 1) * 8));

        for (int r = 0; r < tableau.RowsCount; r++)
        {
            _sb.Append(FormatRowLabel(tableau, r).PadLeft(maxLabelWidth));
            _sb.Append(" = ");
            for (int c = 0; c < tableau.ColsCount; c++)
                _sb.Append(Format(tableau.Data[r, c]).PadLeft(8));
            _sb.Append(Format(tableau.Data[r, tableau.ColsCount]).PadLeft(11));
            _sb.AppendLine();
        }

        _sb.Append("Z".PadLeft(maxLabelWidth)).Append(" = ");
        for (int c = 0; c < tableau.ColsCount; c++)
            _sb.Append(Format(tableau.Data[tableau.RowsCount, c]).PadLeft(8));
        _sb.Append(Format(tableau.Data[tableau.RowsCount, tableau.ColsCount]).PadLeft(11));
        _sb.AppendLine();
        _sb.AppendLine();
    }

    private string FormatRowLabel(SimplexTableau tableau, int row) =>
        FormatStructuralName(tableau.BasisVariables[row], tableau);

    private string FormatColLabel(SimplexTableau tableau, int col) =>
        "-" + FormatStructuralName(tableau.ColumnVariables[col], tableau);

    private string FormatStructuralName(int id, SimplexTableau tableau)
    {
        if (_gomoryMode)
        {
            int n = _problemVariableCount;
            int m = _originalConstraintCount;
            if (id < n) return $"x{id + 1}";
            if (id < n + m) return $"y{id - n + 1}";
            return $"s{id - n - m + 1}";
        }

        int xCount = tableau.ProblemVariableCount;
        return id < xCount ? $"x{id + 1}" : $"y{id - xCount + 1}";
    }
}
