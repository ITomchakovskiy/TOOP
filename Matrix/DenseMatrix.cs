using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Matrix;

class GeneralDenseMatrix : IMatrix
{
    public GeneralDenseMatrix(IEnumerable<IEnumerable<double>> _values, MatrixSymmetry? symmetry = null, bool? isPositiveDefinite = null)
    {
        values = [.. _values.Select(i => i.ToArray())];
        if(symmetry is null)
        {
            if(IsSquare)
            {
                bool isSymmetric = true;
                for(int i = 0; i < values.Length; ++i)
                {
                    for(int j = 0; j < i; ++j)
                        if(values[i][j] != values[j][i]) isSymmetric = false;
                }
                if(isSymmetric) Symmetry = MatrixSymmetry.Symmetric;
                else Symmetry = MatrixSymmetry.Asymmetric;
            }
            else Symmetry = MatrixSymmetry.Asymmetric;
        }
        else Symmetry = (MatrixSymmetry)symmetry;

        IsPositiveDefinite = isPositiveDefinite;
    }

    public GeneralDenseMatrix(IEnumerable<IEnumerable<double>> _values, bool? isPositiveDefinite) : this(_values, null, isPositiveDefinite) {}

    public GeneralDenseMatrix(IMatrix matrix) : this(matrix, matrix.Symmetry, matrix.IsPositiveDefinite) {}

    protected readonly double[][] values;
    public MatrixSymmetry Symmetry {get;} //if explicitly symmetric only lower triangle is accessed

    public bool IsSquare => RowsCount == ColumnsCount;

    public int RowsCount => values.Length;

    public int ColumnsCount => values[^1].Length;

    public bool? IsPositiveDefinite {get; set;} = null;

    public double this[int index1, int index2] => index1 >= index2 ? values[index1][index2] : values[index2][index1];

    public IEnumerator<IEnumerable<double>> GetEnumerator() => values.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
}

class DenseMatrix : GeneralDenseMatrix, IMatrixOperators<DenseMatrix>, IMatrixOperators
{
    public DenseMatrix(IEnumerable<IEnumerable<double>> _values, MatrixSymmetry? symmetry = null, bool? isPositiveDefinite = null) : base(_values, symmetry, isPositiveDefinite) {}
    public DenseMatrix(IEnumerable<IEnumerable<double>> _values, bool? isPositiveDefinite) : base(_values, isPositiveDefinite) {} 
    public DenseMatrix(IMatrix matrix) : base(matrix) {}

    public IMatrix Transpose()
    {
        if(Symmetry == MatrixSymmetry.Symmetric) return this;
        double[][] newValues = new double[ColumnsCount][];
        for(int i = 0; i < newValues.Length; ++i) newValues[i] = new double[RowsCount];
        for(int i = 0; i < RowsCount; ++i)
        {
            for(int j = 0; j < ColumnsCount; ++j)
                newValues[j][i] = values[i][j];
        }
        return new DenseMatrix(newValues);
    }

    private DenseMatrix ToAsymmetricFormat()
    {
        if(Symmetry != MatrixSymmetry.Symmetric) return this;
        double[][] newValues = new double[RowsCount][];
        for(int i = 0; i < newValues.Length; ++i) newValues[i] = new double[ColumnsCount];
        for(int i = 0; i < newValues.Length; ++i)
        {
            for(int j = 0; j < newValues[0].Length; ++j)
                newValues[i][j] = this[i,j];
        }
        return new(newValues, Symmetry, IsPositiveDefinite);
    }

    public static DenseMatrix operator +(DenseMatrix A, DenseMatrix B)
    {
        if(A.RowsCount != B.RowsCount || A.ColumnsCount != B.ColumnsCount) throw new ArgumentException("not compatible matrices");
        A = A.ToAsymmetricFormat(); B = B.ToAsymmetricFormat();
        double[][] newValues = [.. A.Select((arow,i) => arow.Select((ael,j) => ael + B[i,j]).ToArray())];
        return new(newValues);
    }

    public static DenseMatrix operator -(DenseMatrix A, DenseMatrix B)
    {
        if(A.RowsCount != B.RowsCount || A.ColumnsCount != B.ColumnsCount) throw new ArgumentException("not compatible matrices");
        A = A.ToAsymmetricFormat(); B = B.ToAsymmetricFormat();
        double[][] newValues = [.. A.Select((arow,i) => arow.Select((ael,j) => ael - B[i,j]).ToArray())];
        return new(newValues);
    }

    public static DenseMatrix operator *(DenseMatrix A, DenseMatrix B)
    {
        if(A.ColumnsCount != B.RowsCount) throw new ArgumentException("not compatible matrices");
        double[][] newValues = new double[A.RowsCount][];
        for(int i = 0; i < newValues.Length; ++i) newValues[i] = new double[B.ColumnsCount];
        for(int i = 0; i < newValues.Length; ++i)
        {
            for(int j = 0; j < newValues[0].Length; ++j)
            {
                for(int k = 0; k < A.ColumnsCount; ++k)
                    newValues[i][j] += A[i,k] * B[k,j];
            }
        }
        return new(newValues);
    }

    public static DenseMatrix operator *(DenseMatrix A, double coef)
    {
        double[][] newValues = [.. A.Select(row => row.Select(el => el * coef).ToArray())];
        return new(newValues);
    }

    public static DenseMatrix operator *(double coef, DenseMatrix A) => A * coef;

    public static DenseMatrix operator /(DenseMatrix A, double coef) => A * (1d/ coef);

    public static IVector operator *(DenseMatrix A, IVector b)
    {
        if(A.ColumnsCount != b.Length) throw new ArgumentException("matrix and vector are not compatible");
        double[] newValues = new double[A.RowsCount];
        for(int i = 0; i < newValues.Length; ++i)
        {
            for(int k = 0; k < b.Length; ++k)
                newValues[i] += A[i,k] * b[k];
        }
        return new Vector(newValues);
    }
}