using Functions;
using TOOP.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace TOOP.Functions;

public class PiecewiseLinearFunction : IParametricFunction
{
    private const double eps = 1.0E-10;
    public IFunction Bind(IVector parameters) //parameters are starting point x, f(starting x) and then pairs of steps and f(previous x + step)
    {
        if (parameters.Length < 2 || parameters.Length % 2 != 0)
        {
            throw new ArgumentException("Expected even number of parameters >= 2: (x0, y0, x1, y1, ...)");
        }

        var knotCount = parameters.Length / 2;
        var knots = new Vector(knotCount);
        var values = new Vector(knotCount);

        knots[0] = parameters[0];
        values[0] = parameters[1];
        for (var i = 1; i < knotCount; i++)
        {
            knots[i] = knots[i-1] + parameters[2 * i]; //X(k+1) = X(k) + Hx(k)
            values[i] = parameters[2 * i + 1];

            if (parameters[2 * i] < eps)
            {
                throw new ArgumentOutOfRangeException("Abscissas do not satisfy the condition: x0 < x1 < ... < xN-1");
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

            // if (x < knots.First() || x > knots.Last())
            // {
            //     throw new ArgumentOutOfRangeException(nameof(point), "Point is out of domain [x0, xN]");
            // }
            if(x < knots.First()) return values.First();
            else if(x > knots.Last()) return values.Last();

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

            var gradient = new Vector(2 * knots.Length);

            if(x <= knots.First()) {gradient[1] = 1d; return gradient;}
            else if(x >= knots.Last()) {gradient[^1] = 1d; return gradient;}

            // if (x <= knots.First() || x >= knots.Last())
            //     throw new ArgumentOutOfRangeException(nameof(point), "Gradient is not defined outside (x0, xN)");

            var segmentIndex = knots.Find(x);
            //  could lead to problems during minimization
            // if (x.IsEqualsPrecision(knots[segmentIndex]) ||  
            //     x.IsEqualsPrecision(knots[segmentIndex + 1]))
            //     throw new InvalidOperationException("Gradient is not defined in knots.");

            var i = segmentIndex;
            var x0 = knots[i];
            var x1 = knots[i + 1];
            var y0 = values[i];
            var y1 = values[i + 1];

            var dx = x1 - x0;
            var dy = y1 - y0;

            //df/dx
            var dfdx = -dy/dx;

            for(int k = 0; k <= i; ++k)
                gradient[k * 2] = dfdx;

            //df/d(dx)
            var dfddx = -dy * (x - x0) / dx / dx;

            gradient[2 * (i + 1)] = dfddx;

            //df/dy

            var dfdy1 = (x - x0) / dx;
            var dfdy0 = 1d - dfdy1;

            gradient[2 * i + 1] = dfdy0;
            gradient[2 * (i + 1) + 1] = dfdy1;

            return gradient;
        }
    }
}