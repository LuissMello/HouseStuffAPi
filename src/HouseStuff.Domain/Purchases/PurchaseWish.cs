namespace HouseStuff.Domain.Purchases;

public sealed class PurchaseWish
{
    private PurchaseWish(Guid id, Guid residenceId, string name, string? storeUrl, int priority, DateTimeOffset now)
    {
        Id = id;
        ResidenceId = residenceId;
        Name = name;
        StoreUrl = storeUrl;
        Priority = priority;
        CreatedAt = now;
        UpdatedAt = now;
    }

    private PurchaseWish()
    {
        Name = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string Name { get; private set; }
    public string? StoreUrl { get; private set; }
    public int Priority { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static PurchaseWishChangeResult Create(Guid residenceId, string name, string? storeUrl, int priority, DateTimeOffset now)
    {
        if (residenceId == Guid.Empty)
        {
            return PurchaseWishChangeResult.Failure("residence_required", "O desejo precisa pertencer a uma casa.");
        }

        var validation = Validate(name, storeUrl);
        return validation.Succeeded
            ? PurchaseWishChangeResult.Success(new PurchaseWish(Guid.NewGuid(), residenceId, name.Trim(), NormalizeUrl(storeUrl), priority, now))
            : validation;
    }

    public PurchaseWishChangeResult Update(string name, string? storeUrl, DateTimeOffset now)
    {
        var validation = Validate(name, storeUrl);
        if (!validation.Succeeded)
        {
            return validation;
        }

        Name = name.Trim();
        StoreUrl = NormalizeUrl(storeUrl);
        UpdatedAt = now;
        return PurchaseWishChangeResult.Success(this);
    }

    public void SetPriority(int priority, DateTimeOffset now)
    {
        Priority = priority;
        UpdatedAt = now;
    }

    private static PurchaseWishChangeResult Validate(string name, string? storeUrl)
    {
        if (name.Trim().Length is < 2 or > 120)
        {
            return PurchaseWishChangeResult.Failure("purchase_wish_name_invalid", "O nome deve ter entre 2 e 120 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(storeUrl) &&
            (!Uri.TryCreate(storeUrl.Trim(), UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https") || storeUrl.Trim().Length > 500))
        {
            return PurchaseWishChangeResult.Failure("purchase_wish_url_invalid", "Informe um link HTTP ou HTTPS válido, com até 500 caracteres.");
        }

        return PurchaseWishChangeResult.Success(null);
    }

    private static string? NormalizeUrl(string? storeUrl) => string.IsNullOrWhiteSpace(storeUrl) ? null : storeUrl.Trim();
}

public sealed record PurchaseWishChangeResult(bool Succeeded, PurchaseWish? Wish, string? Code, string? Message)
{
    public static PurchaseWishChangeResult Success(PurchaseWish? wish) => new(true, wish, null, null);
    public static PurchaseWishChangeResult Failure(string code, string message) => new(false, null, code, message);
}
