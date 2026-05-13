namespace Lab01.Logic.GameTheory;

public static class NatureGameSolver
{
    public static double[] PrepareBayesProbabilities(IReadOnlyList<double> bayesProbabilities, int cols)
    {
        ArgumentNullException.ThrowIfNull(bayesProbabilities);

        if (bayesProbabilities.Count != cols)
            throw new ArgumentException($"Кількість ймовірностей ({bayesProbabilities.Count}) має дорівнювати кількості стовпців ({cols}).", nameof(bayesProbabilities));

        double sumP = 0;
        for (int j = 0; j < cols; j++)
        {
            double p = bayesProbabilities[j];
            if (p < 0 || double.IsNaN(p) || double.IsInfinity(p))
                throw new ArgumentException("Ймовірності мають бути невід’ємними скінченними числами.", nameof(bayesProbabilities));
            sumP += p;
        }

        if (sumP <= 0)
            throw new ArgumentException("Сума ймовірностей має бути додатною.", nameof(bayesProbabilities));

        const double tol = 1e-6;
        if (Math.Abs(sumP - 1.0) <= tol)
        {
            var copy = new double[cols];
            for (int j = 0; j < cols; j++)
                copy[j] = bayesProbabilities[j];
            return copy;
        }

        var normalized = new double[cols];
        for (int j = 0; j < cols; j++)
            normalized[j] = bayesProbabilities[j] / sumP;
        return normalized;
    }

    public static NatureGameSolveResult Solve(double[,] u, double hurwiczGamma, IReadOnlyList<double> bayesProbabilities)
    {
        ArgumentNullException.ThrowIfNull(u);
        ArgumentNullException.ThrowIfNull(bayesProbabilities);

        int rows = u.GetLength(0);
        int cols = u.GetLength(1);
        if (rows < 1 || cols < 1)
            throw new ArgumentException("Матриця U повинна мати хоча б один рядок і один стовпець.", nameof(u));

        if (hurwiczGamma is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(hurwiczGamma), "Коефіцієнт Гурвіца γ має бути в діапазоні [0; 1].");

        double[] p = PrepareBayesProbabilities(bayesProbabilities, cols);

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

        IReadOnlyList<int> wald = ArgMaxIndices(rowMin, maximize: true);
        IReadOnlyList<int> maximax = ArgMaxIndices(rowMax, maximize: true);

        var hurwiczScores = new double[rows];
        for (int i = 0; i < rows; i++)
            hurwiczScores[i] = hurwiczGamma * rowMin[i] + (1.0 - hurwiczGamma) * rowMax[i];
        IReadOnlyList<int> hurwicz = ArgMaxIndices(hurwiczScores, maximize: true);

        double[,] regret = BuildRegretMatrix(u, rows, cols);
        var rowMaxRegret = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double mx = regret[i, 0];
            for (int j = 1; j < cols; j++)
                if (regret[i, j] > mx) mx = regret[i, j];
            rowMaxRegret[i] = mx;
        }

        IReadOnlyList<int> savage = ArgMaxIndices(rowMaxRegret, maximize: false);

        var laplaceMeans = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double s = 0;
            for (int j = 0; j < cols; j++)
                s += u[i, j];
            laplaceMeans[i] = s / cols;
        }

        IReadOnlyList<int> laplace = ArgMaxIndices(laplaceMeans, maximize: true);

        var bayesExpected = new double[rows];
        for (int i = 0; i < rows; i++)
        {
            double e = 0;
            for (int j = 0; j < cols; j++)
                e += p[j] * u[i, j];
            bayesExpected[i] = e;
        }

        IReadOnlyList<int> bayes = ArgMaxIndices(bayesExpected, maximize: true);

        var counts = new int[rows];
        void AddVotes(IReadOnlyList<int> idx)
        {
            foreach (int i in idx)
                counts[i]++;
        }

        AddVotes(wald);
        AddVotes(maximax);
        AddVotes(hurwicz);
        AddVotes(savage);
        AddVotes(bayes);
        AddVotes(laplace);

        int bestCount = counts.Max();
        var frequent = new List<int>();
        for (int i = 0; i < rows; i++)
        {
            if (counts[i] == bestCount)
                frequent.Add(i);
        }

        return new NatureGameSolveResult
        {
            WaldRows = wald,
            MaximaxRows = maximax,
            HurwiczRows = hurwicz,
            SavageRows = savage,
            BayesRows = bayes,
            LaplaceRows = laplace,
            SavageRegretMatrix = regret,
            MostFrequentRows = frequent
        };
    }

    private static double[,] BuildRegretMatrix(double[,] u, int rows, int cols)
    {
        var colMax = new double[cols];
        for (int j = 0; j < cols; j++)
        {
            double mx = u[0, j];
            for (int i = 1; i < rows; i++)
                if (u[i, j] > mx) mx = u[i, j];
            colMax[j] = mx;
        }

        var r = new double[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
                r[i, j] = colMax[j] - u[i, j];
        }

        return r;
    }

    private static IReadOnlyList<int> ArgMaxIndices(IReadOnlyList<double> values, bool maximize)
    {
        int n = values.Count;
        if (n == 0)
            return Array.Empty<int>();

        double best = values[0];
        for (int i = 1; i < n; i++)
        {
            double v = values[i];
            if (maximize)
            {
                if (v > best) best = v;
            }
            else
            {
                if (v < best) best = v;
            }
        }

        var list = new List<int>();
        const double eps = 1e-9;
        for (int i = 0; i < n; i++)
        {
            double v = values[i];
            bool isBest = maximize ? Math.Abs(v - best) <= eps * (1 + Math.Abs(best)) : Math.Abs(v - best) <= eps * (1 + Math.Abs(best));
            if (isBest)
                list.Add(i);
        }

        return list;
    }
}
