using Lab01.Logic.Models;

namespace Lab01.Logic.Interfaces;

public interface ILinearProgramParser
{
    LinearProgram Parse(string objectiveText, string constraintsText);
}
