using Lab01.Logic.Models;
using Lab01.Logic.Simplex;

namespace Lab01.Logic.Interfaces;

public interface ISimplexProtocol
{
    void Start(OptimizationMode mode, string objective, string constraints);
    void LogSection(string title);
    void LogInitialTableau(SimplexTableau tableau);
    void LogPivot(int step, SimplexTableau tableau, int pivotRow, int pivotCol);
    void LogTableau(SimplexTableau tableau);
    void LogBasicSolution(SimplexTableau tableau);
    void LogResult(SolverResult result);
    string GetText();
}

