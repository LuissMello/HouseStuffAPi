using HouseStuff.Application.Routine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/routine")]
public sealed class RoutineController(IRoutineOverviewService routine) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> Get(CancellationToken cancellationToken)
    {
        var result = await routine.GetAsync(cancellationToken);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status200OK, result.Value)
            : Problem(statusCode: StatusCodes.Status400BadRequest, title: result.Message, extensions: new Dictionary<string, object?> { ["code"] = result.Code });
    }
}
