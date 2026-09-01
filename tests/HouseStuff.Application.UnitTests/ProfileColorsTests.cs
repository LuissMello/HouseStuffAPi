using HouseStuff.Application.Identity;

namespace HouseStuff.Application.UnitTests;

public sealed class ProfileColorsTests
{
    [Theory]
    [InlineData("#2f6b50", "#2F6B50")]
    [InlineData(" #51469b ", "#51469B")]
    public void NormalizeReturnsCanonicalSupportedColor(string input, string expected)
    {
        Assert.Equal(expected, ProfileColors.Normalize(input));
    }

    [Theory]
    [InlineData("#FFFFFF")]
    [InlineData("red")]
    [InlineData("")]
    public void NormalizeRejectsUnsupportedColor(string input)
    {
        Assert.Null(ProfileColors.Normalize(input));
    }
}
