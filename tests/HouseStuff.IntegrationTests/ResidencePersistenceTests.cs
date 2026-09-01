using HouseStuff.Domain.Residences;
using HouseStuff.Domain.Pots;
using HouseStuff.Domain.Tasks;
using HouseStuff.Domain.Assignments;
using HouseStuff.Domain.Shopping;
using HouseStuff.Domain.Purchases;
using HouseStuff.Domain.Calendar;
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
    public async Task ShoppingCatalogIsUniqueAndCannotCrossResidences()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var first = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        var second = Residence.Create("Casa Dois", "admin-2", DateTimeOffset.UtcNow).Residence!;
        var firstCategory = ShoppingCategory.Create(first.Id, "Limpeza", 0, DateTimeOffset.UtcNow).Value!;
        var secondCategory = ShoppingCategory.Create(second.Id, "Limpeza", 0, DateTimeOffset.UtcNow).Value!;
        database.Residences.AddRange(first, second);
        database.ShoppingCategories.AddRange(firstCategory, secondCategory);
        await database.SaveChangesAsync();

        database.ShoppingItems.Add(ShoppingItem.Create(first.Id, secondCategory.Id, "Detergente", DateTimeOffset.UtcNow).Value!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        database.ChangeTracker.Clear();

        database.ShoppingCategories.Add(ShoppingCategory.Create(first.Id, " limpeza ", 1, DateTimeOffset.UtcNow).Value!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        database.ChangeTracker.Clear();

        database.ShoppingItems.Add(ShoppingItem.Create(first.Id, firstCategory.Id, "Detergente", DateTimeOffset.UtcNow).Value!);
        await database.SaveChangesAsync();
        database.ShoppingItems.Add(ShoppingItem.Create(first.Id, firstCategory.Id, " detergente ", DateTimeOffset.UtcNow).Value!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());

        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task PurchaseWishesRequireResidenceAndKeepPrioritiesSeparated()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var first = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        var second = Residence.Create("Casa Dois", "admin-2", DateTimeOffset.UtcNow).Residence!;
        database.Residences.AddRange(first, second);
        database.PurchaseWishes.AddRange(
            PurchaseWish.Create(first.Id, "Sofá", null, 0, DateTimeOffset.UtcNow).Wish!,
            PurchaseWish.Create(first.Id, "Mesa", null, 1, DateTimeOffset.UtcNow).Wish!,
            PurchaseWish.Create(second.Id, "Sofá", null, 0, DateTimeOffset.UtcNow).Wish!);
        await database.SaveChangesAsync();

        var firstWishes = await database.PurchaseWishes.Where(wish => wish.ResidenceId == first.Id).OrderBy(wish => wish.Priority).ToListAsync();
        Assert.Equal(["Sofá", "Mesa"], firstWishes.Select(wish => wish.Name));

        database.PurchaseWishes.Add(PurchaseWish.Create(Guid.NewGuid(), "Inválido", null, 0, DateTimeOffset.UtcNow).Wish!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task CalendarEventsPersistCivilDatesAndParticipantsInsideResidence()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var residence = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        database.Residences.Add(residence);
        database.Users.Add(new HouseStuffUser { Id = "user-1", Name = "Um", UserName = "um@house.local", ResidenceId = residence.Id });
        var birthday = CalendarEvent.Create(residence.Id, "Aniversário", null, CalendarEventKind.Birthday, false, new DateOnly(1990, 8, 25), null, null, ["user-1"], DateTimeOffset.UtcNow).Event!;
        database.CalendarEvents.Add(birthday);
        await database.SaveChangesAsync();
        database.ChangeTracker.Clear();

        var persisted = await database.CalendarEvents.Include(calendarEvent => calendarEvent.Participants).SingleAsync();
        Assert.Equal(new DateOnly(1990, 8, 25), persisted.AllDayDate);
        Assert.Equal("user-1", Assert.Single(persisted.Participants).UserId);

        database.CalendarEvents.Add(CalendarEvent.Create(Guid.NewGuid(), "Inválido", null, CalendarEventKind.Date, true, new DateOnly(2026, 8, 25), null, null, [], DateTimeOffset.UtcNow).Event!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task UserCanHaveMultipleActiveAssignmentsButTaskRemainsUnique()
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
        await database.SaveChangesAsync();
        Assert.Equal(2, await database.TaskAssignments.CountAsync(item => item.AssignedToUserId == "user-1" && item.CompletedAt == null));

        database.TaskAssignments.Add(TaskAssignment.Create(firstTask.Id, "user-2", DateTimeOffset.UtcNow).Assignment!);
        await Assert.ThrowsAsync<DbUpdateException>(() => database.SaveChangesAsync());
        database.ChangeTracker.Clear();
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task CompletionPersistsAvailabilityAndReleasesActiveAssignmentIndexes()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var completedAt = DateTimeOffset.UtcNow;
        var residence = Residence.Create("Casa Um", "admin-1", completedAt).Residence!;
        var pot = Pot.Create(residence.Id, "Mensal", null, 0, completedAt).Pot!;
        var task = HouseholdTask.Create(residence.Id, pot.Id, "Limpar geladeira", null, HouseholdTaskKind.Recurring, 30, completedAt).Task!;
        var assignment = TaskAssignment.Create(task.Id, "user-1", completedAt.AddMinutes(-10)).Assignment!;
        database.Residences.Add(residence);
        database.Pots.Add(pot);
        database.HouseholdTasks.Add(task);
        database.Users.AddRange(
            new HouseStuffUser { Id = "user-1", Name = "Um", UserName = "um@house.local", ResidenceId = residence.Id },
            new HouseStuffUser { Id = "user-2", Name = "Dois", UserName = "dois@house.local", ResidenceId = residence.Id });
        database.TaskAssignments.Add(assignment);
        await database.SaveChangesAsync();

        assignment.Complete(completedAt);
        task.RegisterCompletion(completedAt);
        database.TaskAssignments.Add(TaskAssignment.Create(task.Id, "user-2", completedAt.AddMinutes(1)).Assignment!);
        await database.SaveChangesAsync();
        database.ChangeTracker.Clear();

        var persistedTask = await database.HouseholdTasks.SingleAsync(item => item.Id == task.Id);
        Assert.NotNull(persistedTask.NextAvailableAt);
        Assert.InRange((persistedTask.NextAvailableAt.Value - completedAt.AddDays(30)).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
        Assert.Equal(2, await database.TaskAssignments.CountAsync(item => item.HouseholdTaskId == task.Id));
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
