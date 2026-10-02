using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP.Extensions;

public static class VectorExtension
{
    internal static int Find(this IVector collection, double xValue)
    {
        var knotsCount = collection.Length;
        if (xValue.IsEqualsPrecision(collection[knotsCount - 1]))
        {
            return knotsCount - 2;
        }

        var left = 0;
        var right = knotsCount - 2;

        while (left <= right)
        {
            var mid = (left + right) >> 1;
            if (xValue < collection[mid])
            {
                right = mid - 1;
            }
            else if (xValue >= collection[mid + 1])
            {
                left = mid + 1;
            }
            else
            {
                return mid;
            }
        }

        return Math.Max(0, Math.Min(knotsCount - 2, left));
    }
}