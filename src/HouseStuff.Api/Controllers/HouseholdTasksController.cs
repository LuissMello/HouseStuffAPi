using HouseStuff.Application.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/admin/tasks")]
public sealed class HouseholdTasksController(IHouseholdTaskService tasks) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> List([FromQuery] Guid? potId, CancellationToken cancellationToken) =>
        ToActionResult(await tasks.ListAsync(potId, includeArchived: true, cancellationToken), StatusCodes.Status200OK);

    [HttpPost]
    public async Task<ObjectResult> Create(SaveHouseholdTaskRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await tasks.CreateAsync(request.ToCommand(), cancellationToken), StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    public async Task<ObjectResult> Update(Guid id, SaveHouseholdTaskRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await tasks.UpdateAsync(id, request.ToCommand(), cancellationToken), StatusCodes.Status200OK);

    [HttpPatch("{id:guid}/status")]
    public async Task<ObjectResult> SetStatus(Guid id, SetHouseholdTaskStatusRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await tasks.SetActiveAsync(id, request.IsActive, cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(HouseholdTaskResult<T> result, int successStatus)
    {
        if (result.Succeeded)
        {
            return StatusCode(successStatus, result.Value);
        }

        var status = result.Code is "task_not_found" or "pot_not_found" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Message, extensions: new Dictionary<string, object?> { ["code"] = result.Code });
    }
}

public sealed record SaveHouseholdTaskRequest(Guid PotId, string Name, string? Description, string Kind, int? RecurrenceDays)
{
    public SaveHouseholdTaskCommand ToCommand() => new(PotId, Name, Description, Kind, RecurrenceDays);
}

public sealed record SetHouseholdTaskStatusRequest(bool IsActive);
