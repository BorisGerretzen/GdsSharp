using System.Numerics;

namespace GdsSharp.Lib.NonTerminals;

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
        X = (int)(x + 0.5f);
        Y = (int)(y + 0.5f);
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
}