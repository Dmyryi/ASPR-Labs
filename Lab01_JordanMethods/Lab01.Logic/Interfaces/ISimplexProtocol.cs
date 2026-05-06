using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Interfaces;

public interface ISimplexProtocol
{
    void Start(OptimizationMode mode, string objective, string constraints);

    /// <summary>
    /// Повний заголовок протоколу для методу Гоморі: постановка, переписані обмеження у формі ≥ 0,
    /// позначення цілих змінних, підготовка до друку симплекс-таблиць.
    /// </summary>
    void StartGomory(OptimizationMode mode, string objective, string constraints, LinearProgram program);

    void LogSection(string title);

    void LogText(string text);

    void LogInitialTableau(SimplexTableau tableau);

    /// <param name="step">Номер кроку; якщо <c>null</c>, рядок «Крок #…» не друкується (формат методички).</param>
    void LogPivot(int? step, SimplexTableau tableau, int pivotRow, int pivotCol);

    void LogTableau(SimplexTableau tableau);

    /// <summary>
    /// Друкує таблицю з окремим заголовком (наприклад, «Симплекс-таблиця з новим обмеженням»).
    /// </summary>
    void LogTableau(string caption, SimplexTableau tableau);

    void LogBasicSolution(SimplexTableau tableau);

    /// <summary>
    /// Неперервний оптимум LP після етапу симплексу (до або між відсіченнями Гоморі).
    /// </summary>
    void LogContinuousOptimalSolution(SimplexTableau tableau);

    void LogGomoryFractionalSolution(int decisionVariableIndex0Based, double basisValue, double fractionalPart);

    /// <summary>
    /// Рівняння відсічення у вигляді методички: sk = Σ β·ζ + (константа) ≥ 0.
    /// </summary>
    void LogGomoryCutEquation(int cutIndex1Based, SimplexTableau tableau, double[] cutRowCoefficients, double cutRhs);

    void LogResult(SolverResult result);

    string GetText();
}
