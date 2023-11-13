namespace GdsSharp.Lib.Parsing.Models;

public class GdsRecordWidth : GenericGdsRecord<int>
{
    private int _value;

    public override int Value
    {
        get => _value;
        set => _value = value < 0 ? -value : value;
    }

    public bool IsAbsolute => Value < 0;
}