using System.Buffers.Binary;

namespace GdsSharp.Lib.Parsing;

public struct GdsDouble
{
    /// <summary>
    ///     Size of a GDSII double in bytes.
    /// </summary>
    public const int Size = 8;

    /// <summary>
    ///     Whether the double is negative.
    /// </summary>
    public bool IsNegative { get; set; }

    /// <summary>
    ///     Exponent of the double.
    ///     Stored in binary as excess 64, this property is normal.
    /// </summary>
    public int Exponent { get; set; }

    /// <summary>
    ///     Mantissa of the double.
    /// </summary>
    public ulong Mantissa { get; set; }

    /// <summary>
    ///     Number of bits used for the exponent.
    /// </summary>
    public int NumExponentBits => 7;

    /// <summary>
    ///     Number of bits used for the mantissa.
    /// </summary>
    public int NumMantissaBits => 56;

    /// <summary>
    ///     Creates a GDSII double from a normal double.
    /// </summary>
    /// <param name="value">Normal double.</param>
    public GdsDouble(double value)
    {
        if (value < 0)
        {
            IsNegative = true;
            value = -value;
        }

        var fexp = Math.Log2(value) / 4;
        if (double.IsNaN(fexp)) fexp = 0;

        var exp = (int)Math.Ceiling(fexp);
        if (Math.Abs(fexp - exp) < 1e-6) exp++;

        var mantissa = (ulong)(value * Math.Pow(16, 14 - exp));
        Exponent = exp;
        Mantissa = mantissa;
    }

    /// <summary>
    ///     Creates a GDSII double from a byte array.
    /// </summary>
    /// <param name="data">
    ///     <Bytes to deserialize./ param>
    ///         <exception cref="ArgumentException">If not exactly <see cref="Size" /> bytes.</exception>
    public GdsDouble(Span<byte> data)
    {
        if (data.Length != Size) throw new ArgumentException($"Data must be {Size} bytes long");

        IsNegative = (data[0] & 0b10000000) != 0;
        if (data[0] == 0) return;
        Exponent = (data[0] & 0b01111111) - 64;
        var uint1 = BinaryPrimitives.ReadUInt32BigEndian(data[1..]);
        Mantissa = ((ulong)uint1 << 24) | (uint)(data[5] << 16) | (ushort)(data[6] << 8) | data[7];
    }

    /// <summary>
    ///     Converts the GdsDouble to regular double.
    /// </summary>
    /// <returns>Regular double.</returns>
    public double AsDouble()
    {
        if (Mantissa == 0 && Exponent == 0) return 0.0f;

        var val = (Mantissa & 0x00FFFFFFFFFFFFFF) / (double)0x0100000000000000;

        var retVal = val * Math.Pow(16, Exponent);

        return IsNegative ? -retVal : retVal;
    }

    /// <summary>
    ///     Converts the GdsDouble to it's binary representation.
    /// </summary>
    /// <returns><see cref="Size" /> bytes.</returns>
    public byte[] AsBytes()
    {
        var bytes = new byte[Size];
        if (Mantissa == 0 && Exponent == 0) return bytes;

        bytes[0] = (byte)(IsNegative ? 0b10000000 : 0);
        bytes[0] |= (byte)(Exponent + 64);
        var uint1 = (uint)(Mantissa >> 24);
        BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan()[1..], uint1);
        bytes[5] = (byte)(Mantissa >> 16);
        bytes[6] = (byte)(Mantissa >> 8);
        bytes[7] = (byte)Mantissa;
        return bytes;
    }
}