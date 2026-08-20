namespace HouseStuff.Application.Assignments;

public sealed record CurrentUserSession(string UserId, Guid ResidenceId);

public sealed record DrawProposalView(
    Guid TaskId,
    Guid PotId,
    string PotName,
    string TaskName,
    string? Description,
    string Kind,
    int? RecurrenceDays);

public sealed record ActiveAssignmentView(
    Guid AssignmentId,
    Guid TaskId,
    Guid PotId,
    string PotName,
    string TaskName,
    string? Description,
    string Kind,
    int? RecurrenceDays,
    DateTimeOffset AcceptedAt);

public sealed record DrawTaskCommand(Guid PotId, IReadOnlyCollection<Guid> ExcludedTaskIds);

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
    Task<AssignmentResult<ActiveAssignmentView?>> GetCurrentAsync(CancellationToken cancellationToken);
    Task<AssignmentResult<DrawProposalView>> DrawAsync(DrawTaskCommand command, CancellationToken cancellationToken);
    Task<AssignmentResult<ActiveAssignmentView>> AcceptAsync(Guid taskId, CancellationToken cancellationToken);
}
