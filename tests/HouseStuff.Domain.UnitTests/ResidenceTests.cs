using HouseStuff.Domain.Residences;

namespace HouseStuff.Domain.UnitTests;

public sealed class ResidenceTests
{
    [Fact]
    public void CreateNormalizesValidName()
    {
        var result = Residence.Create("  Casa do Luis  ", "user-1", DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal("Casa do Luis", result.Residence!.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void CreateRejectsInvalidName(string name)
    {
        var result = Residence.Create(name, "user-1", DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("residence_name_invalid", result.Code);
    }
}
