using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;

namespace Optimizators;

public class SimulatedAnnealingOptimizator : IOptimizator
{
    public SimulatedAnnealingOptimizator(int seed) {random = new Random(seed);}
    public SimulatedAnnealingOptimizator() {random = new Random();}
    private Random random;
    private static double AcceptanceProbability(double f0, double f1, double t) => f1 < f0 ? 1.0 : Math.Exp(-(f1 - f0) / t);
    private double Normal(double mu, double sigma)
    {
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        var z0 = Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(2 * Math.PI * u2);
        return sigma * z0 + mu;
    }
    private IVector Neighbour(IVector x, IVector minimumParameters, IVector maximumParameters, double temperature)
    {
        var x1 = new double[x.Length];
        for (int i = 0; i < x.Length; i++)
        {
            var t = (x[i] - minimumParameters[i]) / (maximumParameters[i] - minimumParameters[i] + 1e-30);
            t = Math.Min(1, Math.Max(Normal(t, temperature), 0));
            x1[i] = (1 - t) * minimumParameters[i] + t * maximumParameters[i];
        }
        return new Vector(x1);
    }

    public IVector Minimize(IFunctional objective, IParametricFunction function, IVector initialParameters, IVector? minimumParameters, IVector? maximumParameters, double precision = 1.0E-7, int maxIterationCount = 1000000, double initialTemperature = 1d)
    {
        if(minimumParameters is null)
        {
            double[] values = new double[initialParameters.Length];
            Array.Fill(values, double.MinValue);
            minimumParameters = new Vector(values);
        }
        if(maximumParameters is null)
        {
            double[] values = new double[initialParameters.Length];
            Array.Fill(values, double.MaxValue);
            maximumParameters = new Vector(values);
        }
        IVector x0 = new Vector([..initialParameters]);
        if (minimumParameters.Length != initialParameters.Length || maximumParameters.Length != initialParameters.Length) throw new ArgumentOutOfRangeException($"{nameof(minimumParameters)} и {nameof(maximumParameters)} должны содержать по {initialParameters.Length} параметров");

        var func = function.Bind(x0);
        var f0 = objective.Value(func);


        var temperature = initialTemperature;
        for (int k = 0; temperature > precision && k < maxIterationCount; ++k)
        {
            temperature = initialTemperature / (1 + Math.Log(1 + k));
            var x1 = Neighbour(x0, minimumParameters, maximumParameters, temperature);
            func = function.Bind(x1);
            var f1 = objective.Value(func);
            var probability = AcceptanceProbability(f0, f1, temperature);

            if (probability >= random.NextDouble())
            {
                f0 = f1;
                x0 = x1;
            }
        }
        return x0;
    }

    public IVector Minimize(IFunctional objective, IParametricFunction function, IVector initialParameters, IVector? minimumParameters = null, IVector? maximumParameters = null)
    {
        return Minimize(objective, function, initialParameters, minimumParameters, maximumParameters);
    }
}