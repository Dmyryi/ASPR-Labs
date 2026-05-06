using Lab01.Logic.Models;

namespace Lab01.Logic.Interfaces;

public interface IZeroRowEliminator
{
    void Eliminate(SimplexTableau tableau);
}
