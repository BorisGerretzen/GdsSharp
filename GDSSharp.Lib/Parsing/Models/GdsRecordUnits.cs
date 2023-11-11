namespace GdsSharp.Lib.Models.Parsing;

public class GdsRecordUnits : IGdsSimpleRead
{
    public double DatabaseUnitInUserUnits { get; set; }
    public double DatabaseUnitInMeters { get; set; }
    
    public double UserUnitInMeters => DatabaseUnitInMeters / DatabaseUnitInUserUnits;
}