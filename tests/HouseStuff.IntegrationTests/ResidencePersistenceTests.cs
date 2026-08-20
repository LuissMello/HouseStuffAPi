using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Tasks;
using HouseStuff.Domain.Assignments;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseStuff.IntegrationTests;

public sealed class ResidencePersistenceTests
{
    private const string AdminConnection = "Host=localhost;Port=54329;Database=postgres;Username=housestuff;Password=housestuff_local";
    private const string TestConnection = "Host=localhost;Port=54329;Database=housestuff_integration_tests;Username=housestuff;Password=housestuff_local";

    [Fact]
    public async Task ForeignKeyRejectsResidenceThatDoesNotExist()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        database.Users.Add(new HouseStuffUser
        {
            Id = "member-1",
            Name = "Morador",
            UserName = "member@house.local",
            ResidenceId = Guid.NewGuid(),
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task ResidenceQueryKeepsMembersSeparated()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var first = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        var second = Residence.Create("Casa Dois", "admin-2", DateTimeOffset.UtcNow).Residence!;
        database.Residences.AddRange(first, second);
        database.Users.AddRange(
            new HouseStuffUser { Id = "user-1", Name = "Um", UserName = "um@house.local", ResidenceId = first.Id },
            new HouseStuffUser { Id = "user-2", Name = "Dois", UserName = "dois@house.local", ResidenceId = second.Id });
        await database.SaveChangesAsync();

        var visible = await database.Users.Where(user => user.ResidenceId == first.Id).Select(user => user.Id).ToListAsync();

        Assert.Equal(["user-1"], visible);
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task PotNamesAreUniqueInsideResidenceAndSeparatedBetweenResidences()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var first = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        var second = Residence.Create("Casa Dois", "admin-2", DateTimeOffset.UtcNow).Residence!;
        database.Residences.AddRange(first, second);
        database.Pots.AddRange(
            Pot.Create(first.Id, "Mensal", null, 0, DateTimeOffset.UtcNow).Pot!,
            Pot.Create(second.Id, "Mensal", null, 0, DateTimeOffset.UtcNow).Pot!);
        await database.SaveChangesAsync();

        var visible = await database.Pots.Where(pot => pot.ResidenceId == first.Id).Select(pot => pot.Name).ToListAsync();
        Assert.Equal(["Mensal"], visible);

        database.Pots.Add(Pot.Create(first.Id, " mensal ", null, 1, DateTimeOffset.UtcNow).Pot!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task HouseholdTaskCannotReferencePotFromAnotherResidence()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var first = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        var second = Residence.Create("Casa Dois", "admin-2", DateTimeOffset.UtcNow).Residence!;
        var secondPot = Pot.Create(second.Id, "Mensal", null, 0, DateTimeOffset.UtcNow).Pot!;
        database.Residences.AddRange(first, second);
        database.Pots.Add(secondPot);
        await database.SaveChangesAsync();

        database.HouseholdTasks.Add(HouseholdTask.Create(first.Id, secondPot.Id, "Tarefa cruzada", null, HouseholdTaskKind.OneTime, null, DateTimeOffset.UtcNow).Task!);

        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        database.ChangeTracker.Clear();
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task ActiveAssignmentIsUniqueForUserAndTask()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var residence = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        var pot = Pot.Create(residence.Id, "Diário", null, 0, DateTimeOffset.UtcNow).Pot!;
        var firstTask = HouseholdTask.Create(residence.Id, pot.Id, "Tarefa um", null, HouseholdTaskKind.Reusable, null, DateTimeOffset.UtcNow).Task!;
        var secondTask = HouseholdTask.Create(residence.Id, pot.Id, "Tarefa dois", null, HouseholdTaskKind.Reusable, null, DateTimeOffset.UtcNow).Task!;
        database.Residences.Add(residence);
        database.Pots.Add(pot);
        database.HouseholdTasks.AddRange(firstTask, secondTask);
        database.Users.AddRange(
            new HouseStuffUser { Id = "user-1", Name = "Um", UserName = "um@house.local", ResidenceId = residence.Id },
            new HouseStuffUser { Id = "user-2", Name = "Dois", UserName = "dois@house.local", ResidenceId = residence.Id });
        database.TaskAssignments.Add(TaskAssignment.Create(firstTask.Id, "user-1", DateTimeOffset.UtcNow).Assignment!);
        await database.SaveChangesAsync();

        database.TaskAssignments.Add(TaskAssignment.Create(secondTask.Id, "user-1", DateTimeOffset.UtcNow).Assignment!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        database.ChangeTracker.Clear();

        database.TaskAssignments.Add(TaskAssignment.Create(firstTask.Id, "user-2", DateTimeOffset.UtcNow).Assignment!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        database.ChangeTracker.Clear();
        await database.Database.EnsureDeletedAsync();
    }

    private static async Task CreateTestDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnection);
        await connection.OpenAsync();
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'housestuff_integration_tests'", connection);
        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand("CREATE DATABASE housestuff_integration_tests", connection);
            await create.ExecuteNonQueryAsync();
        }
    }
}
