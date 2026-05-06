namespace Lab01.Logic.Exceptions;

public sealed class InfeasibleProblemException : SimplexException
{
    public InfeasibleProblemException()
        : base("Система обмежень є несумісною: опорний розв’язок не існує.") { }

    public InfeasibleProblemException(string message) : base(message) { }
}
