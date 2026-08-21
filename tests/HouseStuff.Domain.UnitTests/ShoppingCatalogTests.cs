using HouseStuff.Domain.Shopping;

namespace HouseStuff.Domain.UnitTests;

public sealed class ShoppingCatalogTests
{
    [Fact]
    public void CategoryNormalizesNameAndCanMove()
    {
        var now = DateTimeOffset.UtcNow;
        var result = ShoppingCategory.Create(Guid.NewGuid(), "  Hortifruti  ", 2, now);

        Assert.True(result.Succeeded);
        Assert.Equal("Hortifruti", result.Value!.Name);
        Assert.Equal("HORTIFRUTI", result.Value.NormalizedName);

        result.Value.SetDisplayOrder(1, now.AddMinutes(1));
        Assert.Equal(1, result.Value.DisplayOrder);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void CategoryRejectsInvalidName(string name)
    {
        var result = ShoppingCategory.Create(Guid.NewGuid(), name, 0, DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("shopping_category_name_invalid", result.Code);
    }

    [Fact]
    public void ItemNormalizesAndCanChangeCategory()
    {
        var residenceId = Guid.NewGuid();
        var firstCategoryId = Guid.NewGuid();
        var secondCategoryId = Guid.NewGuid();
        var result = ShoppingItem.Create(residenceId, firstCategoryId, "  Leite  ", DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal("Leite", result.Value!.Name);
        Assert.Equal("LEITE", result.Value.NormalizedName);

        result.Value.Update(secondCategoryId, "Leite integral", DateTimeOffset.UtcNow);
        Assert.Equal(secondCategoryId, result.Value.CategoryId);
        Assert.Equal("LEITE INTEGRAL", result.Value.NormalizedName);
    }
}
