namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordUnits : IGdsSimpleRead
{
    public double UserUnits { get; set; }
    public double PhysicalUnits { get; set; }

    public double UserUnitInMeters => PhysicalUnits / UserUnits;
}