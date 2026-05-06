using Lab01.Logic.Models;

namespace Lab01.Logic.Interfaces;

public interface IOptimalSolution
{
    void Find(SimplexTableau tableau);
}
