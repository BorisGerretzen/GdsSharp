namespace GdsSharp.Lib.Library.Editing;

/// <summary>
///     Stable element identity inside an edit session.
///     Can reference an element in the base library or a newly created element.
/// </summary>
public readonly record struct ElementKey(bool IsBase, int Id)
{
    public static ElementKey Base(int elementId)
    {
        return new ElementKey(true, elementId);
    }

    public static ElementKey New(int newElementId)
    {
        return new ElementKey(false, newElementId);
    }
}