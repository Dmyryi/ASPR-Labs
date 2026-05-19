using System.Globalization;
using System.Text;

namespace Lab01.Logic.Assignment;

public static class AssignmentLpBuilder
{
    public static string BuildObjectiveText(double[,] costs, int n)
    {
        var parts = new List<string>();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double c = costs[i, j];
                if (Math.Abs(c) < 1e-12) continue;
                parts.Add($"{Fmt(c)}x{Index(i, j, n) + 1}");
            }
        }

        return string.Join(" + ", parts);
    }

    public static string BuildDualObjectiveText(double[,] costs, int n)
    {
        var parts = new List<string>();
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double c = costs[i, j];
                if (Math.Abs(c) < 1e-12) continue;
                parts.Add($"{Fmt(-c)}x{Index(i, j, n) + 1}");
            }
        }

        return parts.Count == 0 ? "0" : string.Join(" + ", parts);
    }

    public static string BuildConstraintsText(int n)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < n; i++)
        {
            var parts = new List<string>();
            for (int j = 0; j < n; j++)
                parts.Add($"x{Index(i, j, n) + 1}");
            sb.AppendLine($"- {string.Join(" - ", parts)} + 1 >= 0");
        }

        for (int j = 0; j < n; j++)
        {
            var parts = new List<string>();
            for (int i = 0; i < n; i++)
                parts.Add($"x{Index(i, j, n) + 1}");
            sb.AppendLine($"{string.Join(" + ", parts)} - 1 >= 0");
        }

        return sb.ToString().TrimEnd();
    }

    public static int VariableIndex(int row, int col, int n) => Index(row, col, n);

    private static int Index(int row, int col, int n) => row * n + col;

    private static string Fmt(double x) =>
        Math.Abs(x - Math.Round(x)) < 1e-9
            ? ((long)Math.Round(x)).ToString(CultureInfo.InvariantCulture)
            : x.ToString("0.##", CultureInfo.InvariantCulture);
}
