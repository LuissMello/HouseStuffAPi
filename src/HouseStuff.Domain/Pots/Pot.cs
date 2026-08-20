namespace HouseStuff.Domain.Pots;

public sealed class Pot
{
    private Pot(Guid id, Guid residenceId, string name, string normalizedName, string? description, int displayOrder, DateTimeOffset createdAt)
    {
        Id = id;
        ResidenceId = residenceId;
        Name = name;
        NormalizedName = normalizedName;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = true;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    private Pot()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static PotChangeResult Create(Guid residenceId, string name, string? description, int displayOrder, DateTimeOffset now)
    {
        if (residenceId == Guid.Empty)
        {
            return PotChangeResult.Failure("residence_required", "O pote precisa pertencer a uma casa.");
        }

        var validation = Validate(name, description);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var normalizedName = name.Trim();
        return PotChangeResult.Success(new Pot(
            Guid.NewGuid(), residenceId, normalizedName, NormalizeName(normalizedName), NormalizeDescription(description), displayOrder, now));
    }

    public PotChangeResult Update(string name, string? description, DateTimeOffset now)
    {
        var validation = Validate(name, description);
        if (!validation.Succeeded)
        {
            return validation;
        }

        Name = name.Trim();
        NormalizedName = NormalizeName(Name);
        Description = NormalizeDescription(description);
        UpdatedAt = now;
        return PotChangeResult.Success(this);
    }

    public void SetActive(bool isActive, DateTimeOffset now)
    {
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void SetDisplayOrder(int displayOrder, DateTimeOffset now)
    {
        DisplayOrder = displayOrder;
        UpdatedAt = now;
    }

    public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static PotChangeResult Validate(string name, string? description)
    {
        var normalizedName = name.Trim();
        if (normalizedName.Length is < 2 or > 60)
        {
            return PotChangeResult.Failure("pot_name_invalid", "O nome do pote deve ter entre 2 e 60 caracteres.");
        }

        if (description?.Trim().Length > 200)
        {
            return PotChangeResult.Failure("pot_description_invalid", "A descrição deve ter no máximo 200 caracteres.");
        }

        return PotChangeResult.Success(null);
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();
}

public sealed record PotChangeResult(bool Succeeded, Pot? Pot, string? Code, string? Message)
{
    public static PotChangeResult Success(Pot? pot) => new(true, pot, null, null);
    public static PotChangeResult Failure(string code, string message) => new(false, null, code, message);
}
