using System.Globalization;
using System.Text.RegularExpressions;
using Lab01.Logic.Interfaces;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.MultiCriteria;

public sealed class MultiCriteriaProblemParser
{
    private static readonly Regex ModeSuffixRegex =
        new(@"\s+(max|min)\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly ILinearProgramParser _linearParser;

    public MultiCriteriaProblemParser(ILinearProgramParser linearParser)
    {
        _linearParser = linearParser;
    }

    public IReadOnlyList<MultiCriteriaObjective> ParseObjectives(string objectivesText)
    {
        if (string.IsNullOrWhiteSpace(objectivesText))
            throw new FormatException("Задайте хоча б одну цільову функцію.");

        var lines = objectivesText
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        if (lines.Count == 0)
            throw new FormatException("Задайте хоча б одну цільову функцію.");

        var result = new List<MultiCriteriaObjective>(lines.Count);
        foreach (string line in lines)
        {
            Match m = ModeSuffixRegex.Match(line);
            if (!m.Success)
                throw new FormatException($"Рядок '{line}' має закінчуватися на max або min.");

            OptimizationMode mode = m.Groups[1].Value.Equals("max", StringComparison.OrdinalIgnoreCase)
                ? OptimizationMode.Maximization
                : OptimizationMode.Minimization;

            string expr = line[..m.Index].Trim();
            var lp = _linearParser.Parse(expr, "x1 >= 0");
            result.Add(new MultiCriteriaObjective
            {
                Coefficients = lp.ObjectiveCoefficients,
                Mode = mode,
                SourceText = line
            });
        }

        return result;
    }

    public MultiCriteriaConstraintSet ParseConstraints(string constraintsText, int variableCountHint)
    {
        if (string.IsNullOrWhiteSpace(constraintsText))
            throw new FormatException("Має бути задане хоча б одне обмеження.");

        var equalities = new List<(double[] Coefficients, double RightSide)>();
        var inequalities = new List<(double[] Coefficients, double RightSide)>();
        int maxVar = variableCountHint;

        var lines = constraintsText
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        foreach (string line in lines)
        {
            if (line.Contains('=') && !line.Contains("<=") && !line.Contains(">="))
            {
                var lp = _linearParser.Parse("x1", line);
                maxVar = Math.Max(maxVar, lp.VariableCount);
                var coeff = ExtractRow(lp, 0, maxVar);
                equalities.Add((coeff, lp.RightHandSide[0]));
            }
            else
            {
                var lp = _linearParser.Parse("x1", line);
                maxVar = Math.Max(maxVar, lp.VariableCount);
                for (int i = 0; i < lp.ConstraintCount; i++)
                    inequalities.Add((ExtractRow(lp, i, maxVar), lp.RightHandSide[i]));
            }
        }

        return new MultiCriteriaConstraintSet
        {
            VariableCount = maxVar,
            Equalities = equalities,
            Inequalities = inequalities
        };
    }

    private static double[] ExtractRow(Models.LinearProgram lp, int row, int n)
    {
        var v = new double[n];
        for (int j = 0; j < lp.VariableCount && j < n; j++)
            v[j] = lp.ConstraintMatrix[row, j];
        return v;
    }
}
