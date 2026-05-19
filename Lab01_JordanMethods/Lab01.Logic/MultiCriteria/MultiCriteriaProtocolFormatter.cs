using System.Globalization;
using System.Text;
using Lab01.Logic.Models;

namespace Lab01.Logic.MultiCriteria;

public static class MultiCriteriaProtocolFormatter
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public static string Build(MultiCriteriaSolveResult result)
    {
        var sb = new StringBuilder();
        int k = result.Objectives.Count;
        int n = result.CompromiseSolution.Length;

        sb.AppendLine("Багатокритеріальна оптимізація (ігровий підхід)");
        sb.AppendLine();
        sb.AppendLine("Цільові функції:");
        for (int i = 0; i < k; i++)
            sb.AppendLine($"  Z{i + 1}: {result.Objectives[i].SourceText}");
        sb.AppendLine();
        sb.AppendLine("Обмеження:");
        foreach (string line in result.ConstraintsText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            sb.AppendLine($"  {line.Trim()}");
        sb.AppendLine();

        sb.AppendLine("Оптимальні вектори Xᵢ* (симплекс по кожному Zᵢ):");
        for (int i = 0; i < k; i++)
        {
            sb.Append($"  X(Z{i + 1}) = ({FormatVector(result.PerObjectiveSolutions[i].X)}); ");
            sb.AppendLine($"Z{i + 1}* = {Fmt(result.PerObjectiveSolutions[i].Z)}");
        }
        sb.AppendLine();

        sb.AppendLine("Матриця значень критеріїв F[i,j] = Zⱼ(Xᵢ*):");
        sb.AppendLine(FormatMatrix(result.ObjectiveValues, k, k, "F"));
        sb.AppendLine();

        sb.AppendLine("Матриця неоптимальностей Q:");
        sb.AppendLine(FormatMatrix(result.SuboptimalityMatrix, k, k, "Q"));
        sb.AppendLine();

        sb.AppendLine("Матриця гри G (Gᵢⱼ = max(Q) − Qᵢⱼ):");
        sb.AppendLine(FormatMatrix(result.GameMatrix, k, k, "G"));
        sb.AppendLine($"  max(Q) = {Fmt(MaxMatrix(result.SuboptimalityMatrix))}");
        sb.AppendLine();

        sb.AppendLine($"Розв'язок матричної гри: {result.GameResult.SolutionKind}");
        sb.AppendLine($"  Вага гравця 1 (λ): ({FormatVector(result.Weights)})");
        sb.AppendLine($"  Значення гри: {Fmt(result.GameResult.GameValue)}");
        sb.AppendLine();

        sb.AppendLine($"Компромісний розв'язок X_komp = ({FormatVector(result.CompromiseSolution)}):");
        for (int j = 0; j < k; j++)
        {
            double z = 0;
            for (int i = 0; i < n && i < result.Objectives[j].Coefficients.Length; i++)
                z += result.Objectives[j].Coefficients[i] * result.CompromiseSolution[i];
            sb.AppendLine($"  Z{j + 1}(X_komp) = {Fmt(z)}");
        }

        return sb.ToString();
    }

    private static string FormatMatrix(double[,] m, int rows, int cols, string prefix)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < rows; i++)
        {
            sb.Append($"  {prefix}{i + 1}: ");
            for (int j = 0; j < cols; j++)
            {
                if (j > 0)
                    sb.Append("  ");
                sb.Append(Fmt(m[i, j]).PadLeft(8));
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private static string FormatVector(double[] x)
    {
        return string.Join("; ", x.Select(v => Fmt(v)));
    }

    private static string Fmt(double v) =>
        v.ToString("0.####", Uk);

    private static double MaxMatrix(double[,] m)
    {
        double max = 0;
        for (int i = 0; i < m.GetLength(0); i++)
        {
            for (int j = 0; j < m.GetLength(1); j++)
                max = Math.Max(max, m[i, j]);
        }
        return max;
    }
}
