using HouseStuff.Api.Controllers;
using HouseStuff.Application.Calendar;
using Microsoft.AspNetCore.Authorization;

namespace HouseStuff.Api.UnitTests;

public sealed class CalendarControllerTests
{
    [Fact]
    public async Task ResidentCanCreateEvent()
    {
        var calendarEvent = new CalendarEventView(Guid.NewGuid(), "Dentista", null, "appointment", true, null, DateTimeOffset.UtcNow, null, []);
        var service = new StubCalendarService { EventResult = CalendarResult.Success(calendarEvent) };
        var request = new SaveCalendarEventRequest(calendarEvent.Title, null, calendarEvent.Kind, true, null, calendarEvent.StartsAt, null, []);

        var result = await new CalendarController(service).Create(request, CancellationToken.None);

        Assert.Equal(201, result.StatusCode);
        Assert.Same(calendarEvent, result.Value);
    }

    [Fact]
    public void CalendarRequiresAuthenticationWithoutAdministratorRole()
    {
        var authorization = Assert.Single(typeof(CalendarController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());

        Assert.Null(authorization.Roles);
    }

    [Fact]
    public async Task MissingEventReturnsNotFound()
    {
        var service = new StubCalendarService
        {
            BooleanResult = CalendarResult.Failure<bool>("calendar_event_not_found", "Evento não encontrado."),
        };

        var result = await new CalendarController(service).Delete(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
    }

    private sealed class StubCalendarService : ICalendarService
    {
        public CalendarResult<CalendarEventView> EventResult { get; init; } = CalendarResult.Failure<CalendarEventView>("missing", "missing");
        public CalendarResult<bool> BooleanResult { get; init; } = CalendarResult.Success(true);
        public CalendarResult<CalendarRangeView> RangeResult { get; init; } = CalendarResult.Success(new CalendarRangeView(default, default, []));

        public Task<CalendarResult<CalendarEventView>> CreateAsync(SaveCalendarEventCommand command, CancellationToken cancellationToken) => Task.FromResult(EventResult);
        public Task<CalendarResult<CalendarEventView>> UpdateAsync(Guid id, SaveCalendarEventCommand command, CancellationToken cancellationToken) => Task.FromResult(EventResult);
        public Task<CalendarResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(BooleanResult);
        public Task<CalendarResult<CalendarRangeView>> GetRangeAsync(DateOnly fromDate, DateOnly toDate, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken) => Task.FromResult(RangeResult);
    }
}
