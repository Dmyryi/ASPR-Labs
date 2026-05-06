using Lab01.Logic.Models;

namespace Lab01.Logic.Gomori;

/// <summary>
/// Будує дані відсічення Гоморі для заданого рядка симплекс-таблиці.
/// Якщо <c>x_i = sum_j a_ij * (-ξ_j) + b_i</c> має дробове <c>b_i</c>, то відсічення
/// <c>s_i = sum_j (-β_ij) * (-ξ_j) + (-β_i) ≥ 0</c>, де <c>β = v - floor(v)</c>.
/// </summary>
public sealed class GomoryCutBuilder
{
    private readonly double _epsilon;

    public GomoryCutBuilder() : this(GomoryOptions.Default.IntegerEpsilon) { }

    public GomoryCutBuilder(double integerEpsilon)
    {
        _epsilon = integerEpsilon;
    }

    /// <summary>
    /// Шукає рядок з найбільшою дробовою частиною <c>b_i</c>, у якому базисною є оригінальна змінна.
    /// </summary>
    public int FindMostFractionalRow(SimplexTableau tableau)
    {
        int bestRow = -1;
        double bestFraction = 0;

        for (int row = 0; row < tableau.RowsCount; row++)
        {
            int basisVar = tableau.BasisVariables[row];
            if (basisVar < 0 || basisVar >= tableau.ProblemVariableCount) continue;

            double fraction = FractionalPart(tableau.GetB(row));
            if (fraction > bestFraction + _epsilon)
            {
                bestFraction = fraction;
                bestRow = row;
            }
        }

        return bestRow;
    }

    /// <summary>
    /// Будує відсічення для рядка <paramref name="row"/>: коефіцієнти <c>-β_ij</c> та правa частина <c>-β_i</c>.
    /// Кидає <see cref="Exceptions.NoIntegerSolutionException"/>, якщо коефіцієнти усі цілі — рядок не дає відсічення.
    /// </summary>
    public GomoryCut Build(SimplexTableau tableau, int row)
    {
        var coefficients = new double[tableau.ColsCount];
        bool hasNonZero = false;

        for (int col = 0; col < tableau.ColsCount; col++)
        {
            double frac = FractionalPart(tableau.GetValue(row, col));
            coefficients[col] = -frac;
            if (frac > _epsilon) hasNonZero = true;
        }

        double rhs = -FractionalPart(tableau.GetB(row));

        if (!hasNonZero)
            throw new Exceptions.NoIntegerSolutionException();

        return new GomoryCut(row, coefficients, rhs);
    }

    public bool IsInteger(double value) => FractionalPart(value) <= _epsilon;

    /// <summary>
    /// Дробова частина числа у межах [0, 1), з урахуванням epsilon (для протоколу Гоморі).
    /// </summary>
    public double FractionalComponent(double value) => FractionalPart(value);

    private double FractionalPart(double value)
    {
        double frac = value - Math.Floor(value);
        if (frac < _epsilon) return 0;
        if (1 - frac < _epsilon) return 0;
        return frac;
    }
}

/// <summary>
/// Дані відсічення Гоморі: вихідний рядок, коефіцієнти при стовпцевих змінних і права частина.
/// </summary>
public sealed record GomoryCut(int SourceRow, double[] Coefficients, double Rhs);
