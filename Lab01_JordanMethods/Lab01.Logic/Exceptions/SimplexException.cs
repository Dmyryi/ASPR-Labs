namespace Lab01.Logic.Exceptions;

public abstract class SimplexException : Exception
{
    protected SimplexException(string message) : base(message) { }
}
