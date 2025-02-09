using RBush;

namespace GdsSharp.Lib.Acceleration;

public class GdsElementRef : ISpatialData
{
    private readonly Envelope _envelope;

    public ref readonly Envelope Envelope => ref _envelope;
    
    public string StructureName { get; }
    public int Index { get; }

    public GdsElementRef(Envelope envelope, string structureName, int index)
    {
        _envelope = envelope;
        StructureName = structureName;
        Index = index;
    }
}