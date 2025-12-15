using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.Models;

public class GdsLibraryBuilder
{
    
}

public record GdsLibrary(GdsLibraryInfo Info, List<GdsStructureInfo> Structures);