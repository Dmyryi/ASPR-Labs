using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Interfaces;

public interface ISimplexProtocol
{
    void Start(OptimizationMode mode, string objective, string constraints, LinearProgram? canonicalProgram = null, SimplexProtocolStyle style = SimplexProtocolStyle.PrimalZ);

    void StartGomory(OptimizationMode mode, string objective, string constraints, LinearProgram program);

    void LogSection(string title);

    void LogText(string text);

    void LogInitialTableau(SimplexTableau tableau);

    void LogPivot(int? step, SimplexTableau tableau, int pivotRow, int pivotCol);

    void LogTableau(SimplexTableau tableau);

    void LogTableau(string caption, SimplexTableau tableau);

    void LogBasicSolution(SimplexTableau tableau);

    void LogContinuousOptimalSolution(SimplexTableau tableau);

    void LogGomoryFractionalSolution(int decisionVariableIndex0Based, double basisValue, double fractionalPart);

    void LogGomoryCutEquation(int cutIndex1Based, SimplexTableau tableau, double[] cutRowCoefficients, double cutRhs);

    void LogResult(SolverResult result, string objectiveSymbol = "Z");

    string GetText();
}
