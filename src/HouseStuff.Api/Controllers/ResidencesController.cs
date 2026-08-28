using HouseStuff.Application.Identity;
using HouseStuff.Application.Residences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/residences")]
public sealed class ResidencesController(IResidenceService residences) : ControllerBase
{
    [HttpGet("current")]
    public async Task<IActionResult> Current(CancellationToken cancellationToken) =>
        ToActionResult(await residences.GetCurrentAsync(cancellationToken), StatusCodes.Status200OK);

    [Authorize(Roles = HouseStuffRoles.Administrator)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateResidenceRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await residences.CreateAsync(request.Name, cancellationToken), StatusCodes.Status201Created);

    [Authorize(Roles = HouseStuffRoles.Administrator)]
    [HttpPost("current/members/{userId}")]
    public async Task<IActionResult> AddMember(string userId, CancellationToken cancellationToken) =>
        ToActionResult(await residences.AddMemberAsync(userId, cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult(ResidenceResult<ResidenceView> result, int successStatus)
    {
        if (result.Succeeded)
        {
            return StatusCode(successStatus, result.Value);
        }

        var status = result.Code == "residence_not_found" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        return this.ProblemWithCode(status, result.Message, result.Code);
    }
}

public sealed record CreateResidenceRequest(string Name);
