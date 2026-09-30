using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace TOOP;

interface IVector : IEnumerable<double>
{
    double this[int index] {get; set;}
    int Length {get;}
    double Norm();
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
    public double Norm() => Math.Sqrt(values.Sum(d => d * d));

    public static Vector operator +(Vector A, Vector B)
    {
        if(A.Length != B.Length) throw new ArgumentException("operation on non-matching vectors");
        double[] new_values = [.. A.Select((d,i) => d + B[i])];
        return new(new_values);
    }
    public void operator +=(Vector other) 
    {
        if(Length != other.Length) throw new ArgumentException("operation on non-matching vectors");
        for(int i = 0; i < Length; ++i)
            values[i] += other[i];
    }
    public void operator -=(Vector other)
    {
       if(Length != other.Length) throw new ArgumentException("operation on non-matching vectors");
        for(int i = 0; i < Length; ++i)
            values[i] -= other[i];
    }

    public static Vector operator -(Vector A)
    {
        double[] new_values = [.. A.Select(d => -d)];
        return new(new_values);
    }

    public static Vector operator -(Vector A, Vector B) => A + (-B);

    public static Vector operator *(Vector A, double val) => new([..A.Select(d => d*val)]);
    public static Vector operator *(double val, Vector A) => A * val;

    public static Vector operator /(Vector A, double val) => A * (1d / val);
    
}