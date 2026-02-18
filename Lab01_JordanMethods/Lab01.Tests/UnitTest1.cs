using Lab01.Logic;
using Lab01.Logic.Interfaces;
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
        double[,] testRankMatrix =
        {
            {1, 2,3, 4},
        {2,4,6,8 }
        };

        
        double[,] variant10Matrix = {
    { -1, -2, 1 },
    {  2, -3, 4 },
    { -1,  3, 5 }
};

        double[] variant10VectorB = { 4, 5, 3 }; //

        private readonly IJordan _jordan;
        private readonly IMatrixInverter _matrix;
        private readonly IRankCalculator _rankCalculator;
        private readonly ILinearSystemSolver _linearSystemSolver;
        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;

            _jordan = new JordanSolver();
            _matrix = new MatrixInverter(_jordan);
            _rankCalculator = new RankCalculator(_jordan);
            _linearSystemSolver = new InverseSolveStrategy(_matrix);
        }
        [Fact]
        public void TestJordanSteps()
        {

            var result = _jordan.JordanMethod(testMatrix, 1,1);

        
            foreach (var step in result)
            {
                _output.WriteLine(step.ToString());
               
            }
        }


        [Fact]
        public void TestInvertMatrix() {
            var result = _matrix.Invert(testMatrix);
            foreach (var step in result)
            {
                _output.WriteLine(step.ToString("F2"));

            }
        }

        [Fact]
        public void TestRankCalculator() { 
        var result = _rankCalculator.Calculate(testRankMatrix);
            
                _output.WriteLine(result.ToString());

        }

        [Fact]
        public void TestLinearSystemSolver()
        {
            double[] vectorB = [13, 13, 12];

            var result = _linearSystemSolver.Solve(testMatrix, vectorB);
            foreach (var step in result)
            {
                _output.WriteLine(step.ToString("F2"));

            }
        }
    }
}