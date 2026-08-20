using HouseStuff.Domain.Pots;

namespace HouseStuff.Domain.UnitTests;

public sealed class PotTests
{
    [Fact]
    public void CreateNormalizesFields()
    {
        var result = Pot.Create(Guid.NewGuid(), "  Mensal  ", "  Tarefas do mês  ", 0, DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal("Mensal", result.Pot!.Name);
        Assert.Equal("MENSAL", result.Pot.NormalizedName);
        Assert.Equal("Tarefas do mês", result.Pot.Description);
        Assert.True(result.Pot.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void CreateRejectsInvalidName(string name)
    {
        var result = Pot.Create(Guid.NewGuid(), name, null, 0, DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("pot_name_invalid", result.Code);
    }

    [Fact]
    public void PotCanBeArchivedAndReactivated()
    {
        var pot = Pot.Create(Guid.NewGuid(), "Semanal", null, 0, DateTimeOffset.UtcNow).Pot!;

        pot.SetActive(false, DateTimeOffset.UtcNow);
        Assert.False(pot.IsActive);
        pot.SetActive(true, DateTimeOffset.UtcNow);
        Assert.True(pot.IsActive);
    }
}
