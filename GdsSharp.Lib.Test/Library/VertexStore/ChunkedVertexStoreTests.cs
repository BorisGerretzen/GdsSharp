using GdsSharp.Lib.Library.VertexStore;

namespace GdsSharp.Lib.Test.Library.VertexStore;

[TestFixture]
public class ChunkedVertexStoreTests
{
    [Test]
    public void Write_SingleBatch_ReadsBackCorrectly()
    {
        var sut = new ChunkedVertexStore();
        var points = GeneratePoints(0, 100);
        
        var offset = sut.Write(points);
        
        Assert.That(offset, Is.EqualTo(0));
        
        Span<GdsPoint> buffer = new GdsPoint[100];
        var readCount = sut.Read(0, buffer);
        
        Assert.That(readCount, Is.EqualTo(100));
        Assert.That(buffer.ToArray(), Is.EqualTo(points));
    }

    [Test]
    public void Write_MultipleBatches_AppendsCorrectly()
    {
        var sut = new ChunkedVertexStore();

        var batch1 = GeneratePoints(0, 50);
        var batch2 = GeneratePoints(50, 50);

        var offset1 = sut.Write(batch1);
        var offset2 = sut.Write(batch2);

        Assert.That(offset1, Is.EqualTo(0));
        Assert.That(offset2, Is.EqualTo(50));

        Span<GdsPoint> buffer = new GdsPoint[100];
        sut.Read(0, buffer);

        // Verify the sequence is contiguous [0..99]
        Assert.That(buffer[0].X, Is.EqualTo(0));
        Assert.That(buffer[99].X, Is.EqualTo(99));
    }

    [Test]
    public void Write_CrossingChunkBoundary_PreservesData()
    {
        var sut = new ChunkedVertexStore();

        // We write 8100 (fits in chunk 0), then 200 (overflows to chunk 1)
        var batch1 = GeneratePoints(0, 8100);
        var batch2 = GeneratePoints(8100, 200);

        sut.Write(batch1);
        sut.Write(batch2);

        // Read back across the boundary
        // We want to read from index 8090 to 8210 (crossing 8192)
        Span<GdsPoint> buffer = new GdsPoint[120];
        var readCount = sut.Read(8090, buffer);

        Assert.That(readCount, Is.EqualTo(120));
        Assert.That(buffer[0].X, Is.EqualTo(8090));      // Before boundary
        Assert.That(buffer[102].X, Is.EqualTo(8192));    // The boundary point
        Assert.That(buffer[^1].X, Is.EqualTo(8209));     // After boundary
    }

    [Test]
    public void Read_WithOffsetAndLimit_ReturnsPartialSegment()
    {
        var sut = new ChunkedVertexStore();

        var points = GeneratePoints(0, 1000);
        sut.Write(points);

        Span<GdsPoint> buffer = new GdsPoint[10];
        var readCount = sut.Read(500, buffer);

        Assert.That(readCount, Is.EqualTo(10));
        Assert.That(buffer[0].X, Is.EqualTo(500));
        Assert.That(buffer[9].X, Is.EqualTo(509));
    }

    [Test]
    public void Read_PastEndOfStore_ReturnsTruncatedCount()
    {
        var sut = new ChunkedVertexStore();

        var points = GeneratePoints(0, 100);
        sut.Write(points);

        Span<GdsPoint> buffer = new GdsPoint[50];
        
        // Try to read starting at 90 (only 10 points left)
        var readCount = sut.Read(90, buffer);

        Assert.That(readCount, Is.EqualTo(10));
        Assert.That(buffer[0].X, Is.EqualTo(90));
        Assert.That(buffer[9].X, Is.EqualTo(99));
        
        // Ensure the rest of the buffer wasn't touched (optional sanity check)
        Assert.That(buffer[10].X, Is.EqualTo(0)); 
    }

    [Test]
    public void Read_WayPastEnd_ReturnsZero()
    {
        var sut = new ChunkedVertexStore();

        var points = GeneratePoints(0, 50);
        sut.Write(points);

        Span<GdsPoint> buffer = new GdsPoint[10];
        var readCount = sut.Read(1000, buffer);

        Assert.That(readCount, Is.EqualTo(0));
    }

    [Test]
    public void Fuzz_CompareReference()
    {
        var referenceStore = new ReferenceVertexStore();
        var sut = new ChunkedVertexStore();
        
        var random = new Random(69420);
        long totalPoints = 0;
        
        const int iterations = 50_000;
        
        for (var i = 0; i < iterations; i++)
        {
            var operation = random.NextDouble();

            // 50/50 read/write
            if (operation < 0.5)
            {
                // Write
                var count = random.Next(1, 10_000); 
                var data = GenerateRandomPoints(random, count);

                var refOffset = referenceStore.Write(data);
                var sutOffset = sut.Write(data);

                Assert.That(sutOffset, Is.EqualTo(refOffset), $"Iteration {i}: Write Offset Mismatch");
                totalPoints += count;
            }
            else
            {
                // Read
                if (totalPoints == 0) continue;

                var readOffset = (long)(random.NextDouble() * totalPoints);
                var maxRead = (int)(totalPoints - readOffset);
                if (maxRead == 0) continue;

                var count = random.Next(1, Math.Min(maxRead, 10_000) + 1);

                var refBuffer = new GdsPoint[count];
                var sutBuffer = new GdsPoint[count];

                var refReadCount = referenceStore.Read(readOffset, refBuffer);
                var sutReadCount = sut.Read(readOffset, sutBuffer);

                Assert.That(sutReadCount, Is.EqualTo(refReadCount), $"Iteration {i}: Read Count Mismatch");
                if (!refBuffer.AsSpan().SequenceEqual(sutBuffer))
                {
                    Assert.Fail($"Iteration {i}: Data Mismatch reading {count} points at offset {readOffset}.");
                }
            }
        }
    }

    private static GdsPoint[] GeneratePoints(int startX, int count)
    {
        var pts = new GdsPoint[count];
        for (var i = 0; i < count; i++)
        {
            pts[i] = new GdsPoint(startX + i, 0);
        }
        return pts;
    }

    private static GdsPoint[] GenerateRandomPoints(Random rng, int count)
    {
        var pts = new GdsPoint[count];
        for (var i = 0; i < count; i++)
        {
            pts[i] = new GdsPoint(rng.Next(), rng.Next());
        }
        return pts;
    }
}