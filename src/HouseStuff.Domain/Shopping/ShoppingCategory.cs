namespace HouseStuff.Domain.Shopping;

public sealed class ShoppingCategory
{
    private ShoppingCategory(Guid id, Guid residenceId, string name, string normalizedName, int displayOrder, DateTimeOffset now)
    {
        Id = id;
        ResidenceId = residenceId;
        Name = name;
        NormalizedName = normalizedName;
        DisplayOrder = displayOrder;
        CreatedAt = now;
        UpdatedAt = now;
    }

    private ShoppingCategory()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static ShoppingChangeResult<ShoppingCategory> Create(Guid residenceId, string name, int displayOrder, DateTimeOffset now)
    {
        if (residenceId == Guid.Empty)
        {
            return ShoppingChangeResult.Failure<ShoppingCategory>("residence_required", "A categoria precisa pertencer a uma casa.");
        }

        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 60)
        {
            return ShoppingChangeResult.Failure<ShoppingCategory>("shopping_category_name_invalid", "O nome da categoria deve ter entre 2 e 60 caracteres.");
        }

        return ShoppingChangeResult.Success(new ShoppingCategory(Guid.NewGuid(), residenceId, normalized, NormalizeName(normalized), displayOrder, now));
    }

    public ShoppingChangeResult<ShoppingCategory> Update(string name, DateTimeOffset now)
    {
        var normalized = name.Trim();
        if (normalized.Length is < 2 or > 60)
        {
            return ShoppingChangeResult.Failure<ShoppingCategory>("shopping_category_name_invalid", "O nome da categoria deve ter entre 2 e 60 caracteres.");
        }

        Name = normalized;
        NormalizedName = NormalizeName(normalized);
        UpdatedAt = now;
        return ShoppingChangeResult.Success(this);
    }

    public void SetDisplayOrder(int displayOrder, DateTimeOffset now)
    {
        DisplayOrder = displayOrder;
        UpdatedAt = now;
    }

    public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();
}

public sealed record ShoppingChangeResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class ShoppingChangeResult
{
    public static ShoppingChangeResult<T> Success<T>(T value) => new(true, value, null, null);
    public static ShoppingChangeResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}
