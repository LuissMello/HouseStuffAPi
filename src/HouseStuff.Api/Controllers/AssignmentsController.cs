using HouseStuff.Application.Assignments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/draws")]
public sealed class DrawsController(ITaskAssignmentService assignments) : ControllerBase
{
    [HttpPost]
    public async Task<ObjectResult> Draw(DrawTaskRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await assignments.DrawAsync(new DrawTaskCommand(request.PotId, request.ExcludedTaskIds ?? [], request.Difficulty, request.OnBehalfOfUserId), cancellationToken));

    private ObjectResult ToActionResult<T>(AssignmentResult<T> result) => result.Succeeded
        ? StatusCode(StatusCodes.Status200OK, result.Value)
        : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
}

[ApiController]
[Authorize]
[Route("api/v1/assignments")]
public sealed class AssignmentsController(ITaskAssignmentService assignments) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> Active(CancellationToken cancellationToken) =>
        ToActionResult(await assignments.GetActiveAsync(cancellationToken), StatusCodes.Status200OK);

    [HttpPost("accept")]
    public async Task<ObjectResult> Accept(AcceptTaskRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await assignments.AcceptAsync(request.TaskId, request.OnBehalfOfUserId, cancellationToken), StatusCodes.Status201Created);

    [HttpPost("{assignmentId:guid}/complete")]
    public async Task<ObjectResult> Complete(Guid assignmentId, CompleteTaskRequest? request, CancellationToken cancellationToken) =>
        ToActionResult(await assignments.CompleteAsync(assignmentId, request?.OnBehalfOfUserId, cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(AssignmentResult<T> result, int successStatus) => result.Succeeded
        ? StatusCode(successStatus, result.Value)
        : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
}

public sealed record DrawTaskRequest(Guid PotId, IReadOnlyCollection<Guid>? ExcludedTaskIds, string? Difficulty = null, string? OnBehalfOfUserId = null);
public sealed record AcceptTaskRequest(Guid TaskId, string? OnBehalfOfUserId = null);
public sealed record CompleteTaskRequest(string? OnBehalfOfUserId = null);
