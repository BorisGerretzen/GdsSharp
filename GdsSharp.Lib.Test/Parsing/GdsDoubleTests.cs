using GdsSharp.Lib.Parsing;

namespace GdsSharp.Lib.Test.Parsing;

public class GdsDoubleTests
{
    private readonly byte[] _oneBytes =
    {
        0b01000001, 0b00010000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000
    };

    private readonly byte[] _negOneBytes =
    {
        0b11000001, 0b00010000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000
    };

    private readonly byte[] _zeroBytes =
    {
        0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000
    };

    private readonly byte[] _onePointSevenBytes =
    {
        0b01000001, 0b00011011, 0b00110011, 0b00110011, 0b00110011, 0b00110011, 0b00110011, 0b00110011
    };

    private readonly byte[] _thousandBytes =
    {
        0b01000011, 0b00111110, 0b10000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000, 0b00000000
    };

    #region Memory layout
    
    [Test]
    public void TestDoubleMemoryLayoutPositiveOne()
    {
        var myDouble = new GdsDouble(_oneBytes);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(1));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00010000_00000000_00000000_00000000_00000000_00000000_00000000));
    }

    [Test]
    public void TestDoubleMemoryLayoutNegativeOne()
    {
        var myDouble = new GdsDouble(_negOneBytes);
        Assert.That(myDouble.IsNegative, Is.True);
        Assert.That(myDouble.Exponent, Is.EqualTo(1));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00010000_00000000_00000000_00000000_00000000_00000000_00000000));
    }

    [Test]
    public void TestDoubleMemoryLayoutZero()
    {
        var myDouble = new GdsDouble(_zeroBytes);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(0));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00000000_00000000_00000000_00000000_00000000_00000000_00000000));
    }

    [Test]
    public void TestDoubleMemoryLayoutOnePointSeven()
    {
        var myDouble = new GdsDouble(_onePointSevenBytes);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(1));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00011011_00110011_00110011_00110011_00110011_00110011_00110011));
    }

    [Test]
    public void TestDoubleMemoryLayoutThousand()
    {
        var myDouble = new GdsDouble(_thousandBytes);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(3));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00111110_10000000_00000000_00000000_00000000_00000000_00000000));
    }
    
    #endregion

    #region FromBytes AsDouble

    [Test]
    public void TestFromBytesAsDoublePositiveOne()
    {
        var myDouble = new GdsDouble(_oneBytes);
        Assert.That(myDouble.AsDouble(), Is.EqualTo(1.0d));
    }
    
    [Test]
    public void TestFromBytesAsDoubleNegativeOne()
    {
        var myDouble = new GdsDouble(_negOneBytes);
        Assert.That(myDouble.AsDouble(), Is.EqualTo(-1.0d));
    }
    
    [Test]
    public void TestFromBytesAsDoubleZero()
    {
        var myDouble = new GdsDouble(_zeroBytes);
        Assert.That(myDouble.AsDouble(), Is.EqualTo(0.0d));
    }
    
    [Test]
    public void TestFromBytesAsDoubleOnePointSeven()
    {
        var myDouble = new GdsDouble(_onePointSevenBytes);
        Assert.That(myDouble.AsDouble(), Is.EqualTo(1.7f).Within(1e-6));
    }

    [Test]
    public void TestFromBytesAsDoubleThousand()
    {
        var myDouble = new GdsDouble(_thousandBytes);
        Assert.That(myDouble.AsDouble(), Is.EqualTo(1000.0d).Within(1e-6));
    }
    
    #endregion
    
    #region FromDouble AsDouble
    
    [Test]
    public void TestFromDoublePositiveOne()
    {
        var myDouble = new GdsDouble(1.0d);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(1));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00010000_00000000_00000000_00000000_00000000_00000000_00000000));
        Assert.That(myDouble.AsDouble(), Is.EqualTo(1.0d));
    }
    
    [Test]
    public void TestFromDoubleNegativeOne()
    {
        var myDouble = new GdsDouble(-1.0d);
        Assert.That(myDouble.IsNegative, Is.True);
        Assert.That(myDouble.Exponent, Is.EqualTo(1));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00010000_00000000_00000000_00000000_00000000_00000000_00000000));
    }
    
    [Test]
    public void TestFromDoubleZero()
    {
        var myDouble = new GdsDouble(0.0d);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(0));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00000000_00000000_00000000_00000000_00000000_00000000_00000000));
    }

    [Test]
    public void TestFromDoubleOnePointSeven()
    {
        var myDouble = new GdsDouble(1.7d);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(1));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00011011_00110011_00110011_00110011_00110011_00110011_00110011));
    }

    [Test]
    public void TestFromDoubleThousand()
    {
        var myDouble = new GdsDouble(1000.0d);
        Assert.That(myDouble.IsNegative, Is.False);
        Assert.That(myDouble.Exponent, Is.EqualTo(3));
        Assert.That(myDouble.Mantissa, Is.EqualTo(0b00111110_10000000_00000000_00000000_00000000_00000000_00000000));
    }
    
    #endregion
    
    #region FromBytes ToBytes
    
    [Test]
    public void TestFromBytesToBytesPositiveOne()
    {
        var myDouble = new GdsDouble(_oneBytes);
        var bytes = myDouble.AsBytes();
        Assert.That(bytes, Is.EqualTo(_oneBytes));
    }
    
    [Test]
    public void TestFromBytesToBytesNegativeOne()
    {
        var myDouble = new GdsDouble(_negOneBytes);
        var bytes = myDouble.AsBytes();
        Assert.That(bytes, Is.EqualTo(_negOneBytes));
    }
    
    [Test]
    public void TestFromBytesToBytesZero()
    {
        var myDouble = new GdsDouble(_zeroBytes);
        var bytes = myDouble.AsBytes();
        Assert.That(bytes, Is.EqualTo(_zeroBytes));
    }
    
    [Test]
    public void TestFromBytesToBytesOnePointSeven()
    {
        var myDouble = new GdsDouble(_onePointSevenBytes);
        var bytes = myDouble.AsBytes();
        Assert.That(bytes, Is.EqualTo(_onePointSevenBytes));
    }
    
    [Test]
    public void TestFromBytesToBytesThousand()
    {
        var myDouble = new GdsDouble(_thousandBytes);
        var bytes = myDouble.AsBytes();
        Assert.That(bytes, Is.EqualTo(_thousandBytes));
    }
    
    #endregion
}