using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;

namespace Optimizators;

public abstract class OneDimensionalOptimizator
{
    public abstract double Minimize(IFunctional objective,
                                    IParametricFunction function,
                                    IVector initialParameters,
                                    IVector direction,
                                    IVector? minimumParameters = null,
                                    IVector? maximumParameters = null,
                                    double minimumCoefficient = 0d,
                                    double? maximumCoefficient = null, 
                                    double precision = 1.0E-7);
    protected double CalculateMaximumParameter(IFunctional objective,
                                             IParametricFunction function,
                                             IVector initialParameters,
                                             IVector direction,
                                             IVector? minimumParameters = null,
                                             IVector? maximumParameters = null,
                                             double initialStep = 1.0E-7,
                                             double stepMultiplyer = 2,
                                             double minimumCoefficient = 0d,
                                             int maxIter = 1000000)
    {
        var initialParametersVector = new Vector([.. initialParameters]);
        var directionVector = new Vector([.. direction]);
        var func = function.Bind(initialParameters);
        var minValue = objective.Value(func);
        var step = initialStep;
        for(int i = 0; i < maxIter; step *= stepMultiplyer)
        {
            var coefficient = minimumCoefficient + step;
            var parameters = initialParametersVector + directionVector * coefficient;
            if(minimumParameters is not null) parameters = Vector.ClampFloor(parameters, minimumParameters);
            if(maximumParameters is not null) parameters = Vector.ClampCeiling(parameters, maximumParameters);
            func = function.Bind(parameters);
            var curValue = objective.Value(func);
            if(curValue > minValue)
            {
                return step;
            }
            else minValue = curValue;
        }
        return double.MaxValue;
    }
}

public class GoldenSectionSearch : OneDimensionalOptimizator
{
    private readonly double coef = (3d - Math.Sqrt(5d)) / 2d;
    private double calcLeftPoint(double a, double b) => a * (1d - coef) + b * coef;
    private double calcRightPoint(double a, double b) => b * (1d - coef) + a * coef;

    public override double Minimize(IFunctional objective, IParametricFunction function, IVector initialParameters, IVector direction, IVector? minimumParameters = null, IVector? maximumParameters = null, double minimumCoefficient = 0, double? maximumCoefficient = null, double precision = 1.0E-7)
    {
        Vector initialParametersVector = new([.. initialParameters]);
        Vector directionVector = new([.. direction]);
        maximumCoefficient ??= CalculateMaximumParameter(objective, function, initialParameters, direction, minimumParameters, maximumParameters);
        double coefficient1 = calcLeftPoint(minimumCoefficient, (double)maximumCoefficient);
        IVector parameters = initialParametersVector + directionVector * coefficient1;
        if(minimumParameters is not null) parameters = Vector.ClampFloor(parameters, minimumParameters);
        if(maximumParameters is not null) parameters = Vector.ClampCeiling(parameters, maximumParameters);
        double val1 = objective.Value(function.Bind(parameters));
        double coefficient2 = calcRightPoint(minimumCoefficient, (double)maximumCoefficient);
        parameters = initialParametersVector + directionVector * coefficient2;
        if(minimumParameters is not null) parameters = Vector.ClampFloor(parameters, minimumParameters);
        if(maximumParameters is not null) parameters = Vector.ClampCeiling(parameters, maximumParameters);
        double val2 = objective.Value(function.Bind(parameters));
        while((maximumCoefficient - minimumCoefficient) >= precision)
        {
            if(val1 < val2)
            {
                maximumCoefficient = coefficient2;
                coefficient2 = coefficient1;
                val2 = val1;
                coefficient1 = calcLeftPoint(minimumCoefficient, (double)maximumCoefficient);
                parameters = initialParametersVector + directionVector * coefficient1;
                if(minimumParameters is not null) parameters = Vector.ClampFloor(parameters, minimumParameters);
                if(maximumParameters is not null) parameters = Vector.ClampCeiling(parameters, maximumParameters);
                val1 = objective.Value(function.Bind(parameters));
            }
            else
            {
                minimumCoefficient = coefficient1;
                coefficient1 = coefficient2;
                val1 = val2;
                coefficient2 = calcRightPoint(minimumCoefficient, (double)maximumCoefficient);
                parameters = initialParametersVector + directionVector * coefficient2;
                if(minimumParameters is not null) parameters = Vector.ClampFloor(parameters, minimumParameters);
                if(maximumParameters is not null) parameters = Vector.ClampCeiling(parameters, maximumParameters);
                val2 = objective.Value(function.Bind(parameters));
            }
        }
        return (double)((maximumCoefficient + minimumCoefficient) / 2d);
    }
}