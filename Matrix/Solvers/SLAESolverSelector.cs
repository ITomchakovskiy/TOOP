using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Matrix.Solvers;
static class SLAESolverSelector
{
    public static ISLAESolver Select(IMatrix matrix)
    {
        if(matrix.Symmetry != MatrixSymmetry.Symmetric) return new LUDenseSLAESolver();
        if(matrix.IsPositiveDefinite is not null && !(bool)matrix.IsPositiveDefinite)
            return new LDLTDenseSLAESolver();
        var Cholesky = new CholeskyDenseSLAESolver();
        Cholesky.Decompose(matrix);
        if(Cholesky.IsDecomposed) { matrix.IsPositiveDefinite = true; return Cholesky;}
        return new LDLTDenseSLAESolver();
    }
}