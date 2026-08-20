namespace HouseStuff.Domain.Tasks;

public enum HouseholdTaskKind
{
    OneTime,
    Reusable,
    Recurring,
}

public sealed class HouseholdTask
{
    private HouseholdTask() { }

    private HouseholdTask(Guid residenceId, Guid potId, string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, DateTimeOffset now)
    {
        Id = Guid.NewGuid();
        ResidenceId = residenceId;
        PotId = potId;
        Apply(name, description, kind, recurrenceDays);
        IsActive = true;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public Guid Id { get; private set; }
    public Guid ResidenceId { get; private set; }
    public Guid PotId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string NormalizedName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public HouseholdTaskKind Kind { get; private set; }
    public int? RecurrenceDays { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static HouseholdTaskChangeResult Create(Guid residenceId, Guid potId, string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, DateTimeOffset now)
    {
        if (residenceId == Guid.Empty || potId == Guid.Empty)
        {
            return HouseholdTaskChangeResult.Failure("task_context_required", "A tarefa precisa pertencer a uma casa e a um pote.");
        }

        var validation = Validate(name, description, kind, recurrenceDays);
        return validation.Succeeded
            ? HouseholdTaskChangeResult.Success(new HouseholdTask(residenceId, potId, name, description, kind, recurrenceDays, now))
            : validation;
    }

    public HouseholdTaskChangeResult Update(Guid potId, string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, DateTimeOffset now)
    {
        if (potId == Guid.Empty)
        {
            return HouseholdTaskChangeResult.Failure("pot_required", "Selecione um pote.");
        }

        var validation = Validate(name, description, kind, recurrenceDays);
        if (!validation.Succeeded)
        {
            return validation;
        }

        PotId = potId;
        Apply(name, description, kind, recurrenceDays);
        UpdatedAt = now;
        return HouseholdTaskChangeResult.Success(this);
    }

    public void SetActive(bool isActive, DateTimeOffset now)
    {
        IsActive = isActive;
        UpdatedAt = now;
    }

    public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static HouseholdTaskChangeResult Validate(string name, string? description, HouseholdTaskKind kind, int? recurrenceDays)
    {
        var trimmedName = name.Trim();
        if (trimmedName.Length is < 2 or > 100)
        {
            return HouseholdTaskChangeResult.Failure("task_name_invalid", "O nome da tarefa deve ter entre 2 e 100 caracteres.");
        }

        if (description?.Trim().Length > 300)
        {
            return HouseholdTaskChangeResult.Failure("task_description_invalid", "A descrição deve ter até 300 caracteres.");
        }

        if (!Enum.IsDefined(kind))
        {
            return HouseholdTaskChangeResult.Failure("task_kind_invalid", "Selecione um tipo de tarefa válido.");
        }

        if (kind == HouseholdTaskKind.Recurring && recurrenceDays is not (>= 1 and <= 3650))
        {
            return HouseholdTaskChangeResult.Failure("recurrence_days_invalid", "Informe um intervalo entre 1 e 3650 dias.");
        }

        if (kind != HouseholdTaskKind.Recurring && recurrenceDays is not null)
        {
            return HouseholdTaskChangeResult.Failure("recurrence_not_allowed", "Somente tarefas recorrentes possuem intervalo.");
        }

        return HouseholdTaskChangeResult.Success(null);
    }

    private void Apply(string name, string? description, HouseholdTaskKind kind, int? recurrenceDays)
    {
        Name = name.Trim();
        NormalizedName = NormalizeName(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Kind = kind;
        RecurrenceDays = recurrenceDays;
    }
}

public sealed record HouseholdTaskChangeResult(bool Succeeded, HouseholdTask? Task, string? Code, string? Message)
{
    public static HouseholdTaskChangeResult Success(HouseholdTask? task) => new(true, task, null, null);
    public static HouseholdTaskChangeResult Failure(string code, string message) => new(false, null, code, message);
}
