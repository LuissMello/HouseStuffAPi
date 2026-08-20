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
}
