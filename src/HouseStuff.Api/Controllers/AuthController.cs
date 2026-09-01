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
        var result = await users.SignInWithTokenAsync(request.Email, request.Password, cancellationToken);
        return result.Succeeded
            ? new EmptyResult()
            : this.ProblemWithCode(StatusCodes.Status401Unauthorized, result.Message, result.Code);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await users.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return result.Succeeded
            ? new EmptyResult()
            : this.ProblemWithCode(StatusCodes.Status401Unauthorized, result.Message, result.Code);
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

    [Authorize]
    [HttpPatch("me/color")]
    public async Task<IActionResult> UpdateColor(UpdateProfileColorRequest request, CancellationToken cancellationToken)
    {
        var result = await users.UpdateProfileColorAsync(request.ProfileColor, cancellationToken);
        return result.Succeeded
            ? Ok(result.Value)
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }
}

public sealed record LoginRequest(string Email, string Password, bool RememberMe);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record UpdateProfileColorRequest(string ProfileColor);
