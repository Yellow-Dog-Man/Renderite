namespace Renderite.Shared.Tests;

[TestClass]
public class MathHelperTests
{
    private const float FallbackFloatValue = 0.4f;

    /// <summary>
    /// Verifies that <see cref="MathHelper.FilterInvalid"/> returns the fallback value when an invalid float
    /// (NaN or Infinity) is passed.
    /// </summary>
    /// <remarks>
    /// The expected value should be the provided fallback value if it is not null; otherwise, it should be the
    /// default value for float (0.0f).
    /// </remarks>
    /// <param name="valueToPass">The invalid float value to pass in.</param>
    /// <param name="fallback">The fallback value to use.</param>
    /// <param name="expectedValue">The expected value to be returned.</param>
    [TestMethod(UnfoldingStrategy = TestDataSourceUnfoldingStrategy.Unfold)]
    [DataRow([float.NaN, FallbackFloatValue, FallbackFloatValue])]
    [DataRow([float.NaN, null, default(float)])]
    [DataRow([float.NegativeInfinity, FallbackFloatValue, FallbackFloatValue])]
    [DataRow([float.NegativeInfinity, null, default(float)])]
    [DataRow([float.PositiveInfinity, FallbackFloatValue, FallbackFloatValue])]
    [DataRow([float.PositiveInfinity, null, default(float)])]
    public void FilterInvalid_InvalidFloat_ReturnsFallback(float valueToPass, float? fallback, float expectedValue)
    {
        var actualValue = fallback.HasValue
            ? MathHelper.FilterInvalid(valueToPass, fallback.Value)
            : MathHelper.FilterInvalid(valueToPass);

        Assert.AreEqual(expectedValue, actualValue);
    }

    /// <summary>
    /// Verifies that <see cref="MathHelper.FilterInvalid"/> returns the passed in float value if it is a valid float.
    /// </summary>
    /// <remarks>
    /// The expected value should be the same as the passed in float value since it is valid.
    /// </remarks>
    /// <param name="expectedValue">The expected value to be returned.</param>
    [TestMethod]
    [DataRow(3.2f)]
    [DataRow(-3.2f)]
    [DataRow(default(float))]
    public void FilterInvalid_ValidFloat_ReturnsSameFloat(float expectedValue)
    {
        var actualValue = MathHelper.FilterInvalid(expectedValue);

        Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod(UnfoldingStrategy = TestDataSourceUnfoldingStrategy.Unfold)]
    [DataRow([ulong.MinValue, 0])]
    [DataRow([(ulong)1, 1])]
    [DataRow([(ulong)1 << 1, 2])]
    [DataRow([(ulong)1 << 2, 3])]
    [DataRow([(ulong)1 << 3, 4])]
    [DataRow([(ulong)1 << 7, 8])]
    [DataRow([(ulong)1 << 15, 16])]
    [DataRow([(ulong)1 << 23, 24])]
    [DataRow([(ulong)1 << 31, 32])]
    [DataRow([(ulong)1 << 39, 40])]
    [DataRow([(ulong)1 << 47, 48])]
    [DataRow([(ulong)1 << 55, 56])]
    [DataRow([(ulong)1 << 63, 64])]
    [DataRow([ulong.MaxValue, 64])]
    public void GetNecessaryBits_UInt64Value_ReturnsCorrectBitCount(ulong number, int expectedBits)
    {
        var actualBits = MathHelper.NecessaryBits(number);

        Assert.AreEqual(expectedBits, actualBits);
    }
}
