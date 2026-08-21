namespace HouseStuff.Application.Routine;

public sealed record RoutineOverviewView(
    DateTimeOffset GeneratedAt,
    IReadOnlyList<UpcomingRecurringTaskView> Upcoming,
    IReadOnlyList<CompletionHistoryItemView> History);

public sealed record UpcomingRecurringTaskView(
    Guid TaskId,
    Guid PotId,
    string PotName,
    string TaskName,
    string? Description,
    DateTimeOffset NextAvailableAt);

public sealed record CompletionHistoryItemView(
    Guid AssignmentId,
    Guid TaskId,
    string PotName,
    string TaskName,
    string Kind,
    DateTimeOffset AcceptedAt,
    DateTimeOffset CompletedAt);

public sealed record RoutineOverviewResult(bool Succeeded, RoutineOverviewView? Value, string? Code, string? Message)
{
    public static RoutineOverviewResult Success(RoutineOverviewView value) => new(true, value, null, null);
    public static RoutineOverviewResult Failure(string code, string message) => new(false, null, code, message);
}

public interface IRoutineOverviewService
{
    Task<RoutineOverviewResult> GetAsync(CancellationToken cancellationToken);
}
