using System;
using System.Collections.Generic;
using System.Linq;
using Functions;
using TOOP;

using Real = double;

namespace Functionals;

public class LInfNorm : IFunctional
{
    readonly IVector[] _points;
    readonly Real[] _expectedValues;

    public LInfNorm(IEnumerable<IVector> points, IEnumerable<Real> expectedValues)
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
        Real max = 0;
        for (var i = 0; i < _points.Length; i++)
        {
            max = Math.Max(max, Math.Abs(function.Value(_points[i]) - _expectedValues[i]));
        }

        return max;
    }
}
