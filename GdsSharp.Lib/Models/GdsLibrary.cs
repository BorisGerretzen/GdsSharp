using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.Models;

public record GdsLibrary(GdsLibraryInfo Info, List<GdsStructureInfo> Structures);