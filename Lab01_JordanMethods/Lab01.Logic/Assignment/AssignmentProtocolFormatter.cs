using System.Globalization;
using System.Text;

namespace Lab01.Logic.Assignment;

public static class AssignmentProtocolFormatter
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public static string Build(AssignmentSolveResultWithTrace result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Згенерований протокол обчислення:");
        sb.AppendLine();

        AssignmentSolveResult h = result.Hungarian;
        AssignmentTrace trace = result.Trace;
        int n = h.Size;

        sb.AppendLine("Матриця вартостей:");
        sb.AppendLine(FormatMatrix(h.OriginalCosts, n));
        sb.AppendLine();

        sb.AppendLine("Пошук мінімальних елементів у кожному рядку та віднімання його від кожного елемента в рядку:");
        foreach (RowReductionStep step in trace.RowReductions)
            sb.AppendLine($"Рядок {step.Row + 1}: min = {Fmt(step.Minimum)}");
        sb.AppendLine();
        sb.AppendLine(FormatMatrixAfterRowReduction(h.OriginalCosts, trace.RowReductions, n));
        sb.AppendLine();

        sb.AppendLine("Пошук мінімальних елементів у кожному стовпці та віднімання його від кожного елемента в стовпці:");
        foreach (ColumnReductionStep step in trace.ColumnReductions)
            sb.AppendLine($"Стовпець {step.Column + 1}: min = {Fmt(step.Minimum)}");
        sb.AppendLine();

        int coverPass = 1;
        foreach (CoverIterationStep cover in trace.CoverIterations)
        {
            sb.AppendLine($"Пошук оптимальних призначень (ітерація {coverPass}):");
            sb.AppendLine(FormatMatrix(cover.MatrixBefore, n));
            sb.AppendLine($"Кількість ліній = {cover.LineCount}, призначень = {cover.MatchingCount}, всього робіт = {n}");
            if (cover.IsOptimal)
                sb.AppendLine("Матрицю оптимальних призначень знайдено!");
            else
            {
                sb.AppendLine("Матрицю оптимальних призначень не знайдено.");
                sb.AppendLine($"min (невикреслені) = {Fmt(cover.AdjustmentMin)}");
                sb.AppendLine("Матриця після коригування:");
                sb.AppendLine(FormatMatrix(cover.MatrixAfter, n));
            }

            sb.AppendLine();
            coverPass++;
        }

        sb.AppendLine("Заповнення матриці призначень:");
        foreach (AssignmentFillStep step in trace.AssignmentSteps)
        {
            sb.AppendLine($"k = {step.AssignmentIndex}: {step.Description}, обрано [{step.Row + 1}, {step.Column + 1}]");
        }

        sb.AppendLine();
        sb.AppendLine("Матриця призначень:");
        sb.AppendLine(FormatAssignmentMatrix(h.AssignmentMatrix, n));
        sb.AppendLine();
        sb.AppendLine(FormatCostBreakdown(h.OriginalCosts, h.AssignedColumns, n, h.TotalCost));

        if (!string.IsNullOrWhiteSpace(trace.SimplexProtocolText))
        {
            sb.AppendLine();
            sb.AppendLine("Наведемо приклад пошук розв'язку задачі про призначення симплекс-методом:");
            sb.AppendLine();
            sb.Append(trace.SimplexProtocolText.TrimEnd());
        }

        return sb.ToString().TrimEnd();
    }

    private static string FormatMatrixAfterRowReduction(double[,] original, List<RowReductionStep> steps, int n)
    {
        var m = Clone(original);
        foreach (RowReductionStep step in steps)
        {
            for (int j = 0; j < n; j++)
                m[step.Row, j] -= step.Minimum;
        }

        return FormatMatrix(m, n);
    }

    private static string FormatMatrix(double[,] matrix, int n)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(Fmt(matrix[i, j]));
            }

            if (i < n - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatAssignmentMatrix(int[,] assignment, int n)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(assignment[i, j].ToString(Uk));
            }

            if (i < n - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatCostBreakdown(double[,] costs, int[] assignedCol, int n, double total)
    {
        var terms = new List<string>();
        for (int i = 0; i < n; i++)
            terms.Add(Fmt(costs[i, assignedCol[i]]));
        return $"Загальна вартість робіт: S = {string.Join(" + ", terms)} = {Fmt(total)}";
    }

    private static double[,] Clone(double[,] source)
    {
        int n = source.GetLength(0);
        var copy = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                copy[i, j] = source[i, j];
        }

        return copy;
    }

    private static string Fmt(double x)
    {
        if (Math.Abs(x - Math.Round(x)) < 1e-9)
            return ((long)Math.Round(x)).ToString(Uk);
        return x.ToString("0.##", Uk);
    }
}
