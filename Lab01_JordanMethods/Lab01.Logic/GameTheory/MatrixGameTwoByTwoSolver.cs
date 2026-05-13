namespace Lab01.Logic.GameTheory;

public static class MatrixGameTwoByTwoSolver
{
    private const double Tol = 1e-10;

    public static bool TrySolve(double a00, double a01, double a10, double a11, out double v, out double p0, out double q0)
    {
        double denom = a00 + a11 - a01 - a10;
        if (Math.Abs(denom) < Tol)
        {
            v = double.NaN;
            p0 = double.NaN;
            q0 = double.NaN;
            return false;
        }

        p0 = (a11 - a10) / denom;
        q0 = (a11 - a01) / denom;
        v = (a00 * a11 - a01 * a10) / denom;

        if (p0 < -Tol || p0 > 1 + Tol || q0 < -Tol || q0 > 1 + Tol)
        {
            v = double.NaN;
            p0 = double.NaN;
            q0 = double.NaN;
            return false;
        }

        p0 = Clamp01(p0);
        q0 = Clamp01(q0);
        return true;
    }

    private static double Clamp01(double x)
    {
        if (x < 0) return 0;
        if (x > 1) return 1;
        return x;
    }
}
