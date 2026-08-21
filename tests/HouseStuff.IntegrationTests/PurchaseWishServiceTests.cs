using HouseStuff.Application.Pots;
using HouseStuff.Application.Purchases;
using HouseStuff.Domain.Residences;
using HouseStuff.Infrastructure.Identity;
using HouseStuff.Infrastructure.Purchases;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HouseStuff.IntegrationTests;

public sealed class PurchaseWishServiceTests
{
    private const string AdminConnection = "Host=localhost;Port=54329;Database=postgres;Username=housestuff;Password=housestuff_local";
    private const string TestConnection = "Host=localhost;Port=54329;Database=housestuff_purchase_wishes_integration_tests;Username=housestuff;Password=housestuff_local";

    [Fact]
    public async Task ResidentMaintainsOrderedWishesWithoutCrossingResidences()
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

        var firstService = new PurchaseWishService(database, new StubResidenceContext(firstResidence.Id));
        var secondService = new PurchaseWishService(database, new StubResidenceContext(secondResidence.Id));
        var sofa = (await firstService.CreateAsync(new SavePurchaseWishCommand("Sofá", null), CancellationToken.None)).Value!;
        var table = (await firstService.CreateAsync(new SavePurchaseWishCommand("Mesa", "https://loja.example/mesa"), CancellationToken.None)).Value!;
        var foreign = (await secondService.CreateAsync(new SavePurchaseWishCommand("Cama", null), CancellationToken.None)).Value!;

        var reordered = await firstService.ReorderAsync(new ReorderPurchaseWishesCommand([table.Id, sofa.Id]), CancellationToken.None);
        var incomplete = await firstService.ReorderAsync(new ReorderPurchaseWishesCommand([sofa.Id]), CancellationToken.None);
        var crossed = await firstService.UpdateAsync(foreign.Id, new SavePurchaseWishCommand("Inválido", null), CancellationToken.None);
        var unsafeLink = await firstService.UpdateAsync(sofa.Id, new SavePurchaseWishCommand("Sofá", "javascript:alert(1)"), CancellationToken.None);
        var updated = await firstService.UpdateAsync(sofa.Id, new SavePurchaseWishCommand("Sofá novo", "https://loja.example/sofa"), CancellationToken.None);

        Assert.Equal([table.Id, sofa.Id], reordered.Value!.Select(wish => wish.Id));
        Assert.Equal([0, 1], reordered.Value!.Select(wish => wish.Priority));
        Assert.Equal("purchase_wish_order_invalid", incomplete.Code);
        Assert.Equal("purchase_wish_not_found", crossed.Code);
        Assert.Equal("purchase_wish_url_invalid", unsafeLink.Code);
        Assert.Equal("Sofá novo", updated.Value!.Name);

        Assert.True((await firstService.DeleteAsync(table.Id, CancellationToken.None)).Succeeded);
        var remaining = (await firstService.GetAsync(CancellationToken.None)).Value!;
        Assert.Single(remaining);
        Assert.Equal(0, remaining[0].Priority);
        Assert.Equal(foreign.Id, Assert.Single((await secondService.GetAsync(CancellationToken.None)).Value!).Id);
        await database.Database.EnsureDeletedAsync();
    }

    private static async Task CreateTestDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnection);
        await connection.OpenAsync();
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'housestuff_purchase_wishes_integration_tests'", connection);
        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand("CREATE DATABASE housestuff_purchase_wishes_integration_tests", connection);
            await create.ExecuteNonQueryAsync();
        }
    }

    private sealed class StubResidenceContext(Guid residenceId) : ICurrentResidenceContext
    {
        public Task<Guid?> GetResidenceIdAsync(CancellationToken cancellationToken) => Task.FromResult<Guid?>(residenceId);
    }
}
