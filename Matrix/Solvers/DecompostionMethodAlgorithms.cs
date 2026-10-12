using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Matrix.Solvers;
static class DecompostionMethodAlgorithmsForDenseSLAE
{
    public static IVector ForwardSubstitution(IMatrix Matrix, IVector rightSide, bool isDiagonalIdentity)
    {
        double[] values = new double[rightSide.Length];
        for(int i = 0; i < values.Length; ++i)
        {
            double sum = 0d;
            for(int j = 0; j < i; ++j)
                sum += Matrix[i,j] * values[j];
            
            values[i] = isDiagonalIdentity ? rightSide[i] - sum : (rightSide[i] - sum) / Matrix[i,i];
        }
        return new Vector(values);
    }

    public static IVector BackwardSubstitution(IMatrix Matrix, IVector rightSide, bool isDiagonalIdentity)
    {
        double[] values = new double[rightSide.Length];
        for(int i = values.Length - 1; i >= 0; --i)
        {
            double sum = 0d;
            for(int j = i + 1; j < values.Length; ++j)
                sum += Matrix[i,j] * values[j];
            values[i] = isDiagonalIdentity ? rightSide[i] - sum : (rightSide[i] - sum) / Matrix[i,i];
        }
        return new Vector(values);
    }
}