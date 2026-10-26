using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;
using TOOP.Functions;
using TOOP.Matrix;
using Xunit;

namespace Tests;
public class FunctionalTests
{
    private const double eps = 1.0E-13;
    static FunctionalTests()
    {
        var LinearTestData = FunctionTests.LinearFunctionValueTestData.
                             Select(i => new TheoryDataRow<IVector, IEnumerable<IVector>, double[], IParametricFunction>(FunctionTests.CreateLinearFunctionParameters(i.Data.Item1),
                             FunctionTests.CreatePoints(i.Data.Item2).ToArray(),
                             i.Data.Item3, new LinearFunction()));
        var PiecewiseTestData = FunctionTests.PiecewiseFunctionValueTestData.
                                Select(i => new TheoryDataRow<IVector, IEnumerable<IVector>, double[], IParametricFunction>(FunctionTests.CreatePiecewiseFunctionParameters(i.Data.Item1),
                                FunctionTests.CreatePoints(i.Data.Item2).ToArray(),
                                i.Data.Item3, new PiecewiseLinearFunction()));
        var PolynomialTestData = FunctionTests.PolynomialFunctionValueTestData.
                                 Select(i => new TheoryDataRow<IVector, IEnumerable<IVector>, double[], IParametricFunction>(FunctionTests.CreatePolynimialFunctionParameters(i.Data.Item1),
                                 FunctionTests.CreatePoints(i.Data.Item2).ToArray(),
                                 i.Data.Item3, new PolynomialFunction()));
        var ZeroTests = LinearTestData.Concat(PiecewiseTestData).Concat(PolynomialTestData);
        var l1NormZeroTests = ZeroTests.Select(i => new TheoryDataRow<IVector, IParametricFunction, IFunctional>(i.Data.Item1,i.Data.Item4,new L1Norm(i.Data.Item2, i.Data.Item3)));
        var l2NormZeroTests = ZeroTests.Select(i => new TheoryDataRow<IVector, IParametricFunction, IFunctional>(i.Data.Item1,i.Data.Item4,new L2Norm(i.Data.Item2, i.Data.Item3)));
        var lInfNormZeroTests = ZeroTests.Select(i => new TheoryDataRow<IVector, IParametricFunction, IFunctional>(i.Data.Item1,i.Data.Item4,new LInfNorm(i.Data.Item2, i.Data.Item3)));

        ZeroNormTestData = l1NormZeroTests.Concat(l2NormZeroTests).Concat(lInfNormZeroTests);

        var lastLinearData = LinearTestData.Last().Data;
        var lastPiecewiseData = PiecewiseTestData.Last().Data;
        var lastPolynomialData = PolynomialTestData.Last().Data;
        (IVector, IEnumerable<IVector>, double[], IParametricFunction)[] testData = [lastLinearData, lastPiecewiseData, lastPolynomialData];
        double[] expectedLinear = [20d,20d,20d,20d];
        double[] expectedPiecewise = [7d,7d,7d,7d,7d,7d,7d];
        double[] expectedPolynomial = [0d,-20d,-20d,-20d,-20d,40d,60d];
        double[][] expectedValuesForNorms = [expectedLinear, expectedPiecewise, expectedPolynomial];
        
        var l1Norms =   Enumerable.Range(0,3).Select(i => new L1Norm(testData[i].Item2,expectedValuesForNorms[i])).ToArray();
        var l2Norms =   Enumerable.Range(0,3).Select(i => new L2Norm(testData[i].Item2,expectedValuesForNorms[i])).ToArray();
        var lInfNorms = Enumerable.Range(0,3).Select(i => new LInfNorm(testData[i].Item2,expectedValuesForNorms[i])).ToArray();
        double[] expectedValuesL1Norm = [51d,29d,57d];
        double[] expectedValuesL2Norm = [28.9654967159205,12.1655250605964,30.7657480000081];
        double[] expectedValuesLInfNorm = [23d,6.5,25d];
        
        L1NormValueTestData = testData.Select((el,i) => new TheoryDataRow<IVector,IParametricFunction,IFunctional,double>(el.Item1, el.Item4, l1Norms[i], expectedValuesL1Norm[i]));
        L2NormValueTestData = testData.Select((el,i) => new TheoryDataRow<IVector,IParametricFunction,IFunctional,double>(el.Item1, el.Item4, l2Norms[i], expectedValuesL2Norm[i]));
        LInfNormValueTestData = testData.Select((el,i) => new TheoryDataRow<IVector,IParametricFunction,IFunctional,double>(el.Item1, el.Item4, lInfNorms[i], expectedValuesLInfNorm[i]));
        
        //gradient tests
        Vector[] ExpectedGradientsL1 = [new([0d,4d,5d]),
                                        new([-1.2,0.6,-2.6,2.2,7.8,1.8,3.8,-1.4,0.2,-0.2])];
        Vector[] ExpectedGradientsL2 = [new([0.241666837915904, 4.2809554145103, 2.20952537523112]),
                                        new([-0.0986393923832147, 0.0493196961916073, -0.690475746682503,
                                             0.657595949221431, 2.96740172086171, 0.682255797317235,
                                             1.1014732149459, -0.805555037796253, 0.0904194430179468,-0.0904194430179468])];


        L1NormGradientTestData = L1NormValueTestData.Take(2).Select((el,i) => new TheoryDataRow<IVector, IParametricFunction, IDifferentiableFunctional, IVector>(el.Data.Item1,el.Data.Item2, (IDifferentiableFunctional)el.Data.Item3, ExpectedGradientsL1[i]));
        L2NormGradientTestData = L2NormValueTestData.Take(2).Select((el,i) => new TheoryDataRow<IVector, IParametricFunction, IDifferentiableFunctional, IVector>(el.Data.Item1,el.Data.Item2, (IDifferentiableFunctional)el.Data.Item3, ExpectedGradientsL2[i]));

        //residual test

        Vector[] expectedResiduals = [new([-15d,-7d,6d,23d]), new([1d,4d,4.5,6.5,1.5,-6d,-5.5]), new([1d,1.875,-5d,-15d,1d,8.125,25d])];

        L2NormResidualsTestData = L2NormValueTestData.Select((el,i) => new TheoryDataRow<IVector, IParametricFunction, ILeastSquaresFunctional, IVector>(el.Data.Item1,el.Data.Item2,(ILeastSquaresFunctional)el.Data.Item3, expectedResiduals[i]));

        DenseMatrix[] expectedJacobians = [new(FunctionTests.LinearFunctionGradientData.Last().Data.Item3),
                                           new(FunctionTests.PiecewiseFunctionGradientData.Last().Data.Item3)];

        L2NormJacobianTestData = L2NormValueTestData.Take(2).Select((el,i) => new TheoryDataRow<IVector, IParametricFunction, ILeastSquaresFunctional, IMatrix> (el.Data.Item1,el.Data.Item2, (ILeastSquaresFunctional)el.Data.Item3, expectedJacobians[i]));
    }
    private void NormValueTest(IVector funcParameters, IParametricFunction parametricFunction, IFunctional functional, double expected)
    {
        var func = parametricFunction.Bind(funcParameters);
        var actual = functional.Value(func);
        Assert.Equal(expected, actual, eps);
    }

    private void FunctionalGradientTest(IVector funcParameters, IParametricFunction parametricFunction, IDifferentiableFunctional functional, IVector expected)
    {
        Vector expectedVector = new([.. expected]);
        var func = parametricFunction.Bind(funcParameters);
        var actual = new Vector([.. functional.Gradient(func)]);
        var residual = (expectedVector - actual).Norm();
        Assert.Equal(0d, residual, eps);
    }

    private void ResidualTest(IVector funcParameters, IParametricFunction parametricFunction, ILeastSquaresFunctional functional, IVector expected)
    {
        Vector expectedVector = new([.. expected]);
        var func = parametricFunction.Bind(funcParameters);
        var actual = new Vector([.. functional.Residual(func)]);
        var res = (expectedVector - actual).Norm();
        Assert.Equal(0d, res, eps);
    }

    private void JacobianTest(IVector funcParameters, IParametricFunction parametricFunction, ILeastSquaresFunctional functional, IMatrix expected)
    {
        var func = parametricFunction.Bind(funcParameters);
        var actual = functional.Jacobian(func);
        var diff = MatrixOperators.Subtract(actual,expected);
        double res = 0d;
        for(int i = 0; i < diff.RowsCount; ++i)
        {
            for(int j = 0; j < diff.ColumnsCount; ++j)
                res += diff[i,j]*diff[i,j];
        }
        Assert.Equal(0d, Math.Sqrt(res), eps);
    }

    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,IFunctional>> ZeroNormTestData ;

    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,IFunctional,double>> L1NormValueTestData;
    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,IFunctional,double>> L2NormValueTestData;
    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,IFunctional,double>> LInfNormValueTestData;

    [Theory]
    [MemberData(nameof(ZeroNormTestData))]
    public void ExpectedFunctionValuesEqualsActualaValues_ReturnsZero(IVector funcParameters, IParametricFunction function, IFunctional functional) =>
                    NormValueTest(funcParameters, function, functional,0d);

    [Theory]
    [MemberData(nameof(L1NormValueTestData))]
    public void L1NormValueTest(IVector funcParameters, IParametricFunction function, IFunctional functional, double expected) =>
                    NormValueTest(funcParameters, function, functional, expected);

    [Theory]
    [MemberData(nameof(L2NormValueTestData))]
    public void L2NormValueTest(IVector funcParameters, IParametricFunction function, IFunctional functional, double expected) =>
                    NormValueTest(funcParameters, function, functional, expected);

    [Theory]
    [MemberData(nameof(LInfNormValueTestData))]
    public void LInfNormValueTest(IVector funcParameters, IParametricFunction function, IFunctional functional, double expected) =>
                    NormValueTest(funcParameters, function, functional, expected);

    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,IDifferentiableFunctional,IVector>> L1NormGradientTestData;
    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,IDifferentiableFunctional,IVector>> L2NormGradientTestData;

    [Theory]
    [MemberData(nameof(L1NormGradientTestData))]
    public void L1NormGradientTest(IVector funcParameters, IParametricFunction function, IDifferentiableFunctional functional, IVector expected) =>
                    FunctionalGradientTest(funcParameters, function, functional, expected);

    [Theory]
    [MemberData(nameof(L2NormGradientTestData))]
    public void L2NormGradientTest(IVector funcParameters, IParametricFunction function, IDifferentiableFunctional functional, IVector expected) =>
                    FunctionalGradientTest(funcParameters, function, functional, expected);

    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,ILeastSquaresFunctional,IVector>> L2NormResidualsTestData;

    [Theory]
    [MemberData(nameof(L2NormResidualsTestData))]
    public void L2NormResidualsTest(IVector funcParameters, IParametricFunction function, ILeastSquaresFunctional functional, IVector expected) => 
                    ResidualTest(funcParameters, function, functional, expected);

    public static IEnumerable<TheoryDataRow<IVector,IParametricFunction,ILeastSquaresFunctional,IMatrix>> L2NormJacobianTestData;

    [Theory]
    [MemberData(nameof(L2NormJacobianTestData))]
    public void L2NormJacoabianTest(IVector funcParameters, IParametricFunction function, ILeastSquaresFunctional functional, IMatrix expected) =>
                    JacobianTest(funcParameters, function, functional, expected);
    
}