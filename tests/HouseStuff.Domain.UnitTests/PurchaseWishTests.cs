using HouseStuff.Domain.Purchases;

namespace HouseStuff.Domain.UnitTests;

public sealed class PurchaseWishTests
{
    [Fact]
    public void CreateNormalizesOptionalFieldsAndPriority()
    {
        var result = PurchaseWish.Create(Guid.NewGuid(), "  Sofá novo  ", "  https://loja.example/produto  ", 2, DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal("Sofá novo", result.Wish!.Name);
        Assert.Equal("https://loja.example/produto", result.Wish.StoreUrl);
        Assert.Equal(2, result.Wish.Priority);
    }

    [Theory]
    [InlineData("mercadolivre.com.br/item")]
    [InlineData("ftp://loja.example/item")]
    [InlineData("javascript:alert(1)")]
    public void CreateRejectsUnsafeStoreUrl(string storeUrl)
    {
        var result = PurchaseWish.Create(Guid.NewGuid(), "Sofá", storeUrl, 0, DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("purchase_wish_url_invalid", result.Code);
    }

    [Fact]
    public void WishCanRemoveLinkAndChangePriority()
    {
        var now = DateTimeOffset.UtcNow;
        var wish = PurchaseWish.Create(Guid.NewGuid(), "Sofá", "https://loja.example/sofa", 0, now).Wish!;

        var update = wish.Update("Sofá retrátil", null, now.AddMinutes(1));
        wish.SetPriority(3, now.AddMinutes(2));

        Assert.True(update.Succeeded);
        Assert.Null(wish.StoreUrl);
        Assert.Equal(3, wish.Priority);
    }
}
