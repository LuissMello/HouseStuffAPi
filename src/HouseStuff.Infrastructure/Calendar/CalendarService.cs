using HouseStuff.Application.Calendar;
using HouseStuff.Application.Pots;
using HouseStuff.Domain.Calendar;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Calendar;

internal sealed class CalendarService(HouseStuffDbContext database, ICurrentResidenceContext residenceContext) : ICalendarService
{
    public async Task<CalendarResult<CalendarEventView>> CreateAsync(SaveCalendarEventCommand command, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return CalendarResult.Failure<CalendarEventView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var validation = await ValidateCommandAsync(residenceId.Value, command, cancellationToken);
        if (!validation.Succeeded)
        {
            return CalendarResult.Failure<CalendarEventView>(validation.Code!, validation.Message!);
        }

        var creation = CalendarEvent.Create(residenceId.Value, command.Title, command.Description, validation.Kind!.Value, command.AppliesToAll,
            command.AllDayDate, command.StartsAt, command.EndsAt, command.ParticipantUserIds ?? [], DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return CalendarResult.Failure<CalendarEventView>(creation.Code!, creation.Message!);
        }

        database.CalendarEvents.Add(creation.Event!);
        await database.SaveChangesAsync(cancellationToken);
        return CalendarResult.Success(await ToEventViewAsync(creation.Event!, cancellationToken));
    }

    public async Task<CalendarResult<CalendarEventView>> UpdateAsync(Guid id, SaveCalendarEventCommand command, CancellationToken cancellationToken)
    {
        var scoped = await GetEventAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return CalendarResult.Failure<CalendarEventView>(scoped.Code!, scoped.Message!);
        }

        var validation = await ValidateCommandAsync(scoped.Value!.ResidenceId, command, cancellationToken);
        if (!validation.Succeeded)
        {
            return CalendarResult.Failure<CalendarEventView>(validation.Code!, validation.Message!);
        }

        var update = scoped.Value.Update(command.Title, command.Description, validation.Kind!.Value, command.AppliesToAll,
            command.AllDayDate, command.StartsAt, command.EndsAt, command.ParticipantUserIds ?? [], DateTimeOffset.UtcNow);
        if (!update.Succeeded)
        {
            return CalendarResult.Failure<CalendarEventView>(update.Code!, update.Message!);
        }

        await database.SaveChangesAsync(cancellationToken);
        return CalendarResult.Success(await ToEventViewAsync(scoped.Value, cancellationToken));
    }

    public async Task<CalendarResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var scoped = await GetEventAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return CalendarResult.Failure<bool>(scoped.Code!, scoped.Message!);
        }

        database.CalendarEvents.Remove(scoped.Value!);
        await database.SaveChangesAsync(cancellationToken);
        return CalendarResult.Success(true);
    }

    public async Task<CalendarResult<CalendarRangeView>> GetRangeAsync(
        DateOnly fromDate,
        DateOnly toDate,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return CalendarResult.Failure<CalendarRangeView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        if (toDate <= fromDate || toDate.DayNumber - fromDate.DayNumber > 400 || toUtc <= fromUtc)
        {
            return CalendarResult.Failure<CalendarRangeView>("calendar_range_invalid", "Informe um intervalo válido de até 400 dias.");
        }

        var normalizedFromUtc = fromUtc.ToUniversalTime();
        var normalizedToUtc = toUtc.ToUniversalTime();
        var events = await database.CalendarEvents
            .Include(calendarEvent => calendarEvent.Participants)
            .Where(calendarEvent => calendarEvent.ResidenceId == residenceId &&
                (calendarEvent.Kind == CalendarEventKind.Birthday ||
                 (calendarEvent.AllDayDate >= fromDate && calendarEvent.AllDayDate < toDate) ||
                 (calendarEvent.StartsAt < normalizedToUtc &&
                  ((calendarEvent.EndsAt != null && calendarEvent.EndsAt > normalizedFromUtc) ||
                   (calendarEvent.EndsAt == null && calendarEvent.StartsAt >= normalizedFromUtc)))))
            .ToListAsync(cancellationToken);
        var memberRows = await database.Users.Where(user => user.ResidenceId == residenceId)
            .Select(user => new { user.Id, user.Name, user.ProfileColor })
            .ToListAsync(cancellationToken);
        var members = memberRows.ToDictionary(user => user.Id, user => new MemberIdentity(user.Name, user.ProfileColor));
        var entries = new List<CalendarEntryView>();

        foreach (var calendarEvent in events)
        {
            var participants = ToParticipants(calendarEvent, members);
            if (calendarEvent.Kind == CalendarEventKind.Birthday)
            {
                AddBirthdayOccurrences(entries, calendarEvent, participants, fromDate, toDate);
            }
            else
            {
                entries.Add(ToEntry(calendarEvent, participants, calendarEvent.AllDayDate));
            }
        }

        var recurringTasks = await (from task in database.HouseholdTasks
                                    join pot in database.Pots on task.PotId equals pot.Id
                                    where task.ResidenceId == residenceId
                                        && task.Kind == HouseholdTaskKind.Recurring
                                        && task.IsActive
                                        && pot.IsActive
                                        && task.NextAvailableAt >= normalizedFromUtc
                                        && task.NextAvailableAt < normalizedToUtc
                                    select new { task.Id, task.Name, task.Description, task.NextAvailableAt, PotName = pot.Name })
            .ToListAsync(cancellationToken);
        entries.AddRange(recurringTasks.Select(task => new CalendarEntryView(
            $"task-{task.Id}", null, "task", "recurringTask", task.Name, task.Description, null, null,
            task.NextAvailableAt, null, true, [])));

        var ordered = entries.OrderBy(entry => entry.Date?.DayNumber ?? int.MaxValue)
            .ThenBy(entry => entry.StartsAt ?? DateTimeOffset.MaxValue)
            .ThenBy(entry => entry.Title)
            .ToList();
        return CalendarResult.Success(new CalendarRangeView(fromDate, toDate, ordered));
    }

    private async Task<CalendarResult<CalendarEvent>> GetEventAsync(Guid id, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return CalendarResult.Failure<CalendarEvent>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var calendarEvent = await database.CalendarEvents.Include(item => item.Participants)
            .SingleOrDefaultAsync(item => item.Id == id && item.ResidenceId == residenceId, cancellationToken);
        return calendarEvent is null
            ? CalendarResult.Failure<CalendarEvent>("calendar_event_not_found", "Evento não encontrado na sua casa.")
            : CalendarResult.Success(calendarEvent);
    }

    private async Task<CommandValidation> ValidateCommandAsync(Guid residenceId, SaveCalendarEventCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<CalendarEventKind>(command.Kind, true, out var kind))
        {
            return CommandValidation.Failure("calendar_kind_invalid", "Escolha data, aniversário ou compromisso.");
        }

        var participantIds = (command.ParticipantUserIds ?? []).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToList();
        if (!command.AppliesToAll)
        {
            var validCount = await database.Users.CountAsync(user => user.ResidenceId == residenceId && participantIds.Contains(user.Id), cancellationToken);
            if (validCount != participantIds.Count)
            {
                return CommandValidation.Failure("calendar_participant_not_found", "Um dos moradores selecionados não pertence à sua casa.");
            }
        }

        return CommandValidation.Success(kind);
    }

    private async Task<CalendarEventView> ToEventViewAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken)
    {
        var memberRows = await database.Users.Where(user => user.ResidenceId == calendarEvent.ResidenceId)
            .Select(user => new { user.Id, user.Name, user.ProfileColor })
            .ToListAsync(cancellationToken);
        var members = memberRows.ToDictionary(user => user.Id, user => new MemberIdentity(user.Name, user.ProfileColor));
        return new CalendarEventView(calendarEvent.Id, calendarEvent.Title, calendarEvent.Description, ToKind(calendarEvent.Kind), calendarEvent.AppliesToAll,
            calendarEvent.AllDayDate, calendarEvent.StartsAt, calendarEvent.EndsAt, ToParticipants(calendarEvent, members));
    }

    private static List<CalendarParticipantView> ToParticipants(CalendarEvent calendarEvent, Dictionary<string, MemberIdentity> members) =>
        calendarEvent.Participants.Where(participant => members.ContainsKey(participant.UserId))
            .Select(participant => new CalendarParticipantView(participant.UserId, members[participant.UserId].Name, members[participant.UserId].ProfileColor))
            .OrderBy(participant => participant.Name).ToList();

    private static CalendarEntryView ToEntry(CalendarEvent calendarEvent, IReadOnlyList<CalendarParticipantView> participants, DateOnly? date) =>
        new(calendarEvent.Id.ToString(), calendarEvent.Id, "event", ToKind(calendarEvent.Kind), calendarEvent.Title, calendarEvent.Description,
            date, calendarEvent.AllDayDate, calendarEvent.StartsAt, calendarEvent.EndsAt, calendarEvent.AppliesToAll, participants);

    private static void AddBirthdayOccurrences(
        List<CalendarEntryView> entries,
        CalendarEvent calendarEvent,
        IReadOnlyList<CalendarParticipantView> participants,
        DateOnly fromDate,
        DateOnly toDate)
    {
        var definition = calendarEvent.AllDayDate!.Value;
        for (var year = fromDate.Year; year <= toDate.Year; year++)
        {
            DateOnly occurrence;
            try
            {
                occurrence = new DateOnly(year, definition.Month, definition.Day);
            }
            catch (ArgumentOutOfRangeException)
            {
                continue;
            }

            if (occurrence >= fromDate && occurrence < toDate)
            {
                entries.Add(ToEntry(calendarEvent, participants, occurrence) with { EntryId = $"{calendarEvent.Id}-{year}" });
            }
        }
    }

    private static string ToKind(CalendarEventKind kind) => char.ToLowerInvariant(kind.ToString()[0]) + kind.ToString()[1..];

    private sealed record MemberIdentity(string Name, string ProfileColor);

    private sealed record CommandValidation(bool Succeeded, CalendarEventKind? Kind, string? Code, string? Message)
    {
        public static CommandValidation Success(CalendarEventKind kind) => new(true, kind, null, null);
        public static CommandValidation Failure(string code, string message) => new(false, null, code, message);
    }
}
