using Lab01.Logic.Models;

namespace Lab01.Logic.Gomori;

public sealed class GomoryCutBuilder
{
    private readonly double _epsilon;

    public GomoryCutBuilder() : this(GomoryOptions.Default.IntegerEpsilon) { }

    public GomoryCutBuilder(double integerEpsilon)
    {
        _epsilon = integerEpsilon;
    }

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

   
    public double FractionalComponent(double value) => FractionalPart(value);

    private double FractionalPart(double value)
    {
        double frac = value - Math.Floor(value);
        if (frac < _epsilon) return 0;
        if (1 - frac < _epsilon) return 0;
        return frac;
    }
}

public sealed record GomoryCut(int SourceRow, double[] Coefficients, double Rhs);
