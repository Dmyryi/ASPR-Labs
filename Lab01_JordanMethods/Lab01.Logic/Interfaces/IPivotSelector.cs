using Lab01.Logic.Models;

namespace Lab01.Logic.Interfaces;

public interface IPivotSelector
{
    int FindOptimalPivotRow(SimplexTableau tableau, int pivotCol);

    int FindBasicPivotRow(SimplexTableau tableau, int pivotCol, int fallbackRow);
}
