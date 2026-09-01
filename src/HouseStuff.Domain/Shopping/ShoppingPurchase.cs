namespace HouseStuff.Domain.Shopping;

public sealed record ShoppingPurchaseLine(string CategoryName, string ItemName);

public sealed class ShoppingPurchase
{
    private ShoppingPurchase(Guid id, Guid residenceId, string completedByUserId, DateTimeOffset completedAt, IReadOnlyCollection<ShoppingPurchaseLine> lines)
    {
        Id = id;
        ResidenceId = residenceId;
        CompletedByUserId = completedByUserId;
        CompletedAt = completedAt;
        Items = lines.Select(line => ShoppingPurchaseItem.Create(id, residenceId, line.CategoryName, line.ItemName)).ToList();
    }

    private ShoppingPurchase()
    {
        CompletedByUserId = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string CompletedByUserId { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }
    public List<ShoppingPurchaseItem> Items { get; private set; } = [];

    public static ShoppingChangeResult<ShoppingPurchase> Create(
        Guid residenceId,
        string completedByUserId,
        IReadOnlyCollection<ShoppingPurchaseLine> lines,
        DateTimeOffset completedAt)
    {
        if (residenceId == Guid.Empty || string.IsNullOrWhiteSpace(completedByUserId))
        {
            return ShoppingChangeResult.Failure<ShoppingPurchase>("shopping_purchase_user_required", "A compra precisa pertencer a um morador da casa.");
        }

        if (lines.Count == 0)
        {
            return ShoppingChangeResult.Failure<ShoppingPurchase>("shopping_purchase_empty", "Marque ao menos um item antes de finalizar a compra.");
        }

        return ShoppingChangeResult.Success(new ShoppingPurchase(Guid.NewGuid(), residenceId, completedByUserId, completedAt, lines));
    }
}

public sealed class ShoppingPurchaseItem
{
    private ShoppingPurchaseItem(Guid id, Guid purchaseId, Guid residenceId, string categoryName, string itemName)
    {
        Id = id;
        PurchaseId = purchaseId;
        ResidenceId = residenceId;
        CategoryName = categoryName;
        ItemName = itemName;
    }

    private ShoppingPurchaseItem()
    {
        CategoryName = string.Empty;
        ItemName = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid PurchaseId { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string CategoryName { get; private set; }
    public string ItemName { get; private set; }

    internal static ShoppingPurchaseItem Create(Guid purchaseId, Guid residenceId, string categoryName, string itemName) =>
        new(Guid.NewGuid(), purchaseId, residenceId, categoryName, itemName);
}
