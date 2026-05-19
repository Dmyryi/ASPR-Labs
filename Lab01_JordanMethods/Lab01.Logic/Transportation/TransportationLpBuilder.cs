using System.Globalization;
using System.Text;
using Lab01.Logic.Models;

namespace Lab01.Logic.Transportation;

public static class TransportationLpBuilder
{
    public static LinearProgram BuildProgram(TransportationProblem problem)
    {
        int n = problem.Rows;
        int m = problem.Cols;
        int vars = n * m;
        int rows = n + m;

        var objective = new double[vars];
        var a = new double[rows, vars];
        var b = new double[rows];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                int k = Index(i, j, m);
                objective[k] = problem.Costs[i, j];
                a[i, k] = 1;
            }

            b[i] = problem.Supply[i];
        }

        for (int j = 0; j < m; j++)
        {
            int row = n + j;
            for (int i = 0; i < n; i++)
                a[row, Index(i, j, m)] = -1;
            b[row] = -problem.Demand[j];
        }

        return new LinearProgram
        {
            ObjectiveCoefficients = objective,
            ConstraintMatrix = a,
            RightHandSide = b
        };
    }

    public static string BuildObjectiveText(TransportationProblem problem)
    {
        int m = problem.Cols;
        var parts = new List<string>();
        for (int i = 0; i < problem.Rows; i++)
        {
            for (int j = 0; j < problem.Cols; j++)
            {
                double c = problem.Costs[i, j];
                if (Math.Abs(c) < 1e-12) continue;
                parts.Add($"{Fmt(c)}x{Index(i, j, m) + 1}");
            }
        }

        return string.Join(" + ", parts);
    }

    public static string BuildDualObjectiveText(TransportationProblem problem)
    {
        int m = problem.Cols;
        var parts = new List<string>();
        for (int i = 0; i < problem.Rows; i++)
        {
            for (int j = 0; j < problem.Cols; j++)
            {
                double c = problem.Costs[i, j];
                if (Math.Abs(c) < 1e-12) continue;
                parts.Add($"{Fmt(-c)}x{Index(i, j, m) + 1}");
            }
        }

        return parts.Count == 0 ? "0" : string.Join(" + ", parts);
    }

    public static string BuildConstraintsText(TransportationProblem problem)
    {
        int m = problem.Cols;
        var sb = new StringBuilder();
        for (int i = 0; i < problem.Rows; i++)
        {
            var parts = new List<string>();
            for (int j = 0; j < problem.Cols; j++)
                parts.Add($"x{Index(i, j, m) + 1}");
            sb.AppendLine($"- {string.Join(" - ", parts)} + {Fmt(problem.Supply[i])} >= 0");
        }

        for (int j = 0; j < problem.Cols; j++)
        {
            var parts = new List<string>();
            for (int i = 0; i < problem.Rows; i++)
                parts.Add($"x{Index(i, j, m) + 1}");
            sb.AppendLine($"{string.Join(" + ", parts)} - {Fmt(problem.Demand[j])} >= 0");
        }

        return sb.ToString().TrimEnd();
    }

    public static int VariableIndex(int row, int col, int cols) => Index(row, col, cols);

    private static int Index(int row, int col, int cols) => row * cols + col;

    private static string Fmt(double x) =>
        Math.Abs(x - Math.Round(x)) < 1e-9
            ? ((long)Math.Round(x)).ToString(CultureInfo.InvariantCulture)
            : x.ToString("0.##", CultureInfo.InvariantCulture);
}
