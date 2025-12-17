namespace GdsSharp.Lib.InternalDb.Builder;

public record struct GdsStructureReference(CellId Parent, string TargetName, GdsTransform Transform);