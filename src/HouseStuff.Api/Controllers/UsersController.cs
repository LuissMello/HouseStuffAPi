using HouseStuff.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize(Roles = HouseStuffRoles.Administrator)]
[Route("api/v1/admin/users")]
public sealed class UsersController(IUserAccessService users) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await users.ListAsync(cancellationToken));

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await users.CreateAsync(
            new CreateUserCommand(request.Email, request.Name, request.TemporaryPassword, request.IsAdministrator),
            cancellationToken);
        return result.Succeeded
            ? Created($"/api/v1/admin/users/{result.Value!.Id}", result.Value)
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }

    [HttpPatch("{userId}/role")]
    public async Task<IActionResult> ChangeRole(string userId, ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await users.ChangeRoleAsync(new ChangeUserRoleCommand(userId, request.IsAdministrator), cancellationToken);
        return result.Succeeded
            ? Ok(result.Value)
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }
}

public sealed record CreateUserRequest(string Email, string Name, string TemporaryPassword, bool IsAdministrator);

public sealed record ChangeUserRoleRequest(bool IsAdministrator);
