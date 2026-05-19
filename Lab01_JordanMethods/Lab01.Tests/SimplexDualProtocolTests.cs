using Lab01.Logic.Models;
using Lab01.Logic.Simplex;
using Lab01.Logic.Simplex.Parsing;
using Lab01.Logic.Simplex.Protocols;
using Xunit;

namespace Lab01.Tests;

public class SimplexDualProtocolTests
{
    [Fact]
    public void DualProtocol_StartContainsCanonicalBlockAndTableTitle()
    {
        const string objective = "4x1 + 3x2";
        const string constraints = "x1 + x2 >= 2\n2x1 + x2 >= 1";

        var parser = new LinearProgramParser();
        LinearProgram program = parser.Parse(objective, constraints);

        var protocol = new SimplexProtocol();
        protocol.Start(
            OptimizationMode.Maximization,
            objective,
            constraints,
            program,
            SimplexProtocolStyle.DualW);

        string text = protocol.GetText();

        Assert.Contains("Постановка задачі:", text);
        Assert.Contains("Перепишемо систему обмежень:", text);
        Assert.Contains("X[1]", text);
        Assert.Contains("≥ 0", text);
        Assert.Contains("Вхідна симплекс-таблиця:", text);
        Assert.DoesNotContain("Вхідна симплекс-таблиця для пари взаємно двоїстих задач", text);
    }
}
