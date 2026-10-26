using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Matrix.Solvers;

class CholeskyDenseSLAESolver : ISLAESolver
{
    private IMatrix? decomposedMatrix;

    private const double eps = 1.0E-10;

    private GeneralDenseMatrix? LLT;

    public bool IsDecomposed => LLT is not null;

    public void Decompose(IMatrix matrix)
    {
        if(!matrix.IsSquare || matrix.Symmetry != MatrixSymmetry.Symmetric ||
            matrix.IsPositiveDefinite is not null && !(bool)matrix.IsPositiveDefinite) throw new ArgumentException("Cholesky decomposition requires symmetric positive definite matrices");
        double[][] LLTValues = new double[matrix.RowsCount][];
        for(int i = 0; i < LLTValues.Length; ++i) LLTValues[i] = new double[i + 1];
        for(int j = 0; j < LLTValues.Length; ++j)
        {
            //D
            double sumD = 0d;
            for(int k = 0; k < j; ++k)
                sumD += LLTValues[j][k] * LLTValues[j][k];
            double expr = matrix[j,j] - sumD;
            if(expr < eps) {matrix.IsPositiveDefinite = false; return;}
            LLTValues[j][j] = Math.Sqrt(expr);
            //L
            for(int i = j + 1; i < LLTValues.Length; ++i)
            {
                double sumL = 0d;
                for(int k = 0; k < j; ++k)
                    sumL += LLTValues[i][k] * LLTValues[j][k];
                LLTValues[i][j] = (matrix[i,j] - sumL) / LLTValues[j][j];
            }
        }
        decomposedMatrix = matrix;
        LLT = new(LLTValues, MatrixSymmetry.Symmetric);
    }
    bool ISLAESolver.Solve(IMatrix? matrix, IVector rightSide, out IVector result, double? precision, int? maxIterationCount, IVector? initialSolution)
    {
        if(matrix is not null && matrix != decomposedMatrix)
            Decompose(matrix);
        if(LLT is null) {result = new Vector([]); return false;}
        if(LLT.RowsCount != rightSide.Length) throw new ArgumentException("incompatible matrix and vector sizes");
        var Y = DecompostionMethodAlgorithmsForDenseSLAE.ForwardSubstitution(LLT, rightSide, false);
        var X = DecompostionMethodAlgorithmsForDenseSLAE.BackwardSubstitution(LLT, Y, false);
        result = X;
        return true;
    }
}