using Lab01.Logic.Interfaces;

namespace Lab01.Logic
{
    public class MatrixInverter : IMatrixInverter
    {
        private readonly IJordan _jordan;
        private readonly CalculationLogger? _logger;

        public MatrixInverter(IJordan jordan, CalculationLogger? logger = null)
        {
            _jordan = jordan;
            _logger = logger;
        }

        public double[,] Invert(double[,] matrixA)
        {
            int n = matrixA.GetLength(0);
            double[,] result = matrixA;

            if (_logger != null)
            {
                _logger.LogSection("Знаходження оберненої матриці:");
                _logger.LogMatrix("Вхідна матриця:", matrixA);
                _logger.LogSection("Протокол обчислення:");
            }

            for (int i = 0; i < n; i++)
            {
                if (_logger != null)
                {
                    _logger.LogStep(i + 1, i + 1, i + 1, result[i, i]);
                }
                result = _jordan.JordanMethod(result, i, i);
                if (_logger != null)
                {
                    _logger.LogMatrix("", result);
                }
            }

            if (_logger != null)
            {
                _logger.LogMatrix("Обернена матриця:", result);
            }

            return result;
        }
    }
}