using System.Numerics;

namespace GdsSharp.Lib;

public readonly struct GdsPoint
{
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

    public GdsPoint(double x, double y)
    {
        X = (int)Math.Round(x);
        Y = (int)Math.Round(y);
    }

    public readonly int X;
    public readonly int Y;

    public static GdsPoint operator +(GdsPoint a, GdsPoint b) => new(a.X + b.X, a.Y + b.Y);
    public static GdsPoint operator -(GdsPoint a, GdsPoint b) => new(a.X - b.X, a.Y - b.Y);
    public static GdsPoint operator *(GdsPoint a, GdsPoint b) => new(a.X * b.X, a.Y * b.Y);
    public static GdsPoint operator /(GdsPoint a, GdsPoint b) => new(a.X / b.X, a.Y / b.Y);
    public static GdsPoint operator %(GdsPoint a, GdsPoint b) => new(a.X % b.X, a.Y % b.Y);
    public static GdsPoint operator +(GdsPoint a, int b) => new(a.X + b, a.Y + b);
    public static GdsPoint operator -(GdsPoint a, int b) => new(a.X - b, a.Y - b);
    public static GdsPoint operator *(GdsPoint a, int b) => new(a.X * b, a.Y * b);
    public static GdsPoint operator /(GdsPoint a, int b) => new(a.X / b, a.Y / b);
    public static GdsPoint operator %(GdsPoint a, int b) => new(a.X % b, a.Y % b);
    public static GdsPoint operator +(int a, GdsPoint b) => new(a + b.X, a + b.Y);
    public static GdsPoint operator -(int a, GdsPoint b) => new(a - b.X, a - b.Y);
    public static GdsPoint operator *(int a, GdsPoint b) => new(a * b.X, a * b.Y);
    public static GdsPoint operator /(int a, GdsPoint b) => new(a / b.X, a / b.Y);
    public static GdsPoint operator %(int a, GdsPoint b) => new(a % b.X, a % b.Y);

    public override string ToString()
    {
        return $"({X}, {Y})";
    }

    public GdsPoint Rotate(float sin, float cos)
    {
        return new GdsPoint(
            cos * X - sin * Y,
            sin * X + cos * Y
        );
    }
}