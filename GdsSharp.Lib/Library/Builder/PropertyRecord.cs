namespace GdsSharp.Lib.Library.Builder;

public readonly record struct PropertyRecord(
    int ElementId,
    short Attribute,
    string Value
);