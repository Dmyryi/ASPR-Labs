using System.Globalization;
using System.Text;

namespace Lab01.App;

internal static class MatrixTextFormatter
{
    public static string Format(double[,] matrix)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
                sb.Append(matrix[i, j].ToString("F2", CultureInfo.InvariantCulture)).Append("  ");
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
