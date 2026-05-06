using Lab01.Logic.Simplex;

namespace Lab01.Logic.Exceptions;

public sealed class UnboundedProblemException : SimplexException
{
    public OptimizationMode Mode { get; }

    public UnboundedProblemException(OptimizationMode mode)
        : base(mode == OptimizationMode.Maximization
            ? "Цільова функція не обмежена зверху."
            : "Цільова функція не обмежена знизу.")
    {
        Mode = mode;
    }
}
