using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Functionals;
using Functions;
using TOOP;
using TOOP.Matrix;
using TOOP.Matrix.Solvers;

namespace Optimizators;

public class GaussNewtonOptimizator : IOptimizator
{
    private const double eps = 1.0E-7;
    private const int maxiter = 1000000;
    public IVector Minimize(ILeastSquaresFunctional objective, IParametricFunction function, IVector initialParameters, IVector? minimumParameters = null, IVector? maximumParameters = null, double precision = eps, int maxIterationCount = maxiter)
    {
        Vector parameters = new([.. initialParameters]);
        ISLAESolver? solver = null;
        for(int k = 0; k < maxIterationCount; ++k)
        {
            var func = function.Bind(parameters);
            var Jacobian = objective.Jacobian(func);
            if(Jacobian is not IMatrixOperators)
                Jacobian = new DenseMatrix(Jacobian);
            var JacobianOp = (IMatrixOperators)Jacobian;
            var TransposedJacobian = JacobianOp.Transpose();
            IMatrix A = MatrixOperators.Multiply(TransposedJacobian, Jacobian);
            Vector b = -new Vector([.. MatrixOperators.Multiply(TransposedJacobian, objective.Residual(func))]);
            solver ??= SLAESolverSelector.Select(A);
            if(solver.Solve(A, b, out IVector delta,null,null,null))
            if(delta.Norm() < precision) return parameters;
            parameters += new Vector([..delta]);
            if(minimumParameters is not null)
                parameters = new([..parameters.Select((el,i) => Math.Max(el, minimumParameters[i]))]);
            if(maximumParameters is not null)
                parameters = new([..parameters.Select((el,i) => Math.Min(el, maximumParameters[i]))]);
        }
        return parameters;
    }
    public IVector Minimize(IFunctional objective, IParametricFunction function, IVector initialParameters, IVector? minimumParameters = null, IVector? maximumParameters = null)
    {
        if(objective is ILeastSquaresFunctional objectiveLS) 
            return Minimize(objectiveLS, function, initialParameters, minimumParameters, maximumParameters);
        throw new Exception("objective functional doesn't implement ILeastSquaresFunctional interface");
    }
}