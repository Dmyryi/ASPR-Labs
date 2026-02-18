using Lab01.Logic;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace Lab01.Tests
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _output;
        double[,] testMatrix = {
    { 5, -3, 7 },
    { -1, 4, 3 },
    { 6, -2, 5 }
};
        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public void TestJordanSteps()
        {

            
            var solver = new JordanSolver();
            var result = solver.JordanMethod(testMatrix, 1,1);

        
            foreach (var step in result)
            {
                _output.WriteLine(step.ToString());
               
            }
        }


        [Fact]
        public void TestInvertMatrix() {
            var solver = new JordanSolver();
            var result = solver.InvertMatrix(testMatrix);
            foreach (var step in result)
            {
                _output.WriteLine(step.ToString("F2"));

            }
        }
    }
}
