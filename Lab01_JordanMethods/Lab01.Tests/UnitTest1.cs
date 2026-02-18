using Lab01.Logic;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace Lab01.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;
        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void TestJordanSteps()
        {

            double[,] testMatrix = {
    { 5, -3, 7 },
    { -1, 4, 3 },
    { 6, -2, 5 }
};
            var solver = new JordanSolver();
            var result = solver.JordanMethod(testMatrix); // Твой метод из Class Library

            // Выводим все шаги в консоль тестов
            foreach (var step in result)
            {
                _output.WriteLine(step.ToString());
                // Тут выводишь матрицу step.CurrentMatrixState
            }
        }
    }
}
