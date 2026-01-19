using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Library.Editing;

internal readonly record struct BoundaryOverride(short? Layer, short? DataType, VertexRef? Vertices);

internal readonly record struct PathOverride(short? Layer, short? DataType, GdsPathType? PathType, int? Width, VertexRef? Vertices);

internal readonly record struct BoxOverride(short? Layer, short? BoxType, VertexRef? Vertices);

internal readonly record struct NodeOverride(short? Layer, short? NodeType, VertexRef? Vertices);

internal readonly record struct TextOverride(
    short? Layer,
    short? TextType,
    PresentationInfo? Presentation,
    GdsPathType? PathType,
    int? Width,
    GdsStransInfo? Strans,
    GdsPoint? Origin,
    string? Text);

internal readonly record struct SRefOverride(string? TargetName, GdsStransInfo? Strans, GdsPoint? Origin);

internal readonly record struct ARefOverride(
    string? TargetName,
    GdsStransInfo? Strans,
    int? Rows,
    int? Columns,
    GdsPoint? RowVector,
    GdsPoint? ColumnVector,
    GdsPoint? Origin);