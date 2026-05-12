using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex;

public static class DualMultiplierExtractor
{
    public static double[] FromFinalTableau(SimplexTableau tableau, OptimizationMode mode)
    {
        int m = tableau.RowsCount;
        int n = tableau.ProblemVariableCount;
        var u = new double[m];

        for (int i = 0; i < m; i++)
        {
            int slackVarId = n + i;
            int col = FindColumnForVariable(tableau, slackVarId);
            if (col < 0)
            {
                u[i] = 0;
                continue;
            }

            double zj = tableau.GetZ(col);
            u[i] = mode == OptimizationMode.Maximization ? zj : -zj;
        }

        return u;
    }

    private static int FindColumnForVariable(SimplexTableau tableau, int variableId)
    {
        for (int j = 0; j < tableau.ColsCount; j++)
        {
            if (tableau.ColumnVariables[j] == variableId)
                return j;
        }

        return -1;
    }
}
