using Functions;
using TOOP.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Functions;

class PiecewiseLinearFunction : IParametricFunction
{
    public IFunction Bind(IVector parameters)
    {
        if (parameters.Length < 4 || parameters.Length % 2 != 0)
        {
            throw new ArgumentException("Expected even number of parameters >= 4: (x0, y0, x1, y1, ...)");
        }

        var knotCount = parameters.Length / 2;
        var knots = new Vector(knotCount);
        var values = new Vector(knotCount);

        for (var i = 0; i < knotCount; i++)
        {
            knots[i] = parameters[2 * i];
            values[i] = parameters[2 * i + 1];

            if (i > 0 && knots[i] < knots[i - 1])
            {
                throw new ArgumentException("Abscissas do not satisfy the condition: x0 < x1 < ... < xN-1");
            }
        }

        return new PiecewiseLinearFunctionImpl(knots, values);
    }

    private class PiecewiseLinearFunctionImpl(IVector knots, IVector values) : IDifferentiableFunction
    {
        private readonly IVector _slopes = CalculateSlopes(knots, values);

        private static IVector CalculateSlopes(IVector knots, IVector values)
        {
            var slopes = new Vector(knots.Length - 1);
            for (var i = 0; i < knots.Length - 1; i++)
            {
                var deltaX = knots[i + 1] - knots[i];
                slopes[i] = (values[i + 1] - values[i]) / deltaX;
            }
            return slopes;
        }

        public double Value(IVector point)
        {
            var x = point[0];

            if (x < knots.First() || x > knots.Last())
            {
                throw new ArgumentOutOfRangeException(nameof(point), "Point is out of domain [x0, xN]");
            }

            if (x.IsEqualsPrecision(knots.Last()))
            {
                return values.Last();
            }
            var segmentIndex = knots.Find(x);

            return values[segmentIndex] + _slopes[segmentIndex] * (x - knots[segmentIndex]);
        }

        public IVector Gradient(IVector point)
        {
            var x = point[0];

            if (x <= knots.First() || x >= knots.Last())
                throw new ArgumentOutOfRangeException(nameof(point), "Gradient is not defined outside (x0, xN)");

            var segmentIndex = knots.Find(x);
            if (x.IsEqualsPrecision(knots[segmentIndex]) ||
                x.IsEqualsPrecision(knots[segmentIndex + 1]))
                throw new InvalidOperationException("Gradient is not defined in knots.");

            var i = segmentIndex;
            var x0 = knots[i];
            var x1 = knots[i + 1];
            var y0 = values[i];
            var y1 = values[i + 1];

            var dx = x1 - x0;
            var t = (x - x0) / dx;
            var s = (y1 - y0) / dx;

            var gradient = new Vector(2 * knots.Length);

            // ∂f/∂x_i и ∂f/∂x_{i+1}
            gradient[2 * i] = -s * (1 + t);
            gradient[2 * (i + 1)] = -s * t;

            // ∂f/∂y_i и ∂f/∂y_{i+1}
            gradient[2 * i + 1] = 1 - t;
            gradient[2 * (i + 1) + 1] = t;

            return gradient;
        }
    }
}