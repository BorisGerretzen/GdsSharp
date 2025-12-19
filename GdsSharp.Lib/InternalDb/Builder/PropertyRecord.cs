namespace GdsSharp.Lib.InternalDb.Builder;

public readonly record struct PropertyRecord(
    int ElementId,
    short Attribute,
    string Value
);