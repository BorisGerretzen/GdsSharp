namespace GdsSharp.Lib.InternalDb;

public record struct GdsStructureReference(CellId Parent, string TargetName, GdsTransform Transform);