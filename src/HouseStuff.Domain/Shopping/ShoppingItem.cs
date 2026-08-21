namespace HouseStuff.Domain.Shopping;

public sealed class ShoppingItem
{
    private ShoppingItem(Guid id, Guid residenceId, Guid categoryId, string name, string normalizedName, DateTimeOffset now)
    {
        Id = id;
        ResidenceId = residenceId;
        CategoryId = categoryId;
        Name = name;
        NormalizedName = normalizedName;
        CreatedAt = now;
        UpdatedAt = now;
    }

    private ShoppingItem()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid ResidenceId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ShoppingChangeResult<ShoppingItem> Create(Guid residenceId, Guid categoryId, string name, DateTimeOffset now)
    {
        if (residenceId == Guid.Empty || categoryId == Guid.Empty)
        {
            return ShoppingChangeResult.Failure<ShoppingItem>("shopping_item_category_required", "O item precisa pertencer a uma categoria da casa.");
        }

        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 100)
        {
            return ShoppingChangeResult.Failure<ShoppingItem>("shopping_item_name_invalid", "O nome do item deve ter entre 2 e 100 caracteres.");
        }

        return ShoppingChangeResult.Success(new ShoppingItem(Guid.NewGuid(), residenceId, categoryId, normalized, NormalizeName(normalized), now));
    }

    public ShoppingChangeResult<ShoppingItem> Update(Guid categoryId, string name, DateTimeOffset now)
    {
        if (categoryId == Guid.Empty)
        {
            return ShoppingChangeResult.Failure<ShoppingItem>("shopping_item_category_required", "Escolha uma categoria para o item.");
        }

        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 100)
        {
            return ShoppingChangeResult.Failure<ShoppingItem>("shopping_item_name_invalid", "O nome do item deve ter entre 2 e 100 caracteres.");
        }

        CategoryId = categoryId;
        Name = normalized;
        NormalizedName = NormalizeName(normalized);
        UpdatedAt = now;
        return ShoppingChangeResult.Success(this);
    }

    public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();
}
