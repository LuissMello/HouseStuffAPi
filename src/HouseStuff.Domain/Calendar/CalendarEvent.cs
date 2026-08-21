namespace HouseStuff.Domain.Calendar;

public enum CalendarEventKind
{
    Date,
    Birthday,
    Appointment,
}

public sealed class CalendarEvent
{
    private readonly List<CalendarEventParticipant> participants = [];

    private CalendarEvent(
        Guid id,
        Guid residenceId,
        string title,
        string? description,
        CalendarEventKind kind,
        bool appliesToAll,
        DateOnly? allDayDate,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt,
        IReadOnlyCollection<string> participantUserIds,
        DateTimeOffset now)
    {
        Id = id;
        ResidenceId = residenceId;
        Apply(title, description, kind, appliesToAll, allDayDate, startsAt, endsAt, participantUserIds, now);
        CreatedAt = now;
    }

    private CalendarEvent()
    {
        Title = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public CalendarEventKind Kind { get; private set; }
    public bool AppliesToAll { get; private set; }
    public DateOnly? AllDayDate { get; private set; }
    public DateTimeOffset? StartsAt { get; private set; }
    public DateTimeOffset? EndsAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyCollection<CalendarEventParticipant> Participants => participants;

    public static CalendarEventChangeResult Create(
        Guid residenceId,
        string title,
        string? description,
        CalendarEventKind kind,
        bool appliesToAll,
        DateOnly? allDayDate,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt,
        IReadOnlyCollection<string> participantUserIds,
        DateTimeOffset now)
    {
        var validation = Validate(residenceId, title, description, kind, appliesToAll, allDayDate, startsAt, endsAt, participantUserIds);
        return validation.Succeeded
            ? CalendarEventChangeResult.Success(new CalendarEvent(Guid.NewGuid(), residenceId, title, description, kind, appliesToAll, allDayDate, startsAt, endsAt, participantUserIds, now))
            : validation;
    }

    public CalendarEventChangeResult Update(
        string title,
        string? description,
        CalendarEventKind kind,
        bool appliesToAll,
        DateOnly? allDayDate,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt,
        IReadOnlyCollection<string> participantUserIds,
        DateTimeOffset now)
    {
        var validation = Validate(ResidenceId, title, description, kind, appliesToAll, allDayDate, startsAt, endsAt, participantUserIds);
        if (!validation.Succeeded)
        {
            return validation;
        }

        Apply(title, description, kind, appliesToAll, allDayDate, startsAt, endsAt, participantUserIds, now);
        return CalendarEventChangeResult.Success(this);
    }

    private void Apply(
        string title,
        string? description,
        CalendarEventKind kind,
        bool appliesToAll,
        DateOnly? allDayDate,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt,
        IReadOnlyCollection<string> participantUserIds,
        DateTimeOffset now)
    {
        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Kind = kind;
        AppliesToAll = appliesToAll;
        AllDayDate = kind is CalendarEventKind.Date or CalendarEventKind.Birthday ? allDayDate : null;
        StartsAt = kind == CalendarEventKind.Appointment ? startsAt!.Value.ToUniversalTime() : null;
        EndsAt = kind == CalendarEventKind.Appointment ? endsAt?.ToUniversalTime() : null;
        UpdatedAt = now;
        var requestedIds = appliesToAll
            ? new HashSet<string>(StringComparer.Ordinal)
            : participantUserIds.Where(userId => !string.IsNullOrWhiteSpace(userId)).ToHashSet(StringComparer.Ordinal);
        participants.RemoveAll(participant => !requestedIds.Contains(participant.UserId));
        participants.AddRange(requestedIds
            .Where(userId => participants.All(participant => participant.UserId != userId))
            .Select(userId => new CalendarEventParticipant(Id, ResidenceId, userId)));
    }

    private static CalendarEventChangeResult Validate(
        Guid residenceId,
        string title,
        string? description,
        CalendarEventKind kind,
        bool appliesToAll,
        DateOnly? allDayDate,
        DateTimeOffset? startsAt,
        DateTimeOffset? endsAt,
        IReadOnlyCollection<string> participantUserIds)
    {
        if (residenceId == Guid.Empty)
        {
            return CalendarEventChangeResult.Failure("residence_required", "O evento precisa pertencer a uma casa.");
        }

        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length is < 2 or > 120)
        {
            return CalendarEventChangeResult.Failure("calendar_title_invalid", "O título deve ter entre 2 e 120 caracteres.");
        }

        if (!string.IsNullOrWhiteSpace(description) && description.Trim().Length > 500)
        {
            return CalendarEventChangeResult.Failure("calendar_description_invalid", "A descrição deve ter até 500 caracteres.");
        }

        var participantIds = participantUserIds.Where(userId => !string.IsNullOrWhiteSpace(userId)).Distinct(StringComparer.Ordinal).ToList();
        if ((appliesToAll && participantIds.Count > 0) || (!appliesToAll && participantIds.Count == 0) || participantIds.Any(userId => userId.Length > 450))
        {
            return CalendarEventChangeResult.Failure("calendar_participants_invalid", "Escolha Todos da casa ou pelo menos um morador.");
        }

        if (kind is CalendarEventKind.Date or CalendarEventKind.Birthday)
        {
            if (allDayDate is null || startsAt is not null || endsAt is not null)
            {
                return CalendarEventChangeResult.Failure("calendar_date_invalid", "Datas e aniversários precisam de uma data de dia inteiro.");
            }
        }
        else if (kind == CalendarEventKind.Appointment)
        {
            if (allDayDate is not null || startsAt is null || (endsAt is not null && endsAt <= startsAt))
            {
                return CalendarEventChangeResult.Failure("calendar_time_invalid", "Informe o início e, se houver fim, use um horário posterior.");
            }
        }
        else
        {
            return CalendarEventChangeResult.Failure("calendar_kind_invalid", "Escolha um tipo de evento válido.");
        }

        return CalendarEventChangeResult.Success(null);
    }
}

public sealed class CalendarEventParticipant
{
    internal CalendarEventParticipant(Guid calendarEventId, Guid residenceId, string userId)
    {
        CalendarEventId = calendarEventId;
        ResidenceId = residenceId;
        UserId = userId;
    }

    private CalendarEventParticipant()
    {
        UserId = string.Empty;
    }

    public Guid CalendarEventId { get; private set; }
    public Guid ResidenceId { get; private set; }
    public string UserId { get; private set; }
}

public sealed record CalendarEventChangeResult(bool Succeeded, CalendarEvent? Event, string? Code, string? Message)
{
    public static CalendarEventChangeResult Success(CalendarEvent? calendarEvent) => new(true, calendarEvent, null, null);
    public static CalendarEventChangeResult Failure(string code, string message) => new(false, null, code, message);
}
