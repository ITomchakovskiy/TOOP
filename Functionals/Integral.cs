using System;
using System.Linq;
using Functions;
using TOOP;

using Real = double;

namespace Functionals;

public class Integral : IFunctional
{
    private readonly Real _cellVolume;
    private readonly IVector _lower;
    private readonly IVector _upper;
    private readonly IVector _H;
    private readonly int _N;

    public Integral(IVector lower, IVector upper, IVector H, int? seed = null)
    {
        if (lower.Length == 0 || lower.Length != upper.Length || H.Length != lower.Length)
        {
            throw new ArgumentException("Lower and upper bounds and H must have the same positive length.");
        }

        for (var i = 0; i < lower.Length; i++)
        {
            if (lower[i] > upper[i])
            {
                throw new ArgumentException("Each lower bound must be less than or equal to the matching upper bound.");
            }
        }

        _H = new Vector([..H]);

        Real volume = 1;
        int N = 1;
        for (var i = 0; i < lower.Length; i++)
        {
            var segmentLength = upper[i] - lower[i];
            var segmentsCount = (int)Math.Ceiling(segmentLength / H[i]);
            _H[i] = segmentLength / segmentsCount;
            volume *= _H[i];
            N *= segmentsCount;
        }
        _cellVolume = volume;
        _N = N;
        _lower = lower;
        _upper = upper;
    }

    public Real Value(IFunction function)
    {
        Real sum = 0d;
        for(int i = 0; i < _N; ++i) //cells
        {
            Vector center = new(_lower.Length);
            var ind = i;
            for(int j = 0; j < center.Length; ++j)
            {
                var segmentLength = _upper[j] - _lower[j];
                var segmentsCount = (int)(segmentLength / _H[j]);
                var curInd = ind % segmentsCount;
                var x0 = _lower[j] + _H[j] * curInd;
                var x1 = curInd != segmentsCount - 1 ? x0 + _H[j] : _upper[j];
                center[j] = (x0 + x1) / 2d;
                ind /= segmentsCount;
            }
            sum += function.Value(center);
        }
        return sum * _cellVolume;
    }
}
