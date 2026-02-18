using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab01.Logic.Interfaces;

namespace Lab01.Logic
{
   public class RankCalculator:IRankCalculator
    {
        private readonly IJordan _jordan;

        public RankCalculator(IJordan jordan)
        {
            _jordan = jordan;
        }

        public int Calculate(double[,] matrixA)
        {
            int rows = matrixA.GetLength(0);
            int cols = matrixA.GetLength(1);
            int r = 0;
            double[,] result = matrixA;
            int limit = Math.Min(rows, cols);

            for (int i = 0; i < limit; i++)
            {
                if (result[i,i] != 0)
                {
                   result = _jordan.JordanMethod(result, i, i);
                    r++;
                }
            }
            return r;
        }

    }
}
