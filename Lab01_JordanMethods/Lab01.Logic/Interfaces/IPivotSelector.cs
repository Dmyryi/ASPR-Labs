using Lab01.Logic.Models;

namespace Lab01.Logic.Interfaces;

public interface IPivotSelector
{
    /// <summary>
    /// Знайти ведучий рядок за правилом мінімального додатного відношення b_i / a_ic
    /// при додатних a_ic > 0 (для пошуку оптимального розв’язку).
    /// </summary>
    int FindOptimalPivotRow(SimplexTableau tableau, int pivotCol);

    /// <summary>
    /// Знайти ведучий рядок за правилом мінімального відношення b_i / a_ic
    /// при від’ємних a_ic &lt; 0 (для пошуку опорного розв’язку), або повернути fallbackRow.
    /// </summary>
    int FindBasicPivotRow(SimplexTableau tableau, int pivotCol, int fallbackRow);
}
