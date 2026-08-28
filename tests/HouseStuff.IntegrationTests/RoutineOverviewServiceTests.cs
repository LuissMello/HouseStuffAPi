using HouseStuff.Application.Assignments;
using HouseStuff.Domain.Assignments;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Identity;
using HouseStuff.Infrastructure.Routine;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HouseStuff.IntegrationTests;

public sealed class RoutineOverviewServiceTests
{
    private const string AdminConnection = "Host=localhost;Port=54329;Database=postgres;Username=housestuff;Password=housestuff_local";
    private const string TestConnection = "Host=localhost;Port=54329;Database=housestuff_routine_integration_tests;Username=housestuff;Password=housestuff_local";

    [Fact]
    public async Task OverviewKeepsUpcomingInsideResidenceAndHistoryInsideUser()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var now = DateTimeOffset.UtcNow;
        var firstResidence = Residence.Create("Casa Um", "admin-1", now).Residence!;
        var secondResidence = Residence.Create("Casa Dois", "admin-2", now).Residence!;
        var firstPot = Pot.Create(firstResidence.Id, "Mensal", null, 0, now).Pot!;
        var secondPot = Pot.Create(secondResidence.Id, "Mensal", null, 0, now).Pot!;
        var mine = HouseholdTask.Create(firstResidence.Id, firstPot.Id, "Minha recorrente", null, HouseholdTaskKind.Recurring, 30, now).Task!;
        var anotherUserTask = HouseholdTask.Create(firstResidence.Id, firstPot.Id, "De outro morador", null, HouseholdTaskKind.Reusable, null, now).Task!;
        var anotherResidenceTask = HouseholdTask.Create(secondResidence.Id, secondPot.Id, "De outra casa", null, HouseholdTaskKind.Recurring, 15, now).Task!;
        mine.RegisterCompletion(now);
        anotherResidenceTask.RegisterCompletion(now);
        var myAssignment = TaskAssignment.Create(mine.Id, "user-1", now.AddMinutes(-20)).Assignment!;
        var anotherAssignment = TaskAssignment.Create(anotherUserTask.Id, "user-2", now.AddMinutes(-15)).Assignment!;
        myAssignment.Complete(now.AddMinutes(-10));
        anotherAssignment.Complete(now.AddMinutes(-5));

        database.Residences.AddRange(firstResidence, secondResidence);
        database.Pots.AddRange(firstPot, secondPot);
        database.HouseholdTasks.AddRange(mine, anotherUserTask, anotherResidenceTask);
        database.Users.AddRange(
            new HouseStuffUser { Id = "user-1", Name = "Um", UserName = "um@house.local", ResidenceId = firstResidence.Id },
            new HouseStuffUser { Id = "user-2", Name = "Dois", UserName = "dois@house.local", ResidenceId = firstResidence.Id });
        database.TaskAssignments.AddRange(myAssignment, anotherAssignment);
        await database.SaveChangesAsync();

        var service = new RoutineOverviewService(database, new StubCurrentUserContext(new CurrentUserSession("user-1", firstResidence.Id)));
        var result = await service.GetAsync(CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal(["Minha recorrente"], result.Value!.Upcoming.Select(item => item.TaskName));
        Assert.Equal(["Minha recorrente"], result.Value.History.Select(item => item.TaskName));
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task ReadinessIsHealthyWhenPostgresIsAvailable()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureCreatedAsync();
        var startup = new StartupState();
        startup.MarkReady();

        var result = await new PostgresReadinessCheck(database, startup).CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
        await database.Database.EnsureDeletedAsync();
    }

    private static async Task CreateTestDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnection);
        await connection.OpenAsync();
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'housestuff_routine_integration_tests'", connection);
        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand("CREATE DATABASE housestuff_routine_integration_tests", connection);
            await create.ExecuteNonQueryAsync();
        }
    }

    private sealed class StubCurrentUserContext(CurrentUserSession session) : ICurrentUserContext
    {
        public Task<CurrentUserSession?> GetAsync(CancellationToken cancellationToken) => Task.FromResult<CurrentUserSession?>(session);
    }
}
