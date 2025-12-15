using GdsSharp.Lib.Old.NonTerminals.Enum;

namespace GdsSharp.Lib.Parsing.Models;

public readonly record struct GdsLibraryInfo(
    short Version,
    string Name,
    DateTime ModificationTime,
    DateTime AccessTime,
    IEnumerable<string> ReferencedLibraries,
    IEnumerable<string> Fonts,
    string? AttributeDefinitionFile,
    short Generations,
    double UserUnits,
    double PhysicalUnits,
    GdsFormatType FormatType);