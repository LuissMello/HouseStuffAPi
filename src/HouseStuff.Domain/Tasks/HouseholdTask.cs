namespace HouseStuff.Domain.Tasks;

public enum HouseholdTaskKind
{
    OneTime,
    Reusable,
    Recurring,
}

public enum HouseholdTaskDifficulty
{
    Easy,
    Medium,
    Hard,
}

public sealed class HouseholdTask
{
    private readonly List<HouseholdTaskEligibleUser> eligibleUsers = [];

    private HouseholdTask() { }

    private HouseholdTask(Guid residenceId, Guid potId, string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, DateTimeOffset now, HouseholdTaskDifficulty difficulty, bool isAvailableToAllResidents, IReadOnlyCollection<string>? eligibleUserIds)
    {
        Id = Guid.NewGuid();
        ResidenceId = residenceId;
        PotId = potId;
        Apply(name, description, kind, recurrenceDays, difficulty);
        ApplyEligibility(isAvailableToAllResidents, eligibleUserIds);
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
    public HouseholdTaskDifficulty Difficulty { get; private set; }
    public int? RecurrenceDays { get; private set; }
    public bool IsAvailableToAllResidents { get; private set; }
    public IReadOnlyCollection<HouseholdTaskEligibleUser> EligibleUsers => eligibleUsers;
    public DateTimeOffset? NextAvailableAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static HouseholdTaskChangeResult Create(Guid residenceId, Guid potId, string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, DateTimeOffset now, HouseholdTaskDifficulty difficulty = HouseholdTaskDifficulty.Medium, bool isAvailableToAllResidents = true, IReadOnlyCollection<string>? eligibleUserIds = null)
    {
        if (residenceId == Guid.Empty || potId == Guid.Empty)
        {
            return HouseholdTaskChangeResult.Failure("task_context_required", "A tarefa precisa pertencer a uma casa e a um pote.");
        }

        var validation = Validate(name, description, kind, recurrenceDays, difficulty, isAvailableToAllResidents, eligibleUserIds);
        return validation.Succeeded
            ? HouseholdTaskChangeResult.Success(new HouseholdTask(residenceId, potId, name, description, kind, recurrenceDays, now, difficulty, isAvailableToAllResidents, eligibleUserIds))
            : validation;
    }

    public HouseholdTaskChangeResult Update(Guid potId, string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, DateTimeOffset now, HouseholdTaskDifficulty difficulty = HouseholdTaskDifficulty.Medium, bool isAvailableToAllResidents = true, IReadOnlyCollection<string>? eligibleUserIds = null)
    {
        if (potId == Guid.Empty)
        {
            return HouseholdTaskChangeResult.Failure("pot_required", "Selecione um pote.");
        }

        var validation = Validate(name, description, kind, recurrenceDays, difficulty, isAvailableToAllResidents, eligibleUserIds);
        if (!validation.Succeeded)
        {
            return validation;
        }

        PotId = potId;
        Apply(name, description, kind, recurrenceDays, difficulty);
        ApplyEligibility(isAvailableToAllResidents, eligibleUserIds);
        UpdatedAt = now;
        return HouseholdTaskChangeResult.Success(this);
    }

    public void SetActive(bool isActive, DateTimeOffset now)
    {
        IsActive = isActive;
        UpdatedAt = now;
    }

    public void RegisterCompletion(DateTimeOffset completedAt)
    {
        NextAvailableAt = Kind == HouseholdTaskKind.Recurring
            ? completedAt.AddDays(RecurrenceDays!.Value)
            : null;

        if (Kind == HouseholdTaskKind.OneTime)
        {
            IsActive = false;
        }

        UpdatedAt = completedAt;
    }

    public static string NormalizeName(string name) => name.Trim().ToUpperInvariant();

    private static HouseholdTaskChangeResult Validate(string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, HouseholdTaskDifficulty difficulty, bool isAvailableToAllResidents, IReadOnlyCollection<string>? eligibleUserIds)
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

        if (!Enum.IsDefined(difficulty))
        {
            return HouseholdTaskChangeResult.Failure("task_difficulty_invalid", "Selecione uma dificuldade válida.");
        }

        var selectedUsers = eligibleUserIds?.Where(userId => !string.IsNullOrWhiteSpace(userId)).Distinct(StringComparer.Ordinal).ToArray() ?? [];
        if (!isAvailableToAllResidents && selectedUsers.Length == 0)
        {
            return HouseholdTaskChangeResult.Failure("task_eligible_users_required", "Selecione pelo menos uma pessoa que pode pegar a tarefa.");
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

    private void Apply(string name, string? description, HouseholdTaskKind kind, int? recurrenceDays, HouseholdTaskDifficulty difficulty)
    {
        Name = name.Trim();
        NormalizedName = NormalizeName(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Kind = kind;
        Difficulty = difficulty;
        RecurrenceDays = recurrenceDays;
    }

    private void ApplyEligibility(bool isAvailableToAllResidents, IReadOnlyCollection<string>? eligibleUserIds)
    {
        IsAvailableToAllResidents = isAvailableToAllResidents;
        eligibleUsers.Clear();
        if (isAvailableToAllResidents)
        {
            return;
        }

        foreach (var userId in eligibleUserIds!.Where(userId => !string.IsNullOrWhiteSpace(userId)).Distinct(StringComparer.Ordinal))
        {
            eligibleUsers.Add(new HouseholdTaskEligibleUser(Id, ResidenceId, userId));
        }
    }
}

public sealed record HouseholdTaskChangeResult(bool Succeeded, HouseholdTask? Task, string? Code, string? Message)
{
    public static HouseholdTaskChangeResult Success(HouseholdTask? task) => new(true, task, null, null);
    public static HouseholdTaskChangeResult Failure(string code, string message) => new(false, null, code, message);
}
