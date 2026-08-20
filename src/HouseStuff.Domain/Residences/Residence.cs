namespace HouseStuff.Domain.Residences;

public sealed class Residence
{
    private Residence(Guid id, string name, string createdByUserId, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    private Residence()
    {
        Name = string.Empty;
        CreatedByUserId = string.Empty;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string CreatedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static ResidenceCreationResult Create(string name, string createdByUserId, DateTimeOffset createdAt)
    {
        var normalizedName = name.Trim();
        if (normalizedName.Length is < 2 or > 80)
        {
            return ResidenceCreationResult.Failure("residence_name_invalid", "O nome da casa deve ter entre 2 e 80 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(createdByUserId))
        {
            return ResidenceCreationResult.Failure("creator_required", "A residência precisa de um responsável.");
        }

        return ResidenceCreationResult.Success(new Residence(Guid.NewGuid(), normalizedName, createdByUserId, createdAt));
    }
}

public sealed record ResidenceCreationResult(bool Succeeded, Residence? Residence, string? Code, string? Message)
{
    public static ResidenceCreationResult Success(Residence residence) => new(true, residence, null, null);
    public static ResidenceCreationResult Failure(string code, string message) => new(false, null, code, message);
}
