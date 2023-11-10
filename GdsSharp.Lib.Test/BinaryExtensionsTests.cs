using GdsSharp.Lib.Models.Parsing;

namespace GdsSharp.Lib.Test;

public class BinaryExtensionsTests
{
    // [Test]
    // public void ReadWriteHeaderShouldBeEqual()
    // {
    //     var inputBytes = new byte[]
    //     {
    //         0x00, 0x06, 0x00, 0x02
    //     };
    //
    //     var stream = new MemoryStream(inputBytes);
    //     var reader = new BinaryReader(stream);
    //     var header = reader.ReadStruct<GdsHeader>();
    //
    //     var outputStream = new MemoryStream();
    //     var writer = new BinaryWriter(outputStream);
    //
    //     writer.WriteStruct(header);
    //
    //     var outputBytes = outputStream.ToArray();
    //     
    //     Assert.That(outputBytes, Is.EqualTo(inputBytes));
    // }
    //
    // [Test]
    // public void ReadHeaderShouldBeCorrect()
    // {
    //     var inputBytes = new byte[]
    //     {
    //         0x00, 0x06, 0x00, 0x02
    //     };
    //
    //     var stream = new MemoryStream(inputBytes);
    //     var reader = new BinaryReader(stream);
    //     var header = reader.ReadStruct<GdsHeader>();
    //
    //     Assert.That(header.Length, Is.EqualTo(6));
    //     Assert.That(header.Code, Is.EqualTo(2));
    // }
}