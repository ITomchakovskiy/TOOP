using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TOOP;

interface IVector : IEnumerable<double>
{
    double this[int index] {get; set;}
    int Length {get;}
}
interface IMatrix {}
public class Vector : IVector
{
    private double[] values;
    public int Length => values.Length;

    public double this[int index] { get => values[index]; set => values[index] = value; }
    public Vector(int length) => values = new double[length];
    public Vector(double[] _values) => values = _values;
    public IEnumerator<double> GetEnumerator() => values.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
}