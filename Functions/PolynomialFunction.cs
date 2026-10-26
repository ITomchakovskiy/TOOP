using Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Functions;

public class PolynomialFunction : IParametricFunction
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

            double pow = 1d, res = 0d;
            for(int i = 0; i < parameters.Length; ++i)
            {
                res += parameters[i] * pow;
                pow *= x;
            }

            return res;
        }
    }
}
