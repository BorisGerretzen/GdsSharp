using System.Buffers.Binary;

namespace GdsSharp.Lib;

public static class GdsDoubleConverter
{
    private const int GdsDoubleSize = 8;
    private const ulong SignMask = 0x8000_0000_0000_0000;

    private const int GdsBias = 64;
    private const int IeeeBias = 1023;

    private const ulong IeeeMantissaMask = 0x000F_FFFF_FFFF_FFFF;
    private const ulong IeeeImpliedBit = 0x0010_0000_0000_0000;
    private const ulong GdsMantissaMask = 0x00FF_FFFF_FFFF_FFFF;

    public static double FromGdsBytes(ReadOnlySpan<byte> data)
    {
        if (data.Length < GdsDoubleSize)
            throw new ArgumentOutOfRangeException(nameof(data), "Span must be at least 8 bytes.");

        var raw = BinaryPrimitives.ReadUInt64BigEndian(data);

        if (raw == 0) return 0.0;

        var isNegative = (raw & SignMask) != 0;
        var gdsExponent = (int)((raw >> 56) & 0x7F) - GdsBias;
        var mantissa = raw & GdsMantissaMask;

        var result = Math.ScaleB(mantissa, 4 * gdsExponent - 56);
        return isNegative ? -result : result;
    }

    public static void ToGdsBytes(double value, Span<byte> destination)
    {
        if (destination.Length < GdsDoubleSize)
            throw new ArgumentOutOfRangeException(nameof(destination), "Span must be at least 8 bytes.");

        if (value == 0.0)
        {
            destination[..GdsDoubleSize].Clear();
            return;
        }

        var bits = BitConverter.DoubleToUInt64Bits(value);
        var signBit = bits & SignMask;
        var ieeeExp = (int)((bits >> 52) & 0x7FF);

        if (ieeeExp == 0)
        {
            destination[..GdsDoubleSize].Clear();
            return;
        }

        var realExp2 = ieeeExp - IeeeBias;

        // Convert Base 2 Exponent to Base 16 Exponent
        var gdsExp = (realExp2 >> 2) + 1;

        // Gds Exponent is 7 bits offset by 64. Valid range: -64 to +63.
        if (gdsExp < -64)
        {
            // underflow
            destination[..GdsDoubleSize].Clear();
            return;
        }

        if (gdsExp > 63)
            // overflow
            throw new OverflowException($"Value {value} is too large for GDSII format.");

        // Shift the IEEE mantissa left by (Exp2 % 4)
        var remainder = realExp2 & 3;

        // Build GDS Mantissa
        var ieeeMantissa = (bits & IeeeMantissaMask) | IeeeImpliedBit;
        var gdsMantissa = ieeeMantissa << remainder;

        var finalExp = (ulong)(gdsExp + GdsBias);

        var result = signBit | (finalExp << 56) | (gdsMantissa & GdsMantissaMask);
        BinaryPrimitives.WriteUInt64BigEndian(destination, result);
    }
}