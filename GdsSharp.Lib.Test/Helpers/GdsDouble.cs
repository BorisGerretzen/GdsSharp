using System.Buffers.Binary;

namespace GdsSharp.Lib.Test.Helpers;

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

        if (value == 0.0d)
        {
            Exponent = 0;
            Mantissa = 0;
            return;
        }

        var fexp = Math.Log2(value) / 4;
        if (double.IsNaN(fexp)) fexp = 0;
        var exp = (int)Math.Ceiling(fexp);
        if (value >= Math.Pow(16, exp)) exp++;

        var mantissa = (ulong)(value / Math.Pow(16, exp - 14));
        Exponent = exp;
        Mantissa = mantissa;
    }

    /// <summary>
    ///     Creates a GDSII double from its binary representation.
    /// </summary>
    /// <param name="data">Bytes to deserialize</param>
    /// <exception cref="ArgumentException">If not exactly <see cref="Size" /> bytes.</exception>
    public GdsDouble(ReadOnlySpan<byte> data)
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

    public void WriteTo(Span<byte> destination)
    {
        if (destination.Length < Size)
            throw new ArgumentException($"Destination span must be at least {Size} bytes long.");

        if (Mantissa == 0 && Exponent == 0)
        {
            destination.Clear();
            return;
        }

        destination[0] = (byte)(IsNegative ? 0b10000000 : 0);
        destination[0] |= (byte)(Exponent + 64);
        var uint1 = (uint)(Mantissa >> 24);
        BinaryPrimitives.WriteUInt32BigEndian(destination.Slice(1, 4), uint1);
        destination[5] = (byte)(Mantissa >> 16);
        destination[6] = (byte)(Mantissa >> 8);
        destination[7] = (byte)Mantissa;
    }
}