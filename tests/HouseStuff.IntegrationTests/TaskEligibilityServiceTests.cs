using HouseStuff.Application.Assignments;
using HouseStuff.Application.Pots;
using HouseStuff.Application.Tasks;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Residences;
using HouseStuff.Infrastructure.Assignments;
using HouseStuff.Infrastructure.Identity;
using HouseStuff.Infrastructure.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseStuff.IntegrationTests;

public sealed class TaskEligibilityServiceTests
{
    private const string AdminConnection = "Host=localhost;Port=54329;Database=postgres;Username=housestuff;Password=housestuff_local";
    private const string TestConnection = "Host=localhost;Port=54329;Database=housestuff_task_eligibility_integration_tests;Username=housestuff;Password=housestuff_local";

    [Fact]
    public async Task DrawAndAcceptRespectDifficultyAndSelectedResidents()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var residence = Residence.Create("Casa Um", "user-1", DateTimeOffset.UtcNow).Residence!;
        var otherResidence = Residence.Create("Casa Dois", "user-3", DateTimeOffset.UtcNow).Residence!;
        var pot = Pot.Create(residence.Id, "Semanal", null, 0, DateTimeOffset.UtcNow).Pot!;
        database.Residences.AddRange(residence, otherResidence);
        database.Users.AddRange(
            User("user-1", "Luis", residence.Id),
            User("user-2", "Andressa", residence.Id),
            User("user-3", "Outra casa", otherResidence.Id));
        database.Pots.Add(pot);
        await database.SaveChangesAsync();

        var taskService = new HouseholdTaskService(database, new StubResidenceContext(residence.Id));
        var specific = await taskService.CreateAsync(
            new SaveHouseholdTaskCommand(pot.Id, "Limpar quintal", null, "reusable", null, "easy", false, ["user-1"]),
            CancellationToken.None);
        var forEveryone = await taskService.CreateAsync(
            new SaveHouseholdTaskCommand(pot.Id, "Organizar armário", null, "reusable", null, "hard", true, []),
            CancellationToken.None);
        var foreign = await taskService.CreateAsync(
            new SaveHouseholdTaskCommand(pot.Id, "Tarefa inválida", null, "reusable", null, "medium", false, ["user-3"]),
            CancellationToken.None);

        Assert.True(specific.Succeeded);
        Assert.Equal("easy", specific.Value!.Difficulty);
        Assert.Equal(["user-1"], specific.Value.EligibleUserIds);
        Assert.True(forEveryone.Succeeded);
        Assert.Equal("task_eligible_user_invalid", foreign.Code);

        var andressaAssignments = new TaskAssignmentService(database, new StubUserContext(new CurrentUserSession("user-2", residence.Id)));
        var unavailableDraw = await andressaAssignments.DrawAsync(new DrawTaskCommand(pot.Id, [], "easy"), CancellationToken.None);
        var unavailableAccept = await andressaAssignments.AcceptAsync(specific.Value.Id, CancellationToken.None);
        var hardDraw = await andressaAssignments.DrawAsync(new DrawTaskCommand(pot.Id, [], "hard"), CancellationToken.None);

        Assert.Equal("no_tasks_available", unavailableDraw.Code);
        Assert.Equal("task_unavailable", unavailableAccept.Code);
        Assert.True(hardDraw.Succeeded);
        Assert.Equal(forEveryone.Value!.Id, hardDraw.Value!.TaskId);
        Assert.Equal("hard", hardDraw.Value.Difficulty);

        var luisAssignments = new TaskAssignmentService(database, new StubUserContext(new CurrentUserSession("user-1", residence.Id)));
        var easyDraw = await luisAssignments.DrawAsync(new DrawTaskCommand(pot.Id, [], "easy"), CancellationToken.None);
        Assert.Equal(specific.Value.Id, easyDraw.Value!.TaskId);

        var firstAcceptance = await luisAssignments.AcceptAsync(specific.Value.Id, CancellationToken.None);
        var secondAcceptance = await luisAssignments.AcceptAsync(forEveryone.Value.Id, CancellationToken.None);
        var active = await luisAssignments.GetActiveAsync(CancellationToken.None);

        Assert.True(firstAcceptance.Succeeded);
        Assert.True(secondAcceptance.Succeeded);
        Assert.Equal(2, active.Value!.Count);

        var reservedForAndressa = await andressaAssignments.DrawAsync(new DrawTaskCommand(pot.Id, [], "hard"), CancellationToken.None);
        Assert.Equal("no_tasks_available", reservedForAndressa.Code);

        var completion = await luisAssignments.CompleteAsync(firstAcceptance.Value!.AssignmentId, CancellationToken.None);
        var remaining = await luisAssignments.GetActiveAsync(CancellationToken.None);
        Assert.True(completion.Succeeded);
        Assert.Equal(secondAcceptance.Value!.AssignmentId, Assert.Single(remaining.Value!).AssignmentId);

        await database.Database.EnsureDeletedAsync();
    }

    private static HouseStuffUser User(string id, string name, Guid residenceId) => new()
    {
        Id = id,
        Name = name,
        Email = $"{id}@house.local",
        UserName = $"{id}@house.local",
        ResidenceId = residenceId,
    };

    private static async Task CreateTestDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnection);
        await connection.OpenAsync();
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'housestuff_task_eligibility_integration_tests'", connection);
        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand("CREATE DATABASE housestuff_task_eligibility_integration_tests", connection);
            await create.ExecuteNonQueryAsync();
        }
    }

    private sealed class StubResidenceContext(Guid residenceId) : ICurrentResidenceContext
    {
        public Task<Guid?> GetResidenceIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(residenceId);
    }

    private sealed class StubUserContext(CurrentUserSession session) : ICurrentUserContext
    {
        public Task<CurrentUserSession?> GetAsync(CancellationToken cancellationToken) => Task.FromResult<CurrentUserSession?>(session);
    }
}
