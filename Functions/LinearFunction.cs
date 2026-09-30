using Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Functions;

class LinearFunction : IParametricFunction
{
    public IFunction Bind(IVector parameters)
    {
        return new LinearFunctionImpl(parameters);
    }

    private class LinearFunctionImpl(IVector parameters) : IDifferentiableFunction
    {
        public double Value(IVector point)
        {
            if (parameters.Length != point.Length + 1)
            {
                throw new ArgumentException(
                    $"Incorrect number of parameters. \nParameters length: {parameters.Length}, point length: {point.Length}");
            }

            var result = parameters[0];
            for (var i = 0; i < point.Length; i++)
            {
                result += parameters[i + 1] * point[i];
            }

            return result;
        }

        public IVector Gradient(IVector point)
        {
            return new Vector(point.Prepend(1).ToArray());
        }
    }
}
