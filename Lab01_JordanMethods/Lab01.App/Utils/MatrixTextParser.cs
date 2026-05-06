using System.Globalization;

namespace Lab01.App;

internal static class MatrixTextParser
{
    public static double[,]? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var rows = text.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (rows.Length == 0) return null;

        var firstRow = rows[0].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (firstRow.Length == 0) return null;

        var matrix = new double[rows.Length, firstRow.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            var values = rows[i].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (values.Length != firstRow.Length) return null;

            for (int j = 0; j < values.Length; j++)
            {
                if (!double.TryParse(values[j], NumberStyles.Any, CultureInfo.InvariantCulture, out matrix[i, j]))
                    return null;
            }
        }
        return matrix;
    }

    public static double[]? ParseVector(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        var values = text.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length == 0) return null;

        var vector = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            if (!double.TryParse(values[i], NumberStyles.Any, CultureInfo.InvariantCulture, out vector[i]))
                return null;
        }
        return vector;
    }
}
