namespace Lab01.Logic.Exceptions;

public sealed class CyclingDetectedException : SimplexException
{
    public CyclingDetectedException()
        : base("Виявлено зациклення під час пошуку опорного розв’язку.") { }
}
