using GdsSharp.Benchmarks.Obsolete.Terminals.Abstractions;

namespace GdsSharp.Benchmarks.Obsolete.Terminals.Records;

public class GdsRecordWidth : GenericGdsRecord<int>
{
    private int _value;

    public override int Value
    {
        get => _value;
        set => _value = value < 0 ? -value : value;
    }

    public override ushort Code => 0x0F03;

    public bool IsAbsolute => Value < 0;
}