using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace TOOP.Matrix;

public enum MatrixSymmetry {Asymmetric, SymmetricProfile, Symmetric}

public interface IMatrix : IEnumerable<IEnumerable<double>>
{
    double this[int index1, int index2] {get;}
    int RowsCount {get;}
    int ColumnsCount {get;}
    MatrixSymmetry Symmetry {get;}
    bool IsSquare {get;}
    bool? IsPositiveDefinite {get; set;}
}

interface IMatrixOperators<TSelf> where TSelf : IMatrixOperators<TSelf>, IMatrix
{
    public static abstract TSelf operator +(TSelf A, TSelf B);
    public static abstract TSelf operator -(TSelf A, TSelf B);
    public static abstract TSelf operator *(TSelf A, TSelf B);
    public static abstract TSelf operator *(TSelf A, double coef);
    public static abstract TSelf operator *(double coef, TSelf A);
    public static abstract TSelf operator /(TSelf A, double coef);
    public static abstract IVector operator *(TSelf A, IVector b);
}

interface IMatrixOperators
{
    IMatrix Transpose();
}

public static class MatrixOperators
{
    private static IMatrix MatrixMatrixOperator(IMatrix A, IMatrix B, 
                                  Func<DenseMatrix, DenseMatrix, DenseMatrix> fDense)
    {
        if(A is DenseMatrix AD && B is DenseMatrix BD)
            return fDense(AD, BD);
        return fDense(new DenseMatrix(A), new DenseMatrix(B));
    }

    private static IMatrix MatrixScalarOperator(IMatrix A, double coef, 
                                  Func<DenseMatrix, double, DenseMatrix> fDense)
    {
        if(A is DenseMatrix AD)
            return fDense(AD, coef);
        return fDense(new DenseMatrix(A), coef);
    }
    public static IMatrix Add(IMatrix A, IMatrix B) => MatrixMatrixOperator(A,B,(A_,B_)=>A_+B_);
    public static IMatrix Subtract(IMatrix A, IMatrix B) => MatrixMatrixOperator(A,B,(A_,B_)=>A_-B_);
    public static IMatrix Multiply(IMatrix A, IMatrix B) => MatrixMatrixOperator(A,B,(A_,B_)=>A_*B_);
    public static IMatrix Multiply(IMatrix A, double coef) => MatrixScalarOperator(A, coef, (A_,c) => A_*c);
    public static IVector Multiply(IMatrix A, IVector b)
    {
        if(A is DenseMatrix AD)
            return AD * b;
        return new DenseMatrix(A) * b;
    }
    public static IMatrix Multiply(double coef, IMatrix A) => Multiply(A, coef);
    public static IMatrix Divide(IMatrix A, double coef) => MatrixScalarOperator(A, coef, (A_,c) => A_/c);
}



