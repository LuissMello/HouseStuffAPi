using HouseStuff.Application.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IUserAccessService users) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await users.SignInAsync(request.Email, request.Password, request.RememberMe, cancellationToken);
        return result.Succeeded
            ? Ok(result.Value)
            : Problem(statusCode: StatusCodes.Status401Unauthorized, title: result.Message, extensions: new Dictionary<string, object?> { ["code"] = result.Code });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await users.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var user = await users.GetCurrentAsync(cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}

public sealed record LoginRequest(string Email, string Password, bool RememberMe);
