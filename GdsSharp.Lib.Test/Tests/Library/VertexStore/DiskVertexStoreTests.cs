using GdsSharp.Lib.Library.VertexStore;

namespace GdsSharp.Lib.Test.Library.VertexStore;

[TestFixture]
public class DiskVertexStoreTests : VertexStoreTests<DiskVertexStore>
{
    protected override DiskVertexStore CreateSut()
    {
        return new DiskVertexStore();
    }
}