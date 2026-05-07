namespace Lab01.Logic.Interfaces;

public interface IJordan
{
    double[,] JordanMethod(double[,] matrix, int r, int s);

    double[,] ModifiedJordanMethod(double[,] matrix, int r, int s);
}
