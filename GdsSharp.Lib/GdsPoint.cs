using System.Numerics;

namespace GdsSharp.Lib;

public readonly record struct GdsPoint
{
    public readonly int X;
    public readonly int Y;

    public GdsPoint(int x, int y)
    {
        X = x;
        Y = y;
    }

    public GdsPoint(Vector2 vector) : this(vector.X, vector.Y)
    {
    }

    public GdsPoint(float x, float y)
    {
        X = (int)MathF.Round(x);
        Y = (int)MathF.Round(y);
    }

    public static GdsPoint operator +(GdsPoint a, GdsPoint b)
    {
        return new GdsPoint(a.X + b.X, a.Y + b.Y);
    }

    public static GdsPoint operator -(GdsPoint a, GdsPoint b)
    {
        return new GdsPoint(a.X - b.X, a.Y - b.Y);
    }

    public static GdsPoint operator +(GdsPoint a, int b)
    {
        return new GdsPoint(a.X + b, a.Y + b);
    }

    public static GdsPoint operator -(GdsPoint a, int b)
    {
        return new GdsPoint(a.X - b, a.Y - b);
    }

    public static GdsPoint operator *(GdsPoint a, int b)
    {
        return new GdsPoint(a.X * b, a.Y * b);
    }

    public static GdsPoint operator +(int a, GdsPoint b)
    {
        return new GdsPoint(a + b.X, a + b.Y);
    }

    public static GdsPoint operator -(int a, GdsPoint b)
    {
        return new GdsPoint(a - b.X, a - b.Y);
    }

    public static GdsPoint operator *(int a, GdsPoint b)
    {
        return new GdsPoint(a * b.X, a * b.Y);
    }

    public override string ToString()
    {
        return $"({X}, {Y})";
    }
}