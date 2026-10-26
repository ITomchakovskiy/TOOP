using System;
using System.Collections.Generic;
using System.Linq;
using Functions;
using TOOP;

using Real = double;

namespace Functionals;

public class L1Norm : IDifferentiableFunctional
{
    readonly IVector[] _points;
    readonly Real[] _expectedValues;

    public L1Norm(IEnumerable<IVector> points, IEnumerable<Real> expectedValues)
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

    public Real Value(IFunction function)
    {
        Real sum = 0;
        for (var i = 0; i < _points.Length; i++)
        {
            sum += Math.Abs(function.Value(_points[i]) - _expectedValues[i]);
        }

        return sum;
    }

    public IVector Gradient(IFunction function)
    {
        if (function is not IDifferentiableFunction differentiable)
        {
            throw new ArgumentException("Function must implement IDifferentiableFunction.", nameof(function));
        }

        var gradient = new Vector(differentiable.Gradient(_points[0]).Length);
        for (var i = 0; i < _points.Length; i++)
        {
            var df = function.Value(_points[i]) - _expectedValues[i];
            var sign = Math.Sign(df);

            var contrib = new Vector(differentiable.Gradient(_points[i]).ToArray());
            gradient += contrib * sign;
        }
        
        return gradient;
    }
}
