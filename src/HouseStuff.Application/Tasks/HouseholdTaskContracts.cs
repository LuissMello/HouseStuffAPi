namespace HouseStuff.Application.Tasks;

public sealed record HouseholdTaskView(
    Guid Id,
    Guid PotId,
    string PotName,
    string Name,
    string? Description,
    string Kind,
    int? RecurrenceDays,
    bool IsActive,
    string Difficulty = "medium",
    bool IsAvailableToAllResidents = true,
    IReadOnlyList<string>? EligibleUserIds = null);

public sealed record SaveHouseholdTaskCommand(
    Guid PotId,
    string Name,
    string? Description,
    string Kind,
    int? RecurrenceDays,
    string? Difficulty = null,
    bool? IsAvailableToAllResidents = null,
    IReadOnlyCollection<string>? EligibleUserIds = null);

public sealed record HouseholdTaskResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class HouseholdTaskResult
{
    public static HouseholdTaskResult<T> Success<T>(T value) => new(true, value, null, null);
    public static HouseholdTaskResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface IHouseholdTaskService
{
    Task<HouseholdTaskResult<IReadOnlyList<HouseholdTaskView>>> ListAsync(Guid? potId, bool includeArchived, CancellationToken cancellationToken);
    Task<HouseholdTaskResult<HouseholdTaskView>> CreateAsync(SaveHouseholdTaskCommand command, CancellationToken cancellationToken);
    Task<HouseholdTaskResult<HouseholdTaskView>> UpdateAsync(Guid id, SaveHouseholdTaskCommand command, CancellationToken cancellationToken);
    Task<HouseholdTaskResult<HouseholdTaskView>> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
}
