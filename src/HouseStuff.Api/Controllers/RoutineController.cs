using HouseStuff.Application.Routine;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/routine")]
public sealed class RoutineController(IRoutineOverviewService routine, IHouseholdDashboardService dashboard) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> Get(CancellationToken cancellationToken)
    {
        var result = await routine.GetAsync(cancellationToken);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status200OK, result.Value)
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }

    [HttpGet("dashboard")]
    public async Task<ObjectResult> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await dashboard.GetAsync(cancellationToken);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status200OK, result.Value)
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }
}
