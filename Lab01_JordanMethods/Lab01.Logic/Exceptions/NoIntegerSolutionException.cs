namespace Lab01.Logic.Exceptions;

public sealed class NoIntegerSolutionException : SimplexException
{
    public NoIntegerSolutionException()
        : base("Цілочисловий розв'язок не існує: усі коефіцієнти дробового рядка є цілими.") { }

    public NoIntegerSolutionException(string message) : base(message) { }
}
