using Lab01.Logic.Interfaces;

namespace Lab01.Logic
{
    public class InverseSolveStrategy : ILinearSystemSolver
    {
        private readonly IMatrixInverter _inverter;
        private readonly CalculationLogger? _logger;

        public InverseSolveStrategy(IMatrixInverter inverter, CalculationLogger? logger = null)
        {
            _inverter = inverter;
            _logger = logger;
        }

        private static double[] CalculatedX(double[] vectorB, double[,] invertedMatrix)
        {
            int n = vectorB.Length;
            double[] x = new double[n];
            for (int i = 0; i < n; i++)
            {
                double sum = 0;
                for (int j = 0; j < invertedMatrix.GetLength(1); j++)
                {
                    sum += invertedMatrix[i, j] * vectorB[j];
                }
                x[i] = sum;
            }
            return x;
        }

        public double[] Solve(double[,] vectorA, double[] vectorB)
        {
            if (_logger != null)
            {
                _logger.LogTitle("Згенерований протокол обчислення:");
                _logger.LogSection("Знаходження розв'язків СЛАР 1-м методом (за допомогою оберненої матриці):");
            }

            var invertedMatrix = _inverter.Invert(vectorA);

            if (_logger != null)
            {
                _logger.LogVector("Вхідна матриця В:", vectorB);
                var x = CalculatedX(vectorB, invertedMatrix);
                _logger.LogFinalCalculation(x, invertedMatrix, vectorB);
                return x;
            }

            return CalculatedX(vectorB, invertedMatrix);
        }
    }
}
