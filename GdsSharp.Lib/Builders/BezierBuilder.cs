using System.Numerics;
using GdsSharp.Lib.NonTerminals;
using GdsSharp.Lib.NonTerminals.Elements;

namespace GdsSharp.Lib.Builders;

/// <summary>
/// Helper class for building Bézier curves.
/// </summary>
public class BezierBuilder
{
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
        20922789888000.0f
    };

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
    /// Builds a path from the added control points.
    /// </summary>
    /// <remarks>Often <see cref="BuildPolygon"/> is a better option than this as it offers a better outline of the curve.</remarks>
    /// <param name="width">Width of the created path.</param>
    /// <param name="numVertices">Number of path elements.</param>
    /// <returns>A GdsPath element.</returns>
    public GdsElement BuildLine(int width, int numVertices = 64)
    {
        var points = GeneratePoints(numVertices)
            .Select(p => new GdsPoint(p.Point))
            .ToList();
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

    /// <summary>
    ///     Builds the Bézier curve as a polygon.
    /// </summary>
    /// <param name="width">Width of the line.</param>
    /// <param name="numVertices">Number of vertices to use for the polygon.</param>
    /// <returns>GdsBoundaryElement.</returns>
    public GdsElement BuildPolygon(int width, int numVertices = 64)
    {
        var halfWidth = width / 2.0f;

        // Generate the points on the Bézier curve
        var splinePoints = GeneratePoints(numVertices / 2).ToList();

        // Generate offset points along the normals above and below each point
        var offsetPoints = new GdsPoint[splinePoints.Count * 2 + 1];
        for (var i = 0; i < splinePoints.Count * 2; i++)
        {
            var splineIndex = i % splinePoints.Count;
            if (i >= splinePoints.Count) splineIndex = splinePoints.Count - 1 - splineIndex;

            var (p, n) = splinePoints[splineIndex];

            float x, y;

            if (i < splinePoints.Count)
            {
                x = p.X + n.X * halfWidth;
                y = p.Y + n.Y * halfWidth;
            }
            else
            {
                x = p.X - n.X * halfWidth;
                y = p.Y - n.Y * halfWidth;
            }

            offsetPoints[i] = new GdsPoint(x, y);
        }

        // Close the polygon
        offsetPoints[^1] = offsetPoints[0];

        var element = new GdsElement
        {
            Element = new GdsBoundaryElement
            {
                Points = offsetPoints.ToList()
            }
        };

        return element;
    }

    private IEnumerable<(Vector2 Point, Vector2 Tangent)> GeneratePoints(int numVertices)
    {
        var step = 1.0f / numVertices;
        var t = 0f;
        while (t <= 1)
        {
            var point = Evaluate(t);
            var tangent = EvaluateTangent(t);
            tangent = Vector2.Normalize(tangent);
            var normal = new Vector2(-tangent.Y, tangent.X);
            yield return (point, normal);
            t += step;
        }
    }

    /// <summary>
    ///     Evaluates the Bézier curve at a given t.
    /// </summary>
    /// <param name="t">[0,1]</param>
    /// <returns>Position on the curve at <see cref="t" />.</returns>
    public Vector2 Evaluate(float t)
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

        return new Vector2(x, y);
    }

    /// <summary>
    ///     Evaluates the tangent of the Bézier curve at a given t.
    /// </summary>
    /// <param name="t">[0,1]</param>
    /// <returns>Tangent vector of the curve at <see cref="t" />.</returns>
    public Vector2 EvaluateTangent(float t)
    {
        var n = _controlPoints.Count - 1;
        var nMinusOne = n - 1;
        var x = 0.0f;
        var y = 0.0f;

        for (var i = 0; i < n; i++)
        {
            var b = Bernstein(nMinusOne, i, t);
            var q = Q(i);
            x += b * q.X;
            y += b * q.Y;
        }

        return new Vector2(x, y);

        GdsPoint Q(int i)
        {
            return n * (_controlPoints[i + 1] - _controlPoints[i]);
        }
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
}