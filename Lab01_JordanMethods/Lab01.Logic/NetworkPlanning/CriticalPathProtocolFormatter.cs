using System.Globalization;
using System.Text;

namespace Lab01.Logic.NetworkPlanning;

public static class CriticalPathProtocolFormatter
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public static string Build(CriticalPathSolveResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Згенерований протокол обчислення:");
        sb.AppendLine();
        sb.AppendLine("Пошук критичного шляху виконання робіт");
        sb.AppendLine();

        sb.AppendLine("Розрахунок ранніх дат робіт:");
        foreach (NetworkTask task in result.Tasks.OrderBy(t => t.Id))
        {
            sb.AppendLine(
                $"Робота {task.Id}: тривалість = {task.Duration}, ранній старт = {task.EarlyStart}, ранній фініш = {task.EarlyFinish}");
        }

        sb.AppendLine($"Тривалість проєкту: {result.ProjectDuration}");
        sb.AppendLine();

        sb.AppendLine("Розрахунок пізніх дат робіт:");
        foreach (NetworkTask task in result.Tasks.OrderByDescending(t => t.Id))
        {
            sb.AppendLine(
                $"Робота {task.Id}: пізній фініш = {task.LateFinish}, пізній старт = {task.LateStart}, резерв часу = {task.Reserve}");
        }

        sb.AppendLine();
        sb.AppendLine("Розраховані параметри сіткового графіка робіт:");
        foreach (NetworkTask task in result.Tasks.OrderBy(t => t.Id))
        {
            string mark = task.IsCritical ? "(К) " : string.Empty;
            sb.AppendLine(
                $"{mark}Робота {task.Id}: люди = {task.People}, РС = {task.EarlyStart}, T = {task.Duration}, РФ = {task.EarlyFinish}, ПС = {task.LateStart}, R = {task.Reserve}, ПФ = {task.LateFinish}");
        }

        sb.AppendLine();
        sb.AppendLine($"Критичний шлях: {string.Join("-", result.CriticalPath)}");
        sb.AppendLine();
        AppendResourceLoad(sb, result);
        return sb.ToString().TrimEnd();
    }

    private static void AppendResourceLoad(StringBuilder sb, CriticalPathSolveResult result)
    {
        sb.AppendLine("Календарний план (ранні дати):");
        foreach (NetworkTask task in result.Tasks.OrderBy(t => t.Id))
        {
            sb.AppendLine(
                $"Робота {task.Id}: [{task.EarlyStart}, {task.EarlyFinish}], {task.People} чол." +
                (task.Reserve > 0 ? $", резерв до {task.LateFinish}" : string.Empty));
        }

        sb.AppendLine();
        sb.AppendLine("Завантаження людських ресурсів (ранній графік):");
        var loads = ResourceLoadCalculator.Compute(result.Tasks, result.ProjectDuration);
        foreach ((int from, int to, int people) in loads)
            sb.AppendLine($"[{from}, {to}): {people} чол.");
    }
}
