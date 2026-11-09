using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;

namespace Optimizators;

public class ConjugateGradientMethodOptimizator : IOptimizator
{
    private const int maxiter = 100000;
    private OneDimensionalOptimizator _optimizator = new GoldenSectionSearch();
    public IVector Minimize(IFunctional objective, IParametricFunction function, IVector initialParameters, IVector? minimumParameters = null, IVector? maximumParameters = null)
    {
        if(objective is IDifferentiableFunctional diffObjective)
            return Minimize(diffObjective, function, initialParameters, _optimizator, minimumParameters, maximumParameters);
        throw new ArgumentException("objective functional doesn't implement IDifferentiableFunctional interface");
    }

    public IVector Minimize(IDifferentiableFunctional objective, IParametricFunction function, IVector initialParameters, OneDimensionalOptimizator optimizator, IVector? minimumParameters, IVector? maximumParameters, double precision = 1.0E-7, int maxIterationsCount = maxiter)
    {
        IVector result = initialParameters;
        for(int i = 0; i < maxIterationsCount; ++i)
        {
            if(Cycle(objective, function, result, optimizator, minimumParameters, maximumParameters, precision, out result)) return result;
        }
        return result;
    }

    private bool Cycle(IDifferentiableFunctional objective, IParametricFunction function, IVector initialParameters, OneDimensionalOptimizator optimizator, IVector? minimumParameters, IVector? maximumParameters, double precision, out IVector result)
    {
        Vector parameters = new([.. initialParameters]);
        var func = function.Bind(parameters);
        var gradient = new Vector([.. objective.Gradient(func)]);
        var direction = -gradient;
        for(int i = 0; i <= parameters.Length; ++i)
        {
            double coefficient = optimizator.Minimize(objective, function, parameters, direction, minimumParameters, maximumParameters);
            parameters += coefficient * direction;
            if(minimumParameters is not null)
                parameters = new([..parameters.Select((el,i) => Math.Max(el, minimumParameters[i]))]);
            if(maximumParameters is not null)
                parameters = new([..parameters.Select((el,i) => Math.Min(el, maximumParameters[i]))]);
            func = function.Bind(parameters);
            var curGradient = new Vector([.. objective.Gradient(func)]);
            double omega = curGradient.Norm() / gradient.Norm(); omega *= omega;
            gradient = curGradient;
            direction = -gradient + omega * direction;
            if(direction.Norm() < precision) {result = parameters; return true;} 
        }
        //return Minimize(objective, function, parameters, minimumParameters, maximumParameters);
        result = parameters;
        return false;
    }
}