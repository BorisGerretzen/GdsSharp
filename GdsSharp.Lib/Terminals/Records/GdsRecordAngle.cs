using GdsSharp.Lib.Terminals.Abstractions;
using GdsSharp.SourceGenerators;

namespace GdsSharp.Lib.Terminals.Records;

[GdsAutoPopulate]
public partial class GdsRecordAngle : IGdsSimpleWrite
{
    public ushort Code => 0x1C05;
    public ushort GetLength() => 8;
    public double Value { get; set; }
}