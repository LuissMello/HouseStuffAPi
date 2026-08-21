using HouseStuff.Application.Calendar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/calendar")]
public sealed class CalendarController(ICalendarService calendar) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> GetRange(
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] DateTimeOffset fromUtc,
        [FromQuery] DateTimeOffset toUtc,
        CancellationToken cancellationToken) =>
        ToActionResult(await calendar.GetRangeAsync(fromDate, toDate, fromUtc, toUtc, cancellationToken), StatusCodes.Status200OK);

    [HttpPost("events")]
    public async Task<ObjectResult> Create(SaveCalendarEventRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await calendar.CreateAsync(request.ToCommand(), cancellationToken), StatusCodes.Status201Created);

    [HttpPut("events/{id:guid}")]
    public async Task<ObjectResult> Update(Guid id, SaveCalendarEventRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await calendar.UpdateAsync(id, request.ToCommand(), cancellationToken), StatusCodes.Status200OK);

    [HttpDelete("events/{id:guid}")]
    public async Task<ObjectResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await calendar.DeleteAsync(id, cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(CalendarResult<T> result, int successStatus)
    {
        if (result.Succeeded)
        {
            return StatusCode(successStatus, result.Value);
        }

        var status = result.Code == "calendar_event_not_found" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        return Problem(statusCode: status, title: result.Message, extensions: new Dictionary<string, object?> { ["code"] = result.Code });
    }
}

public sealed record SaveCalendarEventRequest(
    string Title,
    string? Description,
    string Kind,
    bool AppliesToAll,
    DateOnly? AllDayDate,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    IReadOnlyList<string> ParticipantUserIds)
{
    public SaveCalendarEventCommand ToCommand() => new(Title, Description, Kind, AppliesToAll, AllDayDate, StartsAt, EndsAt, ParticipantUserIds ?? []);
}
