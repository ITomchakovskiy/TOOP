using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Matrix.Solvers;
interface ISLAESolver
{
    bool Solve(IMatrix? matrix, IVector rightSide, out IVector result, double? precision , int? maxIterationCount, IVector? initialSolution);
}