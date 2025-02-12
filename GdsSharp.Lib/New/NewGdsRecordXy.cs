using GdsSharp.SourceGenerators.New;

namespace GdsSharp.Lib.New;

public readonly struct NewGdsRecordXy
{
    public readonly NewGdsXyPoint[] Coordinates;

    public NewGdsRecordXy(NewGdsXyPoint[] coordinates)
    {
        Coordinates = coordinates;
    }
}

[BigEndian]
public readonly partial struct NewGdsXyPoint
{
    private readonly int _x;
    private readonly int _y;

    public NewGdsXyPoint(int x, int y)
    {
        _x = x;
        _y = y;
    }
}