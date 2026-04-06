using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab01.Logic.Interfaces.IBasicLogic
{
    public interface IRankCalculator
    {
        public int Calculate(double[,] matrix);
    }
}
