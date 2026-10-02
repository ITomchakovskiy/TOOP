using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace TOOP;

enum MatrixSymmetry {Asymmetric, SymmetricProfile, Symmetric}

interface IMatrix : IEnumerable<IEnumerable<double>>
{
    double this[int index1, int index2] {get;}
    int RowsCount {get;}
    int ColumnsCount {get;}
    MatrixSymmetry Symmetry {get;}
    bool IsSquare {get;}
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
    TSelf Transpose();
}

class DenseMatrix : IMatrix, IMatrixOperators<DenseMatrix>
{
    public DenseMatrix(IEnumerable<IEnumerable<double>> _values)
    {
        values = [.. _values.Select(i => i.ToArray())];
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
    private readonly double[][] values;
    public MatrixSymmetry Symmetry {get;}

    public bool IsSquare => RowsCount == ColumnsCount;

    public int RowsCount => values.Length;

    public int ColumnsCount => values[0].Length;

    public double this[int index1, int index2] => values[index1][index2];

    public DenseMatrix Transpose()
    {
        if(Symmetry == MatrixSymmetry.Symmetric) return new DenseMatrix(values);
        double[][] newValues = new double[ColumnsCount][];
        for(int i = 0; i < newValues.Length; ++i) newValues[i] = new double[RowsCount];
        for(int i = 0; i < RowsCount; ++i)
        {
            for(int j = 0; j < ColumnsCount; ++j)
                newValues[j][i] = values[i][j];
        }
        return new(newValues);
    }
    public IEnumerator<IEnumerable<double>> GetEnumerator() => values.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();

    public static DenseMatrix operator +(DenseMatrix A, DenseMatrix B)
    {
        if(A.RowsCount != B.RowsCount || A.ColumnsCount != B.ColumnsCount) throw new ArgumentException("not compatible matrices");
        double[][] newValues = [.. A.Select((arow,i) => arow.Select((ael,j) => ael + B[i,j]).ToArray())];
        return new(newValues);
    }

    public static DenseMatrix operator -(DenseMatrix A, DenseMatrix B)
    {
        if(A.RowsCount != B.RowsCount || A.ColumnsCount != B.ColumnsCount) throw new ArgumentException("not compatible matrices");
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

