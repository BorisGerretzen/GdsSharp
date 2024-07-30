using GdsSharp.Lib.Terminals.Records;

namespace GdsSharp.Lib;

public class ParseException : Exception
{
    public ParseException(Type expected, object actual, int offset) : base($"Expected token {expected.Name} but found {actual.GetType().Name} at offset {offset}.")
    {
    }

    public ParseException(GdsRecordNoDataType expected, GdsRecordNoDataType actual, int offset) : base($"Expected token {expected} but found {actual} at offset {offset}.")
    {
    }

    public ParseException(GdsRecordNoDataType unexpected, int offset) : base($"Unexpected token {unexpected} at offset {offset}.")
    {
    }
}