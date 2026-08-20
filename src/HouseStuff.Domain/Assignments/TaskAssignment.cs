namespace HouseStuff.Domain.Assignments;

public sealed class TaskAssignment
{
    private TaskAssignment() { }

    private TaskAssignment(Guid householdTaskId, string assignedToUserId, DateTimeOffset acceptedAt)
    {
        Id = Guid.NewGuid();
        HouseholdTaskId = householdTaskId;
        AssignedToUserId = assignedToUserId;
        AcceptedAt = acceptedAt;
    }

    public Guid Id { get; private set; }
    public Guid HouseholdTaskId { get; private set; }
    public string AssignedToUserId { get; private set; } = string.Empty;
    public DateTimeOffset AcceptedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static TaskAssignmentCreationResult Create(Guid householdTaskId, string assignedToUserId, DateTimeOffset acceptedAt)
    {
        if (householdTaskId == Guid.Empty)
        {
            return TaskAssignmentCreationResult.Failure("task_required", "Selecione uma tarefa válida.");
        }

        if (string.IsNullOrWhiteSpace(assignedToUserId))
        {
            return TaskAssignmentCreationResult.Failure("user_required", "Não foi possível identificar o usuário.");
        }

        return TaskAssignmentCreationResult.Success(new TaskAssignment(householdTaskId, assignedToUserId, acceptedAt));
    }

    public TaskAssignmentCompletionResult Complete(DateTimeOffset completedAt)
    {
        if (CompletedAt is not null)
        {
            return TaskAssignmentCompletionResult.Failure("assignment_already_completed", "Esta tarefa já foi concluída.");
        }

        if (completedAt < AcceptedAt)
        {
            return TaskAssignmentCompletionResult.Failure("completion_before_acceptance", "A conclusão não pode ser anterior ao aceite.");
        }

        CompletedAt = completedAt;
        return TaskAssignmentCompletionResult.Success(this);
    }
}

public sealed record TaskAssignmentCreationResult(bool Succeeded, TaskAssignment? Assignment, string? Code, string? Message)
{
    public static TaskAssignmentCreationResult Success(TaskAssignment assignment) => new(true, assignment, null, null);
    public static TaskAssignmentCreationResult Failure(string code, string message) => new(false, null, code, message);
}

public sealed record TaskAssignmentCompletionResult(bool Succeeded, TaskAssignment? Assignment, string? Code, string? Message)
{
    public static TaskAssignmentCompletionResult Success(TaskAssignment assignment) => new(true, assignment, null, null);
    public static TaskAssignmentCompletionResult Failure(string code, string message) => new(false, null, code, message);
}
