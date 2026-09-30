using Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Functions;

class PolynomialFunction : IParametricFunction
{
    public IFunction Bind(IVector parameters)
    {
        return new PolynomialFunctionImpl(parameters);
    }

    private class PolynomialFunctionImpl(IVector parameters) : IFunction
    {
        public double Value(IVector point)
        {
            var x = point[0];

            return parameters
                .Select((p, index) => p * double.Pow(x, index))
                .Sum();
        }
    }
}
