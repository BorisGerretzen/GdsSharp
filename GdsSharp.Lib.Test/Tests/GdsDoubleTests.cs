using System.Buffers.Binary;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test;

public class GdsDoubleTests
{
    // @formatter:off
    public static IEnumerable<(byte[] bytes, double realValue)> TestCases
    {
        get
        {
            yield return ([0b11000001, 0b00010000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000], -1);
            yield return ([0b01000001, 0b00010000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000], 1);
            yield return ([0b01000001, 0b00011011, 0b00110011, 0b00110011, 0b00110011, 0b00110011, 0b00110011, 0b00110011], 1.7d);
            yield return ([0b01000011, 0b00111110, 0b10000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000], 1000);
            yield return ([0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000], 0);
            yield return ([0x3E, 0x41, 0x89, 0x37, 0x4B, 0xC6, 0xA7, 0XF0], 0.001d);
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
        var bytesOut = new byte[GdsDouble.Size];
        myDouble.WriteTo(bytesOut);
        Assert.That(bytesOut, Is.EqualTo(pair.bytes));
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
        var bytesOut = new byte[GdsDouble.Size];
        myDouble.WriteTo(bytesOut);
        Assert.That(bytesOut, Is.EqualTo(pair.bytes));
    }

    [TestCaseSource(nameof(TestCases))]
    public void TestKnownValues((byte[] bytes, double realValue) pair)
    {
        // Check Read
        var value = GdsDoubleConverter.FromGdsBytes(pair.bytes);
        Assert.That(value, Is.EqualTo(pair.realValue).Within(1e-9));

        // Check Write
        var buffer = new byte[8];
        GdsDoubleConverter.ToGdsBytes(pair.realValue, buffer);
        Assert.That(buffer, Is.EqualTo(pair.bytes), "Byte output did not match expected hex.");
    }

    [Test]
    public void Fuzz_CompareReference()
    {
        const int iterations = 100_000;
        var bufferNew = new byte[8];
        var bufferOld = new byte[8];

        for (var i = 0; i < iterations; i++)
        {
            // 1. Generate a Safe GDSII Double
            var val = GetRandomDouble();

            // 2. Convert using OLD (Reference)
            var legacy = new GdsDouble(val);
            legacy.WriteTo(bufferOld);

            // 3. Convert using NEW (Optimized)
            // No checks needed: Input is guaranteed valid by GetRandomDouble
            GdsDoubleConverter.ToGdsBytes(val, bufferNew);

            // 4. Assert Byte-for-Byte Equality
            Assert.That(bufferNew, Is.EqualTo(bufferOld),
                $"Mismatch found for double value: {val}");

            // 5. Round-Trip Check (Sanity Check)
            var readBack = GdsDoubleConverter.FromGdsBytes(bufferNew);
            Assert.That(readBack, Is.EqualTo(val).Within(1e-10),
                $"Round-trip failed for value: {val}");
        }
    }

    private double GetRandomDouble()
    {
        if (TestContext.CurrentContext.Random.NextDouble() < 0.01)
            return 0.0;

        // GDSII Max: ~7.2e75
        // GDSII Min: ~5.4e-79
        // Generate exponent between -75 and +75
        var exponent = TestContext.CurrentContext.Random.NextDouble() * 148.0 - 74.0;
        var mantissa = TestContext.CurrentContext.Random.NextDouble() * 9.0 + 1.0;

        var value = mantissa * Math.Pow(10, exponent);

        return TestContext.CurrentContext.Random.NextBool() ? value : -value;
    }
}