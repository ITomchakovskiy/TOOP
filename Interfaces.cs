using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;
using TOOP.Matrix;

namespace Functions
{
    public interface IParametricFunction
    {
        IFunction Bind(IVector parameters);
    }

    public interface IFunction
    {
        double Value(IVector point);
    }

    public interface IDifferentiableFunction : IFunction
    {
        // По параметрам исходной IParametricFunction
        IVector Gradient(IVector point);
    }
}

namespace Functionals
{
    public interface IFunctional
    {
        double Value(IFunction function);
    }

    public interface IDifferentiableFunctional : IFunctional
    {
        IVector Gradient(IFunction function);
    }

    public interface ILeastSquaresFunctional : IFunctional
    {
        IVector Residual(IFunction function);
        IMatrix Jacobian(IFunction function);
    }
}

namespace Optimizators
{
    public interface IOptimizator
    {
        IVector Minimize(IFunctional objective,
                         IParametricFunction function,
                         IVector initialParameters,
                         IVector? minimumParameters = default,
                         IVector? maximumParameters = default);

    }
}
    