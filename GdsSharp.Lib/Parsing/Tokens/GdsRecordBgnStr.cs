﻿namespace GdsSharp.Lib.Parsing.Tokens;

public class GdsRecordBgnStr : IGdsSimpleRead
{
    public short CreationTimeYear { get; set; }
    public short CreationTimeMonth { get; set; }
    public short CreationTimeDay { get; set; }
    public short CreationTimeHour { get; set; }
    public short CreationTimeMinute { get; set; }
    public short CreationTimeSecond { get; set; }

    public short LastModificationTimeYear { get; set; }
    public short LastModificationTimeMonth { get; set; }
    public short LastModificationTimeDay { get; set; }
    public short LastModificationTimeHour { get; set; }
    public short LastModificationTimeMinute { get; set; }
    public short LastModificationTimeSecond { get; set; }
}