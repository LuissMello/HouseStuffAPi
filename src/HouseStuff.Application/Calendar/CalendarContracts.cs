using HouseStuff.Application.Identity;

namespace HouseStuff.Application.Calendar;

public sealed record CalendarParticipantView(string UserId, string Name, string ProfileColor = ProfileColors.Default);
public sealed record CalendarEventView(
    Guid Id,
    string Title,
    string? Description,
    string Kind,
    bool AppliesToAll,
    DateOnly? AllDayDate,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    IReadOnlyList<CalendarParticipantView> Participants);

public sealed record SaveCalendarEventCommand(
    string Title,
    string? Description,
    string Kind,
    bool AppliesToAll,
    DateOnly? AllDayDate,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    IReadOnlyList<string> ParticipantUserIds);

public sealed record CalendarEntryView(
    string EntryId,
    Guid? EventId,
    string Source,
    string Kind,
    string Title,
    string? Description,
    DateOnly? Date,
    DateOnly? DefinitionDate,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool AppliesToAll,
    IReadOnlyList<CalendarParticipantView> Participants);

public sealed record CalendarRangeView(DateOnly FromDate, DateOnly ToDate, IReadOnlyList<CalendarEntryView> Entries);
public sealed record CalendarResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class CalendarResult
{
    public static CalendarResult<T> Success<T>(T value) => new(true, value, null, null);
    public static CalendarResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface ICalendarService
{
    Task<CalendarResult<CalendarEventView>> CreateAsync(SaveCalendarEventCommand command, CancellationToken cancellationToken);
    Task<CalendarResult<CalendarEventView>> UpdateAsync(Guid id, SaveCalendarEventCommand command, CancellationToken cancellationToken);
    Task<CalendarResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<CalendarResult<CalendarRangeView>> GetRangeAsync(DateOnly fromDate, DateOnly toDate, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken cancellationToken);
}
