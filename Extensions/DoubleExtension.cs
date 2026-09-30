using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Extensions;

public static class DoubleExtension
{
    public static bool IsEqualsPrecision(this double self, double other, double precision = 1e-15)
    {
        return double.Abs(self - other) <= precision;
    }
}
