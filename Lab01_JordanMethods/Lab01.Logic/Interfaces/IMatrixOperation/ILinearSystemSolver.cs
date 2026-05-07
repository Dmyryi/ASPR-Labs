namespace Lab01.Logic.Interfaces.IBasicLogic;

public interface ILinearSystemSolver
{
    double[] Solve(double[,] matrix, double[] vector);
}
