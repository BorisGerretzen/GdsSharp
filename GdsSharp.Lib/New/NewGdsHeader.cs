using System.Runtime.InteropServices;
using GdsSharp.SourceGenerators.New;

namespace GdsSharp.Lib.New;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
[BigEndian]
public readonly partial struct NewGdsHeader
{
    private readonly ushort _length;
    private readonly ushort _recordType;

    public NewGdsHeader(ushort length, ushort recordType)
    {
        _length = length;
        _recordType = recordType;
    }
}