using HouseStuff.Application.Pots;
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

        var firstService = new ShoppingCatalogService(database, new StubResidenceContext(firstResidence.Id));
        var secondService = new ShoppingCatalogService(database, new StubResidenceContext(secondResidence.Id));
        var cleaning = (await firstService.CreateCategoryAsync(new SaveShoppingCategoryCommand("Limpeza"), CancellationToken.None)).Value!;
        var market = (await firstService.CreateCategoryAsync(new SaveShoppingCategoryCommand("Mercado"), CancellationToken.None)).Value!;
        var foreignCategory = (await secondService.CreateCategoryAsync(new SaveShoppingCategoryCommand("Outro"), CancellationToken.None)).Value!;
        var detergent = (await firstService.CreateItemAsync(new SaveShoppingItemCommand(cleaning.Id, "Detergente"), CancellationToken.None)).Value!;

        var moved = await firstService.MoveCategoryAsync(market.Id, -1, CancellationToken.None);
        var duplicate = await firstService.CreateItemAsync(new SaveShoppingItemCommand(cleaning.Id, " detergente "), CancellationToken.None);
        var notEmpty = await firstService.DeleteCategoryAsync(cleaning.Id, CancellationToken.None);
        var crossed = await firstService.CreateItemAsync(new SaveShoppingItemCommand(foreignCategory.Id, "Item cruzado"), CancellationToken.None);

        Assert.Equal([market.Id, cleaning.Id], moved.Value!.Select(category => category.Id));
        Assert.Equal("shopping_item_duplicated", duplicate.Code);
        Assert.Equal("shopping_category_not_empty", notEmpty.Code);
        Assert.Equal("shopping_category_not_found", crossed.Code);

        Assert.True((await firstService.DeleteItemAsync(detergent.Id, CancellationToken.None)).Succeeded);
        Assert.True((await firstService.DeleteCategoryAsync(cleaning.Id, CancellationToken.None)).Succeeded);
        Assert.Single((await firstService.GetCatalogAsync(CancellationToken.None)).Value!);
        Assert.Single((await secondService.GetCatalogAsync(CancellationToken.None)).Value!);
        await database.Database.EnsureDeletedAsync();
    }

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
}
