﻿using GdsSharp.Lib.Terminals.Abstractions;

namespace GdsSharp.Lib.Terminals.Records;

public class GdsRecordUnits : IGdsSimpleRead, IGdsSimpleWrite
{
    public double UserUnits { get; set; }
    public double PhysicalUnits { get; set; }

    public double UserUnitInMeters => PhysicalUnits / UserUnits;

    public ushort Code => 0x0305;

    public ushort GetLength()
    {
        return 16;
    }
}