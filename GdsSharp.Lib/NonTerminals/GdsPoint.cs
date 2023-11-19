namespace GdsSharp.Lib.NonTerminals;

public struct GdsPoint
{
    public GdsPoint(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }
    
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