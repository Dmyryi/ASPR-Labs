namespace Lab01.Logic.Exceptions;

/// <summary>
/// Кинуто, коли метод Гоморі не знайшов цілочислового розв'язку
/// (рядок із дробовим b не має дробових коефіцієнтів — відсічення збудувати неможливо).
/// </summary>
public sealed class NoIntegerSolutionException : SimplexException
{
    public NoIntegerSolutionException()
        : base("Цілочисловий розв'язок не існує: усі коефіцієнти дробового рядка є цілими.") { }

    public NoIntegerSolutionException(string message) : base(message) { }
}
