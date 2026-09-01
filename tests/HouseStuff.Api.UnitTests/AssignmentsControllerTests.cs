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
    public async Task ActiveReturnsAllActiveAssignments()
    {
        var expected = new[]
        {
            new ActiveAssignmentView(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Semanal", "Varrer a casa", null, "reusable", null, DateTimeOffset.UtcNow),
            new ActiveAssignmentView(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Consertos", "Ajustar porta", null, "oneTime", null, DateTimeOffset.UtcNow),
        };
        var service = new StubAssignmentService { ActiveResult = AssignmentResult.Success<IReadOnlyList<ActiveAssignmentView>>(expected) };

        var result = await new AssignmentsController(service).Active(CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public async Task CompleteReturnsRecordedCompletion()
    {
        var expected = new CompletedAssignmentView(Guid.NewGuid(), Guid.NewGuid(), "Lavar louça", "reusable", DateTimeOffset.UtcNow, null, true);
        var service = new StubAssignmentService { CompleteResult = AssignmentResult.Success(expected) };

        var result = await new AssignmentsController(service).Complete(expected.AssignmentId, CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
        Assert.True(service.CompleteCalled);
        Assert.Equal(expected.AssignmentId, service.CompletedAssignmentId);
    }

    private sealed class StubAssignmentService : ITaskAssignmentService
    {
        public bool AcceptCalled { get; private set; }
        public bool CompleteCalled { get; private set; }
        public Guid? CompletedAssignmentId { get; private set; }
        public AssignmentResult<IReadOnlyList<ActiveAssignmentView>> ActiveResult { get; init; } = AssignmentResult.Success<IReadOnlyList<ActiveAssignmentView>>([]);
        public AssignmentResult<DrawProposalView> DrawResult { get; init; } = AssignmentResult.Failure<DrawProposalView>("missing", "missing");
        public AssignmentResult<ActiveAssignmentView> AcceptResult { get; init; } = AssignmentResult.Failure<ActiveAssignmentView>("missing", "missing");
        public AssignmentResult<CompletedAssignmentView> CompleteResult { get; init; } = AssignmentResult.Failure<CompletedAssignmentView>("missing", "missing");

        public Task<AssignmentResult<IReadOnlyList<ActiveAssignmentView>>> GetActiveAsync(CancellationToken cancellationToken) => Task.FromResult(ActiveResult);
        public Task<AssignmentResult<DrawProposalView>> DrawAsync(DrawTaskCommand command, CancellationToken cancellationToken) => Task.FromResult(DrawResult);
        public Task<AssignmentResult<ActiveAssignmentView>> AcceptAsync(Guid taskId, CancellationToken cancellationToken)
        {
            AcceptCalled = true;
            return Task.FromResult(AcceptResult);
        }

        public Task<AssignmentResult<CompletedAssignmentView>> CompleteAsync(Guid assignmentId, CancellationToken cancellationToken)
        {
            CompleteCalled = true;
            CompletedAssignmentId = assignmentId;
            return Task.FromResult(CompleteResult);
        }
    }
}
