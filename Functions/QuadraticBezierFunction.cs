using Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Functions;

class QuadraticBezierFunction : IParametricFunction
{
    private readonly double _xMin;
    private readonly double _xMax;

    public QuadraticBezierFunction(double xMin = 0.0, double xMax = 1.0)
    {
        _xMin = xMin;
        _xMax = xMax;
    }

    public IFunction Bind(IVector parameters)
    {
        return new QuadraticBezierFunctionImpl(parameters, _xMin, _xMax);
    }

    private class QuadraticBezierFunctionImpl : IFunction
    {
        private readonly IVector _controlPoints;
        private readonly int _segmentsLength;
        private readonly double _xMin;
        private readonly double _xMax;

        public QuadraticBezierFunctionImpl(IVector parameters, double xMin, double xMax)
        {
            _controlPoints = parameters;
            _segmentsLength = (_controlPoints.Length - 1) / 2;
            _xMin = xMin;
            _xMax = xMax;
        }

        public double Value(IVector point)
        {
            var x = point[0];
            var tNorm = (x - _xMin) / (_xMax - _xMin);
            tNorm = double.Clamp(tNorm, 0.0, 1.0);

            var tGlobal = tNorm * _segmentsLength;
            var seg = int.Min((int)tGlobal, _segmentsLength - 1);
            var t = tGlobal - seg;

            var i = seg * 2;
            var u = 1 - t;

            return double.Pow(u, 2) * _controlPoints[i]
                   + 2 * u * t * _controlPoints[i + 1]
                   + double.Pow(t, 2) * _controlPoints[i + 2];
        }
    }
}
