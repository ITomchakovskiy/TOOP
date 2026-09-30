using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;

namespace Optimizators;

class ConjugateGradientMethodOptimizator : IOptimizator
{
    public IVector Minimize(IFunctional objective, IParametricFunction function, IVector initialParameters, IVector? minimumParameters = null, IVector? maximumParameters = null)
    {
        if(objective is IDifferentiableFunctional diffObjective)
            return Minimize(diffObjective, function, initialParameters, new GoldenSectionSearch(), minimumParameters, maximumParameters);
        throw new ArgumentException("objective functional doesn't implement IDifferentiableFunctional interface");
    }

    private IVector Minimize(IDifferentiableFunctional objective, IParametricFunction function, IVector initialParameters, OneDimensionalOptimizator optimizator, IVector? minimumParameters, IVector? maximumParameters, double precision = 1.0E-7)
    {
        Vector parameters = new([.. initialParameters]);
        var func = function.Bind(parameters);
        var gradient = new Vector([.. objective.Gradient(func)]);
        var direction = -gradient;
        for(int i = 0; i <= parameters.Length; ++i)
        {
            double coefficient = optimizator.Minimize(objective, function, parameters, direction);
            parameters += coefficient * direction;
            func = function.Bind(parameters);
            var curGradient = new Vector([.. objective.Gradient(func)]);
            double omega = curGradient.Norm() / gradient.Norm(); omega *= omega;
            gradient = curGradient;
            direction = -gradient + omega * direction;
            if(direction.Norm() < precision) return parameters; 
        }
        return Minimize(objective, function, parameters, minimumParameters, maximumParameters);
    }
}