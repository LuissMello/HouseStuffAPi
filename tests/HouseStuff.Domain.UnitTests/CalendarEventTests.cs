using HouseStuff.Domain.Calendar;

namespace HouseStuff.Domain.UnitTests;

public sealed class CalendarEventTests
{
    [Fact]
    public void BirthdayUsesCivilDateAndIndividualParticipants()
    {
        var date = new DateOnly(1990, 8, 25);

        var result = CalendarEvent.Create(Guid.NewGuid(), " Aniversário da Ana ", null, CalendarEventKind.Birthday, false, date, null, null, ["ana", "ana"], DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal("Aniversário da Ana", result.Event!.Title);
        Assert.Equal(date, result.Event.AllDayDate);
        Assert.Equal("ana", Assert.Single(result.Event.Participants).UserId);
    }

    [Fact]
    public void AppointmentNormalizesInstantsToUtc()
    {
        var start = new DateTimeOffset(2026, 8, 25, 14, 0, 0, TimeSpan.FromHours(-3));

        var result = CalendarEvent.Create(Guid.NewGuid(), "Dentista", " Consulta ", CalendarEventKind.Appointment, true, null, start, start.AddHours(1), [], DateTimeOffset.UtcNow);

        Assert.True(result.Succeeded);
        Assert.Equal(TimeSpan.Zero, result.Event!.StartsAt!.Value.Offset);
        Assert.Equal("Consulta", result.Event.Description);
        Assert.Empty(result.Event.Participants);
    }

    [Fact]
    public void EventRejectsMixedAudienceAndInvalidEndTime()
    {
        var now = DateTimeOffset.UtcNow;

        var mixed = CalendarEvent.Create(Guid.NewGuid(), "Reunião", null, CalendarEventKind.Appointment, true, null, now, now.AddHours(1), ["user"], now);
        var invalidEnd = CalendarEvent.Create(Guid.NewGuid(), "Reunião", null, CalendarEventKind.Appointment, false, null, now, now, ["user"], now);

        Assert.Equal("calendar_participants_invalid", mixed.Code);
        Assert.Equal("calendar_time_invalid", invalidEnd.Code);
    }

    [Fact]
    public void DateRejectsTimedFieldsAndMissingAudience()
    {
        var now = DateTimeOffset.UtcNow;

        var timedDate = CalendarEvent.Create(Guid.NewGuid(), "Feriado", null, CalendarEventKind.Date, true, new DateOnly(2026, 9, 7), now, null, [], now);
        var missingAudience = CalendarEvent.Create(Guid.NewGuid(), "Feriado", null, CalendarEventKind.Date, false, new DateOnly(2026, 9, 7), null, null, [], now);

        Assert.Equal("calendar_date_invalid", timedDate.Code);
        Assert.Equal("calendar_participants_invalid", missingAudience.Code);
    }

    [Fact]
    public void UpdateReconcilesParticipantsWithoutDuplicatingThem()
    {
        var now = DateTimeOffset.UtcNow;
        var calendarEvent = CalendarEvent.Create(Guid.NewGuid(), "Passeio", null, CalendarEventKind.Date, false, new DateOnly(2026, 9, 12), null, null, ["ana", "luis"], now).Event!;

        var result = calendarEvent.Update("Passeio novo", null, CalendarEventKind.Date, false, new DateOnly(2026, 9, 13), null, null, ["luis", "bia"], now.AddMinutes(1));

        Assert.True(result.Succeeded);
        Assert.Equal(["bia", "luis"], calendarEvent.Participants.Select(participant => participant.UserId).Order());
    }
}
