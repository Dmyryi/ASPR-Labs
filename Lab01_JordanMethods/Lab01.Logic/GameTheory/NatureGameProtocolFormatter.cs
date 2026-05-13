using System.Globalization;
using System.Text;

namespace Lab01.Logic.GameTheory;

public static class NatureGameProtocolFormatter
{
    private static readonly CultureInfo Uk = CultureInfo.GetCultureInfo("uk-UA");

    public static string Build(double[,] u, double hurwiczGamma, IReadOnlyList<double> bayesProbabilities)
    {
        ArgumentNullException.ThrowIfNull(u);
        ArgumentNullException.ThrowIfNull(bayesProbabilities);

        int rows = u.GetLength(0);
        int cols = u.GetLength(1);
        if (hurwiczGamma is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(hurwiczGamma));

        double[] p = NatureGameSolver.PrepareBayesProbabilities(bayesProbabilities, cols);
        NatureGameSolveResult r = NatureGameSolver.Solve(u, hurwiczGamma, p);

        var rowMin = new double[rows];
        var rowMax = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double mn = u[i, 0];
            double mx = u[i, 0];
            for (int j = 1; j < cols; j++)
            {
                double v = u[i, j];
                if (v < mn) mn = v;
                if (v > mx) mx = v;
            }

            rowMin[i] = mn;
            rowMax[i] = mx;
        }

        var hurwiczScores = new double[rows];
        for (int i = 0; i < rows; i++)
            hurwiczScores[i] = hurwiczGamma * rowMin[i] + (1.0 - hurwiczGamma) * rowMax[i];

        var rowMaxRegret = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double mx = r.SavageRegretMatrix[i, 0];
            for (int j = 1; j < cols; j++)
                if (r.SavageRegretMatrix[i, j] > mx) mx = r.SavageRegretMatrix[i, j];
            rowMaxRegret[i] = mx;
        }

        var bayesExpected = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double e = 0;
            for (int j = 0; j < cols; j++)
                e += p[j] * u[i, j];
            bayesExpected[i] = e;
        }

        double laplaceEq = 1.0 / cols;
        var laplaceMeans = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double s = 0;
            for (int j = 0; j < cols; j++)
                s += u[i, j];
            laplaceMeans[i] = s / cols;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Протокол обчислень — гра з природою");
        sb.AppendLine();
        sb.AppendLine("Матриця корисності U:");
        sb.AppendLine(FormatMatrix(u, rows, cols));
        sb.AppendLine();

        AppendWald(sb, rows, rowMin, r.WaldRows);
        AppendMaximax(sb, rows, rowMax, r.MaximaxRows);
        AppendHurwicz(sb, rows, hurwiczGamma, rowMin, rowMax, hurwiczScores, r.HurwiczRows);
        AppendSavage(sb, rows, cols, r.SavageRegretMatrix, rowMaxRegret, r.SavageRows);
        AppendBayes(sb, rows, cols, u, p, bayesExpected, r.BayesRows);
        AppendLaplace(sb, rows, cols, u, laplaceEq, laplaceMeans, r.LaplaceRows);

        sb.AppendLine();
        sb.AppendLine($"Найчастіше були оптимальними стратегії: {FormatStrategies(r.MostFrequentRows)}");

        return sb.ToString().TrimEnd();
    }

    private static void AppendWald(StringBuilder sb, int rows, double[] rowMin, IReadOnlyList<int> optimal)
    {
        sb.AppendLine("Критерій Вальда");
        for (int i = 0; i < rows; i++)
            sb.AppendLine($"min в рядку {i + 1}: {FmtLoose(rowMin[i])}");

        double best = rowMin.Max();
        sb.AppendLine($"Максимальний елемент: {FmtLoose(best)}");
        sb.AppendLine($"Оптимальні стратегії: {FormatStrategies(optimal)}");
        sb.AppendLine();
    }

    private static void AppendMaximax(StringBuilder sb, int rows, double[] rowMax, IReadOnlyList<int> optimal)
    {
        sb.AppendLine("Критерій максимаксу");
        for (int i = 0; i < rows; i++)
            sb.AppendLine($"max в рядку {i + 1}: {FmtLoose(rowMax[i])}");

        double best = rowMax.Max();
        sb.AppendLine($"Максимальний елемент: {FmtLoose(best)}");
        sb.AppendLine($"Оптимальні стратегії: {FormatStrategies(optimal)}");
        sb.AppendLine();
    }

    private static void AppendHurwicz(
        StringBuilder sb,
        int rows,
        double gamma,
        double[] rowMin,
        double[] rowMax,
        double[] hurwiczScores,
        IReadOnlyList<int> optimal)
    {
        sb.AppendLine("Критерій Гурвіца");
        sb.AppendLine($"Коефіцієнт γ = {FmtLoose(gamma)}");
        sb.AppendLine();
        for (int i = 0; i < rows; i++)
            sb.AppendLine($"min в рядку {i + 1}: {FmtLoose(rowMin[i])}");

        sb.AppendLine();
        for (int i = 0; i < rows; i++)
            sb.AppendLine($"max в рядку {i + 1}: {FmtLoose(rowMax[i])}");

        sb.AppendLine();
        string gStr = FmtLoose(gamma);
        for (int i = 0; i < rows; i++)
        {
            sb.AppendLine(
                $"s{i + 1} = {gStr} * {FmtLoose(rowMin[i])} + (1 - {gStr}) * {FmtLoose(rowMax[i])} = {FmtLoose(hurwiczScores[i])}");
        }

        double bestH = hurwiczScores.Max();
        sb.AppendLine($"Максимальний елемент: {FmtLoose(bestH)}");
        sb.AppendLine($"Оптимальні стратегії: {FormatStrategies(optimal)}");
        sb.AppendLine();
    }

    private static void AppendSavage(
        StringBuilder sb,
        int rows,
        int cols,
        double[,] regret,
        double[] rowMaxRegret,
        IReadOnlyList<int> optimal)
    {
        sb.AppendLine("Критерій Севіджа");
        sb.AppendLine("Матриця ризиків:");
        sb.AppendLine(FormatMatrix(regret, rows, cols));
        sb.AppendLine();
        for (int i = 0; i < rows; i++)
            sb.AppendLine($"max в рядку {i + 1}: {FmtLoose(rowMaxRegret[i])}");

        double best = rowMaxRegret.Min();
        sb.AppendLine($"Мінімальний елемент: {FmtLoose(best)}");
        sb.AppendLine($"Оптимальні стратегії: {FormatStrategies(optimal)}");
        sb.AppendLine();
    }

    private static void AppendBayes(
        StringBuilder sb,
        int rows,
        int cols,
        double[,] u,
        double[] p,
        double[] expected,
        IReadOnlyList<int> optimal)
    {
        sb.AppendLine("Критерій Байєса");
        var probLine = new StringBuilder("Ймовірності застосування природою своїх стратегій: ");
        for (int j = 0; j < cols; j++)
        {
            if (j > 0) probLine.Append("; ");
            probLine.Append($"p{j + 1} = {FmtProb(p[j])}");
        }

        probLine.Append(';');
        sb.AppendLine(probLine.ToString());
        sb.AppendLine();

        for (int i = 0; i < rows; i++)
        {
            var terms = new string[cols];
            for (int j = 0; j < cols; j++)
                terms[j] = $"{FmtLoose(u[i, j])} * {FmtProb(p[j])}";

            sb.AppendLine($"s{i + 1} = {string.Join(" + ", terms)} = {FmtResult(expected[i])}");
        }

        double best = expected.Max();
        sb.AppendLine($"Максимальний елемент: {FmtResult(best)}");
        sb.AppendLine($"Оптимальні стратегії: {FormatStrategies(optimal)}");
        sb.AppendLine();
    }

    private static void AppendLaplace(
        StringBuilder sb,
        int rows,
        int cols,
        double[,] u,
        double eq,
        double[] means,
        IReadOnlyList<int> optimal)
    {
        sb.AppendLine("Критерій Лапласа");
        string eqStr = FmtLoose(eq);
        for (int i = 0; i < rows; i++)
        {
            var terms = new string[cols];
            for (int j = 0; j < cols; j++)
                terms[j] = $"{FmtLoose(u[i, j])} * {eqStr}";

            sb.AppendLine($"s{i + 1} = {string.Join(" + ", terms)} = {FmtResult(means[i])}");
        }

        double best = means.Max();
        sb.AppendLine($"Максимальний елемент: {FmtResult(best)}");
        sb.AppendLine($"Оптимальні стратегії: {FormatStrategies(optimal)}");
    }

    private static string FormatMatrix(double[,] m, int rows, int cols)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (j > 0) sb.Append(' ');
                sb.Append(FmtLoose(m[i, j]));
            }

            if (i < rows - 1) sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatStrategies(IReadOnlyList<int> rowsZeroBased)
    {
        if (rowsZeroBased.Count == 0)
            return "—";
        return string.Join(" або ", rowsZeroBased.Distinct().OrderBy(i => i).Select(i => $"A{i + 1}"));
    }

    private static string FmtLoose(double x) => x.ToString("0.##", Uk);

    private static string FmtProb(double x) => x.ToString("N1", Uk);

    private static string FmtResult(double x) => x.ToString("N2", Uk);
}
