using HouseStuff.Application.Pots;
using HouseStuff.Application.Assignments;
using HouseStuff.Application.Shopping;
using HouseStuff.Domain.Residences;
using HouseStuff.Infrastructure.Identity;
using HouseStuff.Infrastructure.Shopping;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseStuff.IntegrationTests;

public sealed class ShoppingCatalogServiceTests
{
    private const string AdminConnection = "Host=localhost;Port=54329;Database=postgres;Username=housestuff;Password=housestuff_local";
    private const string TestConnection = "Host=localhost;Port=54329;Database=housestuff_shopping_integration_tests;Username=housestuff;Password=housestuff_local";

    [Fact]
    public async Task ResidentMaintainsOrderedCatalogAndCannotCrossResidences()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();
        var firstResidence = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        var secondResidence = Residence.Create("Casa Dois", "admin-2", DateTimeOffset.UtcNow).Residence!;
        database.Residences.AddRange(firstResidence, secondResidence);
        await database.SaveChangesAsync();

        var firstService = CreateService(database, firstResidence.Id, "admin-1");
        var secondService = CreateService(database, secondResidence.Id, "admin-2");
        var cleaning = (await firstService.CreateCategoryAsync(new SaveShoppingCategoryCommand("Limpeza"), CancellationToken.None)).Value!;
        var market = (await firstService.CreateCategoryAsync(new SaveShoppingCategoryCommand("Mercado"), CancellationToken.None)).Value!;
        var foreignCategory = (await secondService.CreateCategoryAsync(new SaveShoppingCategoryCommand("Outro"), CancellationToken.None)).Value!;
        var foreignItem = (await secondService.CreateItemAsync(new SaveShoppingItemCommand(foreignCategory.Id, "Item externo"), CancellationToken.None)).Value!;
        var detergent = (await firstService.CreateItemAsync(new SaveShoppingItemCommand(cleaning.Id, "Detergente"), CancellationToken.None)).Value!;

        var moved = await firstService.MoveCategoryAsync(market.Id, -1, CancellationToken.None);
        var duplicate = await firstService.CreateItemAsync(new SaveShoppingItemCommand(cleaning.Id, " detergente "), CancellationToken.None);
        var notEmpty = await firstService.DeleteCategoryAsync(cleaning.Id, CancellationToken.None);
        var crossed = await firstService.CreateItemAsync(new SaveShoppingItemCommand(foreignCategory.Id, "Item cruzado"), CancellationToken.None);
        var crossedPurchase = await firstService.CompletePurchaseAsync(new CompleteShoppingPurchaseCommand([foreignItem.Id]), CancellationToken.None);

        Assert.Equal([market.Id, cleaning.Id], moved.Value!.Select(category => category.Id));
        Assert.Equal("shopping_item_duplicated", duplicate.Code);
        Assert.Equal("shopping_category_not_empty", notEmpty.Code);
        Assert.Equal("shopping_category_not_found", crossed.Code);
        Assert.Equal("shopping_item_not_found", crossedPurchase.Code);

        Assert.True((await firstService.DeleteItemAsync(detergent.Id, CancellationToken.None)).Succeeded);
        Assert.True((await firstService.DeleteCategoryAsync(cleaning.Id, CancellationToken.None)).Succeeded);
        Assert.Single((await firstService.GetCatalogAsync(CancellationToken.None)).Value!);
        Assert.Single((await secondService.GetCatalogAsync(CancellationToken.None)).Value!);
        await database.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task FinalizingPurchaseRemovesOnlySelectedPendingItemsAndPreservesHistory()
    {
        await CreateTestDatabaseAsync();
        var options = new DbContextOptionsBuilder<HouseStuffDbContext>().UseNpgsql(TestConnection).Options;
        await using var database = new HouseStuffDbContext(options);
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();
        var residence = Residence.Create("Casa Um", "admin-1", DateTimeOffset.UtcNow).Residence!;
        database.Residences.Add(residence);
        database.Users.Add(new HouseStuffUser
        {
            Id = "admin-1",
            Name = "Luis",
            UserName = "luis@example.com",
            NormalizedUserName = "LUIS@EXAMPLE.COM",
            Email = "luis@example.com",
            NormalizedEmail = "LUIS@EXAMPLE.COM",
            ResidenceId = residence.Id,
        });
        await database.SaveChangesAsync();
        var service = CreateService(database, residence.Id, "admin-1");
        var hygiene = (await service.CreateCategoryAsync(new SaveShoppingCategoryCommand("Higiene"), CancellationToken.None)).Value!;
        var toothpaste = (await service.CreateItemAsync(new SaveShoppingItemCommand(hygiene.Id, "Pasta de dente"), CancellationToken.None)).Value!;
        var shampoo = (await service.CreateItemAsync(new SaveShoppingItemCommand(hygiene.Id, "Xampu"), CancellationToken.None)).Value!;

        var completed = await service.CompletePurchaseAsync(new CompleteShoppingPurchaseCommand([toothpaste.Id]), CancellationToken.None);
        var catalog = await service.GetCatalogAsync(CancellationToken.None);
        var history = await service.GetPurchaseHistoryAsync(CancellationToken.None);

        Assert.True(completed.Succeeded);
        Assert.Equal("Luis", completed.Value!.CompletedByName);
        Assert.Equal("Pasta de dente", Assert.Single(completed.Value.Items).ItemName);
        Assert.Equal(shampoo.Id, Assert.Single(Assert.Single(catalog.Value!).Items).Id);
        Assert.Equal("Pasta de dente", Assert.Single(Assert.Single(history.Value!).Items).ItemName);
        await database.Database.EnsureDeletedAsync();
    }

    private static ShoppingCatalogService CreateService(HouseStuffDbContext database, Guid residenceId, string userId) =>
        new(database, new StubResidenceContext(residenceId), new StubCurrentUserContext(userId, residenceId), TimeProvider.System);

    private static async Task CreateTestDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnection);
        await connection.OpenAsync();
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'housestuff_shopping_integration_tests'", connection);
        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand("CREATE DATABASE housestuff_shopping_integration_tests", connection);
            await create.ExecuteNonQueryAsync();
        }
    }

    private sealed class StubResidenceContext(Guid residenceId) : ICurrentResidenceContext
    {
        public Task<Guid?> GetResidenceIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(residenceId);
    }

    private sealed class StubCurrentUserContext(string userId, Guid residenceId) : ICurrentUserContext
    {
        public Task<CurrentUserSession?> GetAsync(CancellationToken cancellationToken) =>
            Task.FromResult<CurrentUserSession?>(new CurrentUserSession(userId, residenceId));
    }
}
