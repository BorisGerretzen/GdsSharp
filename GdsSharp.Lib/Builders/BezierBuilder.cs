using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.Builders;

/// <summary>
/// Helper class for building Bézier curves.
/// </summary>
public class BezierBuilder
{
    private readonly List<GdsPoint> _controlPoints = new();

    /// <summary>
    /// Adds a control point to the Bézier curve.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    /// <exception cref="InvalidOperationException">If more than 16 points are added.</exception>
    public BezierBuilder AddPoint(int x, int y)
    {
        return AddPoint(new GdsPoint(x, y));
    }

    /// <summary>
    /// Adds a control point to the Bézier curve.
    /// </summary>
    /// <param name="point">Point to add.</param>
    /// <exception cref="InvalidOperationException">If more than 16 points are added.</exception>
    public BezierBuilder AddPoint(GdsPoint point)
    {
        if (_controlPoints.Count == 16)
            throw new InvalidOperationException("A bezier curve can only have 16 control points");
        _controlPoints.Add(point);
        return this;
    }

    /// <summary>
    /// Builds a polygon from the added control points.
    /// </summary>
    /// <param name="numVertices">Number of path elements.</param>
    /// <returns>A GdsBoundary element.</returns>
    public GdsElement BuildPolygon(int numVertices = 64)
    {
        var points = GeneratePoints(numVertices).ToList();
        var element = new GdsElement
        {
            Element = new GdsBoundaryElement
            {
                Points = points,
            }
        };

        return element;
    }

    /// <summary>
    /// Builds a path from the added control points.
    /// </summary>
    /// <param name="width">Width of the created path.</param>
    /// <param name="numVertices">Number of path elements.</param>
    /// <returns>A GdsPath element.</returns>
    public GdsElement BuildLine(int width, int numVertices = 64)
    {
        var points = GeneratePoints(numVertices).ToList();
        var element = new GdsElement
        {
            Element = new GdsPathElement
            {
                Points = points,
                Width = width
            }
        };

        return element;
    }

    private IEnumerable<GdsPoint> GeneratePoints(int numVertices)
    {
        var step = 1.0f / numVertices;
        for (var i = 0; i < numVertices; i++)
        {
            var t = i * step;
            var point = Evaluate(t);
            yield return point;
        }
    }

    private GdsPoint Evaluate(float t)
    {
        var n = _controlPoints.Count - 1;
        var x = 0.0f;
        var y = 0.0f;
        for (var i = 0; i <= n; i++)
        {
            var b = Bernstein(n, i, t);
            x += b * _controlPoints[i].X;
            y += b * _controlPoints[i].Y;
        }

        return new GdsPoint((int)x, (int)y);
    }

    private static float Bernstein(int n, int i, float t)
    {
        var ti = MathF.Pow(t, i);
        var tNMinusI = MathF.Pow((1 - t), (n - i));
        var basis = Binomial(n, i) * ti * tNMinusI;
        return basis;
    }

    private static float Binomial(int n, int i)
    {
        var a1 = Factorial[n];
        var a2 = Factorial[i];
        var a3 = Factorial[n - i];
        var ni = a1 / (a2 * a3);
        return ni;
    }
    
    
    private static readonly float[] Factorial =
    {
        1.0f,
        1.0f,
        2.0f,
        6.0f,
        24.0f,
        120.0f,
        720.0f,
        5040.0f,
        40320.0f,
        362880.0f,
        3628800.0f,
        39916800.0f,
        479001600.0f,
        6227020800.0f,
        87178291200.0f,
        1307674368000.0f,
        20922789888000.0f,
    };
}