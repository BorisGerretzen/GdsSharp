namespace GdsSharp.Lib.Reading.TokenStream;

public readonly record struct GdsTokenHeader(ushort Length, ushort Code, long Offset, long PayloadOffset)
{
    public int PayloadLength => Length - 4;
};