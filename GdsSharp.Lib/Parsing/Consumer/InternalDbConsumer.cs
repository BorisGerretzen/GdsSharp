using GdsSharp.Lib.Models;
using GdsSharp.Lib.Old.NonTerminals.Enum;
using GdsSharp.Lib.Parsing.Models;

namespace GdsSharp.Lib.Parsing.Consumer;

public class InternalDbConsumer : IParserConsumer
{
    public void OnBeginLibrary(in GdsLibraryInfo lib)
    {
        throw new NotImplementedException();
    }

    public void OnEndLibrary()
    {
        throw new NotImplementedException();
    }

    public void OnBeginStructure(in GdsStructureInfo str)
    {
        throw new NotImplementedException();
    }

    public void OnEndStructure()
    {
        throw new NotImplementedException();
    }

    public void OnBeginElement(GdsElementKind kind, in GdsElementCommon? common)
    {
        throw new NotImplementedException();
    }

    public void OnProperty(short attr, string value)
    {
        throw new NotImplementedException();
    }

    public void OnEndElement()
    {
        throw new NotImplementedException();
    }

    public void OnBoundary(short layer, short dataType, ReadOnlySpan<GdsPoint> points)
    {
        throw new NotImplementedException();
    }

    public void OnPath(short layer, short dataType, GdsPathType? pathType, int? width, ReadOnlySpan<GdsPoint> points)
    {
        throw new NotImplementedException();
    }

    public void OnBox(short layer, short boxType, ReadOnlySpan<GdsPoint> points)
    {
        throw new NotImplementedException();
    }

    public void OnNode(short layer, short nodeType, ReadOnlySpan<GdsPoint> points)
    {
        throw new NotImplementedException();
    }

    public void OnSref(string structureName, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points)
    {
        throw new NotImplementedException();
    }

    public void OnAref(string structureName, GdsStransInfo? strans, short cols, short rows, ReadOnlySpan<GdsPoint> points)
    {
        throw new NotImplementedException();
    }

    public void OnText(short layer, short textType, PresentationInfo? presentation, GdsPathType? pathType, int? width, GdsStransInfo? strans, ReadOnlySpan<GdsPoint> points, string text)
    {
        throw new NotImplementedException();
    }
}