using Lab01.Logic.GameTheory;
using Xunit;

namespace Lab01.Tests;

public class NatureGameProtocolTests
{
    [Fact]
    public void Protocol_MethodicalExample1_KeyLines()
    {
        double[,] u =
        {
            { -1, 1, 1, 4 },
            { -1, -2, 2, 3 },
            { 3, -1, 3, 2 }
        };
        double[] p = { 0.2, 0.4, 0.1, 0.3 };

        string text = NatureGameProtocolFormatter.Build(u, 0.3, p);

        Assert.Contains("Матриця корисності U:", text);
        Assert.Contains("Критерій Вальда", text);
        Assert.Contains("min в рядку 1: -1", text);
        Assert.Contains("Оптимальні стратегії: A1 або A3", text);
        Assert.Contains("Критерій максимаксу", text);
        Assert.Contains("Оптимальні стратегії: A1", text);
        Assert.Contains("Коефіцієнт γ = 0,3", text);
        Assert.Contains("s1 = 0,3 * -1 + (1 - 0,3) * 4 = 2,5", text);
        Assert.Contains("Матриця ризиків:", text);
        Assert.Contains("4 0 2 0", text);
        Assert.Contains("Мінімальний елемент: 2", text);
        Assert.Contains("Ймовірності застосування природою своїх стратегій: p1 = 0,2; p2 = 0,4; p3 = 0,1; p4 = 0,3;", text);
        Assert.Contains("s1 = -1 * 0,2 + 1 * 0,4 + 1 * 0,1 + 4 * 0,3 = 1,50", text);
        Assert.Contains("s2 = -1 * 0,2 + -2 * 0,4 + 2 * 0,1 + 3 * 0,3 = 0,10", text);
        Assert.Contains("s3 = 3 * 0,2 + -1 * 0,4 + 3 * 0,1 + 2 * 0,3 = 1,10", text);
        Assert.Contains("Критерій Лапласа", text);
        Assert.Contains("s1 = -1 * 0,25 + 1 * 0,25 + 1 * 0,25 + 4 * 0,25 = 1,25", text);
        Assert.Contains("Найчастіше були оптимальними стратегії: A1", text);
    }
}
