namespace HouseStuff.Application.Assignments;

public sealed record CurrentUserSession(string UserId, Guid ResidenceId);

public sealed record DrawProposalView(
    Guid TaskId,
    Guid PotId,
    string PotName,
    string TaskName,
    string? Description,
    string Kind,
    int? RecurrenceDays,
    string Difficulty = "medium");

public sealed record ActiveAssignmentView(
    Guid AssignmentId,
    Guid TaskId,
    Guid PotId,
    string PotName,
    string TaskName,
    string? Description,
    string Kind,
    int? RecurrenceDays,
    DateTimeOffset AcceptedAt,
    string Difficulty = "medium");

public sealed record CompletedAssignmentView(
    Guid AssignmentId,
    Guid TaskId,
    string TaskName,
    string Kind,
    DateTimeOffset CompletedAt,
    DateTimeOffset? NextAvailableAt,
    bool ReturnsToPot);

public sealed record DrawTaskCommand(Guid PotId, IReadOnlyCollection<Guid> ExcludedTaskIds, string? Difficulty = null);

public sealed record AssignmentResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class AssignmentResult
{
    public static AssignmentResult<T> Success<T>(T value) => new(true, value, null, null);
    public static AssignmentResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface ICurrentUserContext
{
    Task<CurrentUserSession?> GetAsync(CancellationToken cancellationToken);
}

public interface ITaskAssignmentService
{
    Task<AssignmentResult<IReadOnlyList<ActiveAssignmentView>>> GetActiveAsync(CancellationToken cancellationToken);
    Task<AssignmentResult<DrawProposalView>> DrawAsync(DrawTaskCommand command, CancellationToken cancellationToken);
    Task<AssignmentResult<ActiveAssignmentView>> AcceptAsync(Guid taskId, CancellationToken cancellationToken);
    Task<AssignmentResult<CompletedAssignmentView>> CompleteAsync(Guid assignmentId, CancellationToken cancellationToken);
}
