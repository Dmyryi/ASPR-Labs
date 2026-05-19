using System.Globalization;
using System.Text;

namespace Lab01.Logic.Transportation;

public static class TransportationProtocolFormatter
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public static string Build(TransportationSolveResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Згенерований протокол обчислення:");
        sb.AppendLine();

        TransportationProblem p = result.Problem;
        AppendInputData(sb, p);

        if (!string.IsNullOrWhiteSpace(result.BalanceNote))
        {
            sb.AppendLine(result.BalanceNote);
            sb.AppendLine();
        }

        TransportationTrace? trace = result.Trace;
        if (trace is not null)
        {
            AppendNorthwestSection(sb, p, result.NorthwestCornerPlan, trace);
            AppendPotentialsSection(sb, p, trace);
        }
        else
        {
            AppendPlanSection(sb, "Опорний план (північно-західний кут)", result.NorthwestCornerPlan, p);
            AppendPlanSection(sb, "Оптимальний план (метод потенціалів)", result.OptimalPlan, p);
        }

        AppendPlanSection(sb, "Опорний план (мінімальний елемент)", result.MinimumElementPlan, p);
        AppendPlanSection(sb, "Оптимальний план (метод потенціалів, від мін. елемента)", result.OptimalPlan, p);

        if (!string.IsNullOrWhiteSpace(trace?.SimplexProtocolText))
        {
            sb.AppendLine();
            sb.AppendLine("Наведемо приклад пошук розв’язку транспортної задачі симплекс-методом:");
            sb.AppendLine();
            sb.Append(trace.SimplexProtocolText.TrimEnd());

            if (trace.SimplexSolution is not null)
            {
                sb.AppendLine();
                AppendPlanFromVector(sb, "Знайдено оптимальний план перевезень:", p, trace.SimplexSolution);
            }
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendInputData(StringBuilder sb, TransportationProblem p)
    {
        sb.AppendLine("Матриця вартостей:");
        sb.AppendLine(FormatMatrix(p.Costs, p.Rows, p.Cols));
        sb.AppendLine($"Вектор запасів: {FormatVectorInline(p.Supply)}");
        sb.AppendLine($"Вектор заявок: {FormatVectorInline(p.Demand)}");
        sb.AppendLine();
    }

    private static void AppendNorthwestSection(
        StringBuilder sb,
        TransportationProblem p,
        TransportationPlan plan,
        TransportationTrace trace)
    {
        sb.AppendLine("Пошук опорного плану перевезень методом північно-західного кута");
        sb.AppendLine();

        if (trace.NorthwestSteps.Count > 0)
        {
            var chain = string.Join(" -> ", trace.NorthwestSteps.Select(s =>
                $"(x{s.Row + 1}{s.Col + 1} = {Fmt(s.Amount)})"));
            sb.AppendLine(chain);
            sb.AppendLine();
        }

        sb.AppendLine(FormatPlanWithX(plan.Allocations, p.Rows, p.Cols));
        sb.AppendLine();
        sb.AppendLine("Вартість перевезень за опорним планом:");
        sb.AppendLine(FormatCostBreakdown(p, plan.Allocations));
        sb.AppendLine();
    }

    private static void AppendPotentialsSection(StringBuilder sb, TransportationProblem p, TransportationTrace trace)
    {
        sb.AppendLine("Пошук оптимального плану перевезень методом потенціалів");
        sb.AppendLine();

        int iteration = 1;
        foreach (PotentialIterationStep step in trace.PotentialIterations)
        {
            sb.AppendLine($"Ітерація {iteration}:");
            sb.AppendLine();
            sb.AppendLine($"Потенціали пунктів відправлення: {FormatVectorInline(step.SupplyPotentials)}");
            sb.AppendLine($"Потенціали пунктів призначення: {FormatVectorInline(step.DemandPotentials)}");
            sb.AppendLine();
            sb.AppendLine("Непрямі вартості:");
            sb.AppendLine(FormatIndirectMatrix(step.IndirectCosts, p, step.PlanBefore));
            sb.AppendLine();

            if (step.IsOptimal)
            {
                sb.AppendLine("Умова оптимальності виконується.");
                sb.AppendLine();
                AppendPlanFromMatrix(sb, "Оптимальний план перевезень:", p, step.PlanAfter);
                sb.AppendLine(FormatCostBreakdown(p, step.PlanAfter));
                sb.AppendLine();
                break;
            }

            sb.AppendLine("Умова оптимальності не виконується.");
            if (step.ProblematicCells.Count > 0)
            {
                string cells = string.Join("; ", step.ProblematicCells.Select(c => $"[{c.Row + 1}, {c.Col + 1}]"));
                sb.AppendLine($"Проблемні клітинки: {cells}");
            }

            if (step.EnterRow is int er && step.EnterCol is int ec)
            {
                sb.AppendLine($"Вводимо клітинку [{er + 1}, {ec + 1}], Δ = {Fmt(step.MaxDifference)}");
                sb.AppendLine();
                sb.AppendLine("Цикл перерахунку:");
                sb.AppendLine(FormatCycleGrid(p, step.PlanBefore, step.Cycle, er, ec, step.Theta));
                sb.AppendLine();
                sb.AppendLine($"λ = {Fmt(step.Theta)}");
                sb.AppendLine();
            }

            AppendPlanFromMatrix(sb, "Новий план перевезень:", p, step.PlanAfter);
            sb.AppendLine(FormatCostBreakdown(p, step.PlanAfter));
            sb.AppendLine();
            iteration++;
        }
    }

    private static void AppendPlanSection(StringBuilder sb, string title, TransportationPlan plan, TransportationProblem problem)
    {
        sb.AppendLine(title);
        sb.AppendLine(FormatPlanWithX(plan.Allocations, problem.Rows, problem.Cols));
        sb.AppendLine(FormatCostBreakdown(problem, plan.Allocations));
        sb.AppendLine();
    }

    private static void AppendPlanFromMatrix(StringBuilder sb, string title, TransportationProblem p, double[,] plan)
    {
        sb.AppendLine(title);
        sb.AppendLine(FormatPlanWithX(plan, p.Rows, p.Cols));
        sb.AppendLine();
    }

    private static void AppendPlanFromVector(StringBuilder sb, string title, TransportationProblem p, double[] x)
    {
        sb.AppendLine(title);
        int m = p.Cols;
        var plan = new double[p.Rows, p.Cols];
        for (int i = 0; i < p.Rows; i++)
        {
            for (int j = 0; j < p.Cols; j++)
            {
                int k = TransportationLpBuilder.VariableIndex(i, j, m);
                plan[i, j] = k < x.Length ? x[k] : 0;
            }
        }

        sb.AppendLine(FormatPlanWithX(plan, p.Rows, p.Cols));
        sb.AppendLine(FormatCostBreakdown(p, plan));
    }

    private static string FormatCycleGrid(
        TransportationProblem p,
        double[,] plan,
        List<(int Row, int Col)> cycle,
        int enterRow,
        int enterCol,
        double theta)
    {
        var marks = new string[p.Rows, p.Cols];
        for (int i = 0; i < p.Rows; i++)
        {
            for (int j = 0; j < p.Cols; j++)
                marks[i, j] = Fmt(plan[i, j]);
        }

        for (int k = 0; k < cycle.Count; k++)
        {
            (int i, int j) = cycle[k];
            if (i == enterRow && j == enterCol && k == 0)
                marks[i, j] = "λ";
            else if (k % 2 == 0)
                marks[i, j] = "+";
            else
                marks[i, j] = "-";
        }

        var sb = new StringBuilder();
        for (int i = 0; i < p.Rows; i++)
        {
            for (int j = 0; j < p.Cols; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(marks[i, j].PadLeft(4));
            }

            if (i < p.Rows - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatIndirectMatrix(double[,] indirect, TransportationProblem p, double[,] plan)
    {
        var basics = TransportationPlanHelper.GetBasicCells(p, plan);
        var sb = new StringBuilder();
        for (int i = 0; i < p.Rows; i++)
        {
            for (int j = 0; j < p.Cols; j++)
            {
                if (j > 0) sb.Append(' ');
                if (basics.Contains((i, j)) || double.IsNaN(indirect[i, j]))
                    sb.Append("x");
                else
                    sb.Append(Fmt(indirect[i, j]));
            }

            if (i < p.Rows - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatPlanWithX(double[,] plan, int rows, int cols)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (j > 0) sb.Append(' ');
                double v = plan[i, j];
                sb.Append(v > 1e-9 ? Fmt(v) : "x");
            }

            if (i < rows - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatCostBreakdown(TransportationProblem p, double[,] plan)
    {
        var terms = new List<string>();
        double sum = 0;
        for (int i = 0; i < p.Rows; i++)
        {
            for (int j = 0; j < p.Cols; j++)
            {
                double v = plan[i, j];
                if (v <= 1e-9) continue;
                terms.Add($"{Fmt(v)} * {Fmt(p.Costs[i, j])}");
                sum += v * p.Costs[i, j];
            }
        }

        return $"S = {string.Join(" + ", terms)} = {Fmt(sum)}";
    }

    private static string FormatMatrix(double[,] m, int rows, int cols)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(Fmt(m[i, j]));
            }

            if (i < rows - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatVectorInline(IReadOnlyList<double> v) =>
        string.Join(" ", v.Select(Fmt));

    private static string Fmt(double x)
    {
        if (Math.Abs(x - Math.Round(x)) < 1e-9)
            return ((long)Math.Round(x)).ToString(Uk);
        return x.ToString("0.##", Uk);
    }
}
