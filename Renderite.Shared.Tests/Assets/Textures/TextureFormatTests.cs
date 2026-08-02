namespace Renderite.Shared.Tests.Assets.Textures;

[TestClass]
public class TextureFormatTests
{
    [TestMethod]
    [DataRow(TextureFormat.RGBAHalf)]
    [DataRow(TextureFormat.ARGBHalf)]
    [DataRow(TextureFormat.RHalf)]
    [DataRow(TextureFormat.RGHalf)]
    [DataRow(TextureFormat.RGBAFloat)]
    [DataRow(TextureFormat.ARGBFloat)]
    [DataRow(TextureFormat.RFloat)]
    [DataRow(TextureFormat.RGFloat)]
    [DataRow(TextureFormat.BC6H)]
    public void IsHDRCompliant_HDRFormat_ReturnsTrue(TextureFormat hdrFormat) =>
        Assert.IsTrue(TextureFormatExtensions.IsHDR(hdrFormat));

    [TestMethod]
    [DataRow(TextureFormat.Alpha8)]
    [DataRow(TextureFormat.R8)]
    [DataRow(TextureFormat.RGB24)]
    [DataRow(TextureFormat.ARGB32)]
    [DataRow(TextureFormat.RGBA32)]
    [DataRow(TextureFormat.BGRA32)]
    [DataRow(TextureFormat.RGB565)]
    [DataRow(TextureFormat.BGR565)]
    [DataRow(TextureFormat.BC1)]
    [DataRow(TextureFormat.BC2)]
    [DataRow(TextureFormat.BC3)]
    [DataRow(TextureFormat.BC4)]
    [DataRow(TextureFormat.BC5)]
    [DataRow(TextureFormat.BC7)]
    [DataRow(TextureFormat.ETC2_RGB)]
    [DataRow(TextureFormat.ETC2_RGBA1)]
    [DataRow(TextureFormat.ETC2_RGBA8)]
    [DataRow(TextureFormat.ASTC_4x4)]
    [DataRow(TextureFormat.ASTC_5x5)]
    [DataRow(TextureFormat.ASTC_6x6)]
    [DataRow(TextureFormat.ASTC_8x8)]
    [DataRow(TextureFormat.ASTC_10x10)]
    [DataRow(TextureFormat.ASTC_12x12)]
    [DataRow(TextureFormat.Unknown)]
    public void IsHDRCompliant_NonHDRFormat_ReturnsFalse(TextureFormat nonHdrFormat) =>
        Assert.IsFalse(TextureFormatExtensions.IsHDR(nonHdrFormat));


    [TestMethod]
    public void IsHDRCompliant_UndefinedValue_ThrowsException()
    {
        var undefinedFormat = (TextureFormat)int.MaxValue;
        Assert.ThrowsExactly<ArgumentException>(() => TextureFormatExtensions.IsHDR(undefinedFormat));
    }
}
