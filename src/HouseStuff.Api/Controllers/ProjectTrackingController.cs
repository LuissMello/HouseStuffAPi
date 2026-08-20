using HouseStuff.Api.ProjectTracking;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Route("api/v1/project-tracking")]
public sealed class ProjectTrackingController(IProjectTrackingReader reader) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ProjectTrackingDocument>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProjectTrackingDocument>> GetAsync(CancellationToken cancellationToken) =>
        Ok(await reader.ReadAsync(cancellationToken));
}
