using Lab01.Logic.Interfaces.IBasicLogic;

namespace Lab01.Logic.BasicLogic;

public sealed class InverseSolveStrategy : ILinearSystemSolver
{
    private readonly IMatrixInverter _inverter;
    private readonly CalculationLogger? _logger;

    public InverseSolveStrategy(IMatrixInverter inverter, CalculationLogger? logger = null)
    {
        _inverter = inverter;
        _logger = logger;
    }

    public double[] Solve(double[,] matrixA, double[] vectorB)
    {
        if (_logger is not null)
        {
            _logger.LogTitle("Згенерований протокол обчислення:");
            _logger.LogSection("Знаходження розв'язків СЛАР методом оберненої матриці...");
        }

        var invertedMatrix = _inverter.Invert(matrixA);
        var x = Multiply(invertedMatrix, vectorB);

        if (_logger is not null)
        {
            _logger.LogVector("Вхідний вектор B:", vectorB);
            _logger.LogFinalCalculation(x, invertedMatrix, vectorB);
        }

        return x;
    }

    private static double[] Multiply(double[,] matrix, double[] vector)
    {
        int n = vector.Length;
        var result = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = 0;
            for (int j = 0; j < matrix.GetLength(1); j++)
                sum += matrix[i, j] * vector[j];
            result[i] = sum;
        }

        return result;
    }
}
