using GdsSharp.Lib.Library.VertexStore;

namespace GdsSharp.Lib.Test.Library.VertexStore;

[TestFixture]
public class ChunkedVertexStoreTests : VertexStoreTests<ChunkedVertexStore>
{
    protected override ChunkedVertexStore CreateSut()
    {
        return new ChunkedVertexStore();
    }
}