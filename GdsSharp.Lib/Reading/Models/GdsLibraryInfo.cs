using GdsSharp.Lib.Reading.Enum;

namespace GdsSharp.Lib.Reading.Models;

public readonly record struct GdsLibraryInfo(
    short Version,
    string Name,
    DateTime ModificationTime,
    DateTime AccessTime,
    IReadOnlyCollection<string> ReferencedLibraries,
    IReadOnlyCollection<string> Fonts,
    string? AttributeDefinitionFile,
    short? Generations,
    double UserUnits,
    double PhysicalUnits,
    GdsFormatType? FormatType)
{
    public static readonly GdsLibraryInfo Default = new(
        700,
        "DefaultLibrary",
        DateTime.Now,
        DateTime.Now,
        [],
        [],
        null,
        null,
        1,
        1e-8,
        GdsFormatType.GdsArchive);
}