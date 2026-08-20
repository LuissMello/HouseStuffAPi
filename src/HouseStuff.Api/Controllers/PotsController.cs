using HouseStuff.Application.Identity;
using HouseStuff.Application.Pots;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pots")]
public sealed class PotsController(IPotService pots) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> List(CancellationToken cancellationToken) =>
        ToActionResult(await pots.ListAsync(includeArchived: false, cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(PotResult<T> result, int successStatus) => result.Succeeded
        ? StatusCode(successStatus, result.Value)
        : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Message, extensions: new Dictionary<string, object?> { ["code"] = result.Code });
}

[ApiController]
[Authorize(Roles = HouseStuffRoles.Administrator)]
[Route("api/v1/admin/pots")]
public sealed class AdminPotsController(IPotService pots) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> List(CancellationToken cancellationToken) =>
        ToActionResult(await pots.ListAsync(includeArchived: true, cancellationToken), StatusCodes.Status200OK);

    [HttpPost]
    public async Task<ObjectResult> Create(SavePotRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await pots.CreateAsync(new SavePotCommand(request.Name, request.Description), cancellationToken), StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    public async Task<ObjectResult> Update(Guid id, SavePotRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await pots.UpdateAsync(id, new SavePotCommand(request.Name, request.Description), cancellationToken), StatusCodes.Status200OK);

    [HttpPatch("{id:guid}/status")]
    public async Task<ObjectResult> SetStatus(Guid id, SetPotStatusRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await pots.SetActiveAsync(id, request.IsActive, cancellationToken), StatusCodes.Status200OK);

    [HttpPost("{id:guid}/move")]
    public async Task<ObjectResult> Move(Guid id, MovePotRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await pots.MoveAsync(id, request.Offset, cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(PotResult<T> result, int successStatus)
    {
        if (result.Succeeded)
        {
            return StatusCode(successStatus, result.Value);
        }

        var status = result.Code == "pot_not_found" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Message, extensions: new Dictionary<string, object?> { ["code"] = result.Code });
    }
}

public sealed record SavePotRequest(string Name, string? Description);
public sealed record SetPotStatusRequest(bool IsActive);
public sealed record MovePotRequest(int Offset);
