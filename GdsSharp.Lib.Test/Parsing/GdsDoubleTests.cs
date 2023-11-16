using System.Buffers.Binary;
using GdsSharp.Lib.Parsing;

namespace GdsSharp.Lib.Test.Parsing;

public class GdsDoubleTests
{
    // @formatter:off
    public static IEnumerable<(byte[] bytes, double realValue)> TestCases
    {
        get
        {
            yield return (new byte[] {0b11000001, 0b00010000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, -1);
            yield return (new byte[] {0b01000001, 0b00010000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, 1);
            yield return (new byte[] {0b01000001, 0b00011011, 0b00110011, 0b00110011, 0b00110011, 0b00110011, 0b00110011, 0b00110011}, 1.7d);
            yield return (new byte[] {0b01000011, 0b00111110, 0b10000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, 1000);
            yield return (new byte[] {0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000}, 0);
            yield return (new byte[] {0x3E, 0x41, 0x89, 0x37, 0x4B, 0xC6, 0xA7, 0XF0}, 0.001d);
        }
    }
    // @formatter:on

    /// <summary>
    ///     Test that the double is the same when converting from bytes to double
    /// </summary>
    [TestCaseSource(nameof(TestCases))]
    public void TestMemoryLayout((byte[] bytes, double realValue) pair)
    {
        var myDouble = new GdsDouble(pair.bytes);
        Assert.That(myDouble.IsNegative, Is.EqualTo(pair.bytes[0] >> 7 == 1));

        // remove sign bit
        pair.bytes[0] &= 0b01111111;
        Assert.That(myDouble.Exponent, Is.EqualTo(pair.bytes[0] == 0 ? 0 : pair.bytes[0] - 64));

        // remove exponent
        pair.bytes[0] = 0;
        Assert.That(myDouble.Mantissa, Is.EqualTo(BinaryPrimitives.ReadUInt64BigEndian(pair.bytes)));
    }

    /// <summary>
    ///     Test that the double is the same when converting from bytes to double
    /// </summary>
    [TestCaseSource(nameof(TestCases))]
    public void TestFromBytesAsDouble((byte[] bytes, double realValue) pair)
    {
        var myDouble = new GdsDouble(pair.bytes);
        Assert.That(myDouble.AsDouble(), Is.EqualTo(pair.realValue).Within(1e-9));
    }

    /// <summary>
    ///     Test that the bytes are the same when converting from double to bytes
    /// </summary>
    [TestCaseSource(nameof(TestCases))]
    public void TestFromDoubleAsBytes((byte[] bytes, double realValue) pair)
    {
        var myDouble = new GdsDouble(pair.realValue);
        Assert.That(myDouble.AsBytes(), Is.EqualTo(pair.bytes));
        Assert.That(myDouble.IsNegative, Is.EqualTo(pair.realValue < 0));

        // remove sign bit
        pair.bytes[0] &= 0b01111111;
        Assert.That(myDouble.Exponent, Is.EqualTo(pair.bytes[0] == 0 ? 0 : pair.bytes[0] - 64));

        // remove exponent
        pair.bytes[0] = 0;
        Assert.That(myDouble.Mantissa, Is.EqualTo(BinaryPrimitives.ReadUInt64BigEndian(pair.bytes)));
    }

    /// <summary>
    ///     Test that the bytes are the same when converting from bytes to double and back to bytes
    /// </summary>
    [TestCaseSource(nameof(TestCases))]
    public void TestFromBytesToBytes((byte[] bytes, double realValue) pair)
    {
        var myDouble = new GdsDouble(pair.bytes);
        var bytesOut = myDouble.AsBytes();
        Assert.That(bytesOut, Is.EqualTo(pair.bytes));
    }
}