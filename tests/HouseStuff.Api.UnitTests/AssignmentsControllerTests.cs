using HouseStuff.Api.Controllers;
using HouseStuff.Application.Assignments;

namespace HouseStuff.Api.UnitTests;

public sealed class AssignmentsControllerTests
{
    [Fact]
    public async Task DrawReturnsProposalWithoutCreatingAssignment()
    {
        var expected = new DrawProposalView(Guid.NewGuid(), Guid.NewGuid(), "Mensal", "Limpar geladeira", null, "recurring", 30);
        var service = new StubAssignmentService { DrawResult = AssignmentResult.Success(expected) };

        var result = await new DrawsController(service).Draw(new DrawTaskRequest(expected.PotId, []), CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
        Assert.False(service.AcceptCalled);
    }

    [Fact]
    public async Task AcceptCreatesAssignment()
    {
        var expected = new ActiveAssignmentView(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Diário", "Lavar louça", null, "reusable", null, DateTimeOffset.UtcNow);
        var service = new StubAssignmentService { AcceptResult = AssignmentResult.Success(expected) };

        var result = await new AssignmentsController(service).Accept(new AcceptTaskRequest(expected.TaskId), CancellationToken.None);

        Assert.Equal(201, result.StatusCode);
        Assert.Same(expected, result.Value);
        Assert.True(service.AcceptCalled);
    }

    [Fact]
    public async Task CurrentReturnsNoActiveAssignmentAsNull()
    {
        var service = new StubAssignmentService { CurrentResult = AssignmentResult.Success<ActiveAssignmentView?>(null) };

        var result = await new AssignmentsController(service).Current(CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.Value);
    }

    private sealed class StubAssignmentService : ITaskAssignmentService
    {
        public bool AcceptCalled { get; private set; }
        public AssignmentResult<ActiveAssignmentView?> CurrentResult { get; init; } = AssignmentResult.Success<ActiveAssignmentView?>(null);
        public AssignmentResult<DrawProposalView> DrawResult { get; init; } = AssignmentResult.Failure<DrawProposalView>("missing", "missing");
        public AssignmentResult<ActiveAssignmentView> AcceptResult { get; init; } = AssignmentResult.Failure<ActiveAssignmentView>("missing", "missing");

        public Task<AssignmentResult<ActiveAssignmentView?>> GetCurrentAsync(CancellationToken cancellationToken) => Task.FromResult(CurrentResult);
        public Task<AssignmentResult<DrawProposalView>> DrawAsync(DrawTaskCommand command, CancellationToken cancellationToken) => Task.FromResult(DrawResult);
        public Task<AssignmentResult<ActiveAssignmentView>> AcceptAsync(Guid taskId, CancellationToken cancellationToken)
        {
            AcceptCalled = true;
            return Task.FromResult(AcceptResult);
        }
    }
}
