using System.Numerics;

namespace GdsSharp.Lib.Test;

public class VectorExtensionsTests
{
    [Test]
    public void Rotate_Rotate90Degrees_CorrectResult()
    {
        var vec = new Vector2(1, 0);
        const float radians = MathF.PI / 2;

        var result = vec.Rotate(radians);

        Assert.Multiple(() =>
        {
            Assert.That(result.X, Is.EqualTo(0).Within(1e-6)); // allowing a small tolerance for floating point errors
            Assert.That(result.Y, Is.EqualTo(1).Within(1e-6));
        });
    }

    [Test]
    public void Rotate_Rotate180Degrees_CorrectResult()
    {
        var vec = new Vector2(1, 0);
        const float radians = MathF.PI;

        var result = vec.Rotate(radians);

        Assert.Multiple(() =>
        {
            Assert.That(result.X, Is.EqualTo(-1).Within(1e-6));
            Assert.That(result.Y, Is.EqualTo(0).Within(1e-6));
        });
    }

    [Test]
    public void Rotate_Rotate360Degrees_CorrectResult()
    {
        var vec = new Vector2(1, 0);
        const float radians = 2 * MathF.PI;

        var result = vec.Rotate(radians);

        Assert.Multiple(() =>
        {
            Assert.That(result.X, Is.EqualTo(1).Within(1e-6));
            Assert.That(result.Y, Is.EqualTo(0).Within(1e-6));
        });
    }

    [Test]
    public void Angle_VectorAt45Degrees_CorrectResult()
    {
        var vec = new Vector2(1, 1);

        var result = vec.Angle();

        Assert.That(result, Is.EqualTo(MathF.PI / 4).Within(1e-6));
    }

    [Test]
    public void Angle_VectorAt90Degrees_CorrectResult()
    {
        var vec = new Vector2(0, 1);

        var result = vec.Angle();

        Assert.That(result, Is.EqualTo(MathF.PI / 2).Within(1e-6));
    }

    [Test]
    public void Angle_VectorAtNegative45Degrees_CorrectResult()
    {
        var vec = new Vector2(1, -1);

        var result = vec.Angle();

        Assert.That(result, Is.EqualTo(-MathF.PI / 4).Within(1e-6));
    }
}