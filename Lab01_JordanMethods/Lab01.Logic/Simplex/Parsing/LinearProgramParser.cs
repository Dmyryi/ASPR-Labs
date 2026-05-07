using System.Globalization;
using System.Text.RegularExpressions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Models;

namespace Lab01.Logic.Simplex.Parsing;

public sealed class LinearProgramParser : ILinearProgramParser
{
    private static readonly Regex VariableRegex =
        new(@"([+\-]?)(\d*(?:\.\d+)?)x(\d+)", RegexOptions.Compiled);

    private static readonly Regex ConstantRegex =
        new(@"([+\-]?\d+(?:\.\d+)?)", RegexOptions.Compiled);

    public LinearProgram Parse(string objectiveText, string constraintsText)
    {
        if (string.IsNullOrWhiteSpace(objectiveText))
            throw new FormatException("Цільова функція не може бути порожньою.");

        var (objective, _) = ParseExpression(objectiveText);
        var constraints = ParseConstraints(constraintsText);

        if (constraints.Count == 0)
            throw new FormatException("Має бути задане хоча б одне обмеження.");

        int variableCount = Math.Max(
            objective.Keys.DefaultIfEmpty(0).Max(),
            constraints.SelectMany(c => c.Coefficients.Keys).DefaultIfEmpty(0).Max());

        if (variableCount <= 0)
            throw new FormatException("Не вдалося визначити змінні. Використовуйте формат x1, x2, ...");

        double[] vectorZ = new double[variableCount];
        foreach (var (index, value) in objective)
            vectorZ[index - 1] = value;

        double[,] matrixA = new double[constraints.Count, variableCount];
        double[] vectorB = new double[constraints.Count];

        for (int i = 0; i < constraints.Count; i++)
        {
            var c = constraints[i];
            int sign = c.Operator == ConstraintOperator.GreaterOrEqual ? -1 : 1;

            foreach (var (index, value) in c.Coefficients)
                matrixA[i, index - 1] = sign * value;

            vectorB[i] = sign * c.RightSide;
        }

        return new LinearProgram
        {
            ObjectiveCoefficients = vectorZ,
            ConstraintMatrix = matrixA,
            RightHandSide = vectorB
        };
    }

    private static List<ParsedConstraint> ParseConstraints(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<ParsedConstraint>();

        var lines = text
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        var result = new List<ParsedConstraint>(lines.Count);

        foreach (string line in lines)
        {
            ConstraintOperator op;
            string[] parts;

            if (line.Contains("<="))
            {
                op = ConstraintOperator.LessOrEqual;
                parts = line.Split("<=", StringSplitOptions.TrimEntries);
            }
            else if (line.Contains(">="))
            {
                op = ConstraintOperator.GreaterOrEqual;
                parts = line.Split(">=", StringSplitOptions.TrimEntries);
            }
            else if (line.Contains('='))
            {
                op = ConstraintOperator.Equal;
                parts = line.Split('=', StringSplitOptions.TrimEntries);
            }
            else
            {
                throw new FormatException($"Обмеження '{line}' має містити <=, >= або =.");
            }

            if (parts.Length != 2)
                throw new FormatException($"Невірний формат обмеження: '{line}'.");

            if (!double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double rightSide))
                throw new FormatException($"Невірне число справа: '{line}'.");

            var (coefficients, constant) = ParseExpression(parts[0]);
            double adjustedRightSide = rightSide - constant;

            if (op == ConstraintOperator.Equal)
            {
                result.Add(new ParsedConstraint(
                    new Dictionary<int, double>(coefficients),
                    ConstraintOperator.LessOrEqual,
                    adjustedRightSide));
                result.Add(new ParsedConstraint(
                    new Dictionary<int, double>(coefficients),
                    ConstraintOperator.GreaterOrEqual,
                    adjustedRightSide));
            }
            else
            {
                result.Add(new ParsedConstraint(coefficients, op, adjustedRightSide));
            }
        }

        return result;
    }

    private static (Dictionary<int, double> coefficients, double constant) ParseExpression(string expression)
    {
        string normalized = expression
            .Replace(" ", string.Empty)
            .Replace("*", string.Empty)
            .Replace(",", ".");

        var coefficients = new Dictionary<int, double>();
        double constant = 0;

        var matches = VariableRegex.Matches(normalized);
        foreach (Match match in matches)
        {
            string signToken = match.Groups[1].Value;
            string valueToken = match.Groups[2].Value;
            int variableIndex = int.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);

            double val = string.IsNullOrEmpty(valueToken)
                ? 1.0
                : double.Parse(valueToken, CultureInfo.InvariantCulture);
            if (signToken == "-") val *= -1.0;

            coefficients[variableIndex] = coefficients.GetValueOrDefault(variableIndex) + val;
        }

        string leftover = VariableRegex.Replace(normalized, "|");
        foreach (Match match in ConstantRegex.Matches(leftover))
        {
            constant += double.Parse(match.Value, CultureInfo.InvariantCulture);
        }

        return (coefficients, constant);
    }

    private enum ConstraintOperator
    {
        LessOrEqual,
        GreaterOrEqual,
        Equal
    }

    private sealed record ParsedConstraint(
        Dictionary<int, double> Coefficients,
        ConstraintOperator Operator,
        double RightSide);
}
