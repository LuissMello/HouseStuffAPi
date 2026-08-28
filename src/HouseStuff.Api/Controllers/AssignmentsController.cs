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
        ToActionResult(await assignments.DrawAsync(new DrawTaskCommand(request.PotId, request.ExcludedTaskIds ?? [], request.Difficulty), cancellationToken));

    private ObjectResult ToActionResult<T>(AssignmentResult<T> result) => result.Succeeded
        ? StatusCode(StatusCodes.Status200OK, result.Value)
        : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
}

[ApiController]
[Authorize]
[Route("api/v1/assignments")]
public sealed class AssignmentsController(ITaskAssignmentService assignments) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ObjectResult> Current(CancellationToken cancellationToken) =>
        ToActionResult(await assignments.GetCurrentAsync(cancellationToken), StatusCodes.Status200OK);

    [HttpPost("accept")]
    public async Task<ObjectResult> Accept(AcceptTaskRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await assignments.AcceptAsync(request.TaskId, cancellationToken), StatusCodes.Status201Created);

    [HttpPost("current/complete")]
    public async Task<ObjectResult> CompleteCurrent(CancellationToken cancellationToken) =>
        ToActionResult(await assignments.CompleteCurrentAsync(cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(AssignmentResult<T> result, int successStatus) => result.Succeeded
        ? StatusCode(successStatus, result.Value)
        : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
}

public sealed record DrawTaskRequest(Guid PotId, IReadOnlyCollection<Guid>? ExcludedTaskIds, string? Difficulty = null);
public sealed record AcceptTaskRequest(Guid TaskId);
