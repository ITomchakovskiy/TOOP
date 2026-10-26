using System.Data;
using System.Runtime.CompilerServices;
using Functions;
using TOOP;
using TOOP.Functions;

namespace Tests;

public class FunctionTests
{
    private const double eps = 1.0E-15;
    public static Vector CreateLinearFunctionParameters(int num)
    {
        Vector parameters = num switch
        {
            0 => new([3d,0d,0d]), //f = 3
            1 => new([2d,3d,0d]), //f = 3x + 2
            2 => new([4d,0d,5d]), //f = 5y + 4
            3 => new([5d,4d,7d]), //f = 7y + 4x + 5
            _ => throw new ArgumentOutOfRangeException()
        };
        return parameters;
    }

    public static ReadOnlySpan<IVector> CreatePoints(int num)
    {
        Vector[] arguments = num switch
        {
            0 => [new([0d,0d]),new([2d,0d]),new([0d,3d]),new([6d,2d])], //for linear
            1 => [new([1d]),new([2.5d]), new([3d]), new([5d]), new([6d]), new([7.5d]), new([8d])],
            _ => throw new ArgumentOutOfRangeException()
        };
        return arguments;
    }

    private void FunctionValueTest(int parametersSet, int pointSet, double[] expected, Func<int,Vector> CreateParametersFunc, IParametricFunction parametric)
    {
        var parameters = CreateParametersFunc(parametersSet);
        var points = CreatePoints(pointSet);
        var func = parametric.Bind(parameters);
        if(points.Length != expected.Length) throw new ArgumentException("points set and expected values should have the same length");
        var actual = points.ToArray().Select(func.Value).ToArray();
        double res = 0d;
        for(int i = 0; i < points.Length; ++i)
        {
            double diff = actual[i] - expected[i]; diff *= diff;
            res += diff;
        }
        Assert.Equal(0d, res, eps);
    }

    public static IEnumerable<TheoryDataRow<int,int,double[]>> LinearFunctionValueTestData =>
    [
        new(0,0,[3d,3d,3d,3d]),
        new(1,0,[2d,8d,2d,20d]),
        new(2,0,[4d,4d,19d,14d]),
        new(3,0,[5d,13d,26d,43d]),
    ];

    [Theory]
    [MemberData(nameof(LinearFunctionValueTestData))]
    public void LinearFunctionValueTest(int parametersSet, int pointSet, double[] expected)
    {
        FunctionValueTest(parametersSet, pointSet, expected, CreateLinearFunctionParameters, new LinearFunction());
    }
    public static IEnumerable<TheoryDataRow<int,int,Vector[]>> LinearFunctionGradientData =>
    [
        new(0,0,[new([1d,0d,0d]),new([1d,2d,0d]),new([1d,0d,3d]),new([1d,6d,2d])]),
        new(1,0,[new([1d,0d,0d]),new([1d,2d,0d]),new([1d,0d,3d]),new([1d,6d,2d])]),
        new(2,0,[new([1d,0d,0d]),new([1d,2d,0d]),new([1d,0d,3d]),new([1d,6d,2d])]),
        new(3,0,[new([1d,0d,0d]),new([1d,2d,0d]),new([1d,0d,3d]),new([1d,6d,2d])]),
    ];
    [Theory]
    [MemberData(nameof(LinearFunctionGradientData))]
    public void LinearFunctionGradientTest(int parametersSet, int pointSet, Vector[] expected)
    {
        var parameters = CreateLinearFunctionParameters(parametersSet);
        IDifferentiableFunction func = (IDifferentiableFunction)new LinearFunction().Bind(parameters);
        var points = CreatePoints(pointSet);
        if(points.Length != expected.Length) throw new ArgumentException("points set and expected values should have the same length");
        var actual = points.ToArray().Select(p => new Vector([..func.Gradient(p)])).ToArray();
        double res = 0d;
        for(int i = 0; i < points.Length; ++i)
        {
            double diff = (actual[i] - expected[i]).Norm(); diff *= diff;
            res += diff;
        }
        Assert.InRange(res, 0d, eps);
    }

    public static Vector CreatePiecewiseFunctionParameters(int num)
    {
        Vector parameters = num switch
        {
            0 => new([0d,3d,10d,3d]), //f = 3
            1 => new([0d,2d,10d,32d]),//f = 3x + 2
            2 => new([0d,3d,5d,3d,10d,32d]), //f = 3 for x in range(0,5); f = 29/5x-26 for x in range(5,10)
            3 => new([0d,6d,2.5,11d,5d,13.5d,7.5,1d,10d,3.5]), //f = 2x + 6 for x in range (0,2.5); f = x + 8.5 for x in range(2.5,5); 
                                                               //f = -5x + 38.5 for x in range(5,7.5); f = x - 6.5 for x in range(7.5,10d);
            _ => throw new ArgumentOutOfRangeException()
        };
        return parameters;
    }

    public static IEnumerable<TheoryDataRow<int,int, double[]>> PiecewiseFunctionValueTestData =>
    [
        new(0,1,[3d,3d,3d,3d,3d,3d,3d]),
        new(1,1,[5d,9.5,11d,17d,20d,24.5d,26d]),
        new(2,1,[3d,3d,3d,3d,8.8,17.5,20.4]),
        new(3,1,[8d,11d,11.5,13.5,8.5,1d,1.5])
    ];

    [Theory]
    [MemberData(nameof(PiecewiseFunctionValueTestData))]
    public void PiecewiseFunctionValueTest(int parametersSet, int pointSet, double[] expected)
    {
        FunctionValueTest(parametersSet, pointSet, expected, CreatePiecewiseFunctionParameters, new PiecewiseLinearFunction());
    }
    public static IEnumerable<TheoryDataRow<int,int,Vector[]>> PiecewiseFunctionGradientData =>
    [
        new(0,1,[new([0d,0.9,0d,0.1]),new([0d,0.75,0d,0.25]),new([0d,0.7,0d,0.3]),
                 new([0d,0.5,0d,0.5]),new([0d,0.4,0d,0.6]),new([0d,0.25,0d,0.75]),new([0d,0.2,0d,0.8])]),
        new(1,1,[new([-2.7,0.9,-0.3,0.1]),new([-2.25,0.75,-0.75,0.25]),new([-2.1,0.7,-0.9,0.3]),
                 new([-1.5,0.5,-1.5,0.5]),new([-1.2,0.4,-1.8,0.6]),new([-0.75,0.25,-2.25,0.75]),new([-0.6,0.2,-2.4,0.8])]),
        new(2,1,[new([0d,0.8,0d,0.2,0d,0d]),new([0d,0.5,0d,0.5,0d,0d]),new([0d,0.4,0d,0.6,0d,0d]),
                 new([0d,0d,-5.8,1d,0d,0d]),new([0d,0d,-4.64,0.8,-1.16,0.2]),new([0d,0d,-2.9,0.5,-2.9,0.5]),new([0d,0d,-2.32,0.4,-3.48,0.6])]),
        new(3,1,[new([-1.2,0.6,-0.8,0.4,0d,0d,0d,0d,0d,0d]),new([0d,0d,-1d,1d,0d,0d,0d,0d,0d,0d]),
                 new([0d,0d,-0.8,0.8,-0.2,0.2,0d,0d,0d,0d]),new([0d,0d,0d,0d,5d,1d,0d,0d,0d,0d]),
                 new([0d,0d,0d,0d,3d,0.6,2d,0.4,0d,0d]),new([0d,0d,0d,0d,0d,0d,-1d,1d,0d,0d]),new([0d,0d,0d,0d,0d,0d,-0.8,0.8,-0.2,0.2])]),
    ];

    [Theory]
    [MemberData(nameof(PiecewiseFunctionGradientData))]
    public void PiecewiseFunctionGradientTest(int parametersSet, int pointSet, Vector[] expected)
    {
        var parameters = CreatePiecewiseFunctionParameters(parametersSet);
        IDifferentiableFunction func = (IDifferentiableFunction)new PiecewiseLinearFunction().Bind(parameters);
        var points = CreatePoints(pointSet);
        if(points.Length != expected.Length) throw new ArgumentException("points set and expected values should have the same length");
        var actual = points.ToArray().Select(p => new Vector([..func.Gradient(p)])).ToArray();
        double res = 0d;
        for(int i = 0; i < points.Length; ++i)
        {
            double diff = (actual[i] - expected[i]).Norm(); diff *= diff;
            res += diff;
        }
        Assert.InRange(res, 0d, eps);
    }

    public static Vector CreatePolynimialFunctionParameters(int num)
    {
        Vector parameters = num switch
        {
            0 => new([3d]), //f = 3
            1 => new([2d,3d]),//f = 3x + 2
            2 => new([5d,2d,-7d,1d]), //f = 5 + 2x - 7x^2 + x^3
            
            _ => throw new ArgumentOutOfRangeException()
        };
        return parameters;
    }

    public static IEnumerable<TheoryDataRow<int,int,double[]>> PolynomialFunctionValueTestData =>
    [
        new(0,1,[3d,3d,3d,3d,3d,3d,3d]),
        new(1,1,[5d,9.5,11d,17d,20d,24.5d,26d]),
        new(2,1,[1d,-18.125,-25d,-35d,-19d,48.125,85d]),
    ];

    [Theory]
    [MemberData(nameof(PolynomialFunctionValueTestData))]
    public void PolynomialFunctionValueTest(int parametersSet, int pointSet, double[] expected)
    {
        FunctionValueTest(parametersSet, pointSet, expected, CreatePolynimialFunctionParameters, new PolynomialFunction());
    }
}


