namespace Renderite.Shared.Tests.Assets.Textures;

[TestClass]
public class TextureFormatTests
{
    public static TextureFormat[] DefinedTextureFormatEnums =>
        Enum.GetValues<TextureFormat>();

    /// <summary>
    /// Tests that the <see cref="TextureFormatExtensions.IsHDR(TextureFormat)"/> method returns
    /// <c>true</c> for known HDR-compliant texture formats.
    /// </summary>
    /// <param name="hdrFormat">A valid HDR-compliant texture format.</param>
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

    /// <summary>
    /// Tests that the <see cref="TextureFormatExtensions.IsHDR(TextureFormat)"/> method returns
    /// <c>false</c> for non-HDR texture formats.
    /// </summary>
    /// <param name="nonHdrFormat">A texture format that is not HDR-compliant.</param>
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

    /// <summary>
    /// Tests that the <see cref="TextureFormatExtensions.IsHDR(TextureFormat)"/> method throws an
    /// exception when given an undefined <see cref="TextureFormat"/> value.
    /// </summary>
    [TestMethod]
    public void IsHDRCompliant_UndefinedValue_ThrowsException()
    {
        var undefinedFormat = (TextureFormat)int.MaxValue;
        Assert.ThrowsExactly<ArgumentException>(() => TextureFormatExtensions.IsHDR(undefinedFormat));
    }

    /// <summary>
    /// Tests that the <see cref="TextureFormatExtensions.IsHDR(TextureFormat)"/> method does not throw an
    /// exception when given a defined <see cref="TextureFormat"/> value.
    /// </summary>
    /// <param name="definedFormat">A defined texture format.</param>
    [TestMethod(UnfoldingStrategy = TestDataSourceUnfoldingStrategy.Unfold)]
    [DynamicData(nameof(DefinedTextureFormatEnums))]
    public void IsHDRCompliant_DefinedValue_DoesNotThrowException(TextureFormat definedFormat)
    {
        try
        {
            _ = TextureFormatExtensions.IsHDR(definedFormat);
        }
        catch
        {
            Assert.Fail($"TextureFormat '{definedFormat}' threw an exception during the call to 'TextureFormatExtensions.IsHDR'.");
        }
    }
}
