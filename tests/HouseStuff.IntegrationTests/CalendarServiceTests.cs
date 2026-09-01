using HouseStuff.Application.Calendar;
using HouseStuff.Application.Pots;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Calendar;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseStuff.IntegrationTests;

public sealed class CalendarServiceTests
{
    private const string AdminConnection = "Host=localhost;Port=54329;Database=postgres;Username=housestuff;Password=housestuff_local";
    private const string TestConnection = "Host=localhost;Port=54329;Database=housestuff_calendar_integration_tests;Username=housestuff;Password=housestuff_local";

    [Fact]
    public async Task ResidentMaintainsEventsAndRangeIncludesBirthdaysAndRecurringTasks()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();
        var now = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        var birthdayOccurrence = today.AddDays(2);
        var firstResidence = Residence.Create("Casa Um", "admin-1", now).Residence!;
        var secondResidence = Residence.Create("Casa Dois", "admin-2", now).Residence!;
        database.Residences.AddRange(firstResidence, secondResidence);
        database.Users.AddRange(
            new HouseStuffUser { Id = "user-1", Name = "Ana", UserName = "ana@house.local", ResidenceId = firstResidence.Id, ProfileColor = "#9B356A" },
            new HouseStuffUser { Id = "user-2", Name = "Luis", UserName = "luis@house.local", ResidenceId = firstResidence.Id, ProfileColor = "#256B78" },
            new HouseStuffUser { Id = "foreign", Name = "Outra", UserName = "outra@house.local", ResidenceId = secondResidence.Id });
        var pot = Pot.Create(firstResidence.Id, "Mensal", null, 0, now).Pot!;
        var task = HouseholdTask.Create(firstResidence.Id, pot.Id, "Limpar geladeira", null, HouseholdTaskKind.Recurring, 3, now).Task!;
        task.RegisterCompletion(now);
        database.Pots.Add(pot);
        database.HouseholdTasks.Add(task);
        await database.SaveChangesAsync();

        var firstService = new CalendarService(database, new StubResidenceContext(firstResidence.Id));
        var secondService = new CalendarService(database, new StubResidenceContext(secondResidence.Id));
        var dateEvent = (await firstService.CreateAsync(new SaveCalendarEventCommand("Feriado", null, "date", true, today.AddDays(1), null, null, []), CancellationToken.None)).Value!;
        var birthday = (await firstService.CreateAsync(new SaveCalendarEventCommand("Aniversário da Ana", null, "birthday", false,
            new DateOnly(2000, birthdayOccurrence.Month, birthdayOccurrence.Day), null, null, ["user-1"]), CancellationToken.None)).Value!;
        var appointment = (await firstService.CreateAsync(new SaveCalendarEventCommand("Dentista", "Consulta", "appointment", false,
            null, now.AddHours(4), now.AddHours(5), ["user-2"]), CancellationToken.None)).Value!;
        var foreignEvent = (await secondService.CreateAsync(new SaveCalendarEventCommand("Outro", null, "date", true, today.AddDays(1), null, null, []), CancellationToken.None)).Value!;

        var invalidParticipant = await firstService.CreateAsync(new SaveCalendarEventCommand("Inválido", null, "date", false, today, null, null, ["foreign"]), CancellationToken.None);
        var crossed = await firstService.UpdateAsync(foreignEvent.Id, new SaveCalendarEventCommand("Cruzado", null, "date", true, today, null, null, []), CancellationToken.None);
        var range = await firstService.GetRangeAsync(today, today.AddDays(10), now.AddHours(-1), now.AddDays(10), CancellationToken.None);
        var updated = await firstService.UpdateAsync(dateEvent.Id, new SaveCalendarEventCommand("Feriado local", null, "date", false, today.AddDays(1), null, null, ["user-1", "user-2"]), CancellationToken.None);

        Assert.Equal("calendar_participant_not_found", invalidParticipant.Code);
        Assert.Equal("calendar_event_not_found", crossed.Code);
        Assert.Contains(range.Value!.Entries, entry => entry.EventId == birthday.Id && entry.Date == birthdayOccurrence);
        Assert.Contains(range.Value.Entries, entry => entry.EventId == birthday.Id && entry.Participants.Single().ProfileColor == "#9B356A");
        Assert.Contains(range.Value.Entries, entry => entry.EventId == appointment.Id && entry.StartsAt == appointment.StartsAt);
        Assert.Contains(range.Value.Entries, entry => entry.Source == "task" && entry.Title == "Limpar geladeira");
        Assert.Equal(2, updated.Value!.Participants.Count);
        Assert.Equal(["#9B356A", "#256B78"], updated.Value.Participants.Select(participant => participant.ProfileColor));
        Assert.True((await firstService.DeleteAsync(dateEvent.Id, CancellationToken.None)).Succeeded);
        Assert.False(await database.CalendarEvents.AnyAsync(calendarEvent => calendarEvent.Id == dateEvent.Id));
        await database.Database.EnsureDeletedAsync();
    }

    private static async Task CreateTestDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnection);
        await connection.OpenAsync();
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'housestuff_calendar_integration_tests'", connection);
        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand("CREATE DATABASE housestuff_calendar_integration_tests", connection);
            await create.ExecuteNonQueryAsync();
        }
    }

    private sealed class StubResidenceContext(Guid residenceId) : ICurrentResidenceContext
    {
        public Task<Guid?> GetResidenceIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(residenceId);
    }
}
