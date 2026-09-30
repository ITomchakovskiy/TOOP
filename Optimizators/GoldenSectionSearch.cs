using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;

namespace Optimizators;

abstract class OneDimensionalOptimizator
{
    public abstract double Minimize(IFunctional objective,
                                    IParametricFunction function,
                                    IVector initialParameters,
                                    IVector direction,
                                    double minimumCoefficient = 0d,
                                    double? maximumCoefficient = null, 
                                    double precision = 1.0E-7);
    protected double CalculateMaximumParameter(IFunctional objective,
                                             IParametricFunction function,
                                             IVector initialParameters,
                                             IVector direction,
                                             double initialStep = 1.0E-7,
                                             double stepMultiplyer = 2,
                                             double minimumCoefficient = 0d,
                                             int maxIter = 1000000)
    {
        double result = double.MaxValue;
        var initialParametersVector = new Vector([.. initialParameters]);
        var directionVector = new Vector([.. direction]);
        var func = function.Bind(initialParameters);
        var minValue = objective.Value(func);
        bool isFound = false;
        var step = initialStep;
        for(;!isFound; step *= stepMultiplyer)
        {
            var coefficient = minimumCoefficient + step;
            var parameters = initialParametersVector + directionVector * coefficient;
            func = function.Bind(parameters);
            var curValue = objective.Value(func);
            if(curValue > minValue)
            {
                result = minimumCoefficient;
                isFound = true;
            }
            else minValue = curValue;
        }
        return result;
    }
}

class GoldenSectionSearch : OneDimensionalOptimizator
{
    private readonly double coef = (3d - Math.Sqrt(5d)) / 2d;
    private double calcLeftPoint(double a, double b) => a * (1d - coef) + b * coef;
    private double calcRightPoint(double a, double b) => b * (1d - coef) + a * coef;
    public override double Minimize(IFunctional objective, IParametricFunction function, IVector initialParameters, IVector direction, double minimumCoefficient = 0, double? maximumCoefficient = null, double precision = 1.0E-7)
    {
        Vector initialParametersVector = new([.. initialParameters]);
        Vector directionVector = new([.. direction]);
        maximumCoefficient ??= CalculateMaximumParameter(objective, function, initialParameters, direction);
        double coefficient1 = calcLeftPoint(minimumCoefficient, (double)maximumCoefficient);
        double val1 = objective.Value(function.Bind(initialParametersVector + directionVector * coefficient1));
        double coefficient2 = calcRightPoint(minimumCoefficient, (double)maximumCoefficient);
        double val2 = objective.Value(function.Bind(initialParametersVector + directionVector * coefficient2));
        while((maximumCoefficient - minimumCoefficient) >= precision)
        {
            if(val1 < val2)
            {
                maximumCoefficient = coefficient2;
                coefficient2 = coefficient1;
                val2 = val1;
                coefficient1 = calcLeftPoint(minimumCoefficient, (double)maximumCoefficient);
                val1 = objective.Value(function.Bind(initialParametersVector + directionVector * coefficient1));
            }
            else
            {
                minimumCoefficient = coefficient1;
                coefficient1 = coefficient2;
                val1 = val2;
                coefficient2 = calcRightPoint(minimumCoefficient, (double)maximumCoefficient);
                val2 = objective.Value(function.Bind(initialParametersVector + directionVector * coefficient2));
            }
        }
        return (double)((maximumCoefficient - minimumCoefficient) / 2d);
    }
}