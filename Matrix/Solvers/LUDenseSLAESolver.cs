using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace TOOP.Matrix.Solvers;

class LUDenseSLAESolver : ISLAESolver
{
    private IMatrix? DecomposedMatrix;
    private GeneralDenseMatrix? LU;
    private const double eps = 1.0E-10;
    public void Decompose(IMatrix matrix) //U diagonal is I
    {
        if(!matrix.IsSquare) throw new ArgumentException("SLAE solving implemented only for square matrices");
        double[][] LUValues = new double[matrix.RowsCount][];
        for(int i = 0; i < LUValues.Length; ++i) LUValues[i] = new double[matrix.ColumnsCount];
        for(int i = 0; i < matrix.RowsCount; ++i) //column for L, row for U
        {
            //D
            double sumD = 0d;
            for(int k = 0; k < i; ++k)
                sumD += LUValues[i][k] * LUValues[k][i];
            LUValues[i][i] = matrix[i,i] - sumD;
            if(Math.Abs(LUValues[i][i]) < eps) return;
            for(int j = i + 1; j < matrix.RowsCount; ++j) //row for L, column for U
            {
                //L
                double sumL = 0d;
                for(int k = 0; k < i; ++k)
                    sumL += LUValues[j][k] * LUValues[k][i];
                LUValues[j][i] = matrix[j,i] - sumL;
                //U
                double sumU = 0d;
                for(int k = 0; k < i; ++k)
                    sumU += LUValues[i][k] * LUValues[k][j];
                LUValues[i][j] = (matrix[i,j] - sumU) / LUValues[i][i];
            }
        }

        LU = new(LUValues, MatrixSymmetry.Asymmetric);
    }

    

    public bool Solve(IMatrix? matrix, IVector rightSide, out IVector result, double? precision, int? maxIterationCount, IVector? initialSolution)
    {
        if(matrix is not null && matrix != DecomposedMatrix)
            Decompose(matrix);
        if(LU is null) {result = new Vector([]); return false;}
        if(LU.RowsCount != rightSide.Length) throw new ArgumentException("incompatible matrix and vector sizes");
        var Y = DecompostionMethodAlgorithmsForDenseSLAE.ForwardSubstitution(LU,rightSide, false);
        var X = DecompostionMethodAlgorithmsForDenseSLAE.BackwardSubstitution(LU,Y,true);
        result = X;
        return true;
    }
}