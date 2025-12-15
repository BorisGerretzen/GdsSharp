namespace GdsSharp.Lib.Old.NonTerminals.Abstractions;

public interface IGdsLayeredElement : IGdsElement
{
    short Layer { get; set; }
}