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
            new CreateUserCommand(request.Email, request.Name, request.TemporaryPassword, request.IsAdministrator, request.HasLogin),
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

    [HttpPatch("{userId}/login")]
    public async Task<IActionResult> LinkLogin(string userId, LinkLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await users.LinkLoginAsync(userId, request.Email, request.TemporaryPassword, cancellationToken);
        return result.Succeeded
            ? Ok(result.Value)
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }

    [HttpPatch("{userId}/color")]
    public async Task<IActionResult> UpdateColor(string userId, UpdateMemberProfileColorRequest request, CancellationToken cancellationToken)
    {
        var result = await users.UpdateMemberProfileColorAsync(userId, request.ProfileColor, cancellationToken);
        return result.Succeeded
            ? Ok(result.Value)
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }
}

public sealed record CreateUserRequest(string? Email, string Name, string? TemporaryPassword, bool IsAdministrator, bool HasLogin = true);

public sealed record ChangeUserRoleRequest(bool IsAdministrator);

public sealed record LinkLoginRequest(string Email, string TemporaryPassword);

public sealed record UpdateMemberProfileColorRequest(string ProfileColor);
