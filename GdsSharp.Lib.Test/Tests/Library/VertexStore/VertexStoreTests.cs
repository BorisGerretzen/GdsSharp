using GdsSharp.Lib.Library.VertexStore;
using GdsSharp.Lib.Test.Helpers;

namespace GdsSharp.Lib.Test.Library.VertexStore;

public abstract class VertexStoreTests<TStore>
    where TStore : IGdsVertexStore
{
    protected abstract TStore CreateSut();

    private void DisposeSut(TStore sut)
    {
        if (sut is IDisposable d) d.Dispose();
    }

    [Test]
    public void Write_EmptyBatch_DoesNotIncreaseCount()
    {
        var sut = CreateSut();
        try
        {
            var points = Array.Empty<GdsPoint>();
            var offset = sut.Write(points);
            Assert.That(offset, Is.Zero);
            Assert.That(sut, Has.Count.Zero);
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    [Test]
    public void Write_IncreasesCount()
    {
        var sut = CreateSut();
        try
        {
            var points = GeneratePoints(0, 150);

            sut.Write(points);
            Assert.That(sut, Has.Count.EqualTo(150));
        }
        finally
        {
            DisposeSut(sut);
        }
    }
    
    [Test]
    public void Write_SingleBatch_ReadsBackCorrectly()
    {
        var sut = CreateSut();
        try
        {
            var points = GeneratePoints(0, 100);

            var offset = sut.Write(points);
            Assert.That(offset, Is.EqualTo(0));

            Span<GdsPoint> buffer = new GdsPoint[100];
            var readCount = sut.Read(0, buffer);

            Assert.That(readCount, Is.EqualTo(100));
            Assert.That(buffer.ToArray(), Is.EqualTo(points));
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    [Test]
    public void Write_MultipleBatches_AppendsCorrectly()
    {
        var sut = CreateSut();
        try
        {
            var batch1 = GeneratePoints(0, 50);
            var batch2 = GeneratePoints(50, 50);

            var offset1 = sut.Write(batch1);
            var offset2 = sut.Write(batch2);

            Assert.That(offset1, Is.EqualTo(0));
            Assert.That(offset2, Is.EqualTo(50));

            Span<GdsPoint> buffer = new GdsPoint[100];
            sut.Read(0, buffer);

            Assert.That(buffer[0].X, Is.EqualTo(0));
            Assert.That(buffer[99].X, Is.EqualTo(99));
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    [Test]
    public void Write_CrossingChunkBoundary_PreservesData()
    {
        var sut = CreateSut();
        try
        {
            var batch1 = GeneratePoints(0, 8100);
            var batch2 = GeneratePoints(8100, 200);

            sut.Write(batch1);
            sut.Write(batch2);

            Span<GdsPoint> buffer = new GdsPoint[120];
            var readCount = sut.Read(8090, buffer);

            Assert.That(readCount, Is.EqualTo(120));
            Assert.That(buffer[0].X, Is.EqualTo(8090));
            Assert.That(buffer[102].X, Is.EqualTo(8192));
            Assert.That(buffer[^1].X, Is.EqualTo(8209));
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    [Test]
    public void Read_WithOffsetAndLimit_ReturnsPartialSegment()
    {
        var sut = CreateSut();
        try
        {
            sut.Write(GeneratePoints(0, 1000));

            Span<GdsPoint> buffer = new GdsPoint[10];
            var readCount = sut.Read(500, buffer);

            Assert.That(readCount, Is.EqualTo(10));
            Assert.That(buffer[0].X, Is.EqualTo(500));
            Assert.That(buffer[9].X, Is.EqualTo(509));
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    [Test]
    public void Read_PastEndOfStore_ReturnsTruncatedCount_AndDoesNotTouchTail()
    {
        var sut = CreateSut();
        try
        {
            sut.Write(GeneratePoints(0, 100));

            // Fill with sentinel so we can prove "tail not touched"
            var sentinel = new GdsPoint(int.MinValue, int.MinValue);
            var arr = new GdsPoint[50];
            for (var i = 0; i < arr.Length; i++) arr[i] = sentinel;

            var buffer = arr.AsSpan();
            var readCount = sut.Read(90, buffer);

            Assert.That(readCount, Is.EqualTo(10));
            Assert.That(buffer[0].X, Is.EqualTo(90));
            Assert.That(buffer[9].X, Is.EqualTo(99));

            // Everything after readCount should remain sentinel
            for (var i = readCount; i < buffer.Length; i++) Assert.That(buffer[i], Is.EqualTo(sentinel), $"Index {i} was modified unexpectedly.");
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    [Test]
    public void Read_WayPastEnd_ReturnsZero()
    {
        var sut = CreateSut();
        try
        {
            sut.Write(GeneratePoints(0, 50));

            Span<GdsPoint> buffer = new GdsPoint[10];
            var readCount = sut.Read(1000, buffer);

            Assert.That(readCount, Is.EqualTo(0));
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    [Test]
    public void Fuzz_CompareReference()
    {
        var referenceStore = new ReferenceVertexStore();
        var sut = CreateSut();
        try
        {
            var random = new Random(69420);
            long totalPoints = 0;

            const int iterations = 50_000;

            for (var i = 0; i < iterations; i++)
                if (random.NextDouble() < 0.5)
                {
                    var count = random.Next(1, 10_000);
                    var data = GenerateRandomPoints(random, count);

                    var refOffset = referenceStore.Write(data);
                    var sutOffset = sut.Write(data);

                    Assert.That(sutOffset, Is.EqualTo(refOffset), $"Iteration {i}: Write Offset Mismatch");
                    totalPoints += count;
                }
                else
                {
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
                    Assert.That(sutBuffer.AsSpan().SequenceEqual(refBuffer), Is.True,
                        $"Iteration {i}: Data mismatch reading {count} points at offset {readOffset}.");
                }
        }
        finally
        {
            DisposeSut(sut);
        }
    }

    private static GdsPoint[] GeneratePoints(int startX, int count)
    {
        var pts = new GdsPoint[count];
        for (var i = 0; i < count; i++) pts[i] = new GdsPoint(startX + i, 0);
        return pts;
    }

    private static GdsPoint[] GenerateRandomPoints(Random rng, int count)
    {
        var pts = new GdsPoint[count];
        for (var i = 0; i < count; i++) pts[i] = new GdsPoint(rng.Next(), rng.Next());
        return pts;
    }
}