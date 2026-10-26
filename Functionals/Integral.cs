using System;
using System.Linq;
using Functions;
using TOOP;

using Real = double;

namespace Functionals;

public class Integral : IFunctional
{
    readonly Real _volume;
    readonly IVector[] _samples;

    public Integral(IVector lower, IVector upper, int sampleCount = 1000, int? seed = null)
    {
        if (lower.Length == 0 || lower.Length != upper.Length)
        {
            throw new ArgumentException("Lower and upper bounds must have the same positive length.");
        }
        if (sampleCount < 1)
        {
            throw new ArgumentException("At least one sample is required.", nameof(sampleCount));
        }

        for (var i = 0; i < lower.Length; i++)
        {
            if (lower[i] > upper[i])
            {
                throw new ArgumentException("Each lower bound must be less than or equal to the matching upper bound.");
            }
        }

        Real volume = 1;
        for (var i = 0; i < lower.Length; i++)
        {
            volume *= upper[i] - lower[i];
        }
        _volume = volume;

        Random random = seed is null ?
            new Random() :
            new Random(seed.Value);

        _samples = new IVector[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var point = new Real[lower.Length];
            for (var dimension = 0; dimension < lower.Length; dimension++)
            {
                point[dimension] = lower[dimension] + random.NextDouble() * (upper[dimension] - lower[dimension]);
            }

            _samples[i] = new Vector(point);
        }
    }

    public Real Value(IFunction function)
    {
        Real sum = 0;
        for (var i = 0; i < _samples.Length; i++)
        {
            sum += function.Value(_samples[i]);
        }

        return _volume * sum / _samples.Length;
    }
}
