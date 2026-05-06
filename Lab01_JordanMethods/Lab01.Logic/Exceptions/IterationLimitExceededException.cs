namespace Lab01.Logic.Exceptions;

public sealed class IterationLimitExceededException : SimplexException
{
    public int Limit { get; }

    public IterationLimitExceededException(int limit, string stage)
        : base($"Перевищено ліміт ітерацій ({limit}) на етапі: {stage}.")
    {
        Limit = limit;
    }
}
