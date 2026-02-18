using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab01.Logic.Interfaces;

namespace Lab01.Logic
{
    public class InverseSolveStrategy:ILinearSystemSolver
    {
     
        private readonly IMatrixInverter _inverter;
        public InverseSolveStrategy(IMatrixInverter inverter) { 
     
            _inverter = inverter;
        }

        private static double[] CalculatedX(double[] vectorB, double[,] invertedMatrix)
        {
            int n = vectorB.Length;
            double[] x = new double[n];

            for (int i = 0; i < n; i++)
            {

                double sum = 0;
                for (int j = 0; j < invertedMatrix.GetLength(1); j++)
                {
                    sum += invertedMatrix[i, j] * vectorB[j];
                }
                x[i] = sum;
            }
            return x;
        }

        public double[] Solve(double[,] vectorA, double[] vectorB)
        {
            var invertedMatrix = _inverter.Invert(vectorA);
            return CalculatedX(vectorB, invertedMatrix);
        }
    }
}
