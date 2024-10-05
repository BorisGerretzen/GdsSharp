using System.Buffers;
using System.Numerics;
using GdsSharp.Lib.NonTerminals;

namespace GdsSharp.Benchmarks.PathBuilder.Implementations;

public class PathBuilderArray : PathBuilderBase
{
    public PathBuilderArray(float initialWidth, Vector2? initialPosition = null, Vector2? initialHeading = null) : base(initialWidth, initialPosition, initialHeading)
    {
    }
    
    protected override IEnumerable<GdsPoint> GetPolygonPoints()
    {
        var segments = GetPathPoints();
        int segmentCount = segments.Count;

        if (segmentCount == 0)
            return Array.Empty<GdsPoint>();

        // Step 1: Calculate total number of points
        int totalMeshUpPoints = 0;
        foreach (var (_, points) in segments)
        {
            totalMeshUpPoints += points.Count;
        }

        int totalMeshDownPoints = totalMeshUpPoints;
        int totalPoints = totalMeshUpPoints + totalMeshDownPoints;

        // Step 2: Rent a single array to hold all points
        GdsPoint[] combinedPoints = ArrayPool<GdsPoint>.Shared.Rent(totalPoints);

        try
        {
            int meshUpIndex = 0;
            int meshDownIndex = totalPoints - 1;

            foreach (var (_, points) in segments)
            {
                foreach (var (point, normal, width) in points)
                {
                    Vector2 halfWidthNormal = normal * (width / 2f);

                    // Assign meshUp points from the start
                    combinedPoints[meshUpIndex++] = new GdsPoint(point + halfWidthNormal);

                    // Assign meshDown points from the end
                    combinedPoints[meshDownIndex--] = new GdsPoint(point - halfWidthNormal);
                }
            }

            // Step 3: Create a new array with the exact number of points
            GdsPoint[] result = new GdsPoint[totalPoints];
            Array.Copy(combinedPoints, result, totalPoints);

            return result;
        }
        finally
        {
            // Return the rented array to the pool
            ArrayPool<GdsPoint>.Shared.Return(combinedPoints, clearArray: true);
        }
    }
}