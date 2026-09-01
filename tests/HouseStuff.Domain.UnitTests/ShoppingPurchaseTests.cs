using HouseStuff.Domain.Shopping;

namespace HouseStuff.Domain.UnitTests;

public sealed class ShoppingPurchaseTests
{
    [Fact]
    public void PurchaseRequiresAtLeastOneItem()
    {
        var result = ShoppingPurchase.Create(Guid.NewGuid(), "user-1", [], DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("shopping_purchase_empty", result.Code);
    }

    [Fact]
    public void PurchasePreservesCategoryAndItemNames()
    {
        var result = ShoppingPurchase.Create(
            Guid.NewGuid(),
            "user-1",
            [new ShoppingPurchaseLine("Higiene", "Pasta de dente")],
            DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        var item = Assert.Single(result.Value!.Items);
        Assert.Equal("Higiene", item.CategoryName);
        Assert.Equal("Pasta de dente", item.ItemName);
    }
}
