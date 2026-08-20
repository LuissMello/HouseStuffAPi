using HouseStuff.Domain.Assignments;

namespace HouseStuff.Domain.UnitTests;

public sealed class TaskAssignmentTests
{
    [Fact]
    public void CreatesActiveAssignmentForIdentifiedUser()
    {
        var acceptedAt = DateTimeOffset.UtcNow;

        var result = TaskAssignment.Create(Guid.NewGuid(), "user-1", acceptedAt);

        Assert.True(result.Succeeded);
        Assert.Equal("user-1", result.Assignment!.AssignedToUserId);
        Assert.Equal(acceptedAt, result.Assignment.AcceptedAt);
        Assert.Null(result.Assignment.CompletedAt);
    }

    [Fact]
    public void RejectsMissingTask()
    {
        var result = TaskAssignment.Create(Guid.Empty, "user-1", DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("task_required", result.Code);
    }

    [Fact]
    public void RejectsMissingUser()
    {
        var result = TaskAssignment.Create(Guid.NewGuid(), " ", DateTimeOffset.UtcNow);

        Assert.False(result.Succeeded);
        Assert.Equal("user_required", result.Code);
    }

    [Fact]
    public void CompletesActiveAssignmentOnce()
    {
        var acceptedAt = DateTimeOffset.UtcNow;
        var assignment = TaskAssignment.Create(Guid.NewGuid(), "user-1", acceptedAt).Assignment!;
        var completedAt = acceptedAt.AddMinutes(10);

        var result = assignment.Complete(completedAt);
        var repeated = assignment.Complete(completedAt.AddMinutes(1));

        Assert.True(result.Succeeded);
        Assert.Equal(completedAt, assignment.CompletedAt);
        Assert.False(repeated.Succeeded);
        Assert.Equal("assignment_already_completed", repeated.Code);
    }

    [Fact]
    public void RejectsCompletionBeforeAcceptance()
    {
        var acceptedAt = DateTimeOffset.UtcNow;
        var assignment = TaskAssignment.Create(Guid.NewGuid(), "user-1", acceptedAt).Assignment!;

        var result = assignment.Complete(acceptedAt.AddSeconds(-1));

        Assert.False(result.Succeeded);
        Assert.Equal("completion_before_acceptance", result.Code);
        Assert.Null(assignment.CompletedAt);
    }
}
