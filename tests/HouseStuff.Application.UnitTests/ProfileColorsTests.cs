using HouseStuff.Application.Identity;

namespace HouseStuff.Application.UnitTests;

public sealed class ProfileColorsTests
{
    [Theory]
    [InlineData("#2f6b50", "#2F6B50")]
    [InlineData(" #51469b ", "#51469B")]
    [InlineData("#FFFFFF", "#FFFFFF")]
    [InlineData("#00a1fF", "#00A1FF")]
    public void NormalizeReturnsCanonicalHexadecimalColor(string input, string expected)
    {
        Assert.Equal(expected, ProfileColors.Normalize(input));
    }

    [Theory]
    [InlineData("red")]
    [InlineData("#FFF")]
    [InlineData("#GGGGGG")]
    [InlineData("112233")]
    [InlineData("")]
    public void NormalizeRejectsInvalidHexadecimalColor(string input)
    {
        Assert.Null(ProfileColors.Normalize(input));
    }
}
