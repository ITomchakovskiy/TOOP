using System;
using System.Collections.Generic;
using System.Linq;
using Functions;
using TOOP;
using TOOP.Matrix;
using Real = double;

namespace Functionals;

public class L2Norm : IDifferentiableFunctional, ILeastSquaresFunctional
{
    readonly IVector[] _points;
    readonly Real[] _expectedValues;

    public L2Norm(IEnumerable<IVector> points, IEnumerable<Real> expectedValues)
    {
        _points = points.ToArray();
        _expectedValues = expectedValues.ToArray();

        if (_points.Length == 0)
        {
            throw new ArgumentException("At least one sample point is required.", nameof(points));
        }
        if (_points.Length != _expectedValues.Length)
        {
            throw new ArgumentException("The number of points must match the number of expected values.");
        }
    }

    public IVector Residual(IFunction function)
    {
        var residual = new Real[_points.Length];
        for (var i = 0; i < _points.Length; i++)
        {
            residual[i] = function.Value(_points[i]) - _expectedValues[i];
        }

        return new Vector(residual);
    }

    public Real Value(IFunction function)
    {
        var residual = Residual(function);
        Real sum = 0;
        for (var i = 0; i < residual.Length; i++)
        {
            sum += residual[i] * residual[i];
        }

        return Math.Sqrt(sum);
    }

    public IMatrix Jacobian(IFunction function)
    {
        if (function is not IDifferentiableFunction differentiable)
        {
            throw new ArgumentException("Function must implement IDifferentiableFunction.", nameof(function));
        }

        var rows = new Real[_points.Length][];
        for (var i = 0; i < _points.Length; i++)
        {
            rows[i] = differentiable.Gradient(_points[i]).ToArray();
        }

        return new DenseMatrix(rows);
    }

    public IVector Gradient(IFunction function)
    {
        var residual = new Vector(Residual(function).ToArray());
        var norm = residual.Norm();
        var jacobian = (DenseMatrix)Jacobian(function);
        if (norm == 0)
        {
            return new Vector(jacobian.ColumnsCount);
        }

        return new Vector(((DenseMatrix)jacobian.Transpose() * residual).ToArray()) / norm;
    }
}
