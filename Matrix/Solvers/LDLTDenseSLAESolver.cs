using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Matrix.Solvers;

class LDLTDenseSLAESolver : ISLAESolver
{
    private const double eps = 1.0E-10;
    private IMatrix? decomposedMatrix;
    private IMatrix? LDLT;
    public void Decompose(IMatrix matrix)
    {
        if(!matrix.IsSquare || matrix.Symmetry != MatrixSymmetry.Symmetric) throw new ArgumentException("LDLT decomposition requires symmetric matrices");
        double[][] LDLTValues = new double[matrix.RowsCount][];
        for(int i = 0; i < LDLTValues.Length; ++i) LDLTValues[i] = new double[i + 1];
        for(int j = 0; j < LDLTValues.Length; ++j)
        {
            //D
            double sumD = 0d;
            for(int k = 0; k < j; ++k)
                sumD += LDLTValues[j][k] * LDLTValues[j][k] * LDLTValues[k][k];
            double di = matrix[j,j] - sumD; if(Math.Abs(di) < eps ) return;
            LDLTValues[j][j] = di;
            //L
            for(int i = j + 1; i < LDLTValues.Length; ++i)
            {
                double sumL = 0d;
                for(int k = 0; k < j; ++k)
                    sumL += LDLTValues[i][k] * LDLTValues[k][k] * LDLTValues[j][k];
                LDLTValues[i][j] = (matrix[i,j] - sumL) / di;
            }
        }
        LDLT = new GeneralDenseMatrix(LDLTValues);
    }
    bool ISLAESolver.Solve(IMatrix? matrix, IVector rightSide, out IVector result, double? precision, int? maxIterationCount, IVector? initialSolution)
    {
        if(matrix is not null && matrix != decomposedMatrix)
            Decompose(matrix);
        if(LDLT is null) {result = new Vector([]); return false;}
        if(LDLT.RowsCount != rightSide.Length) throw new ArgumentException("incompatible matrix and vector sizes");
        var P = DecompostionMethodAlgorithmsForDenseSLAE.ForwardSubstitution(LDLT, rightSide, true);
        var Y = new Vector([.. P.Select((el,i) => el / LDLT[i,i])]);
        var X = DecompostionMethodAlgorithmsForDenseSLAE.BackwardSubstitution(LDLT,Y,true);
        result = X;
        return true;
    }
}