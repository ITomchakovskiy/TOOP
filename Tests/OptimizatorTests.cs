using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Functionals;
using Functions;
using Optimizators;
using TOOP;
using TOOP.Functions;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole;

namespace Tests;
public class OptimizatorTests
{
    private const double eps = 1.0E-7;

    private const double minParam = -8d;
    private const double maxParam = 50d;

    private const double initStep = 1d;

    private const double minStep = 0.8;
    private const double maxParamForSteps = 13d;

    private const double maxParamForInitialKnot = 3d;

    private const double minParamForInitialKnot = -3d;

    private ITestOutputHelper _console;

    public OptimizatorTests(ITestOutputHelper console) {_console = console;}

    static OptimizatorTests()
    {
        var RectanglePoints = CreatePointsInRectangle((0d,0d),(10d,10d), 3, 3);

        var SegmentPoints = CreatePointsInSegment(-3d,13d, 40);
        
        IFunctional ChangeFunctionalPoints(IVector parameters, IParametricFunction func, IFunctional functional)
        {
            var function = func.Bind(parameters);
            var points = func switch
            {
                LinearFunction => RectanglePoints,
                _ => SegmentPoints
            };
            return functional switch
            {
                L1Norm =>   new L1Norm(points,points.Select(function.Value)),
                L2Norm =>   new L2Norm(points,points.Select(function.Value)),
                LInfNorm => new LInfNorm(points,points.Select(function.Value)),
                _ => throw new NotImplementedException()
            };
        }

        var GeneralTests = FunctionalTests.ZeroNormTestData.Select(el => new TheoryDataRow<IParametricFunction, IFunctional, IVector>
                                                                                       (el.Data.Item2,ChangeFunctionalPoints(el.Data.Item1, el.Data.Item2, el.Data.Item3)
                                                                                        , el.Data.Item1));
        GeneralTests = GeneralTests.Where((el, i) => !(el.Data.Item1 is PiecewiseLinearFunction && i % 11 == 7 || el.Data.Item1 is PolynomialFunction && i % 11 == 10));
        
        SimulatedAnnealingTestData = GeneralTests;

        ConjugentGradientTestData = GeneralTests.Where(el => el.Data.Item1 is not PolynomialFunction && el.Data.Item2 is not LInfNorm);

        GaussNewtonTestData = GeneralTests.Where(el => el.Data.Item1 is not PolynomialFunction && el.Data.Item2 is L2Norm);
    }
    private static IEnumerable<Vector> CreatePointsInSegment(double x0, double x1, int internalPointsCount)
    {
        var result = new Vector[internalPointsCount + 2];
        for(int i = 0; i < internalPointsCount + 2; ++i)
            result[i] = new([x0 * (internalPointsCount + 1 - i) / (internalPointsCount + 1) + x1 * i / (internalPointsCount + 1)]);
        return result;
    }
    private static IEnumerable<Vector> CreatePointsInRectangle((double x, double y) A, (double x, double y) B, int nx, int ny)
    {
        var result = new Vector[(nx + 2)*(ny + 2)];
        var pointsX = CreatePointsInSegment(A.x,B.x,nx).ToArray();
        var pointsY = CreatePointsInSegment(A.y,B.y,ny).ToArray();
        for(int i = 0; i < pointsY.Length; ++i)
        {
            for(int j = 0; j < pointsX.Length; ++j)
                result[i * pointsY.Length + j] = new([pointsX[j][0],pointsY[i][0]]);
        }
        return result;
    }
    private void OptimizatorTest(IParametricFunction parametricFunction, IFunctional functional, IOptimizator optimizator, IVector expectedParameters, IVector? initialParameters = null, IVector? minimumParameters = null, IVector? maximumParameters = null)
    {
        initialParameters ??= new Vector(expectedParameters.Length);
        if(minimumParameters is null)
        {
            double[] arr = new double[initialParameters.Length];
            Array.Fill(arr, minParam);
            if(parametricFunction is PiecewiseLinearFunction)
            {
                for(int i = 1; i < initialParameters.Length/2;++i)
                {
                    arr[i*2] = minStep;
                    initialParameters[i*2] = initStep;
                    initialParameters[i*2 + 1] = i;
                }
                arr[0] = minParamForInitialKnot;
            }
            minimumParameters = new Vector(arr);
        }
        if(maximumParameters is null)
        {
            double[] arr = new double[initialParameters.Length];
            Array.Fill(arr, maxParam);
            if(parametricFunction is PiecewiseLinearFunction)
            {
                for(int i = 1; i < initialParameters.Length/2;++i)
                {
                    arr[i*2] = maxParamForSteps / (initialParameters.Length/2 -1);
                }
                arr[0] = maxParamForInitialKnot;
            }
            maximumParameters = new Vector(arr);
        }
       
        var actual = optimizator.Minimize(functional, parametricFunction, initialParameters, minimumParameters, maximumParameters);
        double res = (new Vector([..actual]) - new Vector([.. expectedParameters])).Norm();
        var value = functional.Value(parametricFunction.Bind(actual));
        _console.WriteLine($"Initial objective functional value: {functional.Value(parametricFunction.Bind(initialParameters))}");
        _console.WriteLine($"Functional Value:{value}\tParameters deviation{res}");
        Assert.Equal(0d,value,1d);
    }

    public static IEnumerable<TheoryDataRow<IParametricFunction, IFunctional, IVector>> SimulatedAnnealingTestData;

    public static IEnumerable<TheoryDataRow<IParametricFunction, IFunctional, IVector>> ConjugentGradientTestData;

    public static IEnumerable<TheoryDataRow<IParametricFunction, IFunctional, IVector>> GaussNewtonTestData;
    
    [Theory]
    [MemberData(nameof(SimulatedAnnealingTestData))]
    public void SimulatedAnnealingOptimizatorTest(IParametricFunction parametricFunction, IFunctional functional, IVector expectedParameters) =>
                    OptimizatorTest(parametricFunction, functional, new SimulatedAnnealingOptimizator(), expectedParameters);

    [Theory]
    [MemberData(nameof(ConjugentGradientTestData))]
    public void ConjugentGradientTest(IParametricFunction parametricFunction, IFunctional functional, IVector expectedParameters) =>
                    OptimizatorTest(parametricFunction, functional, new ConjugateGradientMethodOptimizator(), expectedParameters);
    
    [Theory]
    [MemberData(nameof(GaussNewtonTestData))]
    public void GaussNewtonTest(IParametricFunction parametricFunction, IFunctional functional, IVector expectedParameters) =>
                    OptimizatorTest(parametricFunction, functional, new GaussNewtonOptimizator(), expectedParameters);
    
}