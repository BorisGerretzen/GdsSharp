using GdsSharp.Lib.Reading.Enum;
using GdsSharp.Lib.Reading.Models;

namespace GdsSharp.Lib.Reading;

public interface IParserConsumer
{
    void OnBeginLibrary(in GdsLibraryInfo lib);
    void OnEndLibrary();

    void OnBeginStructure(in GdsStructureInfo str);
    void OnEndStructure();

    void OnBeginElement(GdsElementKind kind, in GdsElementCommon? common);
    void OnProperty(short attr, string value); // called between BeginElement and EndElement
    void OnEndElement();

    void OnBoundary(short layer, short dataType, ReadOnlySpan<GdsPoint> points);
    void OnPath(short layer, short dataType, GdsPathType? pathType, int? width, ReadOnlySpan<GdsPoint> points);
    void OnBox(short layer, short boxType, ReadOnlySpan<GdsPoint> points);
    void OnNode(short layer, short nodeType, ReadOnlySpan<GdsPoint> points);

    void OnSref(string structureName, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points);
    void OnAref(string structureName, GdsStransInfo? strans, short cols, short rows, ReadOnlySpan<GdsPoint> points);

    void OnText(
        short layer,
        short textType,
        PresentationInfo? presentation,
        GdsPathType? pathType,
        int? width,
        GdsStransInfo? strans,
        ReadOnlySpan<GdsPoint> points,
        string text
    );
}